using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    RisingPlatform parentPlatform;

    void Awake()
    {
        // 自動往上層尋找父物件的 RisingPlatform 腳本
        parentPlatform = GetComponentInParent<RisingPlatform>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            parentPlatform.OnPlayerEnter(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            parentPlatform.OnPlayerExit(other.transform);
        }
    }
}