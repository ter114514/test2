using UnityEngine;

/// <summary>
/// 出生點。關卡載入後，把玩家移到 ID 對應的出生點。
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("此出生點的 ID，與 LevelExit 的 targetSpawnID 對應")]
    [SerializeField] string spawnID = "Default";

    void Start()
    {
        // 如果這個出生點就是目標
        if (LevelManager.TargetSpawnID == spawnID)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 移到出生點
                player.transform.position = transform.position;
                if (player.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.linearVelocity = Vector2.zero;

                // 套用跨關卡保留的玩家狀態
                var health = player.GetComponent<PlayerHealthSystem>();
                var stats = player.GetComponent<PlayerStats>();
                var blood = player.GetComponent<BloodResource>();
                GameSession.Load(health, stats, blood);
            }
        }
    }
}