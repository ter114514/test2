using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

/// <summary>
/// 浮空教學文字。玩家進入範圍淡入顯示，離開淡出。
/// 文字支援按鍵佔位符 {Jump} {Attack} 等，自動替換成玩家當前綁定的按鍵。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TutorialText : MonoBehaviour
{
    [Header("教學文字")]
    [Tooltip("用 {ActionName} 當佔位符，如「按 {Jump} 跳躍」")]
    [TextArea(2, 4)]
    [SerializeField] string template = "按 {Jump} 跳躍";

    [Header("顯示")]
    [SerializeField] CanvasGroup textGroup;
    [SerializeField] TMP_Text textLabel;   // 顯示文字的 TMP
    [SerializeField] float fadeSpeed = 3f;
    [SerializeField] bool showOnce = false;

    bool playerInRange;
    bool hasShown;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (textGroup != null) textGroup.alpha = 0;
    }

    void Start()
    {
        RefreshText();   // 開場先套用當前按鍵
    }

    void RefreshText()
    {
        if (textLabel == null) return;

        // 把 {ActionName} 替換成當前綁定的按鍵
        string result = Regex.Replace(template, @"\{(\w+)\}", match =>
        {
            string actionName = match.Groups[1].Value;
            return BindingDisplay.GetKey(actionName);
        });

        textLabel.text = result;
    }

    void Update()
    {
        if (textGroup == null) return;

        bool shouldShow = playerInRange && !(showOnce && hasShown);
        float target = shouldShow ? 1f : 0f;
        textGroup.alpha = Mathf.MoveTowards(
            textGroup.alpha, target, fadeSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        RefreshText();   // 進入時刷新（確保是最新綁定）
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (showOnce) hasShown = true;
    }
}