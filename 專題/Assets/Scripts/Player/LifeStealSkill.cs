using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 吸血技能：對前方敵人傷害 + 吸血值，技能期間 Boss 以外全停（含玩家）。
/// 施放時播吸血動畫，玩家定住。
/// </summary>
public class LifeStealSkill : MonoBehaviour, ISkillUISource
{
    [Header("鎖定")]
    [SerializeField] float lockRange = 6f;
    [SerializeField] LayerMask enemyLayer;

    [Header("傷害與吸血")]
    [SerializeField] float damage = 30f;
    [SerializeField] float knockbackForce = 3f;
    [SerializeField] float bloodGain = 40f;

    [Header("時停")]
    [SerializeField] float pauseDuration = 3f;

    [Header("冷卻與消耗")]
    [SerializeField] float cooldown = 5f;
    [SerializeField] float bloodCost = 20f;

    [Header("動畫")]
    [SerializeField] Animator animator;
    [SerializeField] string animTrigger = "LifeSteal";

    [Header("特效（可選）")]
    [SerializeField] GameObject castEffectPrefab;
    [SerializeField] GameObject hitEffectPrefab;

    PlayerInputHandler input;
    BloodResource blood;
    PlayerMovement movement;
    PlayerCombatController combat;
    PlayerSkillCaster skillCaster;
    BladeDashSkill dashSkill;
    Rigidbody2D rb;

    float cooldownTimer;
    bool isCasting;

    public float CooldownProgress => cooldown > 0 ? Mathf.Clamp01(cooldownTimer / cooldown) : 0;
    public bool CanUse => cooldownTimer <= 0 && blood != null && blood.CurrentBlood >= bloodCost;
    public bool HasEnoughBlood => blood != null && blood.CurrentBlood >= bloodCost;

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        blood = GetComponent<BloodResource>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombatController>();
        skillCaster = GetComponent<PlayerSkillCaster>();
        dashSkill = GetComponent<BladeDashSkill>();
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        if (input != null) input.OnSkill3Pressed += TryUse;
    }

    void OnDisable()
    {
        if (input != null) input.OnSkill3Pressed -= TryUse;
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void TryUse()
    {
        if (isCasting) return;
        if (cooldownTimer > 0) return;
        if (blood == null || blood.CurrentBlood < bloodCost) return;

        EnemyHealth target = FindTarget();
        if (target == null) return;

        blood.TryConsume(bloodCost);
        cooldownTimer = cooldown;

        StartCoroutine(SkillRoutine(target));
    }

    EnemyHealth FindTarget()
    {
        int facing = transform.localScale.x >= 0 ? 1 : -1;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lockRange, enemyLayer);

        EnemyHealth nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dx = hit.transform.position.x - transform.position.x;
            if (Mathf.Sign(dx) != facing) continue;
            float dist = Mathf.Abs(dx);
            if (dist > lockRange) continue;

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

    IEnumerator SkillRoutine(EnemyHealth target)
    {
        isCasting = true;

        // 播吸血動畫
        if (animator != null)
            animator.SetTrigger(animTrigger);

        // 面向目標
        float faceDir = Mathf.Sign(target.transform.position.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * faceDir,
            transform.localScale.y, transform.localScale.z);

        // ---- 玩家定住（鎖所有操作）----
        SetPlayerFrozen(true);

        if (castEffectPrefab != null)
            Instantiate(castEffectPrefab, transform.position, Quaternion.identity);

        // 時停：暫停 Boss 以外的單位
        List<IPausable> paused = PauseAllExceptBoss();

        // 傷害
        Vector2 knockback = new Vector2(faceDir, 0.2f).normalized * knockbackForce;
        target.TakeDamage(damage, knockback);

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, target.transform.position, Quaternion.identity);

        // 增加吸血值
        if (blood != null)
            blood.Gain(bloodGain);

        // 時停持續
        yield return new WaitForSeconds(pauseDuration);

        // 恢復敵人
        foreach (var p in paused)
            if (p != null) p.ResumeActions();

        // 解除玩家定住
        SetPlayerFrozen(false);

        isCasting = false;
    }

    // 凍結/解凍玩家所有操作
    void SetPlayerFrozen(bool frozen)
    {
        // 停止移動
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // 停用各操作元件（凍結時停用，解凍時啟用）
        if (movement != null) movement.enabled = !frozen;
        if (combat != null) combat.enabled = !frozen;
        if (skillCaster != null) skillCaster.enabled = !frozen;
        if (dashSkill != null) dashSkill.enabled = !frozen;

        // 凍結時暫停動畫（可選，讓玩家也定格）
        // if (animator != null) animator.speed = frozen ? 0 : 1;
    }

    List<IPausable> PauseAllExceptBoss()
    {
        var paused = new List<IPausable>();
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var mb in all)
        {
            if (mb is IPausable p && !p.IsBoss)
            {
                p.PauseActions();
                paused.Add(p);
            }
        }
        return paused;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockRange);
    }
}