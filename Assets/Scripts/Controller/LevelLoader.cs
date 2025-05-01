using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelLoader : MonoBehaviour
{
    public void LoadGameWithDifficulty(int difficulty)
    {
        GameManager.selectedDifficulty = (GameManager.Difficulty)difficulty;
        SceneManager.LoadScene("MainGame");
    }

    public void GoToStatistics()
    {
        SceneManager.LoadScene("Statistics");
    }
}