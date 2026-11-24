using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("DNA / Sifat Genetik")]
    public float speed = 4f;
    public float size = 1f;
    public float senseRadius = 10f;

    [Header("Status Hidup & Reproduksi")]
    public float energy = 100f;
    public float energyToReproduce = 150f;
    public int generation = 1;

    private Transform targetFood;
    private Renderer meshRenderer;
    private float hopTimer;

    private Renderer[] childRenderers; // Pakai array karena model 3D punya banyak bagian

    void Start()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
        ApplyDNAVisuals();
    }

    public void ApplyDNAVisuals()
    {
        transform.localScale = Vector3.one * size;

        float speedRatio = Mathf.InverseLerp(2f, 10f, speed);
        Color targetColor = Color.Lerp(Color.white, Color.red, speedRatio); // Putih ke Merah biar tekstur asli tetap kelihatan

        if (childRenderers != null)
        {
            foreach (Renderer r in childRenderers)
            {
                r.material.color = targetColor;
            }
        }
    }

    void Update()
    {
        // Energi berkurang lebih pelan agar sempat mencari makan
        float energyDrain = (speed * 0.25f) + (size * 0.3f);
        energy -= energyDrain * Time.deltaTime;

        if (energy <= 0f)
        {
            Destroy(gameObject); // Mati kelaparan
            return;
        }

        // Cek apakah cukup energi untuk bereproduksi
        if (energy >= energyToReproduce)
        {
            Reproduce();
        }

        if (targetFood == null)
        {
            FindNearestFood();
        }

        if (targetFood != null)
        {
            MoveTowards(targetFood.position);
        }
        else
        {
            Wander();
        }
    }

    void FindNearestFood()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, senseRadius);
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Food"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    targetFood = hit.transform;
                }
            }
        }
    }

    void MoveTowards(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            transform.position += direction * speed * Time.deltaTime;
            DoLittleHop();
        }
    }

    void Wander()
    {
        transform.position += transform.forward * (speed * 0.5f) * Time.deltaTime;
        transform.Rotate(0f, Random.Range(-120f, 120f) * Time.deltaTime, 0f);
        DoLittleHop();

        // Balik arah jika mendekati tepi arena
        if (Mathf.Abs(transform.position.x) > 23f || Mathf.Abs(transform.position.z) > 23f)
        {
            Vector3 backToCenter = (Vector3.zero - transform.position).normalized;
            backToCenter.y = 0f;
            transform.rotation = Quaternion.LookRotation(backToCenter);
        }
    }

    void DoLittleHop()
    {
        // Efek loncat kecil-kecil saat bergerak
        hopTimer += Time.deltaTime * speed * 2f;
        if (Mathf.Sin(hopTimer) > 0.9f && transform.position.y <= size + 0.1f)
        {
            transform.position += Vector3.up * 0.08f;
        }
    }

    void Reproduce()
    {
        // Bagi dua energi antara induk dan anak
        energy *= 0.5f;

        // Munculkan anak tepat di sebelah induknya
        Vector3 spawnPos = transform.position + (transform.right * size);
        GameObject childObj = Instantiate(gameObject, spawnPos, Quaternion.identity);

        Creature child = childObj.GetComponent<Creature>();
        child.generation = generation + 1;
        child.energy = energy;

        // Mutasi acak sebesar +/- 15% pada tiap gen
        child.speed = Mathf.Clamp(speed * Random.Range(0.85f, 1.15f), 1.5f, 12f);
        child.size = Mathf.Clamp(size * Random.Range(0.85f, 1.15f), 0.5f, 2.5f);
        child.senseRadius = Mathf.Clamp(senseRadius * Random.Range(0.85f, 1.15f), 3f, 25f);

        child.ApplyDNAVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            energy += 45f;
            Destroy(other.gameObject);
            targetFood = null;
        }
    }
}