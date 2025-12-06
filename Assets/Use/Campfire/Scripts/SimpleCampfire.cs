using UnityEngine;

public class SimpleCampfire : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem flameParticles;
    public Light fireLight;
    public AudioSource crackleAudio;

    [Header("Burning")]
    public float burnSecondsPerFuel = 10f;
    private float burnTimer = 0f;

    void Start()
    {
        SetActiveFire(false);
    }

    void Update()
    {
        if (burnTimer > 0f)
        {
            burnTimer -= Time.deltaTime;
            if (!flameParticles.isPlaying) SetActiveFire(true);

            // small light flicker
            if (fireLight) fireLight.intensity = 1f + Mathf.PerlinNoise(Time.time * 5f, 0f) * 0.5f;
        }
        else
        {
            if (flameParticles != null && flameParticles.isPlaying)
                SetActiveFire(false);
        }
    }

    // Called when a fuel item adds fuel
    public void AddFuel(float seconds)
    {
        burnTimer += seconds;
        if (burnTimer > 0f) SetActiveFire(true);
    }

    public void IgniteInstant()
    {
        // ensure at least a minimum
        if (burnTimer <= 0f) burnTimer = burnSecondsPerFuel;
        SetActiveFire(true);
    }

    public void Extinguish()
    {
        burnTimer = 0f;
        SetActiveFire(false);
    }

    private void SetActiveFire(bool on)
    {
        if (flameParticles != null)
        {
            if (on)
            {
                if (!flameParticles.isPlaying) flameParticles.Play();
                if (crackleAudio != null && !crackleAudio.isPlaying) crackleAudio.Play();
                if (fireLight != null) fireLight.enabled = true;
            }
            else
            {
                flameParticles.Stop();
                if (crackleAudio != null) crackleAudio.Stop();
                if (fireLight != null) fireLight.enabled = false;
            }
        }
    }
}
