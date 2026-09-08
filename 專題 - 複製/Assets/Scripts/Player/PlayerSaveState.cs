using UnityEngine;

/// <summary>
/// 玩家位置存檔。實作 ISaveable，存讀玩家的位置。
/// </summary>
public class PlayerSaveState : MonoBehaviour, ISaveable
{
    public void SaveState(SaveData data)
    {
        data.playerPosX = transform.position.x;
        data.playerPosY = transform.position.y;
    }

    public void LoadState(SaveData data)
    {
        Vector3 pos = transform.position;
        pos.x = data.playerPosX;
        pos.y = data.playerPosY;
        transform.position = pos;

        // 歸零速度，避免讀檔後帶著慣性
        if (TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = Vector2.zero;
    }
}