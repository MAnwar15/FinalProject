using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] patrolPoints;
    private int currentPoint = 0;

    public Transform player;
    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float fieldOfView = 100f; // vision cone
    public LayerMask obstacleMask;

    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer() && distanceToPlayer <= detectionRange)
        {
            // Start chasing
            isChasing = true;
        }
        else if (distanceToPlayer > chaseRange)
        {
            // Stop chasing
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPoint].position;
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView / 2f)
        {
            // Check if there’s a wall between enemy and player
            if (!Physics.Linecast(transform.position, player.position, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }
}
