using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 查詢當前按鍵綁定的顯示文字。考慮玩家重綁後的結果。
/// </summary>
public static class BindingDisplay
{
    /// <summary>取得某 Action 當前綁定的按鍵文字（如 "Space"、"J"）</summary>
    public static string GetKey(string actionName, int bindingIndex = 0)
    {
        if (RebindManager.Instance == null) return actionName;

        var action = RebindManager.Instance.InputActions.FindAction(actionName);
        if (action == null) return actionName;

        // 回傳當前綁定的顯示文字（重綁後會是新按鍵）
        return action.GetBindingDisplayString(bindingIndex);
    }
}
