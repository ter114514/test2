using UnityEngine;

/// <summary>
/// 記錄玩家最後的安全位置（在地面、非陷阱上）。
/// 玩家碰陷阱時傳送回這個位置。掛在玩家身上。
/// </summary>
public class SafePositionTracker : MonoBehaviour
{
    [Header("地面偵測")]
    [Tooltip("腳底位置")]
    [SerializeField] Transform groundCheck;
    [Tooltip("偵測半徑")]
    [SerializeField] float checkRadius = 0.2f;
    [Tooltip("地面圖層")]
    [SerializeField] LayerMask groundLayer;
    [Tooltip("陷阱圖層（站在這上面不算安全）")]
    [SerializeField] LayerMask trapLayer;

    [Header("記錄設定")]
    [Tooltip("多久記錄一次安全點（秒）")]
    [SerializeField] float recordInterval = 0.3f;
    [Tooltip("需要在安全地面持續多久才記錄")]
    [SerializeField] float stableTime = 0.2f;
    [Tooltip("垂直速度低於此值才算站穩（放寬可避免抖動導致判定失敗）")]
    [SerializeField] float verticalSpeedThreshold = 0.5f;

    [Header("除錯")]
    [Tooltip("勾選會印出偵測狀態，排查完可取消")]
    [SerializeField] bool debugLog = true;

    Vector2 lastSafePosition;
    float recordTimer;
    float stableTimer;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastSafePosition = transform.position;
    }

    void Update()
    {
        if (IsSafelyGrounded())
        {
            stableTimer += Time.deltaTime;

            if (stableTimer >= stableTime)
            {
                recordTimer += Time.deltaTime;
                if (recordTimer >= recordInterval)
                {
                    recordTimer = 0f;
                    lastSafePosition = transform.position;
                    if (debugLog) Debug.Log($"記錄安全點：{lastSafePosition}");
                }
            }
        }
        else
        {
            stableTimer = 0f;
            recordTimer = 0f;
        }
    }

    bool IsSafelyGrounded()
    {
        if (groundCheck == null)
        {
            if (debugLog) Debug.LogError("【SafePositionTracker】groundCheck 沒設！");
            return false;
        }

        bool onGround = Physics2D.OverlapCircle(
            groundCheck.position, checkRadius, groundLayer);
        bool onTrap = Physics2D.OverlapCircle(
            groundCheck.position, checkRadius, trapLayer);
        bool stable = rb == null ||
            Mathf.Abs(rb.linearVelocity.y) < verticalSpeedThreshold;

        if (debugLog)
            Debug.Log($"onGround={onGround}, onTrap={onTrap}, stable={stable}");

        return onGround && !onTrap && stable;
    }

    /// <summary>取得最後的安全位置</summary>
    public Vector2 GetSafePosition() => lastSafePosition;

    /// <summary>把玩家傳送回安全位置</summary>
    public void RespawnToSafe()
    {
        transform.position = lastSafePosition;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // 改用 OnDrawGizmos（不用選中就一直顯示綠點）
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSafePosition, 0.3f);

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}