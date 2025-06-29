using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;

public class DashboardEditor : EditorWindow
{
    private const string apiUrl = "https://lwpntihdqhchqyeeffad.supabase.co/rest/v1/questions";
    private const string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imx3cG50aWhkcWhjaHF5ZWVmZmFkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTExMDI0NDEsImV4cCI6MjA2NjY3ODQ0MX0.ktypWP0Qno27AetfuBN3YGnTIWnFB3EGc7wFzF6i3FM";

    private List<QuestionItem> questions = new List<QuestionItem>();
    private Vector2 scrollPos;

    private string newQuestionText = "";
    private string[] newAnswers = new string[4];
    private int newCorrectIndex = 0;
    private string selectedQuestionId = null;

    [MenuItem("Quiz Game/Question Dashboard")]
    public static void ShowWindow()
    {
        GetWindow<DashboardEditor>("Question Dashboard");
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawQuestionList();
        GUILayout.FlexibleSpace();
        DrawQuestionForm();
    }

    #region UI Sections

    private void DrawHeader()
    {
        GUILayout.Label("Quiz Game Dashboard", EditorStyles.boldLabel);

        if (GUILayout.Button("Fetch Questions"))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(GetQuestions());
        }

        GUILayout.Label("Total Questions: " + questions.Count);
    }

    private void DrawQuestionList()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
        foreach (var q in questions)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Question:", q.questionText);

            for (int i = 0; i < q.answers.Length; i++)
            {
                EditorGUILayout.LabelField($"Answer {i + 1}:", q.answers[i]);
            }

            EditorGUILayout.LabelField("Correct Answer Index:", q.correctAnswerIndex.ToString());

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Edit"))
            {
                SelectQuestionForEditing(q);
            }

            if (GUILayout.Button("Delete"))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(DeleteQuestion(q.id));
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawQuestionForm()
    {
        GUILayout.Space(20);
        GUILayout.Label("Add / Update Question", EditorStyles.boldLabel);

        newQuestionText = EditorGUILayout.TextField("Question", newQuestionText);

        for (int i = 0; i < 4; i++)
        {
            newAnswers[i] = EditorGUILayout.TextField($"Answer {i + 1}", newAnswers[i]);
        }

        newCorrectIndex = EditorGUILayout.IntField("Correct Answer Index", newCorrectIndex);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Question"))
        {
            var newQuestion = BuildQuestionFromInput();
            EditorCoroutineUtility.StartCoroutineOwnerless(AddQuestion(newQuestion));
        }

        if (!string.IsNullOrEmpty(selectedQuestionId))
        {
            if (GUILayout.Button("Update Question"))
            {
                var updatedQuestion = BuildQuestionFromInput();
                updatedQuestion.id = selectedQuestionId;
                EditorCoroutineUtility.StartCoroutineOwnerless(UpdateQuestion(updatedQuestion));
            }
        }

        GUILayout.EndHorizontal();
    }

    private void SelectQuestionForEditing(QuestionItem q)
    {
        selectedQuestionId = q.id;
        newQuestionText = q.questionText;
        newAnswers = (string[])q.answers.Clone();
        newCorrectIndex = q.correctAnswerIndex;
    }

    private QuestionItem BuildQuestionFromInput()
    {
        return new QuestionItem
        {
            questionText = newQuestionText,
            answers = newAnswers,
            correctAnswerIndex = newCorrectIndex
        };
    }

    #endregion

    #region API Calls

    private System.Collections.IEnumerator GetQuestions()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        SetRequestHeaders(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"questions\":" + request.downloadHandler.text + "}";
            QuestionList list = JsonUtility.FromJson<QuestionList>(json);
            questions = new List<QuestionItem>(list.questions);

            Debug.Log("Questions loaded.");
        }
        else
        {
            Debug.LogError("Error fetching questions: " + request.error);
        }
    }

    private System.Collections.IEnumerator AddQuestion(QuestionItem question)
    {
        string answersFormatted = PostgresArrayFormatter.FormatStringArray(question.answers);
        string jsonData = $"{{\"questionText\":\"{question.questionText}\",\"answers\":\"{answersFormatted}\",\"correctAnswerIndex\":{question.correctAnswerIndex}}}";

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        SetRequestHeaders(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Question added.");
            EditorCoroutineUtility.StartCoroutineOwnerless(GetQuestions());
        }
        else
        {
            Debug.LogError("Error adding question: " + request.error);
            Debug.LogError("Request Sent: " + jsonData);
        }
    }

    private System.Collections.IEnumerator DeleteQuestion(string questionId)
    {
        UnityWebRequest request = UnityWebRequest.Delete(apiUrl + "?id=eq." + questionId);
        SetRequestHeaders(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Question deleted.");
            EditorCoroutineUtility.StartCoroutineOwnerless(GetQuestions());
        }
        else
        {
            Debug.LogError("Error deleting question: " + request.error);
        }
    }

    private System.Collections.IEnumerator UpdateQuestion(QuestionItem question)
    {
        string jsonData = JsonUtility.ToJson(question);

        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "?id=eq." + question.id, jsonData);
        SetRequestHeaders(request);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Question updated successfully.");
            selectedQuestionId = null;
            EditorCoroutineUtility.StartCoroutineOwnerless(GetQuestions());
        }
        else
        {
            Debug.LogError("Error updating question: " + request.error);
        }
    }

    private void SetRequestHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
    }

    #endregion

    #region Data Classes

    [System.Serializable]
    public class QuestionItem
    {
        public string id;
        public string questionText;
        public string[] answers;
        public int correctAnswerIndex;
    }

    [System.Serializable]
    public class QuestionList
    {
        public QuestionItem[] questions;
    }

    #endregion
}

public static class PostgresArrayFormatter
{
    public static string FormatStringArray(string[] array)
    {
        return "{" + string.Join(",", array) + "}";
    }
}