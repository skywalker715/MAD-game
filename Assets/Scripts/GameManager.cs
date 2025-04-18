using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TMP_Text timerText;
    public TMP_Text scoreText;
    public GameObject endScreen;
    public AudioSource winSound;

    private float timeElapsed = 0f;
    private bool timerRunning = true;
    private int score = 0; // Kept for backward compatibility
    private int totalMatches = 0;

    // New scoring system variables
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
        
        // Initialize scoring system based on difficulty
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
        
        // Initialize current score
        currentScore = baseScore;
        scoreText.text = $"Score: {currentScore}";
        
        GenerateLevel();
    }

    void Update()
    {
        if (timerRunning)
        {
            timeElapsed += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timeElapsed / 60);
            int seconds = Mathf.FloorToInt(timeElapsed % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            
            // Apply time penalty to score
            currentScore = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(baseScore - (attemptCount * attemptPenalty) - (timeElapsed * timePenalty)));
            scoreText.text = $"Score: {currentScore}";
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
                matches = 4;     // 8 cards
                columns = 2;
                break;
            case Difficulty.Medium:
                matches = 8;     // 16 cards
                columns = 4;
                break;
            case Difficulty.Hard:
                matches = 12;    // 24 cards
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
        if (isProcessing || card.IsFlipped || card.IsMatched)
            return;

        card.FlipCard();
        
        // Increment attempt count for each card flip
        attemptCount++;
        
        // Update score based on the formula
        currentScore = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(baseScore - (attemptCount * attemptPenalty) - (timeElapsed * timePenalty)));
        scoreText.text = $"Score: {currentScore}";

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

        if (firstRevealed.cardId == secondRevealed.cardId)
        {
            firstRevealed.SetMatched(true);
            secondRevealed.SetMatched(true);
            score++; // Keep for backward compatibility
            
            // Score is already updated in CheckCard and Update methods
            
            if (score >= totalMatches)
            {
                timerRunning = false;
                endScreen.SetActive(true);
                winSound.Play();
                
                // Display final score on end screen if there's a text component
                TMP_Text finalScoreText = endScreen.GetComponentInChildren<TMP_Text>();
                if (finalScoreText != null)
                {
                    finalScoreText.text = $"You did it! \n Final Score: {currentScore}";
                }
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
        
        // Reset scoring system
        attemptCount = 0;
        currentScore = baseScore;
        scoreText.text = $"Score: {currentScore}";
        timerText.text = "Time: 00:00";

        if (endScreen != null)
            endScreen.SetActive(false);
        winSound.Stop();
        firstRevealed = null;
        secondRevealed = null;
    }
    
    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScene");
    }
}