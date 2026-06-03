using UnityEngine;

public class QuizUIButtonRouter : MonoBehaviour
{
    public void TekanJawabBenar()
    {
        if (QuizGateManager.gerbangAktif != null)
            QuizGateManager.gerbangAktif.JawabBenar();
    }

    public void TekanJawabSalah()
    {
        if (QuizGateManager.gerbangAktif != null)
            QuizGateManager.gerbangAktif.JawabSalah();
    }

    public void TekanTutupMenang()
    {
        if (QuizGateManager.gerbangAktif != null)
            QuizGateManager.gerbangAktif.TombolTutupMenang();
    }

    public void TekanTutupKalah()
    {
        if (QuizGateManager.gerbangAktif != null)
            QuizGateManager.gerbangAktif.TombolTutupKalah();
    }
}