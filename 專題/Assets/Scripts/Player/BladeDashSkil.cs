using UnityEngine;
using System.Collections;

/// <summary>
/// 瞬殺閃：鎖定前方中距離敵人，突進至其身後斬擊。擊殺重置冷卻。
/// 實作 ISkillUISource 供技能 UI 顯示。
/// </summary>
public class BladeDashSkill : MonoBehaviour, ISkillUISource
{
    [Header("鎖定")]
    [SerializeField] float minLockRange = 2f;
    [SerializeField] float maxLockRange = 8f;
    [SerializeField] LayerMask enemyLayer;

    [Header("突進")]
    [SerializeField] float behindOffset = 1f;
    [SerializeField] float dashDuration = 0.1f;

    [Header("傷害")]
    [SerializeField] float damage = 40f;
    [SerializeField] float knockbackForce = 3f;

    [Header("冷卻與消耗")]
    [SerializeField] float cooldown = 3f;
    [SerializeField] float bloodCost = 30f;

    [Header("特效（可選）")]
    [SerializeField] GameObject dashEffectPrefab;
    [SerializeField] GameObject slashEffectPrefab;

    PlayerInputHandler input;
    BloodResource blood;
    PlayerHealthSystem health;
    Rigidbody2D rb;
    PlayerMovement movement;

    float cooldownTimer;
    bool isDashing;

    // ---- ISkillUISource ----
    public float CooldownProgress => cooldown > 0 ? Mathf.Clamp01(cooldownTimer / cooldown) : 0;
    public bool CanUse => cooldownTimer <= 0 && blood != null && blood.CurrentBlood >= bloodCost;
    public bool HasEnoughBlood => blood != null && blood.CurrentBlood >= bloodCost;   // 只看吸血值

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        blood = GetComponent<BloodResource>();
        health = GetComponent<PlayerHealthSystem>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        if (input != null) input.OnSkill2Pressed += TryUse;
    }

    void OnDisable()
    {
        if (input != null) input.OnSkill2Pressed -= TryUse;
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void TryUse()
    {
        if (isDashing) return;
        if (cooldownTimer > 0) return;
        if (blood == null || blood.CurrentBlood < bloodCost) return;

        EnemyHealth target = FindTarget();
        if (target == null) return;

        blood.TryConsume(bloodCost);

        StartCoroutine(DashRoutine(target));
    }

    EnemyHealth FindTarget()
    {
        int facing = transform.localScale.x >= 0 ? 1 : -1;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxLockRange, enemyLayer);

        EnemyHealth nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dx = hit.transform.position.x - transform.position.x;
            if (Mathf.Sign(dx) != facing) continue;

            float dist = Mathf.Abs(dx);
            if (dist < minLockRange || dist > maxLockRange) continue;

            if (hit.TryGetComponent<EnemyHealth>(out var eh) && !eh.IsDead)
            {
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = eh;
                }
            }
        }

        return nearest;
    }

    IEnumerator DashRoutine(EnemyHealth target)
    {
        isDashing = true;
        cooldownTimer = cooldown;

        if (movement != null) movement.enabled = false;

        if (dashEffectPrefab != null)
            Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);

        float side = -Mathf.Sign(transform.position.x - target.transform.position.x);
        Vector3 behindPos = target.transform.position + new Vector3(side * behindOffset, 0, 0);

        Vector3 startPos = transform.position;
        float elapsed = 0f;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, behindPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = behindPos;

        float faceDir = Mathf.Sign(target.transform.position.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * faceDir,
            transform.localScale.y, transform.localScale.z);

        if (slashEffectPrefab != null)
            Instantiate(slashEffectPrefab, target.transform.position, Quaternion.identity);

        bool wasDead = target.IsDead;
        Vector2 knockback = new Vector2(faceDir, 0.2f).normalized * knockbackForce;
        target.TakeDamage(damage, knockback);

        if (!wasDead && target.IsDead)
            cooldownTimer = 0;

        if (movement != null) movement.enabled = true;
        isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, maxLockRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minLockRange);
    }
}