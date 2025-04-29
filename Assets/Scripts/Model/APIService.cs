using System;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using System.Linq;
using UnityEngine.Networking;

public class APIService : MonoBehaviour
{
    private static APIService _instance;
    public static APIService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("APIService");
                _instance = obj.AddComponent<APIService>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public string serverUrl = "http://localhost:3000";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void RegisterUser(string username, string password, Action<RegisterResponse> onSuccess, Action<Exception> onError)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            onError?.Invoke(new Exception("Username or password is empty"));
            return;
        }
        
        UserData userData = new UserData
        {
            username = username,
            password = password
        };
        
        string jsonData = JsonUtility.ToJson(userData);
        Debug.Log($"Sending registration request to: {serverUrl}/register with data: {jsonData}");
        
        RestClient.Post<RegisterResponse>($"{serverUrl}/register", userData)
            .Then(response => {
                Debug.Log("Registration response: " + JsonUtility.ToJson(response));
                onSuccess?.Invoke(response);
            })
            .Catch(error => {
                Debug.LogError("Registration error: " + error.Message);
                if (error is RequestException requestError)
                {
                    Debug.LogError($"Status code: {requestError.StatusCode}, Response: {requestError.Response}");
                }
                onError?.Invoke(error);
            });
    }

    public void LoginUser(string username, string password, Action<LoginResponse> onSuccess, Action<Exception> onError)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            onError?.Invoke(new Exception("Username or password is empty"));
            return;
        }
        
        UserData userData = new UserData
        {
            username = username,
            password = password
        };
        
        string jsonData = JsonUtility.ToJson(userData);
        Debug.Log($"Sending login request to: {serverUrl}/login with data: {jsonData}");
        
        RestClient.Post<LoginResponse>($"{serverUrl}/login", userData)
            .Then(response => {
                Debug.Log("Login response: " + JsonUtility.ToJson(response));
                onSuccess?.Invoke(response);
            })
            .Catch(error => {
                Debug.LogError("Login error: " + error.Message);
                if (error is RequestException requestError)
                {
                    Debug.LogError($"Status code: {requestError.StatusCode}, Response: {requestError.Response}");
                }
                onError?.Invoke(error);
            });
    }



    public void SendGameScore(int userId, int score, Action<string> onSuccess, Action<Exception> onError)
    {
        if (userId == -1)
        {
            onError?.Invoke(new Exception("User not logged in!"));
            return;
        }

        ScoreSubmission scoreData = new ScoreSubmission
        {
            user_id = userId,
            score = score
        };
        
        string jsonData = JsonUtility.ToJson(scoreData);
        Debug.Log($"Sending score to: {serverUrl}/game-score with data: {jsonData}");
        
        RestClient.Post($"{serverUrl}/game-score", scoreData)
            .Then(response => {
                Debug.Log("Score saved successfully: " + response.Text);
                onSuccess?.Invoke(response.Text);
            })
            .Catch(error => {
                Debug.LogError("Error saving score: " + error.Message);
                if (error is RequestException requestError)
                {
                    Debug.LogError($"Status code: {requestError.StatusCode}, Response: {requestError.Response}");
                }
                onError?.Invoke(error);
            });
    }



    public void GetUserStatistics(int userId, Action<UserStatistics> onSuccess, Action<Exception> onError)
    {
        RestClient.Get<UserStatistics>($"{serverUrl}/user/{userId}/statistics")
            .Then(response => {
                Debug.Log($"User statistics loaded: {response.total_games} games, highest score: {response.highest_score}");
                onSuccess?.Invoke(response);
            })
            .Catch(error => {
                Debug.LogError("Error loading user statistics: " + error.Message);
                onError?.Invoke(error);
            });
    }
    
    public void GetUserScores(int userId, Action<List<ScoreData>> onSuccess, Action<Exception> onError)
    {
        RestClient.GetArray<ScoreData>($"{serverUrl}/user/{userId}/scores")
            .Then(response => {
                List<ScoreData> scoreHistory = response.ToList();
                Debug.Log($"User scores loaded: {scoreHistory.Count} scores");
                onSuccess?.Invoke(scoreHistory);
            })
            .Catch(error => {
                Debug.LogError("Error loading user scores: " + error.Message);
                onError?.Invoke(error);
            });
    }
    
    public void GetScoreGraph(int userId, Action<ScoreGraphResponse> onSuccess, Action<Exception> onError)
    {
        RestClient.Get<ScoreGraphResponse>($"{serverUrl}/user/{userId}/score-graph")
            .Then(response => {
                onSuccess?.Invoke(response);
            })
            .Catch(error => {
                Debug.LogError("Error getting score graph: " + error.Message);
                onError?.Invoke(error);
            });
    }

    public void LoadChartImage(string imageUrl, Action<Sprite> onSuccess, Action<Exception> onError)
    {
        RestClient.Get(new RequestHelper {
            Uri = imageUrl,
            DownloadHandler = new DownloadHandlerTexture()
        })
        .Then(response => {
            try
            {
                Texture2D texture = ((DownloadHandlerTexture)response.Request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                onSuccess?.Invoke(sprite);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating chart sprite: {e.Message}");
                onError?.Invoke(e);
            }
        })
        .Catch(error => {
            Debug.LogError($"Error loading chart image: {error.Message}");
            onError?.Invoke(error);
        });
    }



    [Serializable]
    public class RegisterResponse
    {
        public int id;
        public string username;
    }

    [Serializable]
    public class LoginResponse
    {
        public int id;
        public string username;
        public string message;
    }

    [Serializable]
    public class UserData
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class ScoreData
    {
        public int score;
        public string played_at;
    }

    [Serializable]
    public class UserStatistics
    {
        public int total_games;
        public int highest_score;
        public float average_score;
        public string first_game;
        public string last_game;
    }
    
    [Serializable]
    public class ScoreGraphResponse
    {
        public string chartUrl;
    }

    [Serializable]
    public class ScoreSubmission
    {
        public int user_id;
        public int score;
    }

} 