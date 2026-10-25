using UnityEngine;
using System.Collections;

/// <summary>
/// 命中停頓管理器。攻擊命中時短暫凍結遊戲，強化打擊感。
/// 全域單例，任何攻擊命中時呼叫。
/// </summary>
public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    Coroutine currentRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>觸發命中停頓</summary>
    /// <param name="duration">凍結時間（真實秒數）</param>
    /// <param name="timeScale">凍結時的時間比例（0=完全靜止）</param>
    public void DoHitStop(float duration = 0.08f, float timeScale = 0f)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(HitStopRoutine(duration, timeScale));
    }

    IEnumerator HitStopRoutine(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);   // 真實時間（不受 timeScale 影響）
        Time.timeScale = 1f;
        currentRoutine = null;
    }
}