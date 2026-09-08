using UnityEngine;

/// <summary>點擊時播放粒子效果。</summary>
public class ClickParticle : MonoBehaviour
{
    [SerializeField] ParticleSystem particles;

    void Awake()
    {
        // 一開始不播
        if (particles != null)
        {
            var main = particles.main;
            particles.Stop();
        }
    }

    /// <summary>按鈕 OnClick 呼叫這個</summary>
    public void PlayParticles()
    {
        if (particles != null)
        {
            particles.Play();
        }
    }
}
