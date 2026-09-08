using UnityEngine;

/// <summary>
/// 怪物 AI 狀態機：巡邏 → 發現玩家追蹤 → 接近攻擊 → 失去目標回巡邏。
/// 掛在敵人身上，需要 Rigidbody2D。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Attack }

    [Header("偵測")]
    [Tooltip("發現玩家的範圍")]
    [SerializeField] float detectRange = 6f;
    [Tooltip("進入攻擊的範圍")]
    [SerializeField] float attackRange = 1.5f;
    [Tooltip("失去玩家的範圍（比偵測大，避免邊緣抖動）")]
    [SerializeField] float loseRange = 8f;

    [Header("移動")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 4f;

    [Header("巡邏")]
    [Tooltip("巡邏範圍（從起點往兩邊各這麼遠）")]
    [SerializeField] float patrolDistance = 3f;
    [Tooltip("巡邏到邊緣的等待時間")]
    [SerializeField] float patrolWaitTime = 1f;

    [Header("攻擊")]
    [Tooltip("攻擊間隔")]
    [SerializeField] float attackCooldown = 1.5f;

    [Header("地面/牆偵測")]
    [Tooltip("前方偵測點（避免走出平台或撞牆）")]
    [SerializeField] Transform edgeCheck;
    [SerializeField] float edgeCheckDist = 1f;
    [SerializeField] LayerMask groundLayer;

    [Header("朝向")]
    [Tooltip("角色圖預設朝左則勾")]
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
        // 死了不動作
        if (health != null && health.IsDead) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        UpdateStateTransitions();
        RunState();
        UpdateAnimator();
    }

    // ---- 狀態轉換判斷 ----
    void UpdateStateTransitions()
    {
        if (player == null) { state = State.Patrol; return; }

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                if (dist <= detectRange) state = State.Chase;   // 發現玩家
                break;

            case State.Chase:
                if (dist > loseRange) state = State.Patrol;      // 失去玩家
                else if (dist <= attackRange) state = State.Attack;  // 夠近，攻擊
                break;

            case State.Attack:
                if (dist > attackRange) state = State.Chase;     // 玩家跑遠，追
                break;
        }
    }

    // ---- 執行當前狀態 ----
    void RunState()
    {
        switch (state)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    // ---- 巡邏 ----
    void Patrol()
    {
        // 巡邏等待中
        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 到達巡邏邊緣，或前方沒地面/有牆 → 轉向 + 等待
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

    // ---- 追蹤 ----
    void Chase()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        facingDir = (int)dir;

        // 前方沒地面就不追過去（避免掉下平台）
        if (HasGroundAhead())
            rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // ---- 攻擊 ----
    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);   // 攻擊時站定

        // 面向玩家
        if (player != null)
            facingDir = (int)Mathf.Sign(player.position.x - transform.position.x);

        // 冷卻好了就攻擊
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            if (animator != null) animator.SetTrigger("Attack");
            // 實際傷害由攻擊動畫的 Animation Event 觸發（見 EnemyAttackHitbox）
        }
    }

    // ---- 前方有沒有地面 ----
    bool HasGroundAhead()
    {
        if (edgeCheck == null) return true;   // 沒設就當有地面

        Vector2 origin = edgeCheck.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDist, groundLayer);
        return hit.collider != null;
    }

    // ---- 更新動畫與朝向 ----
    void UpdateAnimator()
    {
        // 朝向（翻轉圖片）
        float scaleX = Mathf.Abs(transform.localScale.x);
        int visualDir = spriteFacesLeft ? -facingDir : facingDir;
        transform.localScale = new Vector3(scaleX * visualDir, transform.localScale.y, transform.localScale.z);

        // 移動動畫（Speed 參數）
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    // ---- Gizmos（範圍視覺化）----
    void OnDrawGizmosSelected()
    {
        // 偵測範圍（黃）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        // 攻擊範圍（紅）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        // 失去範圍（灰）
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        // 巡邏範圍（藍）
        Vector2 origin = Application.isPlaying ? patrolOrigin : (Vector2)transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin + Vector2.left * patrolDistance,
                        origin + Vector2.right * patrolDistance);

        // 前方地面偵測（綠）
        if (edgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(edgeCheck.position,
                edgeCheck.position + Vector3.down * edgeCheckDist);
        }
    }
}