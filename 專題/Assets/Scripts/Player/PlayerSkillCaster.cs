using UnityEngine;

/// <summary>
/// 玩家技能施放。按技能鍵，消耗吸血值釋放氣刃。
/// 實作 ISkillUISource 供技能 UI 顯示。
/// </summary>
public class PlayerSkillCaster : MonoBehaviour, ISkillUISource
{
    [Header("氣刃技能")]
    [SerializeField] GameObject bladeProjectilePrefab;
    [SerializeField] Vector2 spawnOffset = new Vector2(1f, 0f);
    [SerializeField] float castCooldown = 0.5f;

    PlayerInputHandler input;
    BloodResource blood;
    PlayerCombatController combat;
    Animator animator;

    float cooldownTimer;
    int facingDir = 1;

    // ---- ISkillUISource ----
    public float CooldownProgress =>
        castCooldown > 0 ? Mathf.Clamp01(cooldownTimer / castCooldown) : 0;
    public bool CanUse =>
        cooldownTimer <= 0 && blood != null && blood.CanCast
        && (combat == null || !combat.IsInHardState);
    public bool HasEnoughBlood => blood != null && blood.CanCast;   // 只看吸血值

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
        facingDir = transform.localScale.x >= 0 ? 1 : -1;
    }

    void TryCastSkill()
    {
        if (cooldownTimer > 0) return;
        if (combat != null && combat.IsInHardState) return;
        if (blood == null || !blood.CanCast) return;

        if (!blood.TryCastSkill()) return;

        cooldownTimer = castCooldown;

        if (animator != null)
            animator.SetTrigger("CastSkill");

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