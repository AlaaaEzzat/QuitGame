using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SinglePlayerQuizManager : QuizManagerBase
{
    protected override void OnQuizCompleted()
    {
        PlayerPrefs.SetInt("FinalScore", score);
        SceneManager.LoadScene("Results");
    }
}
