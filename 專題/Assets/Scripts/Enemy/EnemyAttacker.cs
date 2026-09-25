using System.Collections;
using UnityEngine;

/// <summary>
/// 站定間歇攻擊的敵人，攻擊時向面向方向撲擊位移。
/// 實作 IPausable，吸血技能時停時暫停攻擊。
/// </summary>
[RequireComponent(typeof(EnemyStats), typeof(Rigidbody2D))]
public class EnemyAttacker : MonoBehaviour, IPausable
{
    [Header("攻擊判定範圍")]
    [SerializeField] Vector2 attackBoxSize = new Vector2(1.2f, 1f);
    [SerializeField] float attackReach = 0.8f;
    [SerializeField] LayerMask targetLayer;

    [Header("攻擊位移（撲擊）")]
    [SerializeField] float lungeForce = 8f;
    [SerializeField] float lungeDelay = 0.2f;
    [SerializeField] float lungeDuration = 0.15f;

    static readonly int AttackHash = Animator.StringToHash("Attack");

    EnemyStats stats;
    EnemyHealth health;
    Rigidbody2D rb;
    Animator animator;
    float lastAttackTime;
    bool isAttacking;

    bool isPaused;   // 時停

    // ---- IPausable ----
    public bool IsBoss => false;   // 一般敵人不是 Boss

    public void PauseActions()
    {
        isPaused = true;
        if (animator != null) animator.speed = 0;   // 動畫定格
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void ResumeActions()
    {
        isPaused = false;
        if (animator != null) animator.speed = 1;   // 動畫恢復
    }

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (health != null && health.IsDead) return;
        if (isPaused) return;        // 時停中不動作
        if (isAttacking) return;

        if (Time.time >= lastAttackTime + stats.AttackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger(AttackHash);

        yield return new WaitForSeconds(lungeDelay);

        if (health != null && health.IsDead) { isAttacking = false; yield break; }

        // 時停中就不撲擊、不判定（等恢復）
        if (isPaused)
        {
            // 等到解除時停才繼續（或直接中斷這次攻擊）
            isAttacking = false;
            yield break;
        }

        float facing = Mathf.Sign(transform.localScale.x);
        if (facing == 0) facing = 1;
        rb.linearVelocity = new Vector2(-facing * lungeForce, rb.linearVelocity.y);

        DoAttackHit(facing);

        yield return new WaitForSeconds(lungeDuration);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        isAttacking = false;
    }

    void DoAttackHit(float facing)
    {
        // 時停中不判定傷害
        if (isPaused) return;

        Vector2 center = (Vector2)transform.position + Vector2.right * facing * attackReach;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                Vector2 knockback = new Vector2(
                    facing * stats.KnockbackForce,
                    stats.KnockbackForce * 0.3f);
                target.TakeDamage(stats.AttackPower, knockback);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        if (facing == 0) facing = 1;
        Vector2 center = (Vector2)transform.position + Vector2.right * facing * attackReach;
        Gizmos.color = new Color(1, 0.4f, 0, 0.6f);
        Gizmos.DrawWireCube(center, attackBoxSize);
    }
}