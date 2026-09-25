using System;
using UnityEngine;

/// <summary>
/// 血格碎片系統。玩家撿血格碎片，集滿門檻提升最大血格。
/// 實作 ISaveable，存檔記錄進度與已撿清單。
/// </summary>
public class MaskShardSystem : MonoBehaviour, ISaveable
{
    [Header("碎片設定")]
    [Tooltip("集滿幾片提升一格血格")]
    [SerializeField] int shardsPerUpgrade = 3;

    // ---- 事件 ----
    /// <summary>碎片數變化：(當前累積, 集滿所需)</summary>
    public event Action<int, int> OnMaskShardChanged;
    /// <summary>集滿提升血格時</summary>
    public event Action OnMaskUpgraded;

    public int CurrentShards { get; private set; }

    PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        OnMaskShardChanged?.Invoke(CurrentShards, shardsPerUpgrade);
    }

    /// <summary>撿到一片血格碎片</summary>
    public void CollectShard()
    {
        CurrentShards++;

        // 集滿門檻 → 提升血格
        if (CurrentShards >= shardsPerUpgrade)
        {
            CurrentShards -= shardsPerUpgrade;
            stats?.IncreaseMaxMasks(1);   // 最大血格 +1（PlayerStats 會發事件，血量補滿+HUD更新）
            OnMaskUpgraded?.Invoke();
        }

        OnMaskShardChanged?.Invoke(CurrentShards, shardsPerUpgrade);
    }

    /// <summary>直接設定碎片進度（讀檔用）</summary>
    public void SetShards(int shards)
    {
        CurrentShards = shards;
        OnMaskShardChanged?.Invoke(CurrentShards, shardsPerUpgrade);
    }

    // ---- ISaveable ----
    public void SaveState(SaveData data)
    {
        data.maskShardProgress = CurrentShards;
        data.collectedMaskShards = CollectedMaskShardTracker.ToList();
    }

    public void LoadState(SaveData data)
    {
        SetShards(data.maskShardProgress);
        CollectedMaskShardTracker.RestoreFrom(data.collectedMaskShards);
    }
}