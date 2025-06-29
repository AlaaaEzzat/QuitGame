using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;

    public Transform breakdownContent;
    public GameObject breakdownItemPrefab;

    void Start()
    {
        var gm = GameManager.Instance;

        if (gm.IsMultiplayerMatch)
        {
            if (gm.PlayerScore > gm.OpponentScore)
                resultText.text = "You Win!";
            else if (gm.PlayerScore < gm.OpponentScore)
                resultText.text = "You Lose!";
            else
                resultText.text = "Draw!";
        }
        else
        {
            resultText.text = "Quiz Completed!";
        }

        scoreText.text = $"Your Score: {gm.PlayerScore}";

        if (gm.IsMultiplayerMatch)
            scoreText.text += $"\nOpponent Score: {gm.OpponentScore}";

        ShowBreakdown();
    }

    void ShowBreakdown()
    {
        foreach (var answer in GameManager.Instance.AnswerResults)
        {
            GameObject item = Instantiate(breakdownItemPrefab, breakdownContent);

            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            texts[0].text = answer.QuestionText;
            texts[1].text = "Your Answer: " + answer.PlayerAnswer;
            texts[2].text = "Correct Answer: " + answer.CorrectAnswer;

            texts[1].color = answer.IsCorrect ? Color.green : Color.red;
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        GameManager.Instance.ResetScore();
    }
}
[System.Serializable]
public class AnswerResult
{
    public string QuestionText;
    public string PlayerAnswer;
    public string CorrectAnswer;
    public bool IsCorrect;
}
