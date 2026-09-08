using System;
using UnityEngine;

/// <summary>
/// 血瓶系統。玩家按鍵使用血瓶回血（格子制），有數量限制。
/// 篝火休息補滿，碎片集滿可提升上限。
/// 實作 ISaveable，存檔記錄血瓶上限與當前數量。
/// </summary>
public class PotionSystem : MonoBehaviour, ISaveable
{
    [Header("血瓶設定")]
    [Tooltip("最大血瓶數")]
    [SerializeField] int maxPotions = 3;
    [Tooltip("每瓶回幾格血")]
    [SerializeField] int healMasks = 2;
    [Tooltip("使用冷卻時間（避免連續狂喝）")]
    [SerializeField] float useCooldown = 0.8f;

    // ---- 事件 ----
    /// <summary>血瓶數變化：(當前, 最大)</summary>
    public event Action<int, int> OnPotionCountChanged;
    /// <summary>使用血瓶時</summary>
    public event Action OnPotionUsed;
    /// <summary>沒血瓶可用時</summary>
    public event Action OnNoPotion;

    public int CurrentPotions { get; private set; }
    public int MaxPotions => maxPotions;

    PlayerInputHandler input;
    PlayerHealthSystem health;
    PlayerCombatController combat;
    float cooldownTimer;

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        health = GetComponent<PlayerHealthSystem>();
        combat = GetComponent<PlayerCombatController>();
        CurrentPotions = maxPotions;   // 初始給滿
    }

    void OnEnable()
    {
        if (input != null) input.OnUsePotionPressed += TryUsePotion;
    }

    void OnDisable()
    {
        if (input != null) input.OnUsePotionPressed -= TryUsePotion;
    }

    void Start()
    {
        OnPotionCountChanged?.Invoke(CurrentPotions, maxPotions);
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    // ---- 使用血瓶 ----
    void TryUsePotion()
    {
        if (cooldownTimer > 0) return;                              // 冷卻中
        if (combat != null && combat.IsInHardState) return;        // 攻擊/防禦硬直中
        if (CurrentPotions <= 0) { OnNoPotion?.Invoke(); return; } // 沒血瓶
        // 血格已滿不浪費
        if (health != null && health.CurrentMasks >= health.MaxMasks) return;

        CurrentPotions--;
        cooldownTimer = useCooldown;
        health?.Heal(healMasks);   // 回格數

        OnPotionUsed?.Invoke();
        OnPotionCountChanged?.Invoke(CurrentPotions, maxPotions);
    }

    // ---- 補充 / 提升上限 ----
    /// <summary>補滿（篝火呼叫）</summary>
    public void RefillPotions()
    {
        CurrentPotions = maxPotions;
        OnPotionCountChanged?.Invoke(CurrentPotions, maxPotions);
    }

    /// <summary>提升上限（碎片集滿呼叫）</summary>
    public void IncreaseMaxPotions(int amount = 1)
    {
        maxPotions += amount;
        CurrentPotions = maxPotions;   // 提升時補滿
        OnPotionCountChanged?.Invoke(CurrentPotions, maxPotions);
    }

    /// <summary>直接設定當前與上限（讀檔用）</summary>
    public void SetState(int current, int max)
    {
        maxPotions = max;
        CurrentPotions = Mathf.Clamp(current, 0, max);
        OnPotionCountChanged?.Invoke(CurrentPotions, maxPotions);
    }

    // ---- ISaveable ----
    public void SaveState(SaveData data)
    {
        data.potionMax = maxPotions;
        data.potionCurrent = CurrentPotions;
    }

    public void LoadState(SaveData data)
    {
        if (data.potionMax > 0)
            SetState(data.potionCurrent, data.potionMax);
    }
}