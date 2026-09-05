using UnityEngine;

/// <summary>場景中的藥罐碎片。玩家碰到即撿取。</summary>
[RequireComponent(typeof(Collider2D))]
public class PotionShard : MonoBehaviour
{
    [Tooltip("此碎片的唯一 ID")]
    [SerializeField] string shardId;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // 已撿過就不出現
        if (CollectedShardTracker.IsCollected(shardId))
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CollectedShardTracker.MarkCollected(shardId);

        var shardSystem = other.GetComponent<PotionShardSystem>();
        shardSystem?.CollectShard();

        Destroy(gameObject);
    }
}