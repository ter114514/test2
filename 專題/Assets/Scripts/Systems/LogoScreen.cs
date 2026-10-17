using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 開場團隊 Logo。淡入、停留、淡出後進主選單。可按鍵跳過。
/// </summary>
public class LogoScreen : MonoBehaviour
{
    [Header("Logo")]
    [Tooltip("Logo 的 CanvasGroup（控制淡入淡出）")]
    [SerializeField] CanvasGroup logoGroup;

    [Header("時間")]
    [SerializeField] float fadeInTime = 1f;
    [SerializeField] float holdTime = 1.5f;
    [SerializeField] float fadeOutTime = 1f;

    [Header("下一個場景")]
    [SerializeField] string nextScene = "MainMenu";

    [Header("跳過")]
    [Tooltip("是否允許按鍵/點擊跳過")]
    [SerializeField] bool allowSkip = true;

    bool finished;

    void Start()
    {
        if (logoGroup != null) logoGroup.alpha = 0f;   // 一開始透明
        StartCoroutine(LogoRoutine());
    }

    void Update()
    {
        if (!allowSkip || finished) return;

        // 新版 Input System：按任意鍵或滑鼠點擊跳過
        bool anyKey = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool anyClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (anyKey || anyClick)
            GoToNext();
    }

    IEnumerator LogoRoutine()
    {
        // 淡入
        yield return Fade(0f, 1f, fadeInTime);
        // 停留
        yield return new WaitForSeconds(holdTime);
        // 淡出
        yield return Fade(1f, 0f, fadeOutTime);

        GoToNext();
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (logoGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            logoGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        logoGroup.alpha = to;
    }

    void GoToNext()
    {
        if (finished) return;
        finished = true;
        SceneManager.LoadScene(nextScene);
    }
}