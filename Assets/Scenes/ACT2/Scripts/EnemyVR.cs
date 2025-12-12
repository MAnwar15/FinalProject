using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyVR : MonoBehaviour
{
    // 🔥 تشغيل الجيم اوفر مرة واحدة فقط مهما كان عدد الأعداء
    public static bool gameOverTriggered = false;

    public Transform[] waypoints;
    public float waitTime = 1f;
    public float viewRange = 10f;
    public float viewAngle = 60f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public float chaseSpeed = 4f;
    public float normalSpeed = 2f;

    int currentPoint = 0;
    NavMeshAgent agent;
    bool isChasing = false;

    Transform player;

    [Header("Game Over")]
    public GameObject gameOverUI;
    public AudioSource gameOverSound;
    public float gameOverDelay = 1.5f;

    bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent.speed = normalSpeed;
        GoToNextPoint();
    }

    void Update()
    {
        if (!isChasing)
        {
            Patrol();
            DetectPlayer();
        }
        else
        {
            ChasePlayer();
        }
    }

    void Patrol()
    {
        if (!isWaiting && agent.remainingDistance <= 0.3f)
        {
            StartCoroutine(WaitAndGo());
        }
    }

    IEnumerator WaitAndGo()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        GoToNextPoint();
        isWaiting = false;
    }

    void GoToNextPoint()
    {
        agent.destination = waypoints[currentPoint].position;
        currentPoint = (currentPoint + 1) % waypoints.Length;
    }

    void DetectPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < viewRange)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle < viewAngle / 2)
            {
                if (!Physics.Linecast(transform.position, player.position, obstacleMask))
                {
                    isChasing = true;
                    agent.speed = chaseSpeed;
                }
            }
        }
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);

        // 🔥 أول مرة فقط لو العدو لمس اللاعب
        if (Vector3.Distance(transform.position, player.position) < 1.2f)
        {
            if (!gameOverTriggered)
            {
                gameOverTriggered = true;
                StartCoroutine(TriggerGameOver());
            }
        }
    }

    IEnumerator TriggerGameOver()
    {
        agent.isStopped = true;

        if (gameOverSound != null)
            gameOverSound.Play();

        yield return new WaitForSeconds(gameOverDelay);

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }
}
