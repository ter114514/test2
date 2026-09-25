using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家 HUD 總管。
/// 血量面具、吸血值（圖片數字% + 頭像框背景 4 階段）、血瓶、碎片。
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("資料來源")]
    [SerializeField] PlayerHealthSystem health;
    [SerializeField] BloodResource blood;
    [SerializeField] PotionSystem potion;
    [SerializeField] PotionShardSystem shardSystem;

    [Header("血量面具")]
    [SerializeField] Image[] maskIcons;
    [SerializeField] Sprite maskFullSprite;
    [SerializeField] Sprite maskEmptySprite;

    [Header("吸血值 - 圖片數字")]
    [SerializeField] ImageNumberDisplay bloodNumberDisplay;

    [Header("吸血值 - 頭像框背景（4 階段）")]
    [SerializeField] Image portraitFrameBg;
    [SerializeField] Sprite frameBg100;
    [SerializeField] Sprite frameBg99to50;
    [SerializeField] Sprite frameBg49to1;
    [SerializeField] Sprite frameBg0;

    [Header("頭像（隨血格換圖）")]
    [SerializeField] Image portraitImage;
    [SerializeField] PortraitState[] portraitStates;

    [Header("血瓶（分階段換圖）")]
    [SerializeField] Image potionImage;
    [SerializeField] PotionStage[] potionStages;

    [Header("碎片圖示")]
    [SerializeField] Image[] shardIcons;
    [SerializeField] Sprite shardFilledSprite;
    [SerializeField] Sprite shardEmptySprite;

    [System.Serializable]
    public struct PortraitState
    {
        public float healthThreshold;
        public Sprite portrait;
    }

    [System.Serializable]
    public struct PotionStage
    {
        public float threshold;
        public Sprite sprite;
    }

    void Awake()
    {
        if (health == null) health = FindFirstObjectByType<PlayerHealthSystem>();
        if (blood == null) blood = FindFirstObjectByType<BloodResource>();
        if (potion == null) potion = FindFirstObjectByType<PotionSystem>();
        if (shardSystem == null) shardSystem = FindFirstObjectByType<PotionShardSystem>();
    }

    void OnEnable()
    {
        if (health != null) health.OnHealthChanged += HandleHealthChanged;
        if (blood != null) blood.OnBloodChanged += HandleBloodChanged;
        if (potion != null) potion.OnPotionCountChanged += HandlePotionChanged;
        if (shardSystem != null) shardSystem.OnShardChanged += HandleShardChanged;
    }

    void OnDisable()
    {
        if (health != null) health.OnHealthChanged -= HandleHealthChanged;
        if (blood != null) blood.OnBloodChanged -= HandleBloodChanged;
        if (potion != null) potion.OnPotionCountChanged -= HandlePotionChanged;
        if (shardSystem != null) shardSystem.OnShardChanged -= HandleShardChanged;
    }

    // ---- 血量：面具 + 頭像 ----
    void HandleHealthChanged(int current, int max)
    {
        UpdateMasks(current, max);
        float percent = max > 0 ? (float)current / max : 0;
        UpdatePortrait(percent);
    }

    void UpdateMasks(int current, int max)
    {
        if (maskIcons == null) return;
        for (int i = 0; i < maskIcons.Length; i++)
        {
            if (maskIcons[i] == null) continue;
            if (i < max)
            {
                maskIcons[i].enabled = true;
                maskIcons[i].sprite = (i < current) ? maskFullSprite : maskEmptySprite;
            }
            else maskIcons[i].enabled = false;
        }
    }

    // ---- 吸血值：圖片數字% + 頭像框背景 ----
    void HandleBloodChanged(float current, float max)
    {
        float percent = max > 0 ? current / max : 0;
        int percentInt = Mathf.RoundToInt(percent * 100);

        // 圖片數字
        if (bloodNumberDisplay != null)
            bloodNumberDisplay.SetNumber(percentInt);

        // 頭像框背景 4 階段
        UpdateFrameBg(percentInt);
    }

    void UpdateFrameBg(int percentInt)
    {
        if (portraitFrameBg == null) return;

        Sprite chosen;
        if (percentInt >= 100) chosen = frameBg100;
        else if (percentInt >= 50) chosen = frameBg99to50;
        else if (percentInt >= 1) chosen = frameBg49to1;
        else chosen = frameBg0;

        if (chosen != null && portraitFrameBg.sprite != chosen)
            portraitFrameBg.sprite = chosen;
    }

    // ---- 血瓶 ----
    void HandlePotionChanged(int current, int max)
    {
        float percent = max > 0 ? (float)current / max : 0;
        UpdatePotionStage(percent);
    }

    void UpdatePotionStage(float percent)
    {
        if (potionImage == null || potionStages == null || potionStages.Length == 0) return;
        Sprite chosen = potionStages[potionStages.Length - 1].sprite;
        foreach (var stage in potionStages)
        {
            if (percent >= stage.threshold) { chosen = stage.sprite; break; }
        }
        if (chosen != null && potionImage.sprite != chosen)
            potionImage.sprite = chosen;
    }

    // ---- 碎片 ----
    void HandleShardChanged(int current, int needed)
    {
        if (shardIcons == null) return;
        for (int i = 0; i < shardIcons.Length; i++)
        {
            if (shardIcons[i] == null) continue;
            if (i < needed)
            {
                shardIcons[i].enabled = true;
                shardIcons[i].sprite = (i < current) ? shardFilledSprite : shardEmptySprite;
            }
            else shardIcons[i].enabled = false;
        }
    }

    void UpdatePortrait(float hpPercent)
    {
        if (portraitImage == null || portraitStates == null || portraitStates.Length == 0) return;
        Sprite chosen = portraitStates[portraitStates.Length - 1].portrait;
        foreach (var state in portraitStates)
        {
            if (hpPercent >= state.healthThreshold) { chosen = state.portrait; break; }
        }
        if (chosen != null && portraitImage.sprite != chosen)
            portraitImage.sprite = chosen;
    }
}