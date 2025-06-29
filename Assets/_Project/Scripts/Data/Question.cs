using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public string id;
    public string questionText;
    public string[] answers;
    public int correctAnswerIndex;
}

[System.Serializable]
public class QuestionList
{
    public List<Question> questions;
}