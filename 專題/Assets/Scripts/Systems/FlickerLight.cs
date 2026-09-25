using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 火光搖曳。讓 Light 2D 的亮度自然跳動，像真火。
/// 掛在有 Light 2D 的物件上。
/// </summary>
public class FlickerLight : MonoBehaviour
{
    [SerializeField] Light2D targetLight;
    [Tooltip("最暗亮度")]
    [SerializeField] float minIntensity = 1.2f;
    [Tooltip("最亮亮度")]
    [SerializeField] float maxIntensity = 1.8f;
    [Tooltip("跳動速度")]
    [SerializeField] float flickerSpeed = 8f;

    float seed;

    void Awake()
    {
        if (targetLight == null) targetLight = GetComponent<Light2D>();
        seed = Random.value * 100f;   // 每個火把跳動不同步
    }

    void Update()
    {
        if (targetLight == null) return;

        // Perlin noise 讓亮度自然跳動
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + seed, 0);
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}