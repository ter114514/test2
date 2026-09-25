/// <summary>
/// 技能 UI 需要的資訊。
/// </summary>
public interface ISkillUISource
{
    /// <summary>冷卻進度 0~1（1=剛用完，0=好了）</summary>
    float CooldownProgress { get; }

    /// <summary>能不能用（冷卻好 + 吸血值夠）</summary>
    bool CanUse { get; }

    /// <summary>吸血值夠不夠（不管冷卻，給紅遮罩用）</summary>
    bool HasEnoughBlood { get; }
}