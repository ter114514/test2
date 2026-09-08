using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 選單按鈕懸停時，讓選單底圖播放動畫。
/// 任一按鈕被懸停就觸發，全部移開才恢復。
/// 掛在一個管理物件上，把按鈕和底圖 Animator 指定進來。
/// </summary>
public class MenuBgHoverAnim : MonoBehaviour
{
    [Header("觸發來源")]
    [Tooltip("會觸發底圖動畫的所有選單按鈕")]
    [SerializeField] List<RectTransform> buttons = new();

    [Header("底圖")]
    [Tooltip("要播動畫的選單底圖 Animator")]
    [SerializeField] Animator bgAnimator;
    [Tooltip("底圖動畫的 bool 參數名")]
    [SerializeField] string hoverParam = "Hover";

    static readonly int HoverHash = Animator.StringToHash("Hover");

    int hoverCount = 0;   // 目前有幾個按鈕正被懸停

    void Start()
    {
        // 為每個按鈕自動掛上懸停偵測
        foreach (var btn in buttons)
        {
            if (btn == null) continue;

            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, OnAnyEnter);
            AddEvent(trigger, EventTriggerType.PointerExit, OnAnyExit);
        }
    }

    void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    void OnAnyEnter()
    {
        hoverCount++;
        UpdateAnimator();
    }

    void OnAnyExit()
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (bgAnimator != null)
            bgAnimator.SetBool(HoverHash, hoverCount > 0);
    }
}