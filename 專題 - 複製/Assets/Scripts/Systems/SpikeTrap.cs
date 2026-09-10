using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Header("傷害設定")]
    [Tooltip("對玩家造成的傷害量")]
    public float damage = 1f;
    [Tooltip("玩家踩到地刺時受傷彈飛的向上力道")]
    public float upwardKnockback = 12f;

    [Header("時間週期設定 (秒)")]
    [Tooltip("初始延遲秒數（可用於錯開複數地刺節奏）")]
    public float initialDelay = 0f;
    [Tooltip("縮回狀態的安全停留時間")]
    public float retractedTime = 2.0f;
    [Tooltip("伸出狀態的致命停留時間")]
    public float extendedTime = 1.5f;
    [Tooltip("升起或收回的動畫移動時間（秒）")]
    public float slideDuration = 0.25f;

    [Header("位移設定")]
    [Tooltip("縮回時相對於原位往下掉的距離 (例如 -1 表示向下沉 1 個單位)")]
    public float hideOffsetY = -1.0f;

    Vector3 extendedLocalPos;
    Vector3 retractedLocalPos;
    Collider2D spikeCollider;
    bool isDangerous;

    void Awake()
    {
        spikeCollider = GetComponent<Collider2D>();
        spikeCollider.isTrigger = true;

        extendedLocalPos = transform.localPosition;
        retractedLocalPos = extendedLocalPos + new Vector3(0, hideOffsetY, 0);
    }

    void Start()
    {
        transform.localPosition = retractedLocalPos;
        SetDangerousState(false);

        StartCoroutine(SpikeCycleRoutine());
    }

    IEnumerator SpikeCycleRoutine()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // 1. 停留在縮回狀態（安全）
            yield return new WaitForSeconds(retractedTime);

            // 2. 向上平滑升起（伸出）
            yield return StartCoroutine(SlideToPosition(retractedLocalPos, extendedLocalPos, true));

            // 3. 停留在伸出狀態（致命）
            yield return new WaitForSeconds(extendedTime);

            // 4. 向下平滑縮回（收回）
            yield return StartCoroutine(SlideToPosition(extendedLocalPos, retractedLocalPos, false));
        }
    }

    IEnumerator SlideToPosition(Vector3 startPos, Vector3 endPos, bool targetDangerousState)
    {
        float elapsed = 0f;

        if (targetDangerousState)
        {
            SetDangerousState(true);
        }

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.localPosition = endPos;

        if (!targetDangerousState)
        {
            SetDangerousState(false);
        }
    }

    void SetDangerousState(bool active)
    {
        isDangerous = active;
        spikeCollider.enabled = active;
    }

    void OnTriggerEnter2D(Collider2D other) => TryDamageTarget(other);

    void OnTriggerStay2D(Collider2D other) => TryDamageTarget(other);

    void TryDamageTarget(Collider2D target)
    {
        if (!isDangerous) return;

        if (target.CompareTag("Player"))
        {
            Vector2 knockback = Vector2.up * upwardKnockback;

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, knockback);
            }
            else
            {
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, upwardKnockback);
                }
            }
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }
}