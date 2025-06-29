using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiplayerQuizManager : QuizManagerBase
{
    public TextMeshProUGUI opponentScoreText;
    public GameObject WaitingOtherPlayer;

    private int opponentScore;
    private PhotonView photonViewComponent;
    private bool hasFinished = false;
    private bool opponentFinished = false;


    protected override void Start()
    {
        WaitingOtherPlayer.SetActive(false);
        photonViewComponent = GetComponent<PhotonView>();


        if (!gameManager.GetQuestionsDataFromLocal)
            QData = gameManager.MainQuestionList;
        timerImage = timerText.GetComponentInParent<Image>();
        baseColor = answerButtons[0].image.color;

        if (PhotonNetwork.IsMasterClient)
        {
            photonViewComponent.RPC("ReceiveQuestions", RpcTarget.All, JsonUtility.ToJson(new QuestionPack { questions = QData.QuizQuestions }));
        }
    }

    [PunRPC]
    void ReceiveQuestions(string questionJson)
    {
        QuestionPack pack = JsonUtility.FromJson<QuestionPack>(questionJson);
        QData.QuizQuestions = pack.questions;

        StartQuiz();
    }

    void StartQuiz()
    {
        currentQuestionIndex = 0;
        score = 0;
        opponentScore = 0;

        ShowQuestion();
    }

    protected override void SelectAnswer(int index)
    {
        base.SelectAnswer(index);

        bool isCorrect = index == QData.QuizQuestions[currentQuestionIndex].correctAnswerIndex;

        if (isCorrect)
        {
            photonViewComponent.RPC("UpdateOpponentScore", RpcTarget.Others, score);
        }
    }

    [PunRPC]
    void UpdateOpponentScore(int newScore)
    {
        opponentScore = newScore;
        gameManager.OpponentScore = newScore;
        opponentScoreText.text = "Opponent: " + opponentScore;
    }

    [PunRPC]
    void NotifyPlayerFinished(int opponentScore)
    {
        gameManager.OpponentScore = opponentScore;
        opponentFinished = true;

        if (hasFinished)
        {
            SceneManager.LoadScene("Results");
        }
    }

    protected override void OnQuizCompleted()
    {
        hasFinished = true;
        photonViewComponent.RPC("NotifyPlayerFinished", RpcTarget.Others, gameManager.PlayerScore);

        if (opponentFinished)
        {
            SceneManager.LoadScene("Results");
        }
        else
        {
            WaitingOtherPlayer.SetActive(true);
        }

    }

    public override void LeaveQuizMatch()
    {
        base.LeaveQuizMatch();
        if (!PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            PhotonNetwork.LeaveLobby();
        }
    }
}

[System.Serializable]
public class QuestionPack
{
    public List<Question> questions;
}
