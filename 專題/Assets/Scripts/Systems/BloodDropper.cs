using UnityEngine;

public class BloodDropper : MonoBehaviour
{
    [SerializeField] BloodOrb orbPrefab;
    [SerializeField] int orbCount = 5;
    [Tooltip("血珠左右散開的範圍")]
    [SerializeField] float spreadRange = 2f;
    [SerializeField] float spawnHeightOffset = 0.3f;

    [Header("落地判定")]
    [Tooltip("只勾地面所在的 Layer")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckDistance = 5f;

    [Header("特效（可選）")]
    [SerializeField] GameObject spraySprefab;   // 拖入 BloodSpray
    /// <summary>吸血攻擊命中時呼叫這個。</summary>
    public void Drop()
    {
        if (orbPrefab == null) return;

        Vector2 from = (Vector2)transform.position + Vector2.up * spawnHeightOffset;

        if (spraySprefab != null)
            Instantiate(spraySprefab, from, Quaternion.identity);

        for (int i = 0; i < orbCount; i++)
        {
            float x = Random.Range(-spreadRange, spreadRange);   // 完全隨機，更散亂
            Vector2 to = FindGround(from.x + x, from.y);

            var orb = Instantiate(orbPrefab);
            orb.Launch(from, to);
        }
    }

    Vector2 FindGround(float x, float fromY)
    {
        var hit = Physics2D.Raycast(new Vector2(x, fromY), Vector2.down,
                                    groundCheckDistance, groundMask);
        if (hit.collider != null)
            return hit.point + Vector2.up * 0.15f;

        return new Vector2(x, transform.position.y);   // 找不到地面就落在敵人腳邊高度
    }
}