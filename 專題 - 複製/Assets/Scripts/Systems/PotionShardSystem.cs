using System;
using UnityEngine;

/// <summary>
/// 藥罐碎片系統。玩家撿碎片，集滿門檻提升血瓶上限。
/// 實作 ISaveable，存檔記錄碎片進度與已撿碎片清單。
/// </summary>
public class PotionShardSystem : MonoBehaviour, ISaveable
{
    [Header("碎片設定")]
    [Tooltip("集滿幾片提升一次血瓶上限")]
    [SerializeField] int shardsPerUpgrade = 4;

    // ---- 事件 ----
    /// <summary>碎片數變化：(當前累積, 集滿所需)</summary>
    public event Action<int, int> OnShardChanged;
    /// <summary>集滿提升上限時</summary>
    public event Action OnPotionUpgraded;

    /// <summary>目前朝下一次升級累積的碎片數</summary>
    public int CurrentShards { get; private set; }

    PotionSystem potion;

    void Awake()
    {
        potion = GetComponent<PotionSystem>();
    }

    void Start()
    {
        OnShardChanged?.Invoke(CurrentShards, shardsPerUpgrade);
    }

    /// <summary>撿到一片碎片</summary>
    public void CollectShard()
    {
        CurrentShards++;

        // 集滿門檻 → 提升血瓶上限
        if (CurrentShards >= shardsPerUpgrade)
        {
            CurrentShards -= shardsPerUpgrade;   // 扣掉，剩餘進下一輪
            potion?.IncreaseMaxPotions(1);
            OnPotionUpgraded?.Invoke();
        }

        OnShardChanged?.Invoke(CurrentShards, shardsPerUpgrade);
    }

    /// <summary>直接設定碎片進度（讀檔用）</summary>
    public void SetShards(int shards)
    {
        CurrentShards = shards;
        OnShardChanged?.Invoke(CurrentShards, shardsPerUpgrade);
    }

    // ---- ISaveable ----

    public void SaveState(SaveData data)
    {
        data.shardProgress = CurrentShards;
        data.collectedShards = CollectedShardTracker.ToList();
    }

    public void LoadState(SaveData data)
    {
        SetShards(data.shardProgress);
        CollectedShardTracker.RestoreFrom(data.collectedShards);
    }
}