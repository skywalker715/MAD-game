using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    public GameObject endScreen;
    public AudioSource winSound;
    
    private float timer;
    private bool isTimerRunning = true;

    private Card firstRevealed;
    private Card secondRevealed;
    private int score;
    private int totalMatches;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = 0f;
        isTimerRunning = true;
        totalMatches = GameObject.FindObjectsOfType<Card>().Length / 2;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public void CardRevealed(Card card)
    {
        if (firstRevealed == null)
        {
            firstRevealed = card;
        }
        else if (secondRevealed == null)
        {
            secondRevealed = card;
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator<WaitForSeconds> CheckMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstRevealed.cardId == secondRevealed.cardId)
        {
            firstRevealed.SetMatched(true);
            secondRevealed.SetMatched(true);
            score++;
            scoreText.text = $"Score: {score}";

            if (score >= totalMatches)
            {
                EndGame();
            }
        }
        else
        {
            firstRevealed.FlipCard();
            secondRevealed.FlipCard();
        }

        firstRevealed = null;
        secondRevealed = null;
    }

    private void EndGame()
    {
        isTimerRunning = false;
        winSound?.Play();
        endScreen.SetActive(true);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame"); // Use your scene name
    }
}