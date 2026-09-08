using UnityEngine;

/// <summary>
/// 敵人攻擊判定框。由攻擊動畫的 Animation Event 開關，碰到玩家造成傷害。
/// 掛在敵人的攻擊判定子物件上（含 Trigger Collider）。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] float damage = 1f;   // 格子制通常扣 1 格
    [SerializeField] float knockbackForce = 8f;

    Collider2D hitbox;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
        hitbox.enabled = false;   // 平時關
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            damageable.TakeDamage(damage, dir * knockbackForce);
        }
    }

    // Animation Event 呼叫
    public void EnableHitbox() => hitbox.enabled = true;
    public void DisableHitbox() => hitbox.enabled = false;
}