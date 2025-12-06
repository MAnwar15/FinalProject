using System.Collections;
using UnityEngine;

public class FuelSphere : MonoBehaviour
{
    [Header("Fuel")]
    public float fuelSeconds = 8f;      // how many seconds this fuel adds to a campfire
    public bool destroyOnIgnite = true;

    [Header("Optional burn effect")]
    public ParticleSystem burnEffectPrefab; // small burn particle to spawn when ignited

    // Called by spark/metalrod when ignition happens
    public void Ignite()
    {
        // burn effect
        if (burnEffectPrefab != null)
        {
            var ps = Instantiate(burnEffectPrefab, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, 2f);
        }

        // find campfire
        SimpleCampfire camp = FindNearestCampfire(5f);

        // ignite fuel NOW → ignite campfire LATER
        if (camp != null)
        {
            camp.AddFuel(fuelSeconds);     // adds burn time but does NOT ignite flame yet
            StartCoroutine(IgniteCampfireDelayed(camp));
        }

        if (destroyOnIgnite)
            Destroy(gameObject);
    }

    IEnumerator IgniteCampfireDelayed(SimpleCampfire camp)
    {
        yield return new WaitForSeconds(3f);   // ← delay before main fire lights
        camp.IgniteInstant();
    }



    SimpleCampfire FindNearestCampfire(float radius)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, radius);
        SimpleCampfire best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var camp = c.GetComponentInParent<SimpleCampfire>();
            if (camp != null)
            {
                float d = Vector3.SqrMagnitude(camp.transform.position - transform.position);
                if (d < bestDist) { best = camp; bestDist = d; }
            }
        }
        return best;
    }
}
