using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BloodOrb : MonoBehaviour
{
    [Header("飛行")]
    [SerializeField] float flightTime = 1f;      // 噴出到落地的時間
    [SerializeField] float arcHeight = 1.5f;     // 拋物線最高點

    [Header("吸血值")]
    [SerializeField] float percentPerOrb = 1f;   // 每顆佔最大吸血值的 %

    [Header("其他")]
    [Tooltip("落地後幾秒消失，0 = 不消失")]
    [SerializeField] float lifetimeAfterLanding = 0f;

    Vector2 start, end;
    float timer;
    bool flying;
    bool landed;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Launch(Vector2 from, Vector2 to)
    {
        start = from;
        end = to;
        timer = 0f;
        flying = true;
        landed = false;
        transform.position = from;
    }

    void Update()
    {
        if (!flying) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / flightTime);

        Vector2 pos = Vector2.Lerp(start, end, t);
        pos.y += arcHeight * 4f * t * (1f - t);   // 拋物線
        transform.position = pos;

        if (t >= 1f) Land();
    }

    void Land()
    {
        flying = false;
        landed = true;
        transform.position = end;
        if (lifetimeAfterLanding > 0f)
            Destroy(gameObject, lifetimeAfterLanding);
    }

    void OnTriggerEnter2D(Collider2D other) { TryCollect(other); }
    void OnTriggerStay2D(Collider2D other) { TryCollect(other); }  // 剛好落在玩家身上也能撿

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