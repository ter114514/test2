using UnityEngine;

/// <summary>
/// 命中特效。生成後播放（動畫或放大淡出），播完自動銷毀。
/// 掛在命中特效 Prefab 上。
/// </summary>
public class HitEffect : MonoBehaviour
{
    [Tooltip("特效存活時間（播完動畫的長度）")]
    [SerializeField] float lifetime = 0.4f;

    [Tooltip("是否隨機旋轉（讓每次命中特效方向不同）")]
    [SerializeField] bool randomRotation = true;

    void Start()
    {
        // 隨機旋轉，讓連續命中的特效不一樣
        if (randomRotation)
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // 播完自動銷毀
        Destroy(gameObject, lifetime);
    }
}