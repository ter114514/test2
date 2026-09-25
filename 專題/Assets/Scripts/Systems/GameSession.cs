using UnityEngine;

/// <summary>
/// 跨關卡的玩家狀態暫存（static，資料存記憶體不隨場景銷毀）。
/// 切關卡前 Save 寫入，進新關卡後 Load 套回，讓玩家狀態延續。
/// 血量為格子制（空洞騎士風）。
/// </summary>
public static class GameSession
{
    public static bool HasData { get; private set; }
    public static int PlayerCurrentMasks;
    public static float AttackPower;
    public static float Defense;
    public static float CurrentBlood;

    public static void Save(PlayerHealthSystem health, PlayerStats stats, BloodResource blood = null)
    {
        if (health != null)
        {
            PlayerCurrentMasks = health.CurrentMasks;
        }
        if (stats != null)
        {
            AttackPower = stats.AttackPower;
            Defense = stats.Defense;
        }
        if (blood != null)
            CurrentBlood = blood.CurrentBlood;

        HasData = true;
    }

    public static void Load(PlayerHealthSystem health, PlayerStats stats, BloodResource blood = null)
    {
        if (!HasData) return;

        if (stats != null)
        {
            stats.SetAttackPower(AttackPower);
            stats.SetDefense(Defense);
        }
        if (health != null)
            health.SetCurrentMasks(PlayerCurrentMasks);
        if (blood != null)
            blood.ResetBlood(CurrentBlood);
    }

    public static void Clear()
    {
        HasData = false;
        PlayerCurrentMasks = 0;
        AttackPower = 0;
        Defense = 0;
        CurrentBlood = 0;
    }
}