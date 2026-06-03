using UnityEngine;
using System.Collections;
using Sample; 

public class QuizGateManager : MonoBehaviour
{
    public static QuizGateManager gerbangAktif;
    [Header("UI & Gate")]
    public GameObject tembokPenghalang;
    public GameObject textPromptE;
    public GameObject wadahNyawa; // Tarik objek WadahNyawa ke sini
    
    [Header("Feedback UI")]
    public GameObject panelMenang; // UI Saat selesai semua
    public GameObject panelKalah;  // UI Saat nyawa habis

    [Header("Urutan Panel Kuis")]
    public GameObject[] daftarKuis;
    public GameObject[] iconNyawa;

    private int indexSekarang = 0;
    private int nyawa = 3; 
    private bool diDekatGerbang = false;
    private bool kuisSedangAktif = false;
    private bool sedangShake = false; 
    private KidsScript playerScript;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerScript = playerObj.GetComponent<KidsScript>();

        // Matikan semua UI di awal permainan
        if (wadahNyawa != null) wadahNyawa.SetActive(false);
        if (panelMenang != null) panelMenang.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);
        
        foreach (GameObject kuis in daftarKuis) { kuis.SetActive(false); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            diDekatGerbang = true;
            if (!kuisSedangAktif && !panelKalah.activeSelf && !panelMenang.activeSelf) 
                textPromptE.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            diDekatGerbang = false;
            textPromptE.SetActive(false);
            
            // Mekanisme Kalah Anda: Jika menjauh saat kuis aktif/kalah, otomatis auto-reset bersih
            if (kuisSedangAktif || panelKalah.activeSelf) 
            {
                ResetNyawaDanTutup();
            }
        }
    }

    void Update()
    {
        if (diDekatGerbang && Input.GetKeyDown(KeyCode.E) && !kuisSedangAktif && !panelKalah.activeSelf && !panelMenang.activeSelf)
        {
            MulaiKuis();
        }
    }

    void MulaiKuis()
    {
        gerbangAktif = this; 

        kuisSedangAktif = true;
        textPromptE.SetActive(false);
        if (wadahNyawa != null) wadahNyawa.SetActive(true); // Munculkan nyawa
        
        TampilkanKuisAktif();
        RenderUINyawa(); 

        if (playerScript != null) playerScript.LockPlayer(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void JawabBenar()
    {
        if (sedangShake) return; 

        daftarKuis[indexSekarang].SetActive(false);
        indexSekarang++;

        if (indexSekarang < daftarKuis.Length)
        {
            TampilkanKuisAktif();
        }
        else
        {
            // MENANG! Munculkan Panel Menang, sembunyikan kuis & nyawa
            if (wadahNyawa != null) wadahNyawa.SetActive(false);
            if (panelMenang != null) panelMenang.SetActive(true);
        }
    }

    public void JawabSalah()
    {
        if (sedangShake) return; 

        nyawa--; 
        RenderUINyawa(); 

        if (nyawa <= 0)
        {
            // KALAH! Munculkan Panel Kalah, sembunyikan kuis & nyawa
            daftarKuis[indexSekarang].SetActive(false);
            if (wadahNyawa != null) wadahNyawa.SetActive(false);
            if (panelKalah != null) panelKalah.SetActive(true);
        }
        else
        {
            StartCoroutine(ShakePanel(daftarKuis[indexSekarang]));
        }
    }

    // Disambungkan ke tombol "Main Lagi" di Panel Kalah
    public void TombolTutupKalah()
    {
        if (panelKalah != null) panelKalah.SetActive(false);
        ResetNyawaDanTutup();
    }

    // Disambungkan ke tombol "Lanjut" di Panel Menang
    public void TombolTutupMenang()
    {
        if (panelMenang != null) panelMenang.SetActive(false);
        BukaGerbang();
    }

    void RenderUINyawa()
    {
        for (int i = 0; i < iconNyawa.Length; i++)
        {
            if (iconNyawa[i] != null) iconNyawa[i].SetActive(i < nyawa);
        }
    }

    IEnumerator ShakePanel(GameObject panel)
    {
        sedangShake = true;
        RectTransform rect = panel.GetComponent<RectTransform>();
        Vector3 posisiAwal = rect.anchoredPosition;

        float durasi = 0.4f;
        float waktuBerlalu = 0f;
        float kekuatan = 25f; 

        while (waktuBerlalu < durasi)
        {
            float offsetX = Mathf.Sin(waktuBerlalu * 40f) * kekuatan;
            rect.anchoredPosition = new Vector3(posisiAwal.x + offsetX, posisiAwal.y, posisiAwal.z);
            waktuBerlalu += Time.deltaTime;
            yield return null; 
        }

        rect.anchoredPosition = posisiAwal; 
        sedangShake = false;
    }

    void TampilkanKuisAktif()
    {
        daftarKuis[indexSekarang].SetActive(true);
    }

    void ResetNyawaDanTutup()
    {
        kuisSedangAktif = false;
        nyawa = 3; 
        indexSekarang = 0; 
        
        if (wadahNyawa != null) wadahNyawa.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);
        foreach (GameObject kuis in daftarKuis) { kuis.SetActive(false); }
        
        // Hanya munculkan prompt kembali jika setelah di-reset pemain ternyata masih berdiri di dalam area trigger
        if (diDekatGerbang && textPromptE != null) textPromptE.SetActive(true); 

        if (playerScript != null) playerScript.LockPlayer(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void BukaGerbang()
    {
        kuisSedangAktif = false;
        nyawa = 3;
        indexSekarang = 0;

        // PERBAIKAN BUG: Matikan paksa prompt E di sini agar tidak menyala kembali
        if (textPromptE != null) textPromptE.SetActive(false); 

        if (playerScript != null) playerScript.LockPlayer(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (tembokPenghalang != null) tembokPenghalang.SetActive(false); 
        
        // Matikan trigger sensor agar seluruh sistem kuis di gerbang ini selesai selamanya
        gameObject.SetActive(false); 
    }
}