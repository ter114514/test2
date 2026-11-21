using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BloodOrb : MonoBehaviour
{
    [Header("飛行")]
    [SerializeField] float flightTime = 1f;
    [SerializeField] float arcHeight = 1.5f;
    [Tooltip("每顆血珠的拋物線高度隨機浮動")]
    [SerializeField] float arcHeightRandom = 0.5f;

    [Header("吸血值")]
    [SerializeField] float percentPerOrb = 1f;

    [Header("動態效果")]
    [SerializeField] float spinSpeed = 360f;
    [SerializeField] float pulseSpeed = 8f;
    [SerializeField] float pulseAmount = 0.12f;

    [Header("落地彈跳（讓血珠散開）")]
    [SerializeField] float bounceDistance = 0.4f;
    [SerializeField] float bounceHeight = 0.25f;
    [SerializeField] float bounceTime = 0.25f;

    [Header("特效（可選）")]
    [SerializeField] GameObject landSplashPrefab;
    [SerializeField] GameObject stainPrefab;

    Vector2 start, end;
    float timer, bounceTimer, arc;
    Vector2 bounceFrom, bounceTo;
    bool flying, landed, bouncing;
    Vector3 baseScale;
    TrailRenderer trail;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        trail = GetComponent<TrailRenderer>();
        baseScale = transform.localScale;
    }

    public void Launch(Vector2 from, Vector2 to)
    {
        start = from;
        end = to;
        timer = 0f;
        arc = arcHeight + Random.Range(-arcHeightRandom, arcHeightRandom);
        flying = true;
        landed = false;
        bouncing = false;
        transform.position = from;

        if (trail != null) { trail.Clear(); trail.emitting = true; }
    }

    void Update()
    {
        if (flying) UpdateFlight();
        else if (bouncing) UpdateBounce();
        else if (landed) UpdateIdle();
    }

    void UpdateFlight()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / flightTime);

        Vector2 pos = Vector2.Lerp(start, end, t);
        pos.y += arc * 4f * t * (1f - t);
        transform.position = pos;

        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * s;

        if (t >= 1f) Land();
    }

    void Land()
    {
        flying = false;
        landed = true;               // 落地就能撿
        transform.position = end;
        if (trail != null) trail.emitting = false;

        if (landSplashPrefab != null)
            Instantiate(landSplashPrefab, end, Quaternion.identity);

        if (stainPrefab != null)
        {
            var stain = Instantiate(stainPrefab, end, Quaternion.identity);
            float size = Random.Range(0.7f, 1.3f);
            stain.transform.localScale = Vector3.Scale(stain.transform.localScale,
                new Vector3(size * (Random.value < 0.5f ? -1 : 1), size, 1));
        }

        // 小彈跳：往隨機左右方向再滑一小段
        float dir = Random.value < 0.5f ? -1f : 1f;
        bounceFrom = end;
        bounceTo = end + Vector2.right * dir * Random.Range(bounceDistance * 0.5f, bounceDistance);
        bounceTimer = 0f;
        bouncing = true;
    }

    void UpdateBounce()
    {
        bounceTimer += Time.deltaTime;
        float t = Mathf.Clamp01(bounceTimer / bounceTime);

        Vector2 pos = Vector2.Lerp(bounceFrom, bounceTo, t);
        pos.y += bounceHeight * 4f * t * (1f - t);
        transform.position = pos;

        if (t >= 1f) bouncing = false;
    }

    void UpdateIdle()
    {
        // 落地後輕輕跳動，像還在流動的血
        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * s;
    }

    void OnTriggerEnter2D(Collider2D other) { TryCollect(other); }
    void OnTriggerStay2D(Collider2D other) { TryCollect(other); }

    void TryCollect(Collider2D other)
    {
        if (!landed) return;
        if (!other.CompareTag("Player")) return;

        var blood = other.GetComponentInParent<BloodResource>();
        if (blood == null) return;

        blood.Gain(blood.MaxBlood * percentPerOrb / 100f);
        Destroy(gameObject);
    }
}