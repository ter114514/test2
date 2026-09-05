using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 氣刃投射物。往指定方向飛行，穿透多個敵人並造成傷害+擊退。
/// 不傷害玩家自己，不重複打同一敵人。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BladeProjectile : MonoBehaviour
{
    [Header("飛行")]
    [SerializeField] float speed = 15f;
    [SerializeField] float lifetime = 3f;

    [Header("傷害")]
    [SerializeField] float damage = 35f;
    [SerializeField] float knockbackForce = 6f;

    [Header("穿透")]
    [Tooltip("是否穿透敵人（true=穿過繼續飛，false=打到就消失）")]
    [SerializeField] bool piercing = true;
    [Tooltip("最多穿透幾個敵人（0=無限）")]
    [SerializeField] int maxPierceCount = 0;

    [Header("命中特效（可選）")]
    [SerializeField] GameObject hitEffectPrefab;

    int direction = 1;
    int pierceCount;

    // 已打過的敵人（避免穿透時重複打同一個）
    readonly HashSet<Collider2D> hitTargets = new();

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(int dir)
    {
        direction = dir;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 忽略玩家自己
        if (other.CompareTag("Player")) return;

        // 已經打過這個目標，不重複打
        if (hitTargets.Contains(other)) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            hitTargets.Add(other);   // 記錄打過

            // 傷害 + 擊退
            Vector2 knockback = new Vector2(direction, 0.2f).normalized * knockbackForce;
            damageable.TakeDamage(damage, knockback);

            // 命中特效
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position),
                    Quaternion.identity);

            pierceCount++;

            // 不穿透，或達到穿透上限 → 銷毀
            if (!piercing)
            {
                Destroy(gameObject);
            }
            else if (maxPierceCount > 0 && pierceCount >= maxPierceCount)
            {
                Destroy(gameObject);
            }
            // 否則繼續飛（穿透）
        }
    }
}