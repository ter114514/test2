using UnityEngine;
using System.Collections;

/// <summary>
/// Boss 死亡結局。Boss 死亡後播死亡動畫，再顯示「感謝遊玩」畫面。
/// 掛在 Boss 身上。
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class BossDeathEnding : MonoBehaviour
{
    [Header("結束畫面")]
    [Tooltip("感謝遊玩的 UI（預設隱藏）")]
    [SerializeField] GameObject thankYouScreen;
    [Tooltip("Boss 死亡動畫播多久後顯示感謝畫面")]
    [SerializeField] float deathAnimDuration = 3f;

    [Header("結束後（可選）")]
    [Tooltip("顯示感謝畫面後，多久自動回主選單（0 = 不自動）")]
    [SerializeField] float returnToMenuDelay = 0f;

    EnemyHealth health;
    bool triggered;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (thankYouScreen != null) thankYouScreen.SetActive(false);
    }

    void OnEnable()
    {
        if (health != null) health.OnDeath += HandleBossDeath;
    }

    void OnDisable()
    {
        if (health != null) health.OnDeath -= HandleBossDeath;
    }

    void HandleBossDeath()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(EndingRoutine());
    }

    IEnumerator EndingRoutine()
    {
        // 等 Boss 死亡動畫播完
        yield return new WaitForSeconds(deathAnimDuration);

        // 顯示感謝畫面
        if (thankYouScreen != null)
            thankYouScreen.SetActive(true);

        // 可選：延遲後回主選單
        if (returnToMenuDelay > 0)
        {
            yield return new WaitForSeconds(returnToMenuDelay);
            if (SaveManager.Instance != null)
                SaveManager.Instance.ReturnToMenu();
        }
    }
}