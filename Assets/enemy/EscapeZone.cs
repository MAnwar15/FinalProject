using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EscapeZone : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerStatus>();
        if (player != null && player.IsAlive)
        {
            GameManager.Instance.OnPlayerEscaped();
            Debug.Log("[EscapeZone] Player entered escape zone");
        }
    }
}
