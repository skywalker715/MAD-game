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
    [SerializeField] private RectTransform graphContent;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI noDataText;

    [Header("Graph Settings")]
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private float graphPadding = 20f;
    [SerializeField] private float graphHeight = 300f;
    [SerializeField] private float graphWidth = 500f;

    private int userId;
    private const string SERVER_URL = "http://localhost:3000";
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

    private void Start()
    {
        // Get user ID from PlayerPrefs
        userId = PlayerPrefs.GetInt("UserId", -1);
        
        if (userId == -1)
        {
            Debug.LogError("User ID not found. User may not be logged in.");
            ShowNoDataMessage("User not logged in");
            return;
        }

        // Set up main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        // Fetch statistics and score history
        StartCoroutine(FetchStatistics());
        StartCoroutine(FetchScoreHistory());
    }

    private IEnumerator FetchStatistics()
    {
        string url = $"{SERVER_URL}/user/{userId}/statistics";
        
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
        string url = $"{SERVER_URL}/user/{userId}/scores";
        
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
                // Parse the JSON array
                string jsonText = "{\"Items\":" + request.downloadHandler.text + "}";
                ScoreDataWrapper wrapper = JsonUtility.FromJson<ScoreDataWrapper>(jsonText);
                scoreHistory = wrapper.Items;
                
                if (scoreHistory != null && scoreHistory.Count > 0)
                {
                    // Sort by date
                    scoreHistory = scoreHistory.OrderBy(s => DateTime.Parse(s.played_at)).ToList();
                    DrawScoreGraph();
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

    [Serializable]
    private class ScoreDataWrapper
    {
        public List<ScoreData> Items;
    }

    private void UpdateStatisticsUI()
    {
        if (statisticsData == null)
        {
            ShowNoDataMessage("No statistics available");
            return;
        }

        // Update UI with statistics
        if (bestScoreText != null)
            bestScoreText.text = $"Best Score: {statisticsData.highest_score}";
        
        if (averageScoreText != null)
            averageScoreText.text = $"Average Score: {statisticsData.average_score:F1}";
        
        if (totalGamesText != null)
            totalGamesText.text = $"Total Games: {statisticsData.total_games}";

        // Calculate improvement percentage
        if (scoreHistory != null && scoreHistory.Count >= 2)
        {
            int firstScore = scoreHistory.First().score;
            int lastScore = scoreHistory.Last().score;
            
            float improvementPercentage = 0f;
            if (firstScore > 0)
            {
                improvementPercentage = ((float)(lastScore - firstScore) / firstScore) * 100f;
            }
            
            if (improvementText != null)
            {
                string improvementSign = improvementPercentage >= 0 ? "+" : "";
                improvementText.text = $"Improvement: {improvementSign}{improvementPercentage:F1}%";
                
                // Color the text based on improvement
                improvementText.color = improvementPercentage >= 0 ? Color.green : Color.red;
            }
        }
        else if (improvementText != null)
        {
            improvementText.text = "Improvement: N/A";
        }
    }

    private void DrawScoreGraph()
    {
        if (scoreHistory == null || scoreHistory.Count < 2 || graphContent == null)
        {
            return;
        }

        // Clear existing graph
        foreach (Transform child in graphContent)
        {
            Destroy(child.gameObject);
        }

        // Find min and max scores for scaling
        int minScore = scoreHistory.Min(s => s.score);
        int maxScore = scoreHistory.Max(s => s.score);
        
        // Ensure we have a range to display
        if (minScore == maxScore)
        {
            minScore = Math.Max(0, minScore - 100);
            maxScore = maxScore + 100;
        }

        // Calculate scale factors
        float xScale = graphWidth / (scoreHistory.Count - 1);
        float yScale = graphHeight / (maxScore - minScore);

        // Create points and lines
        for (int i = 0; i < scoreHistory.Count; i++)
        {
            // Create point
            GameObject point = Instantiate(pointPrefab, graphContent);
            RectTransform pointRect = point.GetComponent<RectTransform>();
            
            // Position point
            float xPos = i * xScale;
            float yPos = (scoreHistory[i].score - minScore) * yScale;
            pointRect.anchoredPosition = new Vector2(xPos, yPos);

            // Add tooltip with score and date
            string date = DateTime.Parse(scoreHistory[i].played_at).ToString("MM/dd/yyyy");
            TooltipTrigger tooltipTrigger = point.GetComponent<TooltipTrigger>();
            if (tooltipTrigger != null)
            {
                tooltipTrigger.SetTooltip($"Score: {scoreHistory[i].score}\nDate: {date}");
            }

            // Create line connecting to previous point (except for first point)
            if (i > 0)
            {
                GameObject line = Instantiate(linePrefab, graphContent);
                RectTransform lineRect = line.GetComponent<RectTransform>();
                
                // Calculate line position and rotation
                float prevXPos = (i - 1) * xScale;
                float prevYPos = (scoreHistory[i - 1].score - minScore) * yScale;
                
                // Position line at midpoint between points
                lineRect.anchoredPosition = new Vector2((xPos + prevXPos) / 2, (yPos + prevYPos) / 2);
                
                // Calculate line length and rotation
                float lineLength = Vector2.Distance(
                    new Vector2(prevXPos, prevYPos), 
                    new Vector2(xPos, yPos)
                );
                
                lineRect.sizeDelta = new Vector2(lineLength, lineRect.sizeDelta.y);
                
                // Calculate rotation angle
                float angle = Mathf.Atan2(yPos - prevYPos, xPos - prevXPos) * Mathf.Rad2Deg;
                lineRect.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void ShowNoDataMessage(string message)
    {
        if (noDataText != null)
        {
            noDataText.gameObject.SetActive(true);
            noDataText.text = message;
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("HomeScene");
    }
}

// Simple tooltip trigger class
public class TooltipTrigger : MonoBehaviour
{
    private string tooltipText = "";

    public void SetTooltip(string text)
    {
        tooltipText = text;
    }

    public string GetTooltip()
    {
        return tooltipText;
    }
}
