using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家生命狀態系統（格子制）。受傷扣 1 格，播受擊動畫；死亡播死亡動畫。
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealthSystem : MonoBehaviour, IDamageable, ISaveable
{
    [Header("受擊無敵")]
    [SerializeField] float invincibleTime = 1f;
    [SerializeField] float blinkInterval = 0.1f;

    [Header("動畫")]
    [SerializeField] Animator animator;

    // ---- 事件 ----
    public event Action<int, int> OnHealthChanged;
    public event Action OnDamaged;
    public event Action OnHealed;
    public event Action OnDeath;
    public event Action<bool> OnInvincibleChanged;

    public int CurrentMasks { get; private set; }
    public int MaxMasks => stats.MaxMasks;
    public bool IsInvincible { get; private set; }
    public bool IsDead { get; private set; }

    PlayerStats stats;
    SpriteRenderer sr;
    Rigidbody2D rb;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        CurrentMasks = stats.MaxMasks;
    }

    void OnEnable()
    {
        if (stats != null) stats.OnMaxMasksChanged += HandleMaxMasksChanged;
    }

    void OnDisable()
    {
        if (stats != null) stats.OnMaxMasksChanged -= HandleMaxMasksChanged;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    void HandleMaxMasksChanged()
    {
        CurrentMasks = MaxMasks;
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    // ---- IDamageable ----
    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (IsInvincible || IsDead) return;

        CurrentMasks--;
        OnDamaged?.Invoke();
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (CurrentMasks <= 0)
        {
            Die();
            return;
        }

        // 播受擊動畫（沒死才播）
        if (animator != null)
            animator.SetTrigger("Hurt");

        StartCoroutine(InvincibilityRoutine());
    }

    // ---- 治療 ----
    public void Heal(int masks = 1)
    {
        if (IsDead || masks <= 0) return;
        CurrentMasks = Mathf.Min(MaxMasks, CurrentMasks + masks);
        OnHealed?.Invoke();
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    public void FullHeal()
    {
        if (IsDead) return;
        CurrentMasks = MaxMasks;
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    public void SetCurrentMasks(int value)
    {
        CurrentMasks = Mathf.Clamp(value, 0, MaxMasks);
        IsDead = CurrentMasks <= 0;
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    // ---- 無敵時間 ----
    IEnumerator InvincibilityRoutine()
    {
        SetInvincible(true);
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (sr != null) sr.enabled = true;
        SetInvincible(false);
    }

    void SetInvincible(bool value)
    {
        IsInvincible = value;
        OnInvincibleChanged?.Invoke(value);
    }

    // ---- 死亡 ----
    void Die()
    {
        IsDead = true;
        StopAllCoroutines();
        SetInvincible(false);
        if (sr != null) sr.enabled = true;

        if (animator != null)
        {
            animator.ResetTrigger("Revive");
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Die");
        }

        OnDeath?.Invoke();
    }

    // ---- 重置（重生用）----
    public void ResetHealth()
    {
        StopAllCoroutines();
        CurrentMasks = MaxMasks;
        IsDead = false;
        IsInvincible = false;
        if (sr != null) sr.enabled = true;

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.SetTrigger("Revive");
        }

        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    // ---- ISaveable ----
    public void SaveState(SaveData data)
    {
        data.playerCurrentMasks = CurrentMasks;
    }

    public void LoadState(SaveData data)
    {
        CurrentMasks = Mathf.Clamp(data.playerCurrentMasks, 0, MaxMasks);
        IsDead = CurrentMasks <= 0;
        IsInvincible = false;
        if (sr != null) sr.enabled = true;
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }
}