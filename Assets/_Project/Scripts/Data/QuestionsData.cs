using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestionsData", menuName = "Scriptable Objects/QuestionsData")]
public class QuestionsData : ScriptableObject
{
    public List<Question> QuizQuestions;
}
