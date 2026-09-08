using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家生命狀態系統（格子制，空洞騎士風）。
/// 血量以「格」為單位，受傷固定扣 1 格。
/// 最大血格由 PlayerStats 提供；血格上限提升時補滿並更新顯示。
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealthSystem : MonoBehaviour, IDamageable, ISaveable
{
    [Header("受擊無敵")]
    [SerializeField] float invincibleTime = 1f;
    [SerializeField] float blinkInterval = 0.1f;

    // ---- 對外事件 ----
    public event Action<int, int> OnHealthChanged;   // (當前格, 最大格)
    public event Action OnDamaged;
    public event Action OnHealed;
    public event Action OnDeath;
    public event Action<bool> OnInvincibleChanged;

    // ---- 對外唯讀 ----
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

    // 血格上限提升時：補滿血 + 更新面具顯示
    void HandleMaxMasksChanged()
    {
        CurrentMasks = MaxMasks;   // 升級補滿
        OnHealthChanged?.Invoke(CurrentMasks, MaxMasks);
    }

    // ---- IDamageable：受傷固定扣 1 格 ----
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
        StartCoroutine(InvincibilityRoutine());
    }

    // ---- 治療（格數）----
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
        OnDeath?.Invoke();
    }

    // ---- 重置 ----
    public void ResetHealth()
    {
        StopAllCoroutines();
        CurrentMasks = MaxMasks;
        IsDead = false;
        IsInvincible = false;
        if (sr != null) sr.enabled = true;
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