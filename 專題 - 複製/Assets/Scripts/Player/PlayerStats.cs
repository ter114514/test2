using System;
using UnityEngine;

/// <summary>
/// 基礎屬性數值庫。只存資料，不含遊戲邏輯。
/// 血量為格子制（空洞騎士風）。實作 ISaveable，存讀最大血格。
/// </summary>
public class PlayerStats : MonoBehaviour, ISaveable
{
    [Header("生命（格子制）")]
    [Tooltip("最大血格數")]
    [SerializeField] int maxMasks = 5;

    [Header("攻擊")]
    [SerializeField] float attackPower = 20f;
    [SerializeField] float knockbackForce = 10f;

    [Header("防禦")]
    [SerializeField] float defense = 0f;

    [Header("移動")]
    [SerializeField] float moveSpeed = 8f;

    public event Action OnStatsChanged;
    /// <summary>血格上限提升時（HealthSystem、HUD 訂閱）</summary>
    public event Action OnMaxMasksChanged;

    public int MaxMasks => maxMasks;
    public float AttackPower => attackPower;
    public float KnockbackForce => knockbackForce;
    public float Defense => defense;
    public float MoveSpeed => moveSpeed;

    // ---- 血格上限 ----
    public void SetMaxMasks(int value)
    {
        maxMasks = Mathf.Max(1, value);
        OnStatsChanged?.Invoke();
        OnMaxMasksChanged?.Invoke();
    }

    /// <summary>提升最大血格（碎片集滿呼叫）</summary>
    public void IncreaseMaxMasks(int amount = 1)
    {
        maxMasks += amount;
        OnStatsChanged?.Invoke();
        OnMaxMasksChanged?.Invoke();
    }

    // ---- 其他屬性 ----
    public void SetAttackPower(float value)
    {
        attackPower = Mathf.Max(0, value);
        OnStatsChanged?.Invoke();
    }

    public void AddAttackPower(float delta)
    {
        attackPower = Mathf.Max(0, attackPower + delta);
        OnStatsChanged?.Invoke();
    }

    public void SetDefense(float value)
    {
        defense = Mathf.Max(0, value);
        OnStatsChanged?.Invoke();
    }

    public void AddDefense(float delta)
    {
        defense = Mathf.Max(0, defense + delta);
        OnStatsChanged?.Invoke();
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0, value);
        OnStatsChanged?.Invoke();
    }

    // ---- ISaveable ----
    public void SaveState(SaveData data)
    {
        data.playerMaxMasks = maxMasks;
    }

    public void LoadState(SaveData data)
    {
        if (data.playerMaxMasks > 0)   // 有存過才套用（舊存檔用 Inspector 預設）
        {
            maxMasks = data.playerMaxMasks;
            OnMaxMasksChanged?.Invoke();   // 通知 HealthSystem/HUD 更新面具數
        }
    }
}