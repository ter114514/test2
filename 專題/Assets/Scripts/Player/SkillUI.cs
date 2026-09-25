using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用技能 UI。扇形圓顯示冷卻，吸血值不足時顯示半透明紅遮罩。
/// 冷卻中不發紅（冷卻用扇形表示）。
/// </summary>
public class SkillUI : MonoBehaviour
{
    [Header("技能來源")]
    [SerializeField] MonoBehaviour skillSourceBehaviour;

    [Header("冷卻扇形")]
    [SerializeField] Image cooldownFill;

    [Header("吸血值不足時的紅色遮罩")]
    [SerializeField] GameObject redOverlay;

    ISkillUISource source;

    void Awake()
    {
        source = skillSourceBehaviour as ISkillUISource;
        if (source == null)
            Debug.LogWarning($"【SkillUI】{name} 的技能來源未實作 ISkillUISource");
    }

    void Update()
    {
        if (source == null) return;

        // 冷卻扇形
        if (cooldownFill != null)
            cooldownFill.fillAmount = source.CooldownProgress;

        // 紅遮罩只看吸血值（冷卻中不發紅）
        if (redOverlay != null)
            redOverlay.SetActive(!source.HasEnoughBlood);
    }
}