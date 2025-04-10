using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSettings
{
    public static GameManager.Difficulty selectedDifficulty;
}

public class LevelLoader : MonoBehaviour
{
    public void LoadGameWithDifficulty(int difficulty)
    {
        GameManager.selectedDifficulty = (GameManager.Difficulty)difficulty;
        SceneManager.LoadScene("MainGame");
    }
}