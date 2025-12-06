using UnityEngine;

public class MetalRodSpark : MonoBehaviour
{
    public AudioClip sparkClip;   // the sound clip for metallic spark
    public AudioSource audioSource; // the AudioSource that will play sparkClip
    public float minImpactVelocity = 0.3f;
    public float igniteRadius = 1.2f;
    public float igniteChance = 0.9f;


    private Vector3 lastPos;
    private float velocity;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        velocity = (transform.position - lastPos).magnitude / Time.deltaTime;
        lastPos = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<MetalRodSpark>()) return;
        if (velocity < minImpactVelocity) return;

        Vector3 hitPos = transform.position;

        // Spawn spark using pool
        if (SparkPool.Instance != null)
        {
            SparkPool.Instance.GetSpark(hitPos);
        }

        // Play spark sound
        if (audioSource != null && sparkClip != null)
        {
            audioSource.PlayOneShot(sparkClip, 0.7f);
        }

        TryIgniteNearbyFuel(hitPos);
    }


    void TryIgniteNearbyFuel(Vector3 pos)
    {
        Collider[] cols = Physics.OverlapSphere(pos, igniteRadius);

        foreach (var c in cols)
        {
            FuelSphere f = c.GetComponent<FuelSphere>();
            if (f != null && Random.value <= igniteChance)
            {
                f.Ignite();
                return;
            }
        }
    }

    // Returns true if any SimpleCampfire within radius has its flame particles playing or light enabled
    bool IsActiveCampfireNearby(Vector3 pos, float radius)
    {
        Collider[] cols = Physics.OverlapSphere(pos, radius);
        foreach (var c in cols)
        {
            var camp = c.GetComponentInParent<SimpleCampfire>();
            if (camp != null)
            {
                // Check particle system or light state safely (null checks)
                var flame = camp.flameParticles;
                var light = camp.fireLight;

                bool particlesOn = flame != null && flame.isPlaying;
                bool lightOn = light != null && light.enabled;

                if (particlesOn || lightOn)
                    return true;
            }
        }
        return false;
    }
}
