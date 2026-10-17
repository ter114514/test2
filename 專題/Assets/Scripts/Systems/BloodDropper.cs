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

    /// <summary>吸血攻擊命中時呼叫這個。</summary>
    public void Drop()
    {
        if (orbPrefab == null) return;

        Vector2 from = (Vector2)transform.position + Vector2.up * spawnHeightOffset;

        for (int i = 0; i < orbCount; i++)
        {
            float ratio = orbCount == 1 ? 0.5f : i / (float)(orbCount - 1);
            float x = Mathf.Lerp(-spreadRange, spreadRange, ratio) + Random.Range(-0.2f, 0.2f);

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