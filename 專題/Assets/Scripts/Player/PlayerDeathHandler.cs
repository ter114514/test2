using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 玩家死亡處理。訂閱 OnDeath，播死亡畫面後回到最近篝火重生。
/// 掛在玩家身上。
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("死亡設定")]
    [Tooltip("死亡畫面 UI（YOU DIED），留空則不顯示")]
    [SerializeField] GameObject deathScreenUI;
    [Tooltip("死亡後停留幾秒才重生")]
    [SerializeField] float deathDelay = 2.5f;
    [Tooltip("重生時重置已擊敗的敵人")]
    [SerializeField] bool respawnEnemies = true;

    PlayerHealthSystem health;
    PlayerMovement movement;
    bool isDead;

    void Awake()
    {
        health = GetComponent<PlayerHealthSystem>();
        movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        if (health != null) health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (health != null) health.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // 鎖玩家操作
        if (movement != null) movement.enabled = false;

        // 顯示死亡畫面
        if (deathScreenUI != null)
            deathScreenUI.SetActive(true);

        // 停留（讓死亡動畫、畫面播放）
        yield return new WaitForSecondsRealtime(deathDelay);

        // 隱藏死亡畫面
        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        Respawn();
    }

    void Respawn()
    {
        // 重置敵人
        if (respawnEnemies)
            DefeatedEnemyTracker.Clear();

        // 沒有重生點 → 重載當前場景
        if (!RespawnPoint.HasRespawn)
        {
            Debug.LogWarning("沒有重生點（未在篝火休息過），重載當前場景");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // 重生點在當前場景 → 直接移動
        if (RespawnPoint.SceneName == SceneManager.GetActiveScene().name)
        {
            transform.position = RespawnPoint.Position;
            RecoverPlayer();
        }
        else
        {
            // 重生點在別場景 → 載入該場景（需配合場景載入後移動，較複雜）
            SceneManager.LoadScene(RespawnPoint.SceneName);
            // 注意：跨場景重生需額外處理場景載入後的定位
        }
    }

    void RecoverPlayer()
    {
        // 恢復血、血瓶
        if (health != null) health.ResetHealth();
        if (TryGetComponent<PotionSystem>(out var potion))
            potion.RefillPotions();

        // 歸零速度
        if (TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = Vector2.zero;

        // 解鎖操作
        if (movement != null) movement.enabled = true;

        isDead = false;
    }
}