using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

/// <summary>
/// 篝火存檔點。玩家進入範圍後按 E 存檔、回滿血、補滿血瓶。
/// 按 E 後提示文字變成「已存檔」給予回饋。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SaveCampfire : MonoBehaviour
{
    [Header("互動")]
    [Tooltip("提示 UI 物件（進範圍顯示）")]
    [SerializeField] GameObject promptUI;
    [Tooltip("提示的文字元件（用來切換文字）")]
    [SerializeField] TMP_Text promptText;
    [Tooltip("進範圍時顯示的提示")]
    [SerializeField] string restPrompt = "按 E 休息";
    [Tooltip("按 E 存檔後顯示的文字")]
    [SerializeField] string savedText = "已存檔";
    [Tooltip("「已存檔」顯示幾秒後恢復提示")]
    [SerializeField] float savedTextDuration = 1.5f;

    [Header("篝火動畫（可選）")]
    [SerializeField] Animator campfireAnimator;

    bool playerInRange;
    GameObject player;
    Coroutine savedTextRoutine;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DoRest();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        player = other.gameObject;

        // 顯示提示，文字設為「按 E 休息」
        if (promptUI != null) promptUI.SetActive(true);
        if (promptText != null) promptText.text = restPrompt;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        player = null;
        if (promptUI != null) promptUI.SetActive(false);

        // 離開時停掉「已存檔」的計時（避免殘留）
        if (savedTextRoutine != null)
        {
            StopCoroutine(savedTextRoutine);
            savedTextRoutine = null;
        }
    }

    void DoRest()
    {
        // 點燃篝火（可選）
        if (campfireAnimator != null)
            campfireAnimator.SetBool("IsLit", true);

        // 回滿血
        if (player != null && player.TryGetComponent<PlayerHealthSystem>(out var health))
            health.FullHeal();

        // 補滿血瓶
        if (player != null && player.TryGetComponent<PotionSystem>(out var potion))
            potion.RefillPotions();

        // 存檔
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveToCurrentSlot();

        // 提示文字變成「已存檔」，短暫後恢復
        ShowSavedText();

        Debug.Log("已在篝火休息：回滿血、補滿血瓶、存檔");
    }

    void ShowSavedText()
    {
        if (promptText == null) return;

        if (savedTextRoutine != null)
            StopCoroutine(savedTextRoutine);
        savedTextRoutine = StartCoroutine(SavedTextRoutine());
    }

    IEnumerator SavedTextRoutine()
    {
        promptText.text = savedText;                     // 顯示「已存檔」
        yield return new WaitForSeconds(savedTextDuration);

        // 時間到，若玩家還在範圍內，恢復成「按 E 休息」
        if (playerInRange && promptText != null)
            promptText.text = restPrompt;

        savedTextRoutine = null;
    }
}