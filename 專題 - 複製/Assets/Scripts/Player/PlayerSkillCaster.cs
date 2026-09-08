using UnityEngine;

/// <summary>
/// 玩家技能施放。按技能鍵，消耗吸血值釋放氣刃。掛在玩家身上。
/// </summary>
public class PlayerSkillCaster : MonoBehaviour
{
    [Header("氣刃技能")]
    [Tooltip("氣刃投射物 Prefab")]
    [SerializeField] GameObject bladeProjectilePrefab;
    [Tooltip("生成位置偏移（相對玩家，身前）")]
    [SerializeField] Vector2 spawnOffset = new Vector2(1f, 0f);
    [Tooltip("施法冷卻時間")]
    [SerializeField] float castCooldown = 0.5f;

    PlayerInputHandler input;
    BloodResource blood;
    PlayerCombatController combat;
    Animator animator;

    float cooldownTimer;
    int facingDir = 1;

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        blood = GetComponent<BloodResource>();
        combat = GetComponent<PlayerCombatController>();
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        if (input != null) input.OnCastSkillPressed += TryCastSkill;
    }

    void OnDisable()
    {
        if (input != null) input.OnCastSkillPressed -= TryCastSkill;
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // 依 localScale 記錄面向
        facingDir = transform.localScale.x >= 0 ? 1 : -1;
    }

    void TryCastSkill()
    {
        if (cooldownTimer > 0) return;                            // 冷卻中
        if (combat != null && combat.IsInHardState) return;      // 硬直中
        if (blood == null || !blood.CanCast) return;             // 吸血值不夠

        if (!blood.TryCastSkill()) return;                       // 消耗吸血值

        cooldownTimer = castCooldown;

        if (animator != null)
            animator.SetTrigger("CastSkill");                    // 施法動畫（可選）

        SpawnBlade();
    }

    void SpawnBlade()
    {
        if (bladeProjectilePrefab == null) return;

        Vector3 spawnPos = transform.position +
            new Vector3(spawnOffset.x * facingDir, spawnOffset.y, 0);

        var blade = Instantiate(bladeProjectilePrefab, spawnPos, Quaternion.identity);

        if (blade.TryGetComponent<BladeProjectile>(out var proj))
            proj.SetDirection(facingDir);
    }
}
