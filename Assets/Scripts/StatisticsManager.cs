using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class StatisticsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI averageScoreText;
    [SerializeField] private TextMeshProUGUI totalGamesText;
    [SerializeField] private TextMeshProUGUI improvementText;
    [SerializeField] private Image graphImage;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI noDataText;

    private int userId;
    private List<ScoreData> scoreHistory = new List<ScoreData>();
    private StatisticsData statisticsData;

    [Serializable]
    private class ScoreData
    {
        public int score;
        public string played_at;
    }

    [Serializable]
    private class StatisticsData
    {
        public int total_games;
        public int highest_score;
        public double average_score;
        public string first_game;
        public string last_game;
    }

    [Serializable]
    private class ChartResponse
    {
        public string chartUrl;
    }

    [Serializable]
    private class ScoreDataWrapper
    {
        public List<ScoreData> Items;
    }

    private void Start()
    {
        userId = PlayerPrefs.GetInt("UserId", -1);
        
        if (userId == -1)
        {
            Debug.LogError("User ID not found. User may not be logged in.");
            ShowNoDataMessage("User not logged in");
            return;
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        StartCoroutine(FetchStatistics());
        StartCoroutine(FetchScoreHistory());
        StartCoroutine(FetchScoreGraph());
    }

    private IEnumerator FetchStatistics()
    {
        string url = $"{AuthManager.Instance.serverUrl}/user/{userId}/statistics";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching statistics: {request.error}");
                ShowNoDataMessage("Failed to load statistics");
                yield break;
            }

            try
            {
                statisticsData = JsonUtility.FromJson<StatisticsData>(request.downloadHandler.text);
                UpdateStatisticsUI();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing statistics data: {e.Message}");
                ShowNoDataMessage("Error parsing statistics data");
            }
        }
    }

    private IEnumerator FetchScoreHistory()
    {
        string url = $"{AuthManager.Instance.serverUrl}/user/{userId}/scores";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching score history: {request.error}");
                ShowNoDataMessage("Failed to load score history");
                yield break;
            }

            try
            {
                string jsonText = "{\"Items\":" + request.downloadHandler.text + "}";
                ScoreDataWrapper wrapper = JsonUtility.FromJson<ScoreDataWrapper>(jsonText);
                scoreHistory = wrapper.Items;
                
                if (scoreHistory != null && scoreHistory.Count > 0)
                {
                    scoreHistory = scoreHistory.OrderBy(s => DateTime.Parse(s.played_at)).ToList();
                    UpdateStatisticsUI();
                }
                else
                {
                    ShowNoDataMessage("No score history available");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing score history: {e.Message}");
                ShowNoDataMessage("Error parsing score history");
            }
        }
    }

    private IEnumerator FetchScoreGraph()
    {
        string url = $"{AuthManager.Instance.serverUrl}/user/{userId}/score-graph";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching score graph: {request.error}");
                ShowNoDataMessage("Failed to load score graph");
                yield break;
            }

            try
            {
                ChartResponse response = JsonUtility.FromJson<ChartResponse>(request.downloadHandler.text);
                if (string.IsNullOrEmpty(response.chartUrl))
                {
                    ShowNoDataMessage("No score graph available");
                    yield break;
                }

                StartCoroutine(LoadChartImage(response.chartUrl));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing score graph response: {e.Message}");
                ShowNoDataMessage("Error parsing score graph data");
            }
        }
    }

    private IEnumerator LoadChartImage(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error loading chart image: {request.error}");
                ShowNoDataMessage("Failed to load chart image");
                yield break;
            }

            try
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                
                if (graphImage != null)
                {
                    graphImage.sprite = sprite;
                    graphImage.gameObject.SetActive(true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating chart sprite: {e.Message}");
                ShowNoDataMessage("Error displaying chart image");
            }
        }
    }

    private void UpdateStatisticsUI()
    {
        if (statisticsData == null)
        {
            ShowNoDataMessage("No statistics available");
            return;
        }

        if (bestScoreText != null)
            bestScoreText.text = $"Best Score: {statisticsData.highest_score}";
        
        if (averageScoreText != null)
            averageScoreText.text = $"Average Score: {statisticsData.average_score:F1}";
        
        if (totalGamesText != null)
            totalGamesText.text = $"Total Games: {statisticsData.total_games}";

        if (improvementText != null)
        {
            if (scoreHistory != null && scoreHistory.Count > 0)
            {
                int lastScore = scoreHistory.Last().score;
                double averageScore = statisticsData.average_score;
                
                double difference = lastScore - averageScore;
                double improvementPercentage = (difference / averageScore) * 100.0;
                
                string improvementSign = difference >= 0 ? "+" : "";
                improvementText.text = $"Improvement: {improvementSign}{improvementPercentage:F1}%";
                improvementText.color = difference >= 0 ? Color.green : Color.red;
            }
            else
            {
                improvementText.text = "Improvement: N/A";
            }
        }
    }

    private void ShowNoDataMessage(string message)
    {
        if (noDataText != null)
        {
            noDataText.text = message;
            noDataText.gameObject.SetActive(true);
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("HomeScene");
    }
}
