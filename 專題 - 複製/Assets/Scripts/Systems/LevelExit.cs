using UnityEngine;

/// <summary>
/// 關卡出口。玩家進入時保存當前狀態，並切換到指定關卡。
/// 掛在含 Trigger Collider 的出口/傳送門物件上。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("目標關卡")]
    [Tooltip("要載入的下一關場景名稱")]
    [SerializeField] string nextSceneName = "Level2";
    [Tooltip("進入下一關後玩家的出生點 ID（對應該關 SpawnPoint 的 spawnID）")]
    [SerializeField] string targetSpawnID = "FromLevel1";

    [Header("設定")]
    [Tooltip("是否需要按鍵才切換（否則進入範圍自動切換）")]
    [SerializeField] bool requireKeyPress = false;

    bool playerInRange;
    Collider2D playerCollider;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        // 需要按鍵的模式：玩家在範圍內按 W 或上鍵才切換
        if (requireKeyPress && playerInRange)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.wKey.wasPressedThisFrame)
            {
                TransitionToNextLevel();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        playerCollider = other;

        // 自動切換模式：進入就切
        if (!requireKeyPress)
            TransitionToNextLevel();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
    }

    void TransitionToNextLevel()
    {
        if (playerCollider == null) return;

        // 切關卡前，保存玩家當前狀態（跨關卡延續）
        var health = playerCollider.GetComponent<PlayerHealthSystem>();
        var stats = playerCollider.GetComponent<PlayerStats>();
        var blood = playerCollider.GetComponent<BloodResource>();
        GameSession.Save(health, stats, blood);

        // 切換到下一關
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(nextSceneName, targetSpawnID);
        else
            Debug.LogError("【LevelExit】找不到 LevelManager！");
    }
}