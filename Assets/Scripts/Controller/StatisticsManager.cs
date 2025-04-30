using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI averageScoreText;
    [SerializeField] private TextMeshProUGUI totalGamesText;
    [SerializeField] private TextMeshProUGUI improvementText;
    [SerializeField] private Image graphImage;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI noDataText;

    private int userId;
    private List<APIService.ScoreData> scoreHistory = new List<APIService.ScoreData>();
    private APIService.UserStatistics statisticsData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
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

        RefreshStatistics();
    }

    public void RefreshStatistics()
    {
        if (userId == -1)
        {
            userId = PlayerPrefs.GetInt("UserId", -1);
            if (userId == -1)
            {
                ShowNoDataMessage("User not logged in");
                return;
            }
        }

        FetchStatistics();
        FetchScoreHistory();
        FetchScoreGraph();
    }

    private void FetchStatistics()
    {
        APIService.Instance.GetUserStatistics(
            userId,
            response => {
                statisticsData = response;
                UpdateStatisticsUI();
            },
            error => {
                Debug.LogError($"Error fetching statistics: {error.Message}");
                ShowNoDataMessage("Failed to load statistics");
            }
        );
    }

    private void FetchScoreHistory()
    {
        APIService.Instance.GetUserScores(
            userId,
            response => {
                scoreHistory = response;
                
                if (scoreHistory != null && scoreHistory.Count > 0)
                {
                    scoreHistory = scoreHistory.OrderBy(s => DateTime.Parse(s.played_at)).ToList();
                    UpdateStatisticsUI();
                }
                else
                {
                    ShowNoDataMessage("No score history available");
                }
            },
            error => {
                Debug.LogError($"Error fetching score history: {error.Message}");
                ShowNoDataMessage("Failed to load score history");
            }
        );
    }

    private void FetchScoreGraph()
    {
        APIService.Instance.GetScoreGraph(
            userId,
            response => {
                if (string.IsNullOrEmpty(response.chartUrl))
                {
                    ShowNoDataMessage("No score graph available");
                    return;
                }

                LoadChartImage(response.chartUrl);
            },
            error => {
                Debug.LogError($"Error fetching score graph: {error.Message}");
                ShowNoDataMessage("Failed to load score graph");
            }
        );
    }

    private void LoadChartImage(string imageUrl)
    {
        APIService.Instance.LoadChartImage(
            imageUrl,
            sprite => {
                if (graphImage != null)
                {
                    graphImage.sprite = sprite;
                    graphImage.gameObject.SetActive(true);
                }
            },
            error => {
                Debug.LogError($"Error loading chart image: {error.Message}");
                ShowNoDataMessage("Failed to load chart image");
            }
        );
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
