using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public GameObject quizPanel;
    public TMP_Text questionText;       // TMP untuk pertanyaan
    public Button[] answerButtons;
    public TMP_Text resultText;         // TMP untuk hasil
    public int passScore = 2;

    [HideInInspector] public int regionIndex;
    public System.Action onQuizPass;

    public List<QuestionSet> regionQuests;

    [System.Serializable]
    public class Question
    {
        public string text;
        public string[] answers;
        public int correctIndex;
    }

    [System.Serializable]
    public class QuestionSet
    {
        public int regionId;
        public List<Question> questions;
    }

    private List<Question> currentQuestions;
    private int currentQIndex = 0;
    private int score = 0;

    void Start()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
    }

    public void StartQuiz()
    {
        currentQuestions = GetQuestionsForRegion(regionIndex);
        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogWarning("Tidak ada pertanyaan untuk region " + regionIndex);
            quizPanel.SetActive(false);
            return;
        }

        currentQIndex = 0;
        score = 0;
        quizPanel.SetActive(true);
        if (resultText != null) resultText.gameObject.SetActive(false);
        ShowQuestion();
    }

    private List<Question> GetQuestionsForRegion(int id)
    {
        foreach (var qs in regionQuests)
            if (qs.regionId == id) return qs.questions;
        return null;
    }

    private void ShowQuestion()
    {
        if (currentQIndex >= currentQuestions.Count)
        {
            EndQuiz();
            return;
        }

        Question q = currentQuestions[currentQIndex];
        questionText.text = q.text;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < q.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                // Ambil komponen TMP_Text dari child button
                answerButtons[i].GetComponentInChildren<TMP_Text>().text = q.answers[i];
                int idx = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => CheckAnswer(idx));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void CheckAnswer(int selected)
    {
        if (selected == currentQuestions[currentQIndex].correctIndex)
            score++;
        currentQIndex++;
        ShowQuestion();
    }

    private void EndQuiz()
    {
        quizPanel.SetActive(false);
        bool passed = score >= passScore;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            if (passed)
                resultText.text = "?? Selamat! Kamu lulus! Wilayah berikutnya terbuka. ??";
            else
                resultText.text = "?? Sayang sekali, coba lagi ya. Pelajari hewan-hewannya dulu!";
        }

        if (passed)
        {
            onQuizPass?.Invoke();
        }
        else
        {
            Invoke("HideResult", 2f);
        }
    }

    private void HideResult()
    {
        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }
}