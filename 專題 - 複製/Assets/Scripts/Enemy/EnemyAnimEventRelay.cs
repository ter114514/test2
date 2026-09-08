using UnityEngine;

/// <summary>
/// 敵人動畫事件中轉。
/// Animation Event 只能呼叫 Animator 所在物件的方法，
/// 此腳本轉發給子物件的 EnemyAttackHitbox。
/// 掛在敵人主物件（Animator 所在物件）上。
/// </summary>
public class EnemyAnimEventRelay : MonoBehaviour
{
    [Tooltip("攻擊判定框（在子物件上）")]
    [SerializeField] EnemyAttackHitbox attackHitbox;

    void Awake()
    {
        // 沒手動指定就自動找子物件的
        if (attackHitbox == null)
            attackHitbox = GetComponentInChildren<EnemyAttackHitbox>(true);
    }

    // ---- Animation Event 呼叫這些，轉發給 Hitbox ----

    public void AnimEvent_EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.EnableHitbox();
    }

    public void AnimEvent_DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.DisableHitbox();
    }
}