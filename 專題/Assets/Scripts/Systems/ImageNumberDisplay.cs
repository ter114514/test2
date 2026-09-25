using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用圖片數字顯示數值（0-9 各一張圖）。
/// 依數值換每個位數 Image 的 sprite。
/// </summary>
public class ImageNumberDisplay : MonoBehaviour
{
    [Header("數字圖（索引 0~9 對應數字 0~9）")]
    [SerializeField] Sprite[] digitSprites;

    [Header("位數 Image（由高位到低位，如百十個）")]
    [SerializeField] Image[] digitImages;

    [Header("選項")]
    [Tooltip("隱藏前導零（75 不顯示成 075）")]
    [SerializeField] bool hideLeadingZeros = true;

    /// <summary>顯示數值</summary>
    public void SetNumber(int value)
    {
        value = Mathf.Clamp(value, 0, 999);

        int[] digits = new int[digitImages.Length];
        int temp = value;
        for (int i = digitImages.Length - 1; i >= 0; i--)
        {
            digits[i] = temp % 10;
            temp /= 10;
        }

        bool leadingZero = hideLeadingZeros;

        for (int i = 0; i < digitImages.Length; i++)
        {
            if (digitImages[i] == null) continue;

            int d = digits[i];

            // 前導零隱藏（最後一位保留，至少顯示 0）
            if (leadingZero && d == 0 && i < digitImages.Length - 1)
            {
                digitImages[i].gameObject.SetActive(false);
            }
            else
            {
                leadingZero = false;
                digitImages[i].gameObject.SetActive(true);
                if (d >= 0 && d < digitSprites.Length)
                    digitImages[i].sprite = digitSprites[d];
            }
        }
    }
}