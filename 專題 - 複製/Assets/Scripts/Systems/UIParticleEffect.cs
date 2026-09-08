using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 主畫面氛圍粒子。整個畫面持續緩慢往上飄的粒子。
/// 預設不生成，由標題點擊呼叫 StartSpawning 開始，
/// 收回或離開時呼叫 StopAndClear 停止並銷毀。
/// 掛在 Canvas 下的滿版容器上。
/// </summary>
public class UIAmbientParticles : MonoBehaviour
{
    [Header("粒子")]
    [Tooltip("粒子預製（UI Image）")]
    [SerializeField] GameObject particlePrefab;

    [Header("生成")]
    [Tooltip("每秒生成幾個粒子")]
    [SerializeField] float spawnRate = 8f;
    [Tooltip("粒子往上飄的速度範圍")]
    [SerializeField] float minSpeed = 20f;
    [SerializeField] float maxSpeed = 50f;
    [Tooltip("粒子存活時間")]
    [SerializeField] float minLifetime = 4f;
    [SerializeField] float maxLifetime = 7f;
    [Tooltip("粒子大小範圍")]
    [SerializeField] float minSize = 8f;
    [SerializeField] float maxSize = 20f;

    RectTransform area;
    bool spawning = false;   // 預設不生成，等標題點擊才啟動
    float spawnTimer;
    readonly List<GameObject> activeParticles = new();

    void Awake()
    {
        area = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!spawning) return;

        spawnTimer += Time.unscaledDeltaTime;
        float interval = 1f / spawnRate;
        while (spawnTimer >= interval)
        {
            spawnTimer -= interval;
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        if (particlePrefab == null) return;

        GameObject p = Instantiate(particlePrefab, transform);
        activeParticles.Add(p);

        RectTransform pRect = p.GetComponent<RectTransform>();

        float halfW = area.rect.width / 2f;
        float startX = Random.Range(-halfW, halfW);
        float startY = -area.rect.height / 2f;
        pRect.anchoredPosition = new Vector2(startX, startY);

        float size = Random.Range(minSize, maxSize);
        pRect.sizeDelta = new Vector2(size, size);

        float speed = Random.Range(minSpeed, maxSpeed);
        float lifetime = Random.Range(minLifetime, maxLifetime);
        float drift = Random.Range(-15f, 15f);

        StartCoroutine(FloatUp(p, pRect, speed, lifetime, drift));
    }

    IEnumerator FloatUp(GameObject p, RectTransform pRect,
        float speed, float lifetime, float drift)
    {
        Image img = p.GetComponent<Image>();
        float elapsed = 0f;
        Vector2 startPos = pRect.anchoredPosition;
        float baseAlpha = img != null ? img.color.a : 1f;

        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lifetime;

            pRect.anchoredPosition = startPos + new Vector2(
                drift * t, speed * elapsed);

            if (img != null)
            {
                float alpha;
                if (t < 0.2f) alpha = Mathf.Lerp(0, baseAlpha, t / 0.2f);
                else if (t > 0.8f) alpha = Mathf.Lerp(baseAlpha, 0, (t - 0.8f) / 0.2f);
                else alpha = baseAlpha;

                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }

            yield return null;
        }

        activeParticles.Remove(p);
        Destroy(p);
    }

    /// <summary>開始生成粒子（點擊標題後呼叫）</summary>
    public void StartSpawning()
    {
        spawning = true;
    }

    /// <summary>停止生成並銷毀所有粒子（收回或離開時呼叫）</summary>
    public void StopAndClear()
    {
        spawning = false;
        StopAllCoroutines();

        foreach (var p in activeParticles)
            if (p != null) Destroy(p);
        activeParticles.Clear();
    }
}