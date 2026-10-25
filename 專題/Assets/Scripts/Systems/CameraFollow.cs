using UnityEngine;

/// <summary>
/// 自製攝影機跟隨。SmoothDamp 平滑跟隨玩家,含死區與邊界限制。
/// 加入鏡頭晃動(命中時觸發,增強打擊感)。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("跟隨目標")]
    [SerializeField] Transform target;

    [Header("跟隨設定")]
    [SerializeField] float smoothTime = 0.2f;
    [SerializeField] Vector3 offset = new Vector3(0, 1, -10);

    [Header("死區")]
    [SerializeField] float deadZoneWidth = 1f;
    [SerializeField] float deadZoneHeight = 1f;

    [Header("邊界限制")]
    [SerializeField] bool useBounds = true;
    [SerializeField] float minX = -10f;
    [SerializeField] float maxX = 10f;
    [SerializeField] float minY = -5f;
    [SerializeField] float maxY = 5f;

    Vector3 velocity = Vector3.zero;

    // 晃動
    float shakeTimer;
    float shakeDuration;
    float shakeMagnitude;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        // 死區
        Vector3 current = transform.position;
        float dx = desired.x - current.x;
        float dy = desired.y - current.y;

        Vector3 targetPos = current;
        if (Mathf.Abs(dx) > deadZoneWidth)
            targetPos.x = desired.x - Mathf.Sign(dx) * deadZoneWidth;
        if (Mathf.Abs(dy) > deadZoneHeight)
            targetPos.y = desired.y - Mathf.Sign(dy) * deadZoneHeight;
        targetPos.z = desired.z;

        // 平滑移動
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // 邊界限制
        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        // 晃動(疊加在最終位置上)
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;   // 用真實時間(Hit Stop 凍結時也能抖)
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * (shakeTimer / shakeDuration);
            smoothed.x += shakeOffset.x;
            smoothed.y += shakeOffset.y;
        }

        transform.position = smoothed;
    }

    // ---- 鏡頭晃動 ----

    /// <summary>觸發鏡頭晃動</summary>
    /// <param name="duration">晃動時間</param>
    /// <param name="magnitude">晃動幅度</param>
    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        // 新晃動比當前強才覆蓋(避免小晃動打斷大晃動)
        if (duration * magnitude >= shakeTimer * shakeMagnitude)
        {
            shakeDuration = duration;
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }
    }

    // ---- 動態邊界(之前的,保留)----
    public void SetBounds(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX; maxX = newMaxX;
        minY = newMinY; maxY = newMaxY;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 tl = new Vector3(minX, maxY, 0);
        Vector3 tr = new Vector3(maxX, maxY, 0);
        Vector3 bl = new Vector3(minX, minY, 0);
        Vector3 br = new Vector3(maxX, minY, 0);
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
}