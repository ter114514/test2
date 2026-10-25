using UnityEngine;

/// <summary>
/// 玩家攻擊判定框。命中敵人時造成傷害、命中停頓、鏡頭晃動、特效、音效、吸血值。
/// 每個打擊音效可個別設定音量。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("傷害")]
    [SerializeField] float baseDamage = 20f;
    [SerializeField] float knockbackForce = 6f;

    [Header("命中停頓")]
    [SerializeField] float hitStopDuration = 0.08f;
    [SerializeField] float hitStopTimeScale = 0f;

    [Header("鏡頭晃動")]
    [SerializeField] float shakeDuration = 0.12f;
    [SerializeField] float shakeMagnitude = 0.12f;
    [SerializeField] float heavyShakeMagnitude = 0.25f;

    [Header("命中特效")]
    [SerializeField] GameObject hitEffectPrefab;

    [Header("打擊音效（每個可個別設音量）")]
    [SerializeField] HitSound[] hitSounds;

    [Header("吸血值")]
    [SerializeField] float bloodPerHit = 10f;

    [System.Serializable]
    public struct HitSound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;   // 這個音效的音量
    }

    Collider2D hitbox;
    BloodResource blood;
    AudioSource audioSource;
    float currentMultiplier = 1f;
    readonly System.Collections.Generic.HashSet<Collider2D> hitThisSwing = new();

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
        hitbox.enabled = false;
        blood = GetComponentInParent<BloodResource>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitThisSwing.Contains(other)) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            hitThisSwing.Add(other);

            // 傷害 + 擊退
            float finalDamage = baseDamage * currentMultiplier;
            float facing = Mathf.Sign(transform.lossyScale.x);
            Vector2 knockback = new Vector2(facing, 0.2f).normalized * knockbackForce;
            damageable.TakeDamage(finalDamage, knockback);

            // 命中停頓
            if (HitStopManager.Instance != null)
                HitStopManager.Instance.DoHitStop(hitStopDuration, hitStopTimeScale);

            // 鏡頭晃動
            if (CameraFollow.Instance != null)
            {
                float mag = currentMultiplier >= 1.5f ? heavyShakeMagnitude : shakeMagnitude;
                CameraFollow.Instance.Shake(shakeDuration, mag);
            }

            // 命中特效
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);

            // 打擊音效
            PlayHitSound();

            // 吸血值
            if (blood != null) blood.Gain(bloodPerHit);
        }
    }

    void PlayHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0 || audioSource == null) return;

        // 隨機選一個，用它自己的音量
        var chosen = hitSounds[Random.Range(0, hitSounds.Length)];
        if (chosen.clip != null)
            audioSource.PlayOneShot(chosen.clip, chosen.volume);
    }

    public void EnableHitbox(float multiplier = 1f)
    {
        currentMultiplier = multiplier;
        hitThisSwing.Clear();
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }
}