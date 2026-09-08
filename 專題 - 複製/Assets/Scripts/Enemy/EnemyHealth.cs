using System;
using UnityEngine;

/// <summary>
/// 敵人生命系統。實作 IDamageable，受擊閃紅+擊退，死亡播放動畫後銷毀。
/// 死亡時凍結物理，避免屍體往下掉。發出 OnDeath 事件供其他系統訂閱。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("生命")]
    [SerializeField] float maxHealth = 50f;

    [Header("受擊")]
    [Tooltip("受擊閃紅時間")]
    [SerializeField] float flashDuration = 0.1f;
    [SerializeField] Color flashColor = Color.red;

    [Header("死亡")]
    [Tooltip("死亡動畫播放後多久銷毀")]
    [SerializeField] float destroyDelay = 1.5f;

    [Header("識別")]
    [Tooltip("此敵人的唯一 ID（存檔記錄擊敗用）")]
    [SerializeField] string enemyId;

    // ---- 事件 ----
    /// <summary>死亡時發出</summary>
    public event Action OnDeath;
    /// <summary>血量變化：(當前, 最大)</summary>
    public event Action<float, float> OnHealthChanged;

    float currentHealth;
    bool isDead;

    Rigidbody2D rb;
    Collider2D col;
    SpriteRenderer sr;
    Animator animator;
    Color originalColor;
    float flashTimer;

    public bool IsDead => isDead;
    public string EnemyId => enemyId;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        if (sr != null) originalColor = sr.color;

        currentHealth = maxHealth;
    }

    void Start()
    {
        // 已被擊敗過（存檔記錄）→ 直接消失
        if (!string.IsNullOrEmpty(enemyId) && DefeatedEnemyTracker.IsDefeated(enemyId))
            Destroy(gameObject);
    }

    void Update()
    {
        // 受擊閃紅計時
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0 && sr != null)
                sr.color = originalColor;
        }
    }

    // ---- IDamageable ----
    public void TakeDamage(float damage, Vector2 knockback)
    {
        if (isDead) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 受擊閃紅
        if (sr != null)
        {
            sr.color = flashColor;
            flashTimer = flashDuration;
        }

        // 擊退（死前才推，死後 Kinematic 不推）
        if (rb != null && currentHealth > 0)
        {
            rb.linearVelocity = Vector2.zero;   // 先歸零，擊退才明顯
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 記錄已擊敗（存檔用）
        if (!string.IsNullOrEmpty(enemyId))
            DefeatedEnemyTracker.MarkDefeated(enemyId);

        // 發出死亡事件
        OnDeath?.Invoke();

        // 凍結物理，避免屍體往下掉或被推
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 關閉碰撞（屍體不擋路、不再被打）
        if (col != null)
            col.enabled = false;

        // 停用攻擊行為
        if (TryGetComponent<EnemyAttacker>(out var attacker))
            attacker.enabled = false;

        // 播死亡動畫
        if (animator != null)
            animator.SetBool("IsDead", true);

        // 延遲銷毀
        Destroy(gameObject, destroyDelay);
    }
}