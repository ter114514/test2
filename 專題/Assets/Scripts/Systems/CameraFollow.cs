using UnityEngine;

/// <summary>
/// 自製攝影機跟隨。額外 Y 偏移獨立於死區（正座下帶回彈不卡死區）。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 0.2f;
    [SerializeField] Vector3 offset = new Vector3(0, 1, -10);
    [SerializeField] float deadZoneWidth = 1f;
    [SerializeField] float deadZoneHeight = 1f;
    [SerializeField] bool useBounds = true;
    [SerializeField] float minX = -10f, maxX = 10f, minY = -5f, maxY = 5f;
    [SerializeField] float extraOffsetSpeed = 3f;

    Vector3 velocity = Vector3.zero;
    float shakeTimer, shakeDuration, shakeMagnitude;
    float extraYOffset, targetExtraYOffset;

    void Awake() { Instance = this; }

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

        extraYOffset = Mathf.MoveTowards(extraYOffset, targetExtraYOffset, extraOffsetSpeed * Time.deltaTime);

        // 純跟隨位置（扣掉上一幀的 extra）
        Vector3 current = transform.position;
        current.y -= extraYOffset;

        Vector3 desired = target.position + offset;

        float dx = desired.x - current.x;
        float dy = desired.y - current.y;

        Vector3 targetPos = current;
        if (Mathf.Abs(dx) > deadZoneWidth)
            targetPos.x = desired.x - Mathf.Sign(dx) * deadZoneWidth;
        if (Mathf.Abs(dy) > deadZoneHeight)
            targetPos.y = desired.y - Mathf.Sign(dy) * deadZoneHeight;
        targetPos.z = desired.z;

        Vector3 smoothed = Vector3.SmoothDamp(current, targetPos, ref velocity, smoothTime);

        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        // extraYOffset 最後獨立加上（不經死區，回彈不卡）
        smoothed.y += extraYOffset;

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * (shakeTimer / shakeDuration);
            smoothed.x += shakeOffset.x;
            smoothed.y += shakeOffset.y;
        }

        transform.position = smoothed;
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        if (duration * magnitude >= shakeTimer * shakeMagnitude)
        {
            shakeDuration = duration; shakeTimer = duration; shakeMagnitude = magnitude;
        }
    }

    public void SetExtraYOffset(float newOffset) => targetExtraYOffset = newOffset;

    public void SetBounds(float nMinX, float nMaxX, float nMinY, float nMaxY)
    {
        minX = nMinX; maxX = nMaxX; minY = nMinY; maxY = nMaxY;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 tl = new Vector3(minX, maxY, 0), tr = new Vector3(maxX, maxY, 0);
        Vector3 bl = new Vector3(minX, minY, 0), br = new Vector3(maxX, minY, 0);
        Gizmos.DrawLine(tl, tr); Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl); Gizmos.DrawLine(bl, tl);
    }
}