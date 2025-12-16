using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

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
    [Header("Nav & Patrol")]
    private NavMeshAgent agent;
    private int patrolIndex = 0;
    public float patrolStopDelay = 1f;
    private bool waitingAtPoint = false;

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

    // ================= SUSPICIOUS =================
    [Header("Suspicious")]
    public float suspiciousDelay = 3f;
    private float suspiciousTimer = 0f;

    // ================= CHASE =================
    [Header("Chase")]
    public float loseSightTime = 4f;
    private float lostSightTimer = 0f;

    // ================= AUDIO =================
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip suspiciousClip;
    public AudioClip investigateClip;
    public AudioClip chaseClip;

    private bool playedStateSound = false;
    private State previousState;

    // ================= GAME OVER =================
    [Header("Game Over")]
    public GameObject gameOverUI;
    public float restartDelay = 2f;
    public AudioClip gameOverSound;

    private static bool gameOverTriggered = false;

    // ================= UNITY =================
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (eyes == null) eyes = transform;

        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);

        previousState = currentState;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    void Update()
    {
        if (player == null || gameOverTriggered) return;

        visionCheckTimer += Time.deltaTime;
        if (visionCheckTimer >= visionCheckRate)
        {
            visionCheckTimer = 0f;
            VisionCheck();
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

            case State.Chase:
                ChaseUpdate();
                break;
        }
    }

    // ================= STATE EVENTS =================
    void OnStateChanged(State st)
    {
        playedStateSound = false;

        if (st == State.Suspicious) PlayOnce(suspiciousClip);
        if (st == State.Chase) PlayOnce(chaseClip);
    }

    void PlayOnce(AudioClip clip)
    {
        if (playedStateSound || clip == null) return;
        audioSource.clip = clip;
        audioSource.Play();
        playedStateSound = true;
    }

    // ================= PATROL =================
    void PatrolUpdate()
    {
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

    // ================= SUSPICIOUS =================
    void EnterSuspicious()
    {
        currentState = State.Suspicious;
        suspiciousTimer = suspiciousDelay;
        agent.isStopped = true;
    }

    void SuspiciousUpdate()
    {
        if (CanSeePlayer())
        {
            suspiciousTimer -= Time.deltaTime;
            if (suspiciousTimer <= 0f)
            {
                agent.isStopped = false;
                currentState = State.Chase;
            }
        }
        else
        {
            agent.isStopped = false;
            currentState = State.Patrol;
        }
    }

    // ================= CHASE =================
    void ChaseUpdate()
    {
        agent.SetDestination(player.position);

        if (!CanSeePlayer())
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= loseSightTime)
            {
                lostSightTimer = 0f;
                currentState = State.Patrol;
            }
        }
        else
        {
            lostSightTimer = 0f;
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
            detectionProgress -= detectionLose * visionCheckRate;
            detectionProgress = Mathf.Max(0f, detectionProgress);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null || eyes == null) return false;

        if (helmet != null && helmet.isInvisible)
            return false;

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

    // ================= GAME OVER =================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        agent.isStopped = true;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        if (gameOverSound != null)
        {
            audioSource.Stop();
            audioSource.spatialBlend = 0f;
            audioSource.clip = gameOverSound;
            audioSource.Play();
        }

        StartCoroutine(RestartScene());
    }

    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
