using UnityEngine;

public class BloodStain : MonoBehaviour
{
    [SerializeField] float lifetime = 6f;
    [SerializeField] float fadeTime = 1.5f;

    SpriteRenderer sr;
    float timer;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        timer += Time.deltaTime;
        float fadeStart = lifetime - fadeTime;
        if (timer > fadeStart && sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Clamp01(1f - (timer - fadeStart) / fadeTime);
            sr.color = c;
        }
        if (timer >= lifetime) Destroy(gameObject);
    }
}