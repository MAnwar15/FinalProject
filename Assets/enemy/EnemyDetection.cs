using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyDetection : MonoBehaviour
{
    public Transform eye; // origin of raycast (head point)
    public float viewDistance = 12f;
    [Range(0, 180)] public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    // returns true if player seen
    public bool CanSeePlayer(Transform player)
    {
        if (player == null || eye == null) return false;

        Vector3 dir = (player.position - eye.position);
        float d = dir.magnitude;
        if (d > viewDistance) return false;

        float angle = Vector3.Angle(eye.forward, dir.normalized);
        if (angle > viewAngle * 0.5f) return false;

        // Raycast to check obstacles
        if (!Physics.Raycast(eye.position, dir.normalized, out RaycastHit hit, viewDistance, obstacleMask | playerMask))
            return false;

        // If the first hit is the player (or a child collider on player)
        if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
        {
            // player visible
            return true;
        }

        return false;
    }
}
