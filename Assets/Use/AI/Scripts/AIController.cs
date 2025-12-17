using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class AIController : MonoBehaviour
{
    public enum State { Patrol, Suspicious, Investigate, Chase, ReturnToPatrol }
    public State currentState = State.Patrol;

    [Header("References")]
    public Transform[] patrolPoints;
    public Transform eyes;
    public Transform player;
    public LayerMask obstacleMask;

    [Header("Player Settings")]
    public PlayerHelmet helmet; // reference to helmet script
    [Header("Nav & Patrol")]
    private NavMeshAgent agent;
    private int patrolIndex = 0;
    public float patrolStopDelay = 1.0f;
    private bool waitingAtPoint = false;

    [Header("Game Over")]
    public float catchDistance = 1.2f;
    public Canvas gameOverCanvas;
    public AudioClip gameOverMusic;
    public float restartDelay = 4f;

    private bool gameOverTriggered = false;

    [Header("Vision")]
    public float sightRange = 12f;
    [Range(0, 360)] public float fieldOfView = 110f;
    public float visionCheckRate = 0.25f;
    private float visionCheckTimer = 0f;
    private static RaycastHit[] raycastBuffer = new RaycastHit[1]; // avoids GC

    public float detectionThreshold = 2f;
    public float detectionGain = 1.2f;
    public float detectionLose = 1f;
    private float detectionProgress = 0f;

    [Header("Investigate")]
    public float investigateStopDistance = 0.6f;
    public float investigateLookDuration = 3f;
    public float suspiciousLookDuration = 1.5f;
    public float suspiciousDelay = 3f;
    private float suspiciousTimer = 0f;

    [Header("Chase")]
    public float loseSightTime = 4f;
    public float maxChaseDistance = 25f;
    private float lostSightTimer = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip suspiciousClip;
    public AudioClip investigateClip;
    public AudioClip chaseClip;
    private bool playedStateSound = false;

    private Coroutine investigateRoutine;
    private Vector3 lastKnownPlayerPos;
    private Vector3 lastTargetPosition;
    private State previousState;

    private static int agentCount = 0;
    private int agentIndex;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (eyes == null) eyes = transform;
        agent.updateRotation = true; // agent handles rotation
        agent.autoRepath = false; // manual destination updates

        audioSource ??= GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        agentIndex = agentCount++;
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);

        previousState = currentState;
    }

    void Update()
    {
        if (player == null) return;

        if (!gameOverTriggered && Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            TriggerGameOver();
            return;
        }


        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Distance-based LOD: skip expensive updates if far
        if (distToPlayer > sightRange * 2f) return;

        // Staggered vision checks
        visionCheckTimer += Time.deltaTime;
        if (visionCheckTimer >= visionCheckRate * (agentIndex + 1))
        {
            visionCheckTimer = 0f;
            VisionCheck();
        }

        // State change detection
        if (currentState != previousState)
        {
            OnStateChanged(currentState);
            previousState = currentState;
        }

        // Main state machine
        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate();
                if (CanSeePlayer()) EnterSuspicious();
                break;

            case State.Suspicious:
                SuspiciousUpdate();
                break;

            case State.Investigate:
                InvestigateUpdate();
                break;

            case State.Chase:
                ChaseUpdate();
                break;

            case State.ReturnToPatrol:
                ReturnToPatrol();
                break;
        }
    }

    // ---------------- STATE EVENTS ----------------
    void OnStateChanged(State st)
    {
        playedStateSound = false;

        switch (st)
        {
            case State.Suspicious: PlayOnce(suspiciousClip); break;
            case State.Investigate: PlayOnce(investigateClip); break;
            case State.Chase: PlayOnce(chaseClip); break;
        }
    }

    void PlayOnce(AudioClip c)
    {
        if (playedStateSound || c == null) return;
        audioSource.clip = c;
        audioSource.Play();
        playedStateSound = true;
    }

    // ---------------- PATROL ----------------
    void PatrolUpdate()
    {
        ApplyAgentRotation();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f && !waitingAtPoint)
            StartCoroutine(AdvancePatrolPoint());
    }

    IEnumerator AdvancePatrolPoint()
    {
        waitingAtPoint = true;
        yield return new WaitForSeconds(patrolStopDelay);

        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[patrolIndex].position);

        waitingAtPoint = false;
    }

    // ---------------- SUSPICIOUS ----------------
    void EnterSuspicious()
    {
        currentState = State.Suspicious;
        suspiciousTimer = suspiciousDelay;

        //stop movement
        agent.isStopped = true;
        agent.ResetPath();
    }


    void SuspiciousUpdate()
    {
        if (player == null) return;

        // Look at player
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 4f
            );
        }

        if (CanSeePlayer())
        {
            suspiciousTimer -= Time.deltaTime;
            if (suspiciousTimer <= 0f)
                currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }
    }


    // ---------------- VISION ----------------
    void VisionCheck()
    {
        if (player == null) return;

        if (CanSeePlayer())
        {
            detectionProgress += detectionGain * visionCheckRate;
            if (detectionProgress >= detectionThreshold)
                GoAlert();
        }
        else
        {
            detectionProgress -= detectionLose * visionCheckRate;
            detectionProgress = Mathf.Max(0f, detectionProgress);
        }
    }

    bool CanSeePlayer()
{
    if (player == null || eyes == null) return false;

    // --- NEW CHECK HERE ---
    if (helmet != null && helmet.isInvisible)
        return false;
    // -----------------------

    Vector3 dir = player.position - eyes.position;
    float dist = dir.magnitude;

    if (dist > sightRange) return false;
    if (Vector3.Angle(eyes.forward, dir) > fieldOfView * 0.5f) return false;

    if (Physics.RaycastNonAlloc(eyes.position, dir.normalized, raycastBuffer, dist, obstacleMask) > 0)
    {
        if (raycastBuffer[0].transform != player) return false;
    }

    return true;
}


    // ---------------- INVESTIGATE ----------------
    IEnumerator InvestigateCoroutine()
    {
        float timer = 0f;
        float timeout = 10f;

        while ((agent.pathPending || agent.remainingDistance > investigateStopDistance) && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        float lookTimer = 0f;
        while (lookTimer < investigateLookDuration)
        {
            LookAround();
            lookTimer += Time.deltaTime;
            yield return null;
        }

        currentState = State.ReturnToPatrol;
    }

    void InvestigateUpdate()
    {
        Vector3 toPoint = lastKnownPlayerPos - transform.position;
        toPoint.y = 0f;

        if (toPoint.sqrMagnitude > 0.01f && Time.frameCount % 3 == 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPoint), Time.deltaTime * 4f);
        }
    }

    void LookAround()
    {
        transform.Rotate(0, Mathf.Sin(Time.time * 2f) * 60f * Time.deltaTime, 0);
    }

    // ---------------- GameOver ----------------

    void TriggerGameOver()
    {
        gameOverTriggered = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        StopAllCoroutines();

        if (gameOverCanvas != null)
            gameOverCanvas.gameObject.SetActive(true);

        if (audioSource != null && gameOverMusic != null)
        {
            audioSource.Stop();
            audioSource.spatialBlend = 0f;
            audioSource.clip = gameOverMusic;
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.Play();
        }

        StartCoroutine(RestartSceneAfterDelay());
    }


    IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    // ---------------- CHASE ----------------
    void ChaseUpdate()
    {
        if (player == null) return;

        bool seesPlayer = CanSeePlayer();
        float dist = Vector3.Distance(transform.position, player.position);

        if (seesPlayer)
        {
            lastKnownPlayerPos = player.position;
            lostSightTimer = 0f;

            if ((player.position - lastTargetPosition).sqrMagnitude > 0.5f)
            {
                agent.SetDestination(player.position);
                lastTargetPosition = player.position;
            }

            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f && Time.frameCount % 2 == 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
            }
        }
        else
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= loseSightTime || dist > maxChaseDistance)
            {
                currentState = State.Investigate;
                agent.SetDestination(lastKnownPlayerPos);

                if (investigateRoutine != null) StopCoroutine(investigateRoutine);
                investigateRoutine = StartCoroutine(InvestigateCoroutine());
            }
        }
    }

    void GoAlert()
    {
        currentState = State.Chase;
        lastKnownPlayerPos = player.position;
        agent.SetDestination(player.position);
    }

    // ---------------- RETURN ----------------
    void ReturnToPatrol()
    {
        detectionProgress = 0f;
        currentState = State.Patrol;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    // ---------------- UTILITIES ----------------
    void ApplyAgentRotation()
    {
        Vector3 vel = agent.desiredVelocity;
        vel.y = 0;
        if (vel.sqrMagnitude > 0.01f && Time.frameCount % 2 == 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vel), Time.deltaTime * 6f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, sightRange);

        Gizmos.color = Color.cyan;
        Quaternion left = Quaternion.Euler(0, -fieldOfView * 0.5f, 0);
        Quaternion right = Quaternion.Euler(0, fieldOfView * 0.5f, 0);
        Gizmos.DrawLine(eyes.position, eyes.position + left * eyes.forward * sightRange);
        Gizmos.DrawLine(eyes.position, eyes.position + right * eyes.forward * sightRange);
    }
}
    