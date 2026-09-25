using UnityEngine;
using System.Collections;

/// <summary>
/// Boss 控制器。玩家不在範圍巡邏；發現玩家追蹤、近距離普攻、間歇旋轉揮砍。
/// 實作 IPausable，但 IsBoss = true，不被吸血技能的時停影響。
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class BossController : MonoBehaviour, IPausable
{
    enum State { Patrol, Chase, NormalAttack, SpinAttack }

    [Header("偵測與移動")]
    [SerializeField] float detectRange = 12f;
    [SerializeField] float normalAttackRange = 2f;
    [SerializeField] float chaseSpeed = 3f;

    [Header("巡邏")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float patrolDistance = 4f;
    [SerializeField] Transform edgeCheck;
    [SerializeField] float edgeCheckDist = 1f;
    [SerializeField] LayerMask groundLayer;

    [Header("普攻")]
    [SerializeField] float normalAttackCooldown = 2f;
    [SerializeField] float normalAttackDuration = 1f;

    [Header("技能：旋轉揮砍")]
    [SerializeField] float spinAttackInterval = 8f;
    [SerializeField] float spinRange = 3.5f;
    [SerializeField] float spinDamage = 2f;
    [SerializeField] float spinDuration = 2f;
    [SerializeField] float spinHitInterval = 0.3f;

    [Header("朝向")]
    [SerializeField] bool spriteFacesLeft = false;

    State state = State.Patrol;
    Rigidbody2D rb;
    EnemyHealth health;
    Animator animator;
    Transform player;

    Vector2 patrolOrigin;
    int patrolDir = 1;
    float normalAttackTimer;
    float spinAttackTimer;
    bool isActing;
    int facingDir = 1;

    // ---- IPausable ----
    public bool IsBoss => true;      // 是 Boss，不被時停影響
    public void PauseActions() { }   // Boss 不暫停（空實作）
    public void ResumeActions() { }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        animator = GetComponentInChildren<Animator>();
        patrolOrigin = transform.position;
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        spinAttackTimer = spinAttackInterval;
    }

    void Update()
    {
        if (health != null && health.IsDead) return;

        if (normalAttackTimer > 0) normalAttackTimer -= Time.deltaTime;
        if (spinAttackTimer > 0) spinAttackTimer -= Time.deltaTime;

        if (isActing) return;

        UpdateStateAndAct();
        UpdateFacing();
        UpdateAnimator();
    }

    void UpdateStateAndAct()
    {
        float dist = player != null
            ? Vector2.Distance(transform.position, player.position)
            : float.MaxValue;

        if (player != null && dist <= detectRange)
        {
            if (spinAttackTimer <= 0)
            {
                StartCoroutine(SpinAttackRoutine());
                return;
            }

            if (dist <= normalAttackRange)
            {
                if (normalAttackTimer <= 0)
                {
                    StartCoroutine(NormalAttackRoutine());
                }
                else
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    FacePlayer();
                    state = State.Chase;
                }
            }
            else
            {
                state = State.Chase;
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                facingDir = (int)dir;
                rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        state = State.Patrol;

        float distFromOrigin = transform.position.x - patrolOrigin.x;
        bool reachedEdge = (patrolDir > 0 && distFromOrigin >= patrolDistance)
                        || (patrolDir < 0 && distFromOrigin <= -patrolDistance);

        if (reachedEdge || !HasGroundAhead())
            patrolDir *= -1;

        facingDir = patrolDir;
        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, rb.linearVelocity.y);
    }

    bool HasGroundAhead()
    {
        if (edgeCheck == null) return true;
        RaycastHit2D hit = Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeCheckDist, groundLayer);
        return hit.collider != null;
    }

    IEnumerator NormalAttackRoutine()
    {
        isActing = true;
        state = State.NormalAttack;
        normalAttackTimer = normalAttackCooldown;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FacePlayer();

        if (animator != null) animator.SetTrigger("NormalAttack");

        yield return new WaitForSeconds(normalAttackDuration);

        isActing = false;
    }

    IEnumerator SpinAttackRoutine()
    {
        isActing = true;
        state = State.SpinAttack;
        spinAttackTimer = spinAttackInterval;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (animator != null) animator.SetTrigger("SpinAttack");

        float elapsed = 0f;
        float hitTimer = 0f;
        while (elapsed < spinDuration)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
            {
                DoSpinDamage();
                hitTimer = spinHitInterval;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        isActing = false;
    }

    void DoSpinDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spinRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") &&
                hit.TryGetComponent<IDamageable>(out var dmg))
            {
                Vector2 knockback = (hit.transform.position - transform.position).normalized * 5f;
                dmg.TakeDamage(spinDamage, knockback);
            }
        }
    }

    void FacePlayer()
    {
        if (player != null)
            facingDir = (int)Mathf.Sign(player.position.x - transform.position.x);
    }

    void UpdateFacing()
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        int visualDir = spriteFacesLeft ? -facingDir : facingDir;
        transform.localScale = new Vector3(scaleX * visualDir,
            transform.localScale.y, transform.localScale.z);
    }

    void UpdateAnimator()
    {
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, normalAttackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spinRange);

        Vector2 origin = Application.isPlaying ? patrolOrigin : (Vector2)transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin + Vector2.left * patrolDistance, origin + Vector2.right * patrolDistance);

        if (edgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * edgeCheckDist);
        }
    }
}