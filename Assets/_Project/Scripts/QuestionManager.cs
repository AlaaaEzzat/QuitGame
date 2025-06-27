using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class QuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI questionNumberText;
    public CanvasGroup questionCanvasGroup;
    public CanvasGroup[] answerButtonGroups;

    [Header("Quiz Settings")]
    public List<Question> questions;
    public float TimePerQuestion = 10f;
    public float QuestionFadeDuration;
    public float ButtonFadeDuration;

    private int currentQuestionIndex;
    private int score;
    private float currentTime;
    private bool answered;
    private Color BaseColor;
    private bool questionStarted = false;


    void Start()
    {
        BaseColor = answerButtons[0].image.color;
        currentQuestionIndex = 0;
        score = 0;
        ShowQuestion();
    }

    void Update()
    {
        if (!answered && questionStarted)
        {
            currentTime -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTime).ToString();
            timerText.GetComponentInParent<Image>().fillAmount = currentTime / 10;

            if (currentTime <= 0f)
            {
                SkipQuestion();
            }
        }
    }

    void ShowQuestion()
    {
        answered = false;
        currentTime = TimePerQuestion;
        timerText.GetComponentInParent<Image>().fillAmount = 1;
        questionNumberText.text = "Question: " + (currentQuestionIndex + 1);

        Question q = questions[currentQuestionIndex];
        questionText.text = q.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[i];

            int capturedIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => SelectAnswer(capturedIndex));
            answerButtonGroups[i].alpha = 0f;
            answerButtonGroups[i].interactable = false;
        }
        AnimateQuestionAndButtons();
    }

    void StartTimer()
    {
        currentTime = TimePerQuestion;
        questionStarted = true;
    }

    void SelectAnswer(int index)
    {
        answered = true;
        bool isCorrect = index == questions[currentQuestionIndex].correctAnswerIndex;

        if (isCorrect)
        {
            answerButtons[index].image.color = Color.green;
            score++;
        }
        else
        {
            answerButtons[index].image.color = Color.red;
            answerButtons[questions[currentQuestionIndex].correctAnswerIndex].image.color = Color.green;
        }

        Invoke(nameof(NextQuestion), 1.5f);
        questionStarted = false;
    }

    void SkipQuestion()
    {
        answered = true;
        questionStarted = false;
        answerButtons[questions[currentQuestionIndex].correctAnswerIndex].image.color = Color.green;

        Invoke(nameof(NextQuestion), 1.5f);
    }

    void NextQuestion()
    {
        timerText.text = "10";
        foreach (Button btn in answerButtons)
        {
            btn.image.color = BaseColor;
        }

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion();
        }
        else
        {
            PlayerPrefs.SetInt("FinalScore", score);
            SceneManager.LoadScene("Results");
        }
    }

    void AnimateQuestionAndButtons()
    {
        questionCanvasGroup.alpha = 0f;
        questionCanvasGroup.DOFade(1f, QuestionFadeDuration).OnComplete(() =>
        {
            for (int i = 0; i < answerButtonGroups.Length; i++)
            {
                int index = i;
                DOVirtual.DelayedCall(0.3f * i, () =>
                {
                    answerButtonGroups[index].DOFade(1f, ButtonFadeDuration);
                    answerButtonGroups[index].interactable = true;

                    if (index == answerButtonGroups.Length - 1)
                    {
                        StartTimer();
                    }
                });
            }
        });
    }
}
