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
    private int score = 0;
    private int totalMatches = 0;

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
            score++;
            scoreText.text = $"Score: {score}";

            if (score >= totalMatches)
            {
                timerRunning = false;
                endScreen.SetActive(true);
                winSound.Play();
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
        scoreText.text = "Score: 0";
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