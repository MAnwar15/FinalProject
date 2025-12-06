using System.Collections.Generic;
using UnityEngine;

public class SparkPool : MonoBehaviour
{
    public static SparkPool Instance;

    public ParticleSystem sparkPrefab;
    public int poolSize = 20;

    private List<ParticleSystem> pool = new List<ParticleSystem>();

    void Awake()
    {
        Instance = this;

        // Create pool
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem ps = Instantiate(sparkPrefab, transform);
            ps.gameObject.SetActive(false);
            pool.Add(ps);
        }
    }

    // Get an inactive spark from the pool
    public ParticleSystem GetSpark(Vector3 position)
    {
        foreach (var ps in pool)
        {
            if (!ps.gameObject.activeInHierarchy)
            {
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();
                return ps;
            }
        }

        // If all are active, optionally reuse first one
        ParticleSystem first = pool[0];
        first.transform.position = position;
        first.gameObject.SetActive(true);
        first.Play();
        return first;
    }
}
