using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 標題選單控制。標題一直播 Animator 逐幀動畫（程式不碰標題 sprite）。
/// 懸停時可觸發選單底圖動畫。點擊展開選單。
/// </summary>
public class TitleMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("標題圖（上移縮小）")]
    [SerializeField] RectTransform titleImage;
    [SerializeField] Vector2 titleTargetOffset = new Vector2(0, 200);
    [SerializeField] float titleShrinkScale = 0.6f;

    [Header("選單底圖（下移 + 懸停動畫）")]
    [SerializeField] RectTransform menuBgImage;
    [SerializeField] Vector2 menuBgTargetOffset = new Vector2(0, -150);
    [SerializeField] float menuBgScale = 1f;
    [SerializeField] Animator menuBgAnimator;

    [Header("移動")]
    [SerializeField] float moveSpeed = 4f;

    [Header("選單按鈕")]
    [SerializeField] CanvasGroup menuGroup;
    [SerializeField] float fadeSpeed = 4f;

    [Header("氛圍粒子")]
    [SerializeField] UIAmbientParticles ambientParticles;

    [Header("閒置回復")]
    [SerializeField] float idleTimeout = 8f;

    static readonly int HoverHash = Animator.StringToHash("Hover");

    Vector2 titleBasePos, titleTargetPos;
    Vector3 titleBaseScale;
    Vector2 menuBgBasePos, menuBgTargetPos;
    Vector3 menuBgBaseScale;

    bool isExpanded;
    float idleTimer;

    void Awake()
    {
        titleBasePos = titleImage.anchoredPosition;
        titleTargetPos = titleBasePos + titleTargetOffset;
        titleBaseScale = titleImage.localScale;

        menuBgBasePos = menuBgImage.anchoredPosition;
        menuBgTargetPos = menuBgBasePos + menuBgTargetOffset;
        menuBgBaseScale = menuBgImage.localScale;
    }

    void Start()
    {
        SetMenuVisible(false, instant: true);
    }

    void Update()
    {
        float t = moveSpeed * Time.unscaledDeltaTime;

        titleImage.anchoredPosition = Vector2.Lerp(
            titleImage.anchoredPosition,
            isExpanded ? titleTargetPos : titleBasePos, t);
        titleImage.localScale = Vector3.Lerp(
            titleImage.localScale,
            isExpanded ? titleBaseScale * titleShrinkScale : titleBaseScale, t);

        menuBgImage.anchoredPosition = Vector2.Lerp(
            menuBgImage.anchoredPosition,
            isExpanded ? menuBgTargetPos : menuBgBasePos, t);
        menuBgImage.localScale = Vector3.Lerp(
            menuBgImage.localScale,
            isExpanded ? menuBgBaseScale * menuBgScale : menuBgBaseScale, t);

        menuGroup.alpha = Mathf.MoveTowards(
            menuGroup.alpha, isExpanded ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);

        if (isExpanded)
        {
            if (AnyInput()) idleTimer = 0f;
            else idleTimer += Time.unscaledDeltaTime;
            if (idleTimer >= idleTimeout) Collapse();
        }
    }

    bool AnyInput()
    {
        var mouse = Mouse.current;
        var kb = Keyboard.current;
        bool mouseMoved = mouse != null && mouse.delta.ReadValue().sqrMagnitude > 0.01f;
        bool mouseClick = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool keyPressed = kb != null && kb.anyKey.wasPressedThisFrame;
        return mouseMoved || mouseClick || keyPressed;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isExpanded) return;
        // 懸停只觸發底圖動畫（標題不碰，交給 Animator 一直播）
        if (menuBgAnimator != null) menuBgAnimator.SetBool(HoverHash, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isExpanded) return;
        if (menuBgAnimator != null) menuBgAnimator.SetBool(HoverHash, false);
    }

    public void OnTitleClicked()
    {
        if (!isExpanded) Expand();
    }

    void Expand()
    {
        isExpanded = true;
        idleTimer = 0f;

        if (menuBgAnimator != null) menuBgAnimator.SetBool(HoverHash, false);

        SetMenuVisible(true);

        if (ambientParticles != null)
            ambientParticles.StartSpawning();
    }

    void Collapse()
    {
        isExpanded = false;
        SetMenuVisible(false);

        if (menuBgAnimator != null) menuBgAnimator.SetBool(HoverHash, false);

        if (ambientParticles != null)
            ambientParticles.StopAndClear();
    }

    void SetMenuVisible(bool visible, bool instant = false)
    {
        menuGroup.interactable = visible;
        menuGroup.blocksRaycasts = visible;
        if (instant) menuGroup.alpha = visible ? 1f : 0f;
    }

    public void StopParticles()
    {
        if (ambientParticles != null)
            ambientParticles.StopAndClear();
    }
}
