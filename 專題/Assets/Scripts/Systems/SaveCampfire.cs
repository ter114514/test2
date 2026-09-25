using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// 篝火存檔點。按 E 存檔、回滿血、補滿血瓶、設為重生點。
/// 提示文字：進範圍「按 E 休息」，按 E 後切換「已存檔」。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SaveCampfire : MonoBehaviour
{
    [Header("互動")]
    [SerializeField] GameObject promptUI;
    [Tooltip("提示的文字元件（用來切換文字）")]
    [SerializeField] TMP_Text promptText;
    [Tooltip("進範圍顯示")]
    [SerializeField] string restPrompt = "按 E 休息";
    [Tooltip("按 E 存檔後顯示")]
    [SerializeField] string savedText = "已存檔";
    [Tooltip("「已存檔」顯示幾秒後恢復")]
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
            DoRest();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        player = other.gameObject;

        if (promptUI != null) promptUI.SetActive(true);
        if (promptText != null) promptText.text = restPrompt;   // 顯示「按 E 休息」
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        player = null;
        if (promptUI != null) promptUI.SetActive(false);

        // 離開時停掉「已存檔」計時
        if (savedTextRoutine != null)
        {
            StopCoroutine(savedTextRoutine);
            savedTextRoutine = null;
        }
    }

    void DoRest()
    {
        if (campfireAnimator != null)
            campfireAnimator.SetBool("IsLit", true);

        if (player != null && player.TryGetComponent<PlayerHealthSystem>(out var health))
            health.FullHeal();

        if (player != null && player.TryGetComponent<PotionSystem>(out var potion))
            potion.RefillPotions();

        // 設為重生點
        RespawnPoint.SetRespawn(SceneManager.GetActiveScene().name, transform.position);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveToCurrentSlot();

        // 提示文字切換「已存檔」
        ShowSavedText();

        Debug.Log("已在篝火休息：回滿血、補滿血瓶、設為重生點、存檔");
    }

    void ShowSavedText()
    {
        if (promptText == null) return;
        if (savedTextRoutine != null) StopCoroutine(savedTextRoutine);
        savedTextRoutine = StartCoroutine(SavedTextRoutine());
    }

    IEnumerator SavedTextRoutine()
    {
        promptText.text = savedText;                     // 「已存檔」
        yield return new WaitForSeconds(savedTextDuration);

        if (playerInRange && promptText != null)
            promptText.text = restPrompt;                // 恢復「按 E 休息」

        savedTextRoutine = null;
    }
}