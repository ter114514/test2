using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵人死亡時在背景牆上留下血濺。掛在敵人身上，會自動訂閱 EnemyHealth.OnDeath。
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class BloodSplatterSpawner : MonoBehaviour
{
    [Header("血濺貼圖（美術提供的圖）")]
    [SerializeField] Sprite[] splatSprites;

    [Header("牆面血濺")]
    [SerializeField] int splatCount = 3;
    [SerializeField] float spreadRadius = 1.2f;
    [SerializeField] Vector2 sizeRange = new Vector2(0.8f, 1.8f);
    [Tooltip("要在背景牆之上、角色之下")]
    [SerializeField] string sortingLayerName = "Default";
    [SerializeField] int sortingOrder = -5;
    [SerializeField] Color tint = Color.white;
    [SerializeField] float popTime = 0.18f;

    [Header("數量上限與淡出")]
    [SerializeField] int maxDecals = 40;
    [Tooltip("0 = 永久留著")]
    [SerializeField] float fadeAfter = 0f;
    [SerializeField] float fadeDuration = 3f;

    [Header("噴濺粒子（可選，拖入 BloodSpray）")]
    [SerializeField] GameObject sprayPrefab;
    [SerializeField] float sprayScale = 1.8f;
    [Header("位置與形狀")]
    [SerializeField] float heightOffset = 1.2f;       // 中心在腳底上方多高
    [SerializeField] float horizontalSpread = 1.0f;
    [SerializeField] float verticalSpread = 0.8f;
    [SerializeField] float minHeightAboveFeet = 0.4f; // 最低不低於腳底上方多少
    [SerializeField] float rotationRange = 25f;       // 只允許小角度旋轉

    static readonly Queue<GameObject> alive = new Queue<GameObject>();
    EnemyHealth health;

    Collider2D col;
    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        col = GetComponent<Collider2D>();
    }
    void OnEnable() { health.OnDeath += HandleDeath; }
    void OnDisable() { health.OnDeath -= HandleDeath; }

    void HandleDeath() { Spawn(health.LastKnockback); }

    public void Spawn(Vector2 hitDirection)
    {
        if (splatSprites == null || splatSprites.Length == 0) return;

        float feetY = col != null ? col.bounds.min.y : transform.position.y;
        Vector2 center = new Vector2(transform.position.x, feetY + heightOffset);

        if (sprayPrefab != null)
        {
            var spray = Instantiate(sprayPrefab, center, Quaternion.identity);
            spray.transform.localScale *= sprayScale;
        }

        for (int i = 0; i < splatCount; i++)
        {
            float x = center.x + Random.Range(-horizontalSpread, horizontalSpread);
            if (hitDirection.x != 0)
                x += Mathf.Sign(hitDirection.x) * Random.Range(0f, horizontalSpread * 0.5f);

            float y = center.y + Random.Range(-verticalSpread * 0.3f, verticalSpread);
            y = Mathf.Max(y, feetY + minHeightAboveFeet);   // 不會低於地面

            CreateSplat(new Vector2(x, y));
        }
    }

    void CreateSplat(Vector2 pos)
    {
        var go = new GameObject("BloodSplat");
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-rotationRange, rotationRange));

        float size = Random.Range(sizeRange.x, sizeRange.y);
        Vector3 target = new Vector3(size * (Random.value < 0.5f ? -1 : 1), size, 1);
        go.transform.localScale = target * 0.3f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = splatSprites[Random.Range(0, splatSprites.Length)];
        sr.color = tint;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder + Random.Range(0, 3);

        go.AddComponent<BloodSplatDecal>().Init(target, tint, popTime, fadeAfter, fadeDuration);

        alive.Enqueue(go);
        while (alive.Count > maxDecals)
        {
            var old = alive.Dequeue();
            if (old != null) Destroy(old);
        }
    }
}

/// <summary>每個血濺自己負責出現動畫與淡出，不依賴敵人是否還活著。</summary>
public class BloodSplatDecal : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 target;
    Color baseColor;
    float popTime, fadeAfter, fadeDuration, t;

    public void Init(Vector3 target, Color color, float popTime, float fadeAfter, float fadeDuration)
    {
        sr = GetComponent<SpriteRenderer>();
        this.target = target;
        baseColor = color;
        this.popTime = Mathf.Max(0.01f, popTime);
        this.fadeAfter = fadeAfter;
        this.fadeDuration = Mathf.Max(0.01f, fadeDuration);
    }

    void Update()
    {
        t += Time.deltaTime;

        if (t < popTime)
        {
            float p = t / popTime;
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            float overshoot = 1f + Mathf.Sin(p * Mathf.PI) * 0.15f;
            transform.localScale = Vector3.Lerp(target * 0.3f, target, ease) * overshoot;
            return;
        }

        transform.localScale = target;
        if (fadeAfter <= 0f) { enabled = false; return; }

        float fadeT = (t - popTime - fadeAfter) / fadeDuration;
        if (fadeT <= 0f) return;
        if (fadeT >= 1f) { Destroy(gameObject); return; }

        var c = baseColor;
        c.a = Mathf.Lerp(baseColor.a, 0f, fadeT);
        sr.color = c;
    }
}