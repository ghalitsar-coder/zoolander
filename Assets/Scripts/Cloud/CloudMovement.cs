using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [Header("Kecepatan gerak")]
    public float speed = 2f;

    [Header("Arah awal")]
    public Vector3 direction = Vector3.right;

    [Header("Batas gerak (opsional)")]
    public bool useBoundary = true;
    public float minX = -20f;
    public float maxX = 20f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Gerakkan awan sesuai arah saat ini
        transform.Translate(direction * speed * Time.deltaTime);

        if (useBoundary)
        {
            // Cek jika melebihi batas kanan
            if (transform.position.x > maxX)
            {
                // Posisikan tepat di batas agar tidak tembus
                transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
                // Balik arah (ke kiri)
                direction = Vector3.left;
            }
            // Cek jika melebihi batas kiri
            else if (transform.position.x < minX)
            {
                transform.position = new Vector3(minX, transform.position.y, transform.position.z);
                // Balik arah (ke kanan)
                direction = Vector3.right;
            }
        }
    }
}