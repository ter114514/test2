using UnityEngine;

/// <summary>
/// 怪物 AI 狀態機：巡邏 → 發現玩家追蹤 → 接近攻擊 → 失去回巡邏。
/// 實作 IPausable，吸血技能時停時暫停行動。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IPausable
{
    enum State { Patrol, Chase, Attack }

    [Header("偵測")]
    [SerializeField] float detectRange = 6f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float loseRange = 8f;

    [Header("移動")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 4f;

    [Header("巡邏")]
    [SerializeField] float patrolDistance = 3f;
    [SerializeField] float patrolWaitTime = 1f;

    [Header("攻擊")]
    [SerializeField] float attackCooldown = 1.5f;

    [Header("地面/牆偵測")]
    [SerializeField] Transform edgeCheck;
    [SerializeField] float edgeCheckDist = 1f;
    [SerializeField] LayerMask groundLayer;

    [Header("朝向")]
    [SerializeField] bool spriteFacesLeft = false;

    State state = State.Patrol;
    Rigidbody2D rb;
    Transform player;
    EnemyHealth health;
    Animator animator;

    Vector2 patrolOrigin;
    int patrolDir = 1;
    float patrolWaitTimer;
    float attackTimer;
    int facingDir = 1;

    bool isPaused;   // 時停

    // ---- IPausable ----
    public bool IsBoss => false;   // 一般敵人不是 Boss
    public void PauseActions() => isPaused = true;
    public void ResumeActions() => isPaused = false;

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
    }

    void Update()
    {
        if (health != null && health.IsDead) return;

        // 時停：停下不動作
        if (isPaused)
        {
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null) animator.SetFloat("Speed", 0);
            return;
        }

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        UpdateStateTransitions();
        RunState();
        UpdateAnimator();
    }

    void UpdateStateTransitions()
    {
        if (player == null) { state = State.Patrol; return; }

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                if (dist <= detectRange) state = State.Chase;
                break;
            case State.Chase:
                if (dist > loseRange) state = State.Patrol;
                else if (dist <= attackRange) state = State.Attack;
                break;
            case State.Attack:
                if (dist > attackRange) state = State.Chase;
                break;
        }
    }

    void RunState()
    {
        switch (state)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    void Patrol()
    {
        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distFromOrigin = transform.position.x - patrolOrigin.x;
        bool reachedEdge = (patrolDir > 0 && distFromOrigin >= patrolDistance)
                        || (patrolDir < 0 && distFromOrigin <= -patrolDistance);

        if (reachedEdge || !HasGroundAhead())
        {
            patrolDir *= -1;
            patrolWaitTimer = patrolWaitTime;
            return;
        }

        facingDir = patrolDir;
        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, rb.linearVelocity.y);
    }

    void Chase()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        facingDir = (int)dir;

        if (HasGroundAhead())
            rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (player != null)
            facingDir = (int)Mathf.Sign(player.position.x - transform.position.x);

        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            if (animator != null) animator.SetTrigger("Attack");
        }
    }

    bool HasGroundAhead()
    {
        if (edgeCheck == null) return true;
        RaycastHit2D hit = Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeCheckDist, groundLayer);
        return hit.collider != null;
    }

    void UpdateAnimator()
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        int visualDir = spriteFacesLeft ? -facingDir : facingDir;
        transform.localScale = new Vector3(scaleX * visualDir, transform.localScale.y, transform.localScale.z);

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);

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