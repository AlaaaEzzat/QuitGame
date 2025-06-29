using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public bool GetQuestionsDataFromLocal = true;

    [Header("Events")]
    public UnityEvent OnQuestionsRecieved;
    public UnityEvent OnStartGame;

    [Header("Questions")]
    public QuestionsData MainQuestionList;
    public List<AnswerResult> AnswerResults = new List<AnswerResult>();

    public bool IsMultiplayerMatch = false;
    public int PlayerScore;
    public int OpponentScore;
    private string nextScene;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        OnQuestionsRecieved.AddListener(StartAndLoadGameScene);
    }

    public void ProcessToNextScene(string sceneName)
    {
        if(sceneName == "MultiplayerLobby")
        {
            nextScene = "MultiplayerQuiz";
            GetQuestionsDataFromLocal = false;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            nextScene = sceneName;
            GetQuestionsDataFromLocal = true;
            SceneManager.LoadScene("Loading");
        }
    }

    private void StartAndLoadGameScene()
    {
        PhotonNetwork.LoadLevel(nextScene);
    }

    public void ResetScore()
    {
        PlayerScore = 0;
        OpponentScore = 0;
        IsMultiplayerMatch = false;
        AnswerResults.Clear();
    }
}
