using System.Collections.Generic;

/// <summary>
/// 存檔資料容器。純資料類別，供 JsonUtility 序列化。
/// </summary>
[System.Serializable]
public class SaveData
{
    // ---- 存檔資訊 ----
    public string saveTime;
    public string currentSceneName;

    // ---- 玩家狀態 ----
    public float playerCurrentHealth;   // 舊數值制（保留）
    public float playerMaxHealth;       // 舊數值制（保留）
    public int playerCurrentMasks;      // 格子制：當前血格
    public int playerMaxMasks;          // 格子制：最大血格（升級後保存）
    public float playerPosX;
    public float playerPosY;

    // ---- 玩家屬性 ----
    public float attackPower;
    public float defense;

    // ---- 吸血值 ----
    public float currentBlood;

    // ---- 血瓶 ----
    public int potionMax;
    public int potionCurrent;

    // ---- 藥罐碎片 ----
    public int shardProgress;
    public List<string> collectedShards;

    // ---- 血格碎片 ----
    public int maskShardProgress;
    public List<string> collectedMaskShards;

    // ---- 敵人 ----
    public List<string> defeatedEnemies;

    public SaveData()
    {
        collectedShards = new List<string>();
        collectedMaskShards = new List<string>();
        defeatedEnemies = new List<string>();
    }
}