using UnityEngine;

public class RegionGate : MonoBehaviour
{
    public int requiredRegionIndex = 1;     // Wilayah awal yang harus diselesaikan
    public GameObject quizPanel;
    public QuizManager quizManager;
    public GameObject region2Unlockable;    // Objek-objek wilayah 2 (nonaktif awal)
    public GameObject physicalGate;         // Objek gerbang (opsional, akan dihancurkan)
    public GameObject regionBarrier;        // Barrier/penghalang (ditambahkan)
    public Transform teleportPoint;         // Tempat player dipindah (opsional)

    void OnMouseDown()
    {
        if (RegionUnlockManager.instance == null)
        {
            Debug.LogError("RegionUnlockManager tidak ditemukan!");
            return;
        }

        // Cek apakah wilayah 2 sudah terbuka?
        if (RegionUnlockManager.instance.IsRegionUnlocked(requiredRegionIndex + 1))
        {
            EnterRegion2();
        }
        else
        {
            // Mulai kuis
            if (quizPanel != null) quizPanel.SetActive(true);
            if (quizManager != null)
            {
                quizManager.regionIndex = requiredRegionIndex + 1;
                quizManager.onQuizPass += OnQuizPassed;
                quizManager.StartQuiz();
            }
        }
    }

    private void OnQuizPassed()
    {
        // Buka wilayah 2
        RegionUnlockManager.instance.UnlockRegion(requiredRegionIndex + 1);

        // Aktifkan objek-objek wilayah 2
        if (region2Unlockable != null)
            region2Unlockable.SetActive(true);

        // Hancurkan gerbang fisik (jika ada)
        if (physicalGate != null)
            Destroy(physicalGate);

        // Hancurkan atau nonaktifkan barrier
        if (regionBarrier != null)
            Destroy(regionBarrier);

        // Teleport player (jika ada)
        if (teleportPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = teleportPoint.position;
        }

        // Hapus event listener
        if (quizManager != null)
            quizManager.onQuizPass -= OnQuizPassed;
    }

    private void EnterRegion2()
    {
        // Jika wilayah 2 sudah terbuka, bisa teleport atau lewat
        if (teleportPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = teleportPoint.position;
        }
    }
}