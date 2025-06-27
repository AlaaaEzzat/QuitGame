using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        resultText.text = "Your Score: " + finalScore;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
