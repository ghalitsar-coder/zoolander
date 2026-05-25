using UnityEngine;
using Sample; // Memanggil namespace tim agar bisa mengendalikan KidsScript

public class InfoTrigger : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject textPromptE;     // Tempat menaruh "Tekan E"
    public GameObject panelInfoHewan;  // Tempat menaruh gambar Info Hewan (Singa/Panda/dll)

    private bool isPlayerNear = false;
    private bool isInfoOpen = false;
    private KidsScript playerScript;

    void Start()
    {
        // Pastikan UI mati di awal saat game baru jalan
        if (textPromptE != null) textPromptE.SetActive(false);
        if (panelInfoHewan != null) panelInfoHewan.SetActive(false);

        // Cari script karakter si Boy0 secara otomatis
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerScript = playerObj.GetComponent<KidsScript>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Jika yang masuk ke lingkaran hijau adalah Player
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!isInfoOpen && textPromptE != null) textPromptE.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Jika Player keluar dari lingkaran hijau
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (textPromptE != null) textPromptE.SetActive(false);
            
            // Jika pemain pergi saat panel info masih terbuka, otomatis tutup
            if (isInfoOpen) CloseInfo(); 
        }
    }

    void Update()
    {
        // Jika pemain di dekat kandang DAN menekan tombol E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (isInfoOpen) 
                CloseInfo(); // Jika sedang buka, maka tutup
            else 
                OpenInfo();  // Jika sedang tutup, maka buka
        }
    }

    private void OpenInfo()
    {
        isInfoOpen = true;
        if (textPromptE != null) textPromptE.SetActive(false);
        if (panelInfoHewan != null) panelInfoHewan.SetActive(true);
        
        // Kunci pergerakan karakter (Boy0) agar tidak bisa jalan saat baca info
        if (playerScript != null) playerScript.LockPlayer(true);
    }

    private void CloseInfo()
    {
        isInfoOpen = false;
        if (panelInfoHewan != null) panelInfoHewan.SetActive(false);
        if (isPlayerNear && textPromptE != null) textPromptE.SetActive(true);
        
        // Bebaskan pergerakan karakter kembali setelah panel ditutup
        if (playerScript != null) playerScript.LockPlayer(false);
    }
}