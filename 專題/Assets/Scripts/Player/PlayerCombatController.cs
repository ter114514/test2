using UnityEngine;

/// <summary>
/// 玩家戰鬥控制器。
/// 職責：連段(1→2→3循環)、防禦架勢、攻擊/防禦硬直、指揮 Animator。
/// 含計時保險，防止動畫事件沒觸發導致攻擊卡死。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerCombatController : MonoBehaviour
{
    static readonly int AttackTrigger = Animator.StringToHash("Attack");
    static readonly int ComboStepHash = Animator.StringToHash("ComboStep");
    static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");

    [Header("連段設定")]
    [Tooltip("最大連段數")]
    [SerializeField] int maxCombo = 3;
    [Tooltip("連段窗口：此時間內持續攻擊則循環往下接段，超過則重置回第一段")]
    [SerializeField] float comboWindow = 2.5f;

    [Header("攻擊硬直保險")]
    [Tooltip("攻擊最長持續時間，超過強制解除（防止動畫事件沒觸發卡死）")]
    [SerializeField] float attackMaxDuration = 1f;

    [Header("攻擊前衝")]
    [Tooltip("攻擊時向前輕推的力道（0 = 不前衝）")]
    [SerializeField] float attackLungeForce = 3f;

    [Header("引用")]
    [SerializeField] PlayerAttackHitbox hitbox;

    // ---- 對外唯讀狀態 ----
    public bool IsAttacking { get; private set; }
    public bool IsBlocking { get; private set; }
    public bool IsInHardState => IsAttacking || IsBlocking;

    Animator animator;
    PlayerInputHandler input;
    Rigidbody2D rb;

    int comboStep = 0;
    float lastAttackTime = -999f;
    bool canQueueNext = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInputHandler>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        input.OnAttackPressed += HandleAttackInput;
        input.OnBlockPressed += StartBlock;
        input.OnBlockReleased += EndBlock;
    }

    void OnDisable()
    {
        input.OnAttackPressed -= HandleAttackInput;
        input.OnBlockPressed -= StartBlock;
        input.OnBlockReleased -= EndBlock;
    }

    void Update()
    {
        // 超過連段窗口且非攻擊中，重置回第一段
        if (comboStep > 0 && !IsAttacking
            && Time.time - lastAttackTime > comboWindow)
        {
            comboStep = 0;
            animator.SetInteger(ComboStepHash, 0);
        }
    }

    // ---- 攻擊連段 ----

    void HandleAttackInput()
    {
        if (IsBlocking) return;   // 防禦中不能攻擊

        if (!IsAttacking)
            StartAttack();
        else if (canQueueNext)
            StartAttack();
    }

    void StartAttack()
    {
        // 循環連段：1 → 2 → 3 → 1 → 2 → 3 ...
        comboStep = comboStep % maxCombo + 1;
        lastAttackTime = Time.time;
        IsAttacking = true;
        canQueueNext = false;

        // 向面向方向輕推（前衝感）
        if (rb != null && attackLungeForce > 0)
        {
            float facing = Mathf.Sign(transform.localScale.x);
            rb.linearVelocity = new Vector2(facing * attackLungeForce, rb.linearVelocity.y);
        }

        animator.SetInteger(ComboStepHash, comboStep);
        animator.SetTrigger(AttackTrigger);

        // 保險：超過最長時間強制結束，防止 AttackEnd 沒觸發卡死
        CancelInvoke(nameof(ForceAttackEnd));
        Invoke(nameof(ForceAttackEnd), attackMaxDuration);
    }

    void ForceAttackEnd()
    {
        if (IsAttacking)
        {
            Debug.LogWarning("攻擊硬直逾時，強制解除（AttackEnd 可能沒觸發）");
            AnimEvent_AttackEnd();
        }
    }

    // ---- 以下由 Animation Event 呼叫 ----

    /// <summary>命中幀：開啟判定框</summary>
    public void AnimEvent_EnableHitbox()
    {
        float mult = comboStep >= maxCombo ? 1.5f : 1f;   // 第三段加重
        if (hitbox != null) hitbox.EnableHitbox(mult);
    }

    /// <summary>命中幀結束：關閉判定框</summary>
    public void AnimEvent_DisableHitbox()
    {
        if (hitbox != null) hitbox.DisableHitbox();
    }

    /// <summary>開放連段輸入窗口（放在後搖階段）</summary>
    public void AnimEvent_OpenComboWindow()
    {
        canQueueNext = true;
    }

    /// <summary>攻擊動畫結束：解除硬直（放在最後一幀）</summary>
    public void AnimEvent_AttackEnd()
    {
        IsAttacking = false;
        canQueueNext = false;
        CancelInvoke(nameof(ForceAttackEnd));   // 正常結束就取消保險
    }

    // ---- 防禦架勢 ----

    void StartBlock()
    {
        if (IsAttacking) return;
        IsBlocking = true;
        animator.SetBool(IsBlockingHash, true);
    }

    void EndBlock()
    {
        IsBlocking = false;
        animator.SetBool(IsBlockingHash, false);
    }
}