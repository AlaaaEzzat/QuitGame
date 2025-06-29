using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class FetchQuestionsData : MonoBehaviour
{
    private string apiUrl = "https://lwpntihdqhchqyeeffad.supabase.co/rest/v1/questions";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imx3cG50aWhkcWhjaHF5ZWVmZmFkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTExMDI0NDEsImV4cCI6MjA2NjY3ODQ0MX0.ktypWP0Qno27AetfuBN3YGnTIWnFB3EGc7wFzF6i3FM";
    private GameManager gameManager;
    public QuestionList RecievedQuestionList;

    void Start()
    {
        if(GameManager.Instance != null)
            gameManager = GameManager.Instance;

        if (!gameManager.GetQuestionsDataFromLocal)
        {
            StartCoroutine(GetQuestions());
        }
        else
        {
            gameManager.OnQuestionsRecieved?.Invoke();
        }
    }

    IEnumerator GetQuestions()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.timeout = 5;
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"questions\":" + request.downloadHandler.text + "}";

            RecievedQuestionList = JsonUtility.FromJson<QuestionList>(json);
            gameManager.MainQuestionList.QuizQuestions = RecievedQuestionList.questions;
            gameManager.OnQuestionsRecieved?.Invoke();
        }
        else
        {
            Debug.LogError("Error fetching questions: " + request.error);
        }
    }
}
