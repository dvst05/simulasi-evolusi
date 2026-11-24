using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public float spawnInterval = 1.5f; // Muncul tiap 1.5 detik
    public float arenaRange = 22f;     // Batas luas area muncul (sesuai ukuran Plane)

    void Start()
    {
        // Panggil fungsi SpawnFood berulang kali
        InvokeRepeating(nameof(SpawnFood), 0f, spawnInterval);
    }

    void SpawnFood()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-arenaRange, arenaRange),
            0.3f,
            Random.Range(-arenaRange, arenaRange)
        );
        Instantiate(foodPrefab, randomPos, Quaternion.identity);
    }
}