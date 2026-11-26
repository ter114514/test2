using UnityEngine;

/// <summary>
/// 怪物受擊特效。掛在敵人身上，自動訂閱 EnemyHealth.OnHealthChanged。
/// 需要 EnemyHealth 有 public Vector2 LastKnockback { get; private set; }。
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitFeedback : MonoBehaviour
{
    [Header("受擊特效（粒子或動畫 Prefab，可放多個一起播）")]
    [SerializeField] GameObject[] hitEffectPrefabs;
    [SerializeField] float effectScale = 1f;
    [Tooltip("特效出現位置，相對於敵人碰撞器中心")]
    [SerializeField] Vector2 spawnOffset = Vector2.zero;
    [Tooltip("讓特效的右方朝向擊退方向（適合刀光、噴濺）。粒子 Cone 朝上的話請關閉")]
    [SerializeField] bool alignToKnockback = false;
    [Tooltip("特效出現時額外的隨機旋轉，避免每次都一樣")]
    [SerializeField] float randomRotation = 15f;

    [Header("音效（可選）")]
    [SerializeField] AudioClip[] hitSounds;
    [Range(0f, 1f)][SerializeField] float soundVolume = 0.8f;

    [Header("致命一擊")]
    [Tooltip("打死時也播放受擊特效")]
    [SerializeField] bool playOnKillingBlow = true;

    EnemyHealth health;
    Collider2D col;
    float lastHealth;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        col = GetComponent<Collider2D>();
        lastHealth = health.MaxHealth;
    }

    void OnEnable()
    {
        health.OnHealthChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        health.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(float current, float max)
    {
        Debug.Log($"[EnemyHitFeedback] 血量變化 {lastHealth} → {current}");
        // 只有血量減少才算受擊（避免回血或初始化觸發）
        bool tookDamage = current < lastHealth;
        lastHealth = current;
        if (!tookDamage) return;
        if (current <= 0f && !playOnKillingBlow) return;

        Play(health.LastKnockback);
    }

    public void Play(Vector2 knockback)
    {
        Debug.Log("[EnemyHitFeedback] Play 被呼叫");
        Vector2 center = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        Vector2 pos = center + spawnOffset;

        Quaternion rot = Quaternion.identity;
        if (alignToKnockback && knockback.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(knockback.y, knockback.x) * Mathf.Rad2Deg;
            rot = Quaternion.Euler(0f, 0f, angle);
        }
        rot *= Quaternion.Euler(0f, 0f, Random.Range(-randomRotation, randomRotation));

        if (hitEffectPrefabs != null)
        {
            foreach (var prefab in hitEffectPrefabs)
            {
                if (prefab == null) continue;
                var fx = Instantiate(prefab, pos, rot);
                fx.transform.localScale *= effectScale;
            }
        }

        if (hitSounds != null && hitSounds.Length > 0)
        {
            var clip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, pos, soundVolume);
        }
    }
}