using UnityEngine;

/// <summary>
/// 記錄玩家最後休息的篝火位置與場景（重生點）。
/// static 跨場景保留。篝火休息時更新。
/// </summary>
public static class RespawnPoint
{
    public static bool HasRespawn { get; private set; }
    public static string SceneName { get; private set; }
    public static Vector3 Position { get; private set; }

    public static void SetRespawn(string scene, Vector3 pos)
    {
        SceneName = scene;
        Position = pos;
        HasRespawn = true;
    }

    public static void Clear()
    {
        HasRespawn = false;
        SceneName = null;
        Position = Vector3.zero;
    }
}