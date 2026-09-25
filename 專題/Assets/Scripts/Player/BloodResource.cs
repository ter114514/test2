using System;
using UnityEngine;

/// <summary>
/// 吸血值資源系統。
/// 透過戰鬥或場景互動獲取，施放技能時消耗。
/// 只管理數值增減與「是否足夠施放」的判斷，不含技能邏輯本身。
/// </summary>
public class BloodResource : MonoBehaviour
{
    [Header("吸血值")]
    [SerializeField] float maxBlood = 100f;
    [Tooltip("初始吸血值")]
    [SerializeField] float startBlood = 0f;

    [Header("技能門檻")]
    [Tooltip("施放技能所需的最低吸血值")]
    [SerializeField] float skillCost = 50f;

    // ---- 對外事件 ----

    /// <summary>吸血值變化：(當前值, 最大值)</summary>
    public event Action<float, float> OnBloodChanged;
    /// <summary>是否足夠施放技能的狀態切換：true=足夠</summary>
    public event Action<bool> OnCanCastChanged;
    /// <summary>獲取吸血值時：獲得量</summary>
    public event Action<float> OnBloodGained;
    /// <summary>施放消耗時</summary>
    public event Action OnBloodConsumed;

    // ---- 對外唯讀 ----
    public float CurrentBlood { get; private set; }
    public float MaxBlood => maxBlood;
    public float SkillCost => skillCost;
    public float BloodPercent => maxBlood > 0 ? CurrentBlood / maxBlood : 0;
    /// <summary>是否足夠施放技能</summary>
    public bool CanCast => CurrentBlood >= skillCost;

    bool lastCanCast;

    void Awake()
    {
        CurrentBlood = Mathf.Clamp(startBlood, 0, maxBlood);
        lastCanCast = CanCast;
    }

    void Start()
    {
        OnBloodChanged?.Invoke(CurrentBlood, maxBlood);
        OnCanCastChanged?.Invoke(CanCast);
    }

    // ---- 獲取（戰鬥擊中、擊殺、場景互動呼叫）----

    public void Gain(float amount)
    {
        if (amount <= 0) return;

        CurrentBlood = Mathf.Min(maxBlood, CurrentBlood + amount);
        OnBloodGained?.Invoke(amount);
        OnBloodChanged?.Invoke(CurrentBlood, maxBlood);
        CheckCanCastChanged();
    }

    // ---- 消耗 ----

    /// <summary>嘗試消耗指定量。足夠則扣除回傳 true，不足回傳 false。</summary>
    public bool TryConsume(float amount)
    {
        if (CurrentBlood < amount) return false;

        CurrentBlood -= amount;
        OnBloodConsumed?.Invoke();
        OnBloodChanged?.Invoke(CurrentBlood, maxBlood);
        CheckCanCastChanged();
        return true;
    }

    /// <summary>嘗試施放技能（消耗 skillCost）。足夠回傳 true。</summary>
    public bool TryCastSkill()
    {
        return TryConsume(skillCost);
    }

    // ---- 內部：偵測「能否施放」狀態切換 ----

    void CheckCanCastChanged()
    {
        bool now = CanCast;
        if (now != lastCanCast)
        {
            lastCanCast = now;
            OnCanCastChanged?.Invoke(now);   // 狀態變了才通知（頭像框變化）
        }
    }

    // ---- 設定/重置（跨關卡延續、讀檔用）----

    public void ResetBlood(float value = 0)
    {
        CurrentBlood = Mathf.Clamp(value, 0, maxBlood);
        OnBloodChanged?.Invoke(CurrentBlood, maxBlood);
        CheckCanCastChanged();
    }
}