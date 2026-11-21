using UnityEngine;

/// <summary>
/// 自製攝影機跟隨。含死區、邊界、晃動、額外 Y 偏移（正座鏡頭下帶）。
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

    [Header("額外 Y 偏移（正座鏡頭下帶）")]
    [SerializeField] float extraOffsetSpeed = 3f;

    Vector3 velocity = Vector3.zero;

    // 晃動
    float shakeTimer, shakeDuration, shakeMagnitude;

    // 額外 Y 偏移
    float extraYOffset;
    float targetExtraYOffset;

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

        // 額外 Y 偏移平滑過渡
        extraYOffset = Mathf.MoveTowards(extraYOffset, targetExtraYOffset, extraOffsetSpeed * Time.deltaTime);

        Vector3 desired = target.position + offset + new Vector3(0, extraYOffset, 0);

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

        Vector3 smoothed = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        // 晃動
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * (shakeTimer / shakeDuration);
            smoothed.x += shakeOffset.x;
            smoothed.y += shakeOffset.y;
        }

        transform.position = smoothed;
    }

    // ---- 晃動 ----
    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        if (duration * magnitude >= shakeTimer * shakeMagnitude)
        {
            shakeDuration = duration;
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }
    }

    // ---- 額外 Y 偏移（正座鏡頭下帶）----
    public void SetExtraYOffset(float offset)
    {
        targetExtraYOffset = offset;
    }

    // ---- 動態邊界 ----
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
        Gizmos.DrawLine(tl, tr); Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl); Gizmos.DrawLine(bl, tl);
    }
}