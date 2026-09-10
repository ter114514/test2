using UnityEngine;

public class RisingPlatform : MonoBehaviour
{
    [Header("高度設定")]
    [Tooltip("上升的高度距離 (正值向上)")]
    public float riseDistance = 5f;

    [Header("速度設定")]
    [Tooltip("玩家踩著時的上升速度")]
    public float riseSpeed = 3f;
    [Tooltip("玩家離開時的下降復位速度")]
    public float returnSpeed = 2.5f;

    [Header("反應延遲 (秒)")]
    [Tooltip("玩家踩上後，延遲多久開始上升")]
    public float riseDelay = 0.1f;
    [Tooltip("玩家離開後，延遲多久開始下降（避免原地小跳瞬間抽動）")]
    public float returnDelay = 0.2f;

    Vector3 startPos;
    Vector3 topPos;
    bool isPlayerOnPlatform = false;
    float delayTimer = 0f;
    Transform originalPlayerParent;

    void Awake()
    {
        startPos = transform.position;
        topPos = startPos + new Vector3(0, riseDistance, 0);
    }

    // 玩家踩入感應區
    public void OnPlayerEnter(Transform player)
    {
        isPlayerOnPlatform = true;
        delayTimer = riseDelay;

        // 記錄玩家原本的層級，並設為平台子物件
        originalPlayerParent = player.parent;
        player.SetParent(transform);
    }

    // 玩家離開感應區（跳開或走下平台）
    public void OnPlayerExit(Transform player)
    {
        isPlayerOnPlatform = false;
        delayTimer = returnDelay;

        // 還原玩家原本的層級
        if (player.parent == transform)
        {
            player.SetParent(originalPlayerParent);
        }
    }

    void Update()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        if (isPlayerOnPlatform)
        {
            // 踩著時往頂部升
            if (Vector3.Distance(transform.position, topPos) > 0.001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, topPos, riseSpeed * Time.deltaTime);
            }
        }
        else
        {
            // 離開時回底部降
            if (Vector3.Distance(transform.position, startPos) > 0.001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPos, returnSpeed * Time.deltaTime);
            }
        }
    }

    void OnDisable()
    {
        isPlayerOnPlatform = false;
    }
}