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
    [Tooltip("玩家離開後，延遲多久開始下降（避免起跳時瞬間抽動）")]
    public float returnDelay = 0.2f;

    Vector3 startPos;
    Vector3 topPos;
    bool isPlayerOnPlatform = false;
    float delayTimer = 0f;

    void Awake()
    {
        startPos = transform.position;
        topPos = startPos + new Vector3(0, riseDistance, 0);
    }

    // 玩家進入頂部感應區
    public void OnPlayerEnter(Transform player)
    {
        isPlayerOnPlatform = true;
        delayTimer = riseDelay;

        // 將玩家綁定為子物件，讓玩家平滑跟隨移動不滑動
        player.SetParent(transform);
    }

    // 玩家離開頂部感應區（跳開或走開）
    public void OnPlayerExit(Transform player)
    {
        isPlayerOnPlatform = false;
        delayTimer = returnDelay;

        if (player.parent == transform)
        {
            player.SetParent(null);
        }
    }

    void Update()
    {
        // 倒數延遲計時
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        if (isPlayerOnPlatform)
        {
            // 玩家在上面：往頂端移動
            if (Vector3.Distance(transform.position, topPos) > 0.001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, topPos, riseSpeed * Time.deltaTime);
            }
        }
        else
        {
            // 玩家離開：往原位下降
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