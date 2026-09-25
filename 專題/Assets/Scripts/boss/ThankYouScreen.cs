using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>感謝畫面：按任意鍵回主選單。</summary>
public class ThankYouScreen : MonoBehaviour
{
    void Update()
    {
        // 感謝畫面顯示時，按任意鍵回選單
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.ReturnToMenu();
        }
    }
}