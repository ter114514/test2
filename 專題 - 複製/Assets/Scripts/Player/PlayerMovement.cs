using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家移動系統。訂閱 PlayerInputHandler 的輸入事件，
/// 在 FixedUpdate 以 Rigidbody2D 做物理移動。
/// 內建土狼時間、跳躍緩衝、衝刺。
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 8f;
    public float acceleration = 60f;
    public float deceleration = 70f;

    [Header("跳躍")]
    public float jumpForce = 16f;
    public float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 2f;

    [Header("手感輔助")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("衝刺")]
    [Tooltip("衝刺速度")]
    public float dashForce = 24f;
    [Tooltip("衝刺持續時間")]
    public float dashDuration = 0.15f;
    [Tooltip("衝刺冷卻時間")]
    public float dashCooldown = 0.5f;

    [Header("地面偵測")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    PlayerInputHandler input;
    PlayerCombatController combat;

    float moveInput;
    bool isGrounded;
    float coyoteCounter;
    float jumpBufferCounter;
    bool jumpHeld;
    int facingDir = 1;

    // 衝刺狀態
    bool isDashing;
    float dashCooldownTimer;

    public bool IsDashing => isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputHandler>();
        combat = GetComponent<PlayerCombatController>();   // 可能沒有，允許 null
    }

    void OnEnable()
    {
        input.OnMove += SetMove;
        input.OnJumpPressed += OnJumpPressed;
        input.OnJumpReleased += OnJumpReleased;
        input.OnDashPressed += OnDashPressed;   // 訂閱衝刺
    }

    void OnDisable()
    {
        input.OnMove -= SetMove;
        input.OnJumpPressed -= OnJumpPressed;
        input.OnJumpReleased -= OnJumpReleased;
        input.OnDashPressed -= OnDashPressed;
    }

    void SetMove(float value) => moveInput = value;

    void OnJumpPressed()
    {
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
    }

    void OnJumpReleased()
    {
        jumpHeld = false;
        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier);
    }

    // ---- 衝刺 ----

    void OnDashPressed()
    {
        // 冷卻中、正在衝刺、或攻擊/防禦硬直中，不能衝
        if (dashCooldownTimer > 0) return;
        if (isDashing) return;
        if (combat != null && combat.IsInHardState) return;

        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        // 衝刺方向依面向（沒有輸入時用面向，有輸入用輸入方向）
        float dir = Mathf.Abs(moveInput) > 0.01f ? Mathf.Sign(moveInput) : facingDir;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;                    // 衝刺時無視重力（水平衝）

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector2(dir * dashForce, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;       // 還原重力
        isDashing = false;
    }

    void Update()
    {
        UpdateTimers();
        UpdateFacing();
    }

    void FixedUpdate()
    {
        CheckGround();

        // 衝刺中不做一般水平移動（由衝刺協程控制速度）
        if (!isDashing)
        {
            HandleHorizontal();
            HandleJump();
            HandleBetterGravity();
        }
    }

    void UpdateTimers()
    {
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;   // 衝刺冷卻倒數
    }

    void UpdateFacing()
    {
        if (isDashing) return;   // 衝刺中不改面向
        if (moveInput > 0.01f) facingDir = 1;
        else if (moveInput < -0.01f) facingDir = -1;
        transform.localScale = new Vector3(facingDir, 1, 1);
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleHorizontal()
    {
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = speedDiff * accelRate;

        rb.AddForce(Vector2.right * movement * Time.fixedDeltaTime, ForceMode2D.Impulse);
    }

    void HandleJump()
    {
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }
    }

    void HandleBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}