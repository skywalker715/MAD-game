using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using System.Text;
using Proyecto26;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject pauseMenuPanel;
    private float timeElapsed = 0f;
    private bool timerRunning = true;
    private int score = 0;
    private int totalMatches = 0;

    private int baseScore;
    private int attemptCount = 0;
    private float attemptPenalty;
    private float timePenalty;
    private int currentScore;

    private Card firstRevealed;
    private Card secondRevealed;
    private bool isProcessing = false;

    public enum Difficulty { Easy, Medium, Hard }
    public static Difficulty selectedDifficulty = Difficulty.Easy;
    public Difficulty difficulty = Difficulty.Easy;
    
    public int totalGames = 0;
    public int highestScore = 0;
    public float averageScore = 0;
    public string firstGameDate = "";
    public string lastGameDate = "";
    
    public List<APIService.ScoreData> scoreHistory = new List<APIService.ScoreData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        difficulty = selectedDifficulty;
        
        switch (difficulty)
        {
            case Difficulty.Easy:
                baseScore = 1000;
                attemptPenalty = 5f;
                timePenalty = 1f;
                break;
            case Difficulty.Medium:
                baseScore = 2000;
                attemptPenalty = 4f;
                timePenalty = 0.7f;
                break;
            case Difficulty.Hard:
                baseScore = 3000;
                attemptPenalty = 3f;
                timePenalty = 0.3f;
                break;
            default:
                baseScore = 1000;
                attemptPenalty = 5f;
                timePenalty = 2f;
                break;
        }
        
        currentScore = baseScore;
        UIManager.Instance.UpdateScore(currentScore);
        
        GenerateLevel();
        
        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            LoadUserStatistics(userId);
            LoadUserScores(userId);
        }
    }

    void Update()
    {
        if (timerRunning)
        {
            timeElapsed += Time.deltaTime;
            UIManager.Instance.UpdateTimer(timeElapsed);
            
            currentScore = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(baseScore - (attemptCount * attemptPenalty) - (timeElapsed * timePenalty)));
            UIManager.Instance.UpdateScore(currentScore);
        }
    }

    void GenerateLevel()
    {
        GridManager gridManager = FindObjectOfType<GridManager>();

        int matches;
        int columns;

        switch (difficulty)
        {
            case Difficulty.Easy:
                matches = 4;     
                columns = 2;
                break;
            case Difficulty.Medium:
                matches = 8;     
                columns = 4;
                break;
            case Difficulty.Hard:
                matches = 12;    
                columns = 4;
                break;
            default:
                matches = 4;
                columns = 2;
                break;
        }

        totalMatches = matches;
        gridManager.GenerateGrid(matches, columns);
    }

    public void CheckCard(Card card)
    {
        if (card == null)
        {
            Debug.LogError("Attempted to check a null card!");
            return;
        }

        if (isProcessing || card.IsFlipped || card.IsMatched)
            return;

        card.FlipCard();
        
        attemptCount++;
        
        currentScore = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(baseScore - (attemptCount * attemptPenalty) - (timeElapsed * timePenalty)));
        UIManager.Instance.UpdateScore(currentScore);

        if (firstRevealed == null)
        {
            firstRevealed = card;
        }
        else
        {
            secondRevealed = card;
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        isProcessing = true;
        yield return new WaitForSeconds(1f);

        if (firstRevealed == null || secondRevealed == null)
        {
            Debug.LogError("One of the revealed cards is null!");
            isProcessing = false;
            yield break;
        }

        if (firstRevealed.cardId == secondRevealed.cardId)
        {
            firstRevealed.SetMatched(true);
            secondRevealed.SetMatched(true);
            score++;
            
            if (score >= totalMatches)
            {
                timerRunning = false;
                UIManager.Instance.ShowEndScreen(currentScore);
                SendScoreToDatabase();
            }
        }
        else
        {
            firstRevealed.FlipCard();
            secondRevealed.FlipCard();
        }

        firstRevealed = null;
        secondRevealed = null;
        isProcessing = false;
    }

    private void SendScoreToDatabase()
    {
        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId == -1)
        {
            Debug.LogError("User not logged in!");
            return;
        }

        APIService.Instance.SendGameScore(
            userId,
            currentScore,
            response => {
                Debug.Log("Score saved successfully: " + response);
                LoadUserStatistics(userId);
                LoadUserScores(userId);
            },
            error => {
                Debug.LogError("Error saving score: " + error.Message);
            }
        );
    }
    
    public void LoadUserStatistics(int userId)
    {
        APIService.Instance.GetUserStatistics(
            userId,
            stats => {
                totalGames = stats.total_games;
                highestScore = stats.highest_score;
                averageScore = stats.average_score;
                firstGameDate = stats.first_game;
                lastGameDate = stats.last_game;
                
                Debug.Log($"User statistics loaded: {totalGames} games, highest score: {highestScore}");
            },
            error => {
                Debug.LogError("Error loading user statistics: " + error.Message);
            }
        );
    }
    
    public void LoadUserScores(int userId)
    {
        APIService.Instance.GetUserScores(
            userId,
            scores => {
                scoreHistory.Clear();
                scoreHistory.AddRange(scores);
                Debug.Log($"User scores loaded: {scoreHistory.Count} scores");
            },
            error => {
                Debug.LogError("Error loading user scores: " + error.Message);
            }
        );
    }
    
    public void GetScoreGraphUrl(int userId, Action<string> callback)
    {
        APIService.Instance.GetScoreGraph(
            userId,
            result => {
                callback(result.chartUrl);
            },
            error => {
                Debug.LogError("Error getting score graph: " + error.Message);
                callback("");
            }
        );
    }

    public void RestartGame()
    {
        foreach (Card card in FindObjectsOfType<Card>())
        {
            if (card != null)
            {
                card.SetMatched(false);
                if (card.IsFlipped)
                    card.FlipCard();
            }
        }

        score = 0;
        timeElapsed = 0f;
        timerRunning = true;
        
        attemptCount = 0;
        currentScore = baseScore;
        UIManager.Instance.UpdateScore(currentScore);
        UIManager.Instance.UpdateTimer(timeElapsed);
        UIManager.Instance.HideEndScreen();
        
        firstRevealed = null;
        secondRevealed = null;
         if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScene");
    }
}

