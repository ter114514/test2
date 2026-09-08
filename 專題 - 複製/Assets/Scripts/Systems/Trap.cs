using UnityEngine;

/// <summary>
/// 陷阱。玩家碰到後扣血並傳送回最後的安全位置。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Trap : MonoBehaviour
{
    [Header("傷害")]
    [SerializeField] float damage = 20f;

    [Header("陷阱類型")]
    [SerializeField] bool teleportBack = true;
    [SerializeField] bool continuousDamage = false;
    [SerializeField] float damageInterval = 1f;

    float lastDamageTime;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        DealDamage(other);

        // 傳送回玩家最後的安全位置
        if (teleportBack && other.TryGetComponent<SafePositionTracker>(out var tracker))
            tracker.RespawnToSafe();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!continuousDamage) return;
        if (!other.CompareTag("Player")) return;

        if (Time.time - lastDamageTime >= damageInterval)
            DealDamage(other);
    }

    void DealDamage(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage, Vector2.zero);
            lastDamageTime = Time.time;
        }
    }
}