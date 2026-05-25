using UnityEngine;

public class GerakHewan : MonoBehaviour
{
    public float tinggiLompat = 0.2f;
    public float kecepatan = 1f;

    private Vector3 posisiAwal;

    void Start()
    {
        // Mencatat posisi asli hewan saat game dimulai
        posisiAwal = transform.position;
    }

    void Update()
    {
        // Rumus matematika Sinus untuk gerakan naik turun yang halus
        // Nama variabelnya digabung jadi 'yBaru' agar tidak error
        float yBaru = posisiAwal.y + (Mathf.Sin(Time.time * kecepatan) * tinggiLompat);

        // Terapkan posisi baru ke hewan
        transform.position = new Vector3(posisiAwal.x, yBaru, posisiAwal.z);
    }
}