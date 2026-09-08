using UnityEngine;

/// <summary>
/// 攝影機邊界區域。玩家進入時，把鏡頭邊界改成此區域的範圍。
/// 掛在含 Trigger Collider 的區域物件上。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CameraBoundsZone : MonoBehaviour
{
    [Header("此區域的鏡頭邊界")]
    [SerializeField] float minX = -10f;
    [SerializeField] float maxX = 10f;
    [SerializeField] float minY = -5f;
    [SerializeField] float maxY = 5f;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
            cam.SetBounds(minX, maxX, minY, maxY);
    }

    void OnDrawGizmos()
    {
        // 畫出這個區域的鏡頭邊界（綠色）
        Gizmos.color = Color.green;
        Vector3 tl = new Vector3(minX, maxY, 0);
        Vector3 tr = new Vector3(maxX, maxY, 0);
        Vector3 bl = new Vector3(minX, minY, 0);
        Vector3 br = new Vector3(maxX, minY, 0);
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
}