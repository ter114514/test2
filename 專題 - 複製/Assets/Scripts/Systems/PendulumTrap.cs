using UnityEngine;

/// <summary>
/// 鐘擺陷阱。繞著頂部支點左右來回擺動。
/// 掛在支點物件上（支點在天花板，擺錘是子物件往下垂）。
/// </summary>
public class PendulumTrap : MonoBehaviour
{
    [Header("擺動設定")]
    [Tooltip("最大擺動角度（從中間往兩邊各擺這麼多度）")]
    [SerializeField] float swingAngle = 60f;
    [Tooltip("擺動速度")]
    [SerializeField] float swingSpeed = 2f;
    [Tooltip("起始相位（讓多個擺錘不同步，可設不同值）")]
    [SerializeField] float startPhase = 0f;

    void Update()
    {
        // 用 sin 波做來回擺動
        float angle = swingAngle * Mathf.Sin(Time.time * swingSpeed + startPhase);

        // 繞 Z 軸旋轉（2D 的旋轉）
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}