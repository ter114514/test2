using UnityEngine;

/// <summary>
/// 敵人攻擊判定框。由攻擊動畫的 Animation Event 開關，碰到玩家造成傷害。
/// 時停由 EnemyAI 暫停動畫處理（攻擊動畫定格，判定框不觸發）。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] float damage = 1f;
    [SerializeField] float knockbackForce = 8f;

    Collider2D hitbox;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
        hitbox.enabled = false;
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

    public void EnableHitbox() => hitbox.enabled = true;
    public void DisableHitbox() => hitbox.enabled = false;
}