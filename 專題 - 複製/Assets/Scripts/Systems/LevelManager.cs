using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 關卡切換管理。載入指定關卡，並把玩家放到對應出生點。
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    /// <summary>進入新關卡後，玩家要出現的出生點 ID</summary>
    public static string TargetSpawnID { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>切換到指定關卡，並指定進入後的出生點</summary>
    public void LoadLevel(string sceneName, string spawnID = "")
    {
        TargetSpawnID = spawnID;
        SceneManager.LoadScene(sceneName);
    }
}