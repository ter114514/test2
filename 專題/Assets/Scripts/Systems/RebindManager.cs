using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 按鍵重綁管理器。持有共用的 Input Actions 資產，
/// 負責載入/儲存重綁覆蓋，並跨場景保留。
/// PlayerInputHandler 與 RebindButton 都使用這裡的同一份資產。
/// </summary>
public class RebindManager : MonoBehaviour
{
    public static RebindManager Instance { get; private set; }

    [Header("共用資產")]
    [Tooltip("拖入 PlayerControls 這個 Input Actions 資產")]
    [SerializeField] InputActionAsset inputActions;

    public InputActionAsset InputActions => inputActions;

    const string RebindsKey = "rebinds";

    [Header("除錯")]
    [Tooltip("勾選會在啟動時清除所有重綁存檔（排查用，之後取消）")]
    [SerializeField] bool clearRebindsOnStart = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 診斷：清除重綁存檔（排查 Dash 問題用）
        if (clearRebindsOnStart)
        {
            PlayerPrefs.DeleteKey(RebindsKey);
            PlayerPrefs.Save();
            Debug.Log("【RebindManager】已清除重綁存檔（診斷）");
        }

        LoadRebinds();
    }

    // ---- 載入 / 儲存重綁 ----

    /// <summary>從 PlayerPrefs 載入重綁覆蓋，套用到資產</summary>
    public void LoadRebinds()
    {
        if (inputActions == null)
        {
            Debug.LogError("【RebindManager】InputActions 沒設定！");
            return;
        }

        string json = PlayerPrefs.GetString(RebindsKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            inputActions.LoadBindingOverridesFromJson(json);
            Debug.Log("【RebindManager】已載入重綁");
        }
    }

    /// <summary>把當前重綁覆蓋存進 PlayerPrefs</summary>
    public void SaveRebinds()
    {
        if (inputActions == null) return;

        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindsKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>重置所有重綁，恢復資產原始綁定</summary>
    public void ResetAllRebinds()
    {
        if (inputActions == null) return;

        foreach (var map in inputActions.actionMaps)
            map.RemoveAllBindingOverrides();

        PlayerPrefs.DeleteKey(RebindsKey);
        PlayerPrefs.Save();
        Debug.Log("【RebindManager】已重置所有重綁");
    }
}