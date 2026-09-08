using UnityEngine;

/// <summary>
/// 自製攝影機跟隨。SmoothDamp 平滑跟隨玩家，含死區與邊界限制。
/// 邊界可在執行中動態改變（房間切換、Boss 戰等）。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("跟隨目標")]
    [SerializeField] Transform target;

    [Header("跟隨設定")]
    [Tooltip("跟隨平滑時間，越大越慢")]
    [SerializeField] float smoothTime = 0.2f;
    [Tooltip("攝影機與目標的偏移")]
    [SerializeField] Vector3 offset = new Vector3(0, 1, -10);

    [Header("死區（目標在此範圍內不移動鏡頭）")]
    [SerializeField] float deadZoneWidth = 1f;
    [SerializeField] float deadZoneHeight = 1f;

    [Header("邊界限制")]
    [Tooltip("是否啟用邊界限制")]
    [SerializeField] bool useBounds = true;
    [SerializeField] float minX = -10f;
    [SerializeField] float maxX = 10f;
    [SerializeField] float minY = -5f;
    [SerializeField] float maxY = 5f;

    [Tooltip("邊界切換時是否平滑過渡")]
    [SerializeField] bool smoothBoundTransition = true;
    [SerializeField] float boundTransitionSpeed = 3f;

    Vector3 velocity = Vector3.zero;

    // 目標邊界（動態改變時平滑過渡用）
    float targetMinX, targetMaxX, targetMinY, targetMaxY;

    void Start()
    {
        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }

        // 初始化目標邊界為當前值
        targetMinX = minX; targetMaxX = maxX;
        targetMinY = minY; targetMaxY = maxY;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 邊界平滑過渡（動態改變時）
        if (smoothBoundTransition)
        {
            float t = boundTransitionSpeed * Time.deltaTime;
            minX = Mathf.Lerp(minX, targetMinX, t);
            maxX = Mathf.Lerp(maxX, targetMaxX, t);
            minY = Mathf.Lerp(minY, targetMinY, t);
            maxY = Mathf.Lerp(maxY, targetMaxY, t);
        }

        Vector3 desired = target.position + offset;

        // 死區：目標在死區內不移動鏡頭
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
        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position, targetPos, ref velocity, smoothTime);

        // 邊界限制
        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        transform.position = smoothed;
    }

    // ---- 動態改變邊界 ----

    /// <summary>設定新的攝影機邊界（可平滑過渡）</summary>
    public void SetBounds(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        targetMinX = newMinX;
        targetMaxX = newMaxX;
        targetMinY = newMinY;
        targetMaxY = newMaxY;

        // 不平滑就立刻套用
        if (!smoothBoundTransition)
        {
            minX = newMinX; maxX = newMaxX;
            minY = newMinY; maxY = newMaxY;
        }
    }

    /// <summary>用一個 Rect 設定邊界（方便從區域物件傳入）</summary>
    public void SetBounds(Rect bounds)
    {
        SetBounds(bounds.xMin, bounds.xMax, bounds.yMin, bounds.yMax);
    }

    /// <summary>啟用/停用邊界限制</summary>
    public void SetUseBounds(bool enabled)
    {
        useBounds = enabled;
    }

    void OnDrawGizmosSelected()
    {
        // 畫出邊界範圍
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