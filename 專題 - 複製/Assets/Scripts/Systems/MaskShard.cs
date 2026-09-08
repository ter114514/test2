using UnityEngine;

/// <summary>場景中的血格碎片。玩家碰到即撿取。</summary>
[RequireComponent(typeof(Collider2D))]
public class MaskShard : MonoBehaviour
{
    [Tooltip("此碎片的唯一 ID")]
    [SerializeField] string shardId;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // 已撿過就不出現
        if (CollectedMaskShardTracker.IsCollected(shardId))
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CollectedMaskShardTracker.MarkCollected(shardId);

        var shardSystem = other.GetComponent<MaskShardSystem>();
        shardSystem?.CollectShard();

        Destroy(gameObject);
    }
}