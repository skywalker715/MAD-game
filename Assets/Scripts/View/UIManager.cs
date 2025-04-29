using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject endScreen;
    public GameObject pauseMenuPanel;
    public TextMeshProUGUI finalScoreText;
    public AudioSource winSound;
    
    [Header("Buttons")]
    public Button pauseButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePauseMenu);
        }
    }
    
    private void OnDestroy()
    {
        
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePauseMenu);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void UpdateTimer(float timeElapsed)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60);
            int seconds = Mathf.FloorToInt(timeElapsed % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public void ShowEndScreen(int finalScore)
    {
        if (endScreen != null)
        {
            endScreen.SetActive(true);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = $"You did it! \n Final Score: {finalScore}";
            }
            
            if (winSound != null)
            {
                winSound.Play();
            }
        }
    }

    public void HideEndScreen()
    {
        if (endScreen != null)
        {
            endScreen.SetActive(false);
        }
        
        if (winSound != null)
        {
            winSound.Stop();
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            bool isActive = pauseMenuPanel.activeSelf;
            pauseMenuPanel.SetActive(!isActive);
            
            
            Time.timeScale = isActive ? 1f : 0f;
        }
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}