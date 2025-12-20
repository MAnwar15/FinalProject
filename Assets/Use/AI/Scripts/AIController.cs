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

    // ================= REFERENCES =================
    [Header("References")]
    public Transform[] patrolPoints;
    public Transform eyes;
    public Transform player;
    public LayerMask obstacleMask;

    [Header("Player Settings")]
    public PlayerHelmet helmet;

    // ================= NAV =================
    private NavMeshAgent agent;
    private int patrolIndex = 0;
    public float patrolStopDelay = 1f;
    private bool waitingAtPoint = false;

    // ================= GAME OVER =================
    [Header("Game Over")]
    public float catchDistance = 1.2f;
    public Canvas gameOverCanvas;
    public AudioClip gameOverMusic;
    public float restartDelay = 4f;
    private bool gameOverTriggered = false;

    // ================= VISION =================
    [Header("Vision")]
    public float sightRange = 12f;
    [Range(0, 360)] public float fieldOfView = 110f;
    public float visionCheckRate = 0.25f;
    private float visionCheckTimer = 0f;
    private static RaycastHit[] raycastBuffer = new RaycastHit[1];

    public float detectionThreshold = 2f;
    public float detectionGain = 1.2f;
    public float detectionLose = 1f;
    private float detectionProgress = 0f;

    // ================= INVESTIGATE =================
    [Header("Investigate")]
    public float investigateStopDistance = 0.6f;
    public float investigateLookDuration = 3f;
    public float suspiciousDelay = 3f;
    private float suspiciousTimer = 0f;

    // ================= CHASE =================
    [Header("Chase")]
    public float loseSightTime = 4f;
    public float maxChaseDistance = 25f;
    private float lostSightTimer = 0f;

    // ================= AUDIO =================
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip suspiciousClip;
    public AudioClip investigateClip;
    public AudioClip chaseClip;
    private bool playedStateSound = false;

    private Vector3 lastKnownPlayerPos;
    private Vector3 lastTargetPosition;
    private State previousState;

    private static int agentCount = 0;
    private int agentIndex;

    // ================= UNITY =================
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        if (eyes == null) eyes = transform;

        agent.updateRotation = true;
        agent.autoRepath = false;

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
        if (player == null || gameOverTriggered) return;

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            TriggerGameOver();
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= sightRange * 2f)
        {
            visionCheckTimer += Time.deltaTime;
            if (visionCheckTimer >= visionCheckRate * (agentIndex + 1))
            {
                visionCheckTimer = 0f;
                VisionCheck();
            }
        }

        if (currentState != previousState)
        {
            OnStateChanged(currentState);
            previousState = currentState;
        }

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
                break;

            case State.Chase:
                ChaseUpdate();
                break;

            case State.ReturnToPatrol:
                ReturnToPatrol();
                break;
        }
    }

    // ================= GAME OVER =================
    void TriggerGameOver()
    {
        gameOverTriggered = true;

        // 🔥 STOP ALL OTHER AUDIO (background music etc.)
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource a in allAudio)
            a.Stop();

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        StopAllCoroutines();

        if (gameOverCanvas != null)
            gameOverCanvas.gameObject.SetActive(true);

        audioSource.spatialBlend = 0f;
        audioSource.clip = gameOverMusic;
        audioSource.loop = false;
        audioSource.volume = 1f;
        audioSource.Play();

        StartCoroutine(RestartSceneAfterDelay());
    }

    IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ================= AUDIO STATES =================
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

    // ================= PATROL =================
    void PatrolUpdate()
    {
        agent.isStopped = false;
        ApplyAgentRotation();

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.1f &&
            !waitingAtPoint)
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

    // ================= SUSPICIOUS =================
    void EnterSuspicious()
    {
        currentState = State.Suspicious;
        suspiciousTimer = suspiciousDelay;

        agent.isStopped = true;
        agent.ResetPath();
    }

    void SuspiciousUpdate()
    {
        if (CanSeePlayer())
        {
            suspiciousTimer -= Time.deltaTime;
            if (suspiciousTimer <= 0f)
                currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    // ================= VISION =================
    void VisionCheck()
    {
        if (CanSeePlayer())
        {
            detectionProgress += detectionGain * visionCheckRate;
            if (detectionProgress >= detectionThreshold)
                currentState = State.Chase;
        }
        else
        {
            detectionProgress = Mathf.Max(0f, detectionProgress - detectionLose * visionCheckRate);
        }
    }

    bool CanSeePlayer()
    {
        if (helmet != null && helmet.isInvisible) return false;

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

    // ================= CHASE =================
    void ChaseUpdate()
    {
        if (!CanSeePlayer())
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= loseSightTime)
                currentState = State.ReturnToPatrol;
            return;
        }

        lostSightTimer = 0f;
        agent.SetDestination(player.position);
    }

    // ================= RETURN =================
    void ReturnToPatrol()
    {
        detectionProgress = 0f;
        currentState = State.Patrol;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    // ================= ROTATION =================
    void ApplyAgentRotation()
    {
        Vector3 vel = agent.desiredVelocity;
        vel.y = 0;

        if (vel.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(vel),
                Time.deltaTime * 6f
            );
        }
    }
}
