using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Proyecto26;
using UnityEngine.Networking;

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

        FetchStatistics();
        FetchScoreHistory();
        FetchScoreGraph();
    }

    private void FetchStatistics()
    {
        string url = $"{AuthManager.Instance.serverUrl}/user/{userId}/statistics";
        
        RestClient.Get<StatisticsData>(url)
            .Then(response => {
                statisticsData = response;
                UpdateStatisticsUI();
            })
            .Catch(error => {
                Debug.LogError($"Error fetching statistics: {error.Message}");
                ShowNoDataMessage("Failed to load statistics");
            });
    }

    private void FetchScoreHistory()
    {
        string url = $"{AuthManager.Instance.serverUrl}/user/{userId}/scores";
        
        RestClient.GetArray<ScoreData>(url)
            .Then(response => {
                scoreHistory = response.ToList();
                
                if (scoreHistory != null && scoreHistory.Count > 0)
                {
                    scoreHistory = scoreHistory.OrderBy(s => DateTime.Parse(s.played_at)).ToList();
                    UpdateStatisticsUI();
                }
                else
                {
                    ShowNoDataMessage("No score history available");
                }
            })
            .Catch(error => {
                Debug.LogError($"Error fetching score history: {error.Message}");
                ShowNoDataMessage("Failed to load score history");
            });
    }

    private void FetchScoreGraph()
    {
        string url = $"{AuthManager.Instance.serverUrl}/user/{userId}/score-graph";
        
        RestClient.Get<ChartResponse>(url)
            .Then(response => {
                if (string.IsNullOrEmpty(response.chartUrl))
                {
                    ShowNoDataMessage("No score graph available");
                    return;
                }

                LoadChartImage(response.chartUrl);
            })
            .Catch(error => {
                Debug.LogError($"Error fetching score graph: {error.Message}");
                ShowNoDataMessage("Failed to load score graph");
            });
    }

    private void LoadChartImage(string imageUrl)
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
        })
        .Catch(error => {
            Debug.LogError($"Error loading chart image: {error.Message}");
            ShowNoDataMessage("Failed to load chart image");
        });
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
