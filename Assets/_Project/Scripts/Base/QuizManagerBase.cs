using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public abstract class QuizManagerBase : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI questionNumberText;
    public CanvasGroup questionCanvasGroup;
    public CanvasGroup[] answerButtonGroups;
    public TextMeshProUGUI yourScoreText;

    [Header("Quiz Settings")]
    public QuestionsData QData;
    public float TimePerQuestion = 10f;

    [Header("UI Animation Settings")]
    public float QuestionFadeDuration = 0.5f;
    public float ButtonFadeDuration = 0.5f;

    protected int currentQuestionIndex;
    protected int score;
    protected float currentTime;
    protected bool answered;
    protected Color baseColor;
    protected bool questionStarted = false;
    protected GameManager gameManager;

    protected Image timerImage;

    private void Awake()
    {
        if (GameManager.Instance != null)
            gameManager = GameManager.Instance;
    }

    protected virtual void Start()
    {
        if (!gameManager.GetQuestionsDataFromLocal)
            QData = gameManager.MainQuestionList;

        timerImage = timerText.GetComponentInParent<Image>();

        baseColor = answerButtons[0].image.color;
        currentQuestionIndex = 0;
        score = 0;

        ShowQuestion();
    }

    protected virtual void Update()
    {
        if (!answered && questionStarted)
        {
            currentTime -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTime).ToString();
            timerImage.fillAmount = currentTime / TimePerQuestion;

            if (currentTime <= 0f)
            {
                SkipQuestion();
            }
        }
    }

    protected void ShowQuestion()
    {
        answered = false;
        currentTime = TimePerQuestion;

        timerImage.fillAmount = 1f;
        timerText.text = TimePerQuestion.ToString();
        questionNumberText.text = "Question: " + (currentQuestionIndex + 1);

        Question q = QData.QuizQuestions[currentQuestionIndex];
        questionText.text = q.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[i];

            int capturedIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => SelectAnswer(capturedIndex));

            answerButtonGroups[i].alpha = 0f;
        }

        AnimateQuestionAndButtons();
    }

    protected void StartTimer()
    {
        for (int i = 0; i < answerButtonGroups.Length; i++)
        {
            answerButtonGroups[i].interactable = true;
        }

        currentTime = TimePerQuestion;
        questionStarted = true;
    }

    protected virtual void SelectAnswer(int index)
    {
        answered = true;
        DisableAnswerButtons();

        bool isCorrect = index == QData.QuizQuestions[currentQuestionIndex].correctAnswerIndex;

        if (isCorrect)
        {
            answerButtons[index].image.color = Color.green;
            gameManager.PlayerScore++;
            score++;
        }
        else
        {
            answerButtons[index].image.color = Color.red;
            answerButtons[QData.QuizQuestions[currentQuestionIndex].correctAnswerIndex].image.color = Color.green;
        }

        GameManager.Instance.AnswerResults.Add(new AnswerResult
        {
            QuestionText = QData.QuizQuestions[currentQuestionIndex].questionText,
            PlayerAnswer = QData.QuizQuestions[currentQuestionIndex].answers[index],
            CorrectAnswer = QData.QuizQuestions[currentQuestionIndex].answers[QData.QuizQuestions[currentQuestionIndex].correctAnswerIndex],
            IsCorrect = isCorrect
        });

        yourScoreText.text = "Your Score: " + score;

        answerButtons[index].transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1);

        Invoke(nameof(NextQuestion), 1.5f);
        questionStarted = false;
    }

    protected void SkipQuestion()
    {
        DisableAnswerButtons();
        answered = true;
        questionStarted = false;

        answerButtons[QData.QuizQuestions[currentQuestionIndex].correctAnswerIndex].image.color = Color.green;

        Invoke(nameof(NextQuestion), 1.5f);
    }

    protected void NextQuestion()
    {
        timerText.text = TimePerQuestion.ToString();
        timerImage.DOFillAmount(1f, 0.2f);

        foreach (Button btn in answerButtons)
        {
            btn.image.color = baseColor;
        }

        currentQuestionIndex++;

        if (currentQuestionIndex < QData.QuizQuestions.Count)
        {
            ShowQuestion();
        }
        else
        {
            OnQuizCompleted();
        }
    }

    protected void DisableAnswerButtons()
    {
        for (int i = 0; i < answerButtonGroups.Length; i++)
        {
            answerButtonGroups[i].interactable = false;
        }
    }

    protected void AnimateQuestionAndButtons()
    {
        DOTween.Kill(questionCanvasGroup);
        questionCanvasGroup.alpha = 0f;
        questionCanvasGroup.interactable = true;

        for (int i = 0; i < answerButtonGroups.Length; i++)
        {
            DOTween.Kill(answerButtonGroups[i]);
        }

        questionCanvasGroup.DOFade(1f, QuestionFadeDuration).OnComplete(() =>
        {
            for (int i = 0; i < answerButtonGroups.Length; i++)
            {
                int index = i;

                DOVirtual.DelayedCall(0.3f * i, () =>
                {
                    answerButtonGroups[index].DOFade(1f, ButtonFadeDuration)
                        .SetId(answerButtonGroups[index])
                        .SetRecyclable(true);

                    if (index == answerButtonGroups.Length - 1)
                    {
                        StartTimer();
                    }
                });
            }
        });
    }

    protected virtual void OnQuizCompleted()
    {
        gameManager.PlayerScore = score;
        gameManager.ProcessToNextScene("Results");
    }

    public virtual void LeaveQuizMatch()
    {
        gameManager.ProcessToNextScene("MainMenu");
    }

    protected virtual void OnDestroy()
    {
        DOTween.KillAll();
    }
}
