using UnityEngine;

public class MetalRodSpark : MonoBehaviour
{
    public ParticleSystem sparkPrefab;
    public AudioSource sparkAudio;

    public float minImpactVelocity = 0.3f; // how hard rods must hit
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
        // Must hit another rod
        if (!other.GetComponent<MetalRodSpark>()) return;

        // Must be high enough velocity
        if (velocity < minImpactVelocity) return;

        Vector3 hitPos = transform.position;

        // Spawn spark
        if (sparkPrefab != null)
        {
            var ps = Instantiate(sparkPrefab, hitPos, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, 0.6f);
        }

        if (sparkAudio) sparkAudio.Play();

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
}
