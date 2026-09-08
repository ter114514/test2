using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家 HUD 總管。訂閱各系統事件，更新所有 UI：
/// - 血量面具(空洞騎士風：一排面具，滿的亮、空的暗)
/// - 吸血值條
/// - 頭像(隨血格比例換圖)、頭像框兩裝飾(隨吸血值換圖)
/// - 血瓶(分階段換圖)、碎片圖示
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("資料來源")]
    [SerializeField] PlayerHealthSystem health;
    [SerializeField] BloodResource blood;
    [SerializeField] PotionSystem potion;
    [SerializeField] PotionShardSystem shardSystem;

    [Header("血量面具(空洞騎士風)")]
    [Tooltip("一排面具圖示(依最大血格排好)")]
    [SerializeField] Image[] maskIcons;
    [Tooltip("滿血格的圖(亮)")]
    [SerializeField] Sprite maskFullSprite;
    [Tooltip("空血格的圖(暗)")]
    [SerializeField] Sprite maskEmptySprite;

    [Header("吸血值條")]
    [SerializeField] Image bloodFill;
    [SerializeField] float bloodFillSpeed = 5f;

    [Header("頭像(隨血格比例換圖)")]
    [SerializeField] Image portraitImage;
    [SerializeField] PortraitState[] portraitStates;

    [Header("頭像框兩裝飾(隨吸血值換圖)")]
    [SerializeField] Image deco1;
    [SerializeField] Image deco2;
    [SerializeField] DecoState[] decoStates;

    [Header("血瓶(分階段換圖)")]
    [SerializeField] Image potionImage;
    [SerializeField] PotionStage[] potionStages;

    [Header("碎片圖示")]
    [SerializeField] Image[] shardIcons;
    [SerializeField] Sprite shardFilledSprite;
    [SerializeField] Sprite shardEmptySprite;

    [System.Serializable]
    public struct PortraitState
    {
        [Tooltip("血格比例高於此值時用這張(0~1)")]
        public float healthThreshold;
        public Sprite portrait;
    }

    [System.Serializable]
    public struct DecoState
    {
        [Tooltip("吸血值百分比高於此值時套用(0~1)")]
        public float bloodThreshold;
        public Sprite deco1Sprite;
        public Sprite deco2Sprite;
    }

    [System.Serializable]
    public struct PotionStage
    {
        [Tooltip("剩餘比例達到此值時用這張圖(0~1)，由高到低排")]
        public float threshold;
        public Sprite sprite;
    }

    float targetBloodFill = 0f;

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

    // ---- 血量：面具圖示 + 頭像 ----
    void HandleHealthChanged(int current, int max)
    {
        UpdateMasks(current, max);

        // 頭像用血格比例
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
                // 前 current 個是滿的，其餘是空的
                maskIcons[i].sprite = (i < current) ? maskFullSprite : maskEmptySprite;
            }
            else
            {
                maskIcons[i].enabled = false;   // 超過上限的隱藏
            }
        }
    }

    // ---- 吸血值：吸血值條 + 頭像框裝飾 ----
    void HandleBloodChanged(float current, float max)
    {
        float percent = max > 0 ? current / max : 0;
        targetBloodFill = percent;
        UpdateDeco(percent);
    }

    // ---- 血瓶：分階段換圖 ----
    void HandlePotionChanged(int current, int max)
    {
        float percent = max > 0 ? (float)current / max : 0;
        UpdatePotionStage(percent);
    }

    void UpdatePotionStage(float percent)
    {
        if (potionImage == null || potionStages == null || potionStages.Length == 0)
            return;

        Sprite chosen = potionStages[potionStages.Length - 1].sprite;
        foreach (var stage in potionStages)
        {
            if (percent >= stage.threshold)
            {
                chosen = stage.sprite;
                break;
            }
        }

        if (chosen != null && potionImage.sprite != chosen)
            potionImage.sprite = chosen;
    }

    // ---- 碎片圖示 ----
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
            else
            {
                shardIcons[i].enabled = false;
            }
        }
    }

    void Update()
    {
        if (bloodFill != null)
            bloodFill.fillAmount = Mathf.MoveTowards(
                bloodFill.fillAmount, targetBloodFill, bloodFillSpeed * Time.deltaTime);
    }

    void UpdatePortrait(float hpPercent)
    {
        if (portraitImage == null || portraitStates == null || portraitStates.Length == 0)
            return;

        Sprite chosen = portraitStates[portraitStates.Length - 1].portrait;
        foreach (var state in portraitStates)
        {
            if (hpPercent >= state.healthThreshold) { chosen = state.portrait; break; }
        }
        if (chosen != null && portraitImage.sprite != chosen)
            portraitImage.sprite = chosen;
    }

    void UpdateDeco(float bloodPercent)
    {
        if (decoStates == null || decoStates.Length == 0) return;

        DecoState chosen = decoStates[decoStates.Length - 1];
        foreach (var state in decoStates)
        {
            if (bloodPercent >= state.bloodThreshold) { chosen = state; break; }
        }

        if (deco1 != null && chosen.deco1Sprite != null && deco1.sprite != chosen.deco1Sprite)
            deco1.sprite = chosen.deco1Sprite;
        if (deco2 != null && chosen.deco2Sprite != null && deco2.sprite != chosen.deco2Sprite)
            deco2.sprite = chosen.deco2Sprite;
    }
}