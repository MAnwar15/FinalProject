using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    public enum State { Patrol, Suspicious, Investigate, Alert, Chase, ReturnToPatrol }
    public State currentState = State.Patrol;

    [Header("References")]
    public Transform[] patrolPoints;
    public Transform eyes; // set to a child transform at eye height
    public Transform player; // set to the player's head transform (HMD)
    public LayerMask obstacleMask; // used for line-of-sight checks

    [Header("Nav & Patrol")]
    NavMeshAgent agent;
    int patrolIndex = 0;
    public float patrolStopDelay = 1.0f;

    [Header("Vision")]
    public float sightRange = 12f;
    [Range(0, 360)] public float fieldOfView = 110f;
    public float detectionThreshold = 2.0f; // seconds to fully detect player
    private float detectionProgress = 0f;
    public float detectionGain = 1.2f;
    public float detectionLose = 1.0f;

    [Header("Hearing / Investigation")]
    public float investigateStopDistance = 0.6f;
    public float investigateLookDuration = 3f;
    public float suspiciousLookDuration = 1.5f;
    public float loudNoiseAlertThreshold = 0.9f; // intensity above this = immediate alert

    Vector3 investigatePosition;
    Coroutine investigateRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (eyes == null) eyes = transform;
    }

    void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        // Always check vision each frame
        VisionCheck();

        // State machine behavior
        switch (currentState)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Suspicious: /* Suspicious state mostly controlled by noise handling */ break;
            case State.Investigate: InvestigateUpdate(); break;
            case State.Alert:
            case State.Chase: ChaseUpdate(); break;
        }
    }

    #region Vision
    void VisionCheck()
    {
        if (player == null) return;

        Vector3 dir = (player.position - eyes.position);
        float dist = dir.magnitude;
        if (dist > sightRange)
        {
            // out of range: decay detection
            detectionProgress = Mathf.Max(0f, detectionProgress - detectionLose * Time.deltaTime);
            return;
        }

        float angle = Vector3.Angle(eyes.forward, dir.normalized);
        if (angle > fieldOfView * 0.5f)
        {
            detectionProgress = Mathf.Max(0f, detectionProgress - detectionLose * Time.deltaTime);
            return;
        }

        // line-of-sight
        RaycastHit hit;
        if (Physics.Raycast(eyes.position, dir.normalized, out hit, sightRange, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                // visible
                detectionProgress += detectionGain * Time.deltaTime;
                if (detectionProgress >= detectionThreshold)
                {
                    GoAlert();
                }
                return;
            }
            else
            {
                // occluded
                detectionProgress = Mathf.Max(0f, detectionProgress - detectionLose * Time.deltaTime);
                return;
            }
        }
    }
    #endregion

    #region Patrol
    bool waitingAtPoint = false;

    void PatrolUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f && !waitingAtPoint)
        {
            StartCoroutine(AdvancePatrolPoint());
        }
    }

    IEnumerator AdvancePatrolPoint()
    {
        waitingAtPoint = true;
        yield return new WaitForSeconds(patrolStopDelay);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        waitingAtPoint = false;
    }

    #endregion

    #region Investigate / Suspicious / Chase
    public void OnNoiseHeard(Vector3 noisePos, float intensity, float radius)
    {
        // louder noise => higher chance to alert
        if (currentState == State.Alert || currentState == State.Chase)
            return;

        // stop any investigate coroutine and set new target
        investigatePosition = noisePos;

        if (intensity >= loudNoiseAlertThreshold)
        {
            // immediate alert & chase toward position (or toward player if known)
            currentState = State.Alert;
            agent.SetDestination(investigatePosition);
        }
        else
        {
            // go investigate
            currentState = State.Investigate;
            agent.SetDestination(investigatePosition);

            if (investigateRoutine != null)
                StopCoroutine(investigateRoutine);
            investigateRoutine = StartCoroutine(InvestigateCoroutine());
        }
    }

    IEnumerator InvestigateCoroutine()
    {
        // wait until agent reaches location (or time-out)
        float timeout = 10f;
        float timer = 0f;

        while ((agent.pathPending || agent.remainingDistance > investigateStopDistance) && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // look around
        float lookTimer = 0f;
        while (lookTimer < investigateLookDuration)
        {
            LookAround();
            lookTimer += Time.deltaTime;
            yield return null;
        }

        // if didn't find anything, return to patrol
        ReturnToPatrol();
    }

    void InvestigateUpdate()
    {
        // optionally rotate toward last heard position as you move
        Vector3 lookDir = (investigatePosition - transform.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 4f);
        }
    }

    void LookAround()
    {
        // simple back-and-forth look
        float turnSpeed = 60f;
        transform.Rotate(0, Mathf.Sin(Time.time * 2f) * turnSpeed * Time.deltaTime, 0);
    }

        Vector3 lastKnownPlayerPos;
    float lostSightTimer = 0f;
    public float loseSightTime = 4f; // seconds before giving up chase
    public float maxChaseDistance = 25f; // AI stops if player gets this far away

    void ChaseUpdate()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Always update last seen position when visible
        if (CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            lostSightTimer = 0f;
            agent.SetDestination(player.position);
        }
        else
        {
            lostSightTimer += Time.deltaTime;

            // If lost sight for too long or player too far -> give up
            if (lostSightTimer > loseSightTime || distance > maxChaseDistance)
            {
                // Go investigate last known position
                currentState = State.Investigate;
                agent.SetDestination(lastKnownPlayerPos);

                if (investigateRoutine != null)
                    StopCoroutine(investigateRoutine);
                investigateRoutine = StartCoroutine(InvestigateCoroutine());
                return;
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - eyes.position);
        float dist = dir.magnitude;
        if (dist > sightRange) return false;

        float angle = Vector3.Angle(eyes.forward, dir.normalized);
        if (angle > fieldOfView * 0.5f) return false;

        if (Physics.Raycast(eyes.position, dir.normalized, out RaycastHit hit, sightRange, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }
        return false;
    }

    void GoAlert()
    {
        currentState = State.Chase;
        lostSightTimer = 0f;
        if (player != null)
        {
            lastKnownPlayerPos = player.position;
            agent.SetDestination(player.position);
        }
    }


    void ReturnToPatrol()
    {
        currentState = State.Patrol;
        detectionProgress = 0f;
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }
    #endregion

    #region Debugging Gizmos
    void OnDrawGizmosSelected()
    {
        if (eyes != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyes.position, sightRange);

            // fov lines
            Vector3 forward = eyes.forward;
            Quaternion leftRot = Quaternion.Euler(0, -fieldOfView * 0.5f, 0);
            Quaternion rightRot = Quaternion.Euler(0, fieldOfView * 0.5f, 0);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(eyes.position, eyes.position + leftRot * forward * sightRange);
            Gizmos.DrawLine(eyes.position, eyes.position + rightRot * forward * sightRange);
        }
    }
    #endregion
}
