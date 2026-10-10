using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.LowLevel;

/// <summary>
/// 血泉。靠近按 E 回復吸血值，有冷卻時間。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BloodFountain : MonoBehaviour
{
    [Header("互動")]
    [SerializeField] GameObject promptUI;
    [SerializeField] TMP_Text promptText;
    [SerializeField] string readyPrompt = "按 E 汲取血泉";
    [SerializeField] string usedText = "吸血值已回復";
    [SerializeField] string cooldownText = "血泉乾涸中…";
    [SerializeField] float usedTextDuration = 1.5f;

    [Header("回復設定")]
    [SerializeField] float restoreAmount = 20f;
    [Tooltip("冷卻秒數。勾選 oneTimeUse 則忽略")]
    [SerializeField] float cooldown = 10f;
    [SerializeField] bool oneTimeUse = false;

    bool playerInRange;
    bool used;
    float nextReadyTime;
    GameObject player;
    Coroutine textRoutine;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryUse();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        player = other.gameObject;

        if (promptUI != null) promptUI.SetActive(true);
        RefreshPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        player = null;
        if (promptUI != null) promptUI.SetActive(false);

        if (textRoutine != null)
        {
            StopCoroutine(textRoutine);
            textRoutine = null;
        }
    }

    bool IsAvailable => !used && Time.time >= nextReadyTime;

    void TryUse()
    {
        if (!IsAvailable)
        {
            if (promptText != null) promptText.text = cooldownText;
            return;
        }

        if (player == null) return;

        // 先找玩家本體，找不到再找父物件（避免 Collider 在子物件上）
        var blood = player.GetComponentInParent<BloodResource>();
        if (blood == null)
        {
            Debug.LogWarning("BloodFountain：玩家身上找不到 BloodResource");
            return;
        }

        // 已經滿了就不消耗血泉
        if (blood.CurrentBlood >= blood.MaxBlood) return;

        blood.Gain(restoreAmount);

        nextReadyTime = Time.time + cooldown;
        if (oneTimeUse) used = true;

        ShowUsedText();
    }
    void RefreshPrompt()
    {
        if (promptText == null) return;
        promptText.text = IsAvailable ? readyPrompt : cooldownText;
    }

    void ShowUsedText()
    {
        if (promptText == null) return;
        if (textRoutine != null) StopCoroutine(textRoutine);
        textRoutine = StartCoroutine(UsedTextRoutine());
    }

    IEnumerator UsedTextRoutine()
    {
        promptText.text = usedText;
        yield return new WaitForSeconds(usedTextDuration);

        if (playerInRange && promptText != null)
            RefreshPrompt();

        textRoutine = null;
    }
}