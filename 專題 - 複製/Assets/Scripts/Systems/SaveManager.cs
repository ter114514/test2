using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 存檔管理器（多欄位版）。
/// 管理 3 個存檔欄位、記住當前欄位（跨場景保留）。
/// 讀檔時載入存檔記錄的場景，讓玩家回到存檔當下的那一關。
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SlotCount = 3;

    [Header("設定")]
    [SerializeField] bool useEncryption = false;

    [Header("場景名稱")]
    [Tooltip("預設遊戲場景（舊存檔沒記場景時的退回值）")]
    [SerializeField] string gameSceneName = "Level1";
    [Tooltip("主畫面場景名稱")]
    [SerializeField] string menuSceneName = "MainMenu";

    public int CurrentSlot { get; private set; } = -1;

    public event Action OnSaved;
    public event Action OnLoaded;

    SaveData pendingLoadData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    string GetPath(int slot)
        => Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");

    // ---- 欄位查詢 ----

    public bool SlotHasData(int slot) => File.Exists(GetPath(slot));

    public SaveData PeekSlot(int slot)
    {
        if (!SlotHasData(slot)) return null;
        try
        {
            string json = File.ReadAllText(GetPath(slot));
            if (useEncryption) json = XorObfuscate(json);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"讀取欄位 {slot} 失敗：{e.Message}");
            return null;
        }
    }

    // ---- 新遊戲 ----

    public void StartNewGame(int slot)
    {
        CurrentSlot = slot;
        pendingLoadData = null;
        DefeatedEnemyTracker.Clear();          // 清空已擊敗敵人
        CollectedShardTracker.Clear();         // 清空已撿藥罐碎片
        CollectedMaskShardTracker.Clear();     // 清空已撿血格碎片
        GameSession.Clear();                   // 清空跨關卡暫存
        DeleteSlot(slot);                      // 清掉該欄位舊檔
        SceneManager.LoadScene(gameSceneName);
    }

    // ---- 繼續遊戲 ----

    public bool ContinueGame(int slot)
    {
        var data = PeekSlot(slot);
        if (data == null) return false;

        CurrentSlot = slot;
        pendingLoadData = data;

        string sceneToLoad = string.IsNullOrEmpty(data.currentSceneName)
            ? gameSceneName
            : data.currentSceneName;

        SceneManager.LoadScene(sceneToLoad);
        return true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadData == null) return;

        string expectedScene = string.IsNullOrEmpty(pendingLoadData.currentSceneName)
            ? gameSceneName
            : pendingLoadData.currentSceneName;

        if (scene.name != expectedScene) return;

        DefeatedEnemyTracker.RestoreFrom(pendingLoadData);

        foreach (var s in FindSaveables())
            s.LoadState(pendingLoadData);

        OnLoaded?.Invoke();
        pendingLoadData = null;
        Debug.Log($"已載入欄位 {CurrentSlot + 1} 的存檔（場景：{scene.name}）");
    }

    // ---- 遊戲內存檔（篝火呼叫）----

    public void SaveToCurrentSlot()
    {
        if (CurrentSlot < 0)
        {
            Debug.LogWarning("尚未選擇存檔欄位");
            return;
        }

        var data = new SaveData
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            currentSceneName = SceneManager.GetActiveScene().name
        };

        foreach (var s in FindSaveables())
            s.SaveState(data);

        DefeatedEnemyTracker.WriteTo(data);

        try
        {
            string json = JsonUtility.ToJson(data, true);
            if (useEncryption) json = XorObfuscate(json);
            File.WriteAllText(GetPath(CurrentSlot), json);
            OnSaved?.Invoke();
            Debug.Log($"已存檔至欄位 {CurrentSlot + 1}（場景：{data.currentSceneName}）");
        }
        catch (Exception e)
        {
            Debug.LogError($"存檔失敗：{e.Message}");
        }
    }

    // ---- 其他 ----

    public void DeleteSlot(int slot)
    {
        if (SlotHasData(slot))
        {
            File.Delete(GetPath(slot));
            Debug.Log($"欄位 {slot + 1} 存檔已刪除");
        }
    }

    public void ReturnToMenu()
    {
        CurrentSlot = -1;
        pendingLoadData = null;
        DefeatedEnemyTracker.Clear();
        CollectedShardTracker.Clear();
        CollectedMaskShardTracker.Clear();
        GameSession.Clear();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    ISaveable[] FindSaveables()
    {
        var monos = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        var list = new List<ISaveable>();
        foreach (var m in monos)
            if (m is ISaveable s) list.Add(s);
        return list.ToArray();
    }

    string XorObfuscate(string input)
    {
        const string key = "MyGameKey2026";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
            sb.Append((char)(input[i] ^ key[i % key.Length]));
        return sb.ToString();
    }
}