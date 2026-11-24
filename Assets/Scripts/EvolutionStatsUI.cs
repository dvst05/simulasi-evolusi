using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Wajib untuk TextMeshPro

public class EvolutionStatsUI : MonoBehaviour
{
    [Header("Referensi UI")]
    public TextMeshProUGUI statsText;
    public RectTransform graphContainer;

    [Header("Pengaturan Pengambilan Data")]
    public float sampleInterval = 2f; // Catat data tiap 2 detik
    public int maxDataPoints = 30;    // Maksimal 30 titik di grafik

    private List<float> avgSpeedHistory = new List<float>();
    private List<GameObject> graphObjects = new List<GameObject>();
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= sampleInterval)
        {
            timer = 0f;
            RecordAndDisplayStats();
        }
    }

    void RecordAndDisplayStats()
    {
        // Cari semua makhluk yang masih hidup di arena
        Creature[] creatures = FindObjectsByType<Creature>(FindObjectsSortMode.None);
        int count = creatures.Length;

        if (count == 0)
        {
            statsText.text = "<b>POPULASI PUNAH!</b>\nSemua makhluk kehabisan energi.";
            return;
        }

        float totalSpeed = 0f;
        float totalSize = 0f;
        float totalSense = 0f;
        int maxGen = 1;

        foreach (Creature c in creatures)
        {
            totalSpeed += c.speed;
            totalSize += c.size;
            totalSense += c.senseRadius;
            if (c.generation > maxGen) maxGen = c.generation;
        }

        float avgSpeed = totalSpeed / count;
        float avgSize = totalSize / count;
        float avgSense = totalSense / count;

        // Tampilkan angka ke teks UI
        statsText.text =
            $"<b>STATISTIK EKOSISTEM</b>\n" +
            $"Populasi Hidup : <b>{count}</b> ekor\n" +
            $"Generasi Tertinggi : <b>Gen {maxGen}</b>\n" +
            $"Rata-rata Speed : <color=#FF6B6B><b>{avgSpeed:F2}</b></color>\n" +
            $"Rata-rata Size : <b>{avgSize:F2}</b> | Sense : <b>{avgSense:F1}</b>";

        // Simpan riwayat avgSpeed untuk grafik
        avgSpeedHistory.Add(avgSpeed);
        if (avgSpeedHistory.Count > maxDataPoints)
        {
            avgSpeedHistory.RemoveAt(0); // Hapus data paling lama biar grafik bergeser
        }

        DrawGraph();
    }

    void DrawGraph()
    {
        // Bersihkan titik dan garis grafik lama sebelum menggambar ulang
        foreach (GameObject obj in graphObjects)
        {
            Destroy(obj);
        }
        graphObjects.Clear();

        if (avgSpeedHistory.Count < 2) return;

        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;
        float maxSpeedScale = 12f; // Batas atas nilai Speed pada grafik

        Vector2 prevPointPos = Vector2.zero;

        for (int i = 0; i < avgSpeedHistory.Count; i++)
        {
            float xPos = (i / (float)(maxDataPoints - 1)) * graphWidth;
            float yPos = Mathf.Clamp01(avgSpeedHistory[i] / maxSpeedScale) * graphHeight;
            Vector2 currentPos = new Vector2(xPos, yPos);

            // Buat titik data
            GameObject dot = CreateDot(currentPos);
            graphObjects.Add(dot);

            // Hubungkan titik sebelumnya dengan titik sekarang pakai garis
            if (i > 0)
            {
                GameObject line = CreateLine(prevPointPos, currentPos);
                graphObjects.Add(line);
            }

            prevPointPos = currentPos;
        }
    }

    GameObject CreateDot(Vector2 anchoredPos)
    {
        GameObject dot = new GameObject("Dot", typeof(Image));
        dot.transform.SetParent(graphContainer, false);
        dot.GetComponent<Image>().color = Color.white;

        RectTransform rect = dot.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(5f, 5f);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        return dot;
    }

    GameObject CreateLine(Vector2 pointA, Vector2 pointB)
    {
        GameObject line = new GameObject("Line", typeof(Image));
        line.transform.SetParent(graphContainer, false);
        line.GetComponent<Image>().color = new Color(1f, 0.35f, 0.35f, 1f); // Merah terang

        RectTransform rect = line.GetComponent<RectTransform>();
        Vector2 dir = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.sizeDelta = new Vector2(distance, 2.5f);
        rect.anchoredPosition = pointA + dir * distance * 0.5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localEulerAngles = new Vector3(0f, 0f, angle);
        return line;
    }
}