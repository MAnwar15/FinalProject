using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyHelmetAI : MonoBehaviour
{
    public Transform player;          // اللاعب
    public float detectionRange = 10f;

    public GameObject gameOverUI;     // شاشة الاتقفاش
    public float restartDelay = 2f;   // تأخير إعادة السين

    private PlayerStatus playerStatus;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player != null)
        {
            playerStatus = player.GetComponent<PlayerStatus>();
        }

        // تحذيرات
        if (agent == null)
            Debug.LogWarning("NavMeshAgent missing on enemy!");
        if (player == null)
            Debug.LogWarning("Player not assigned in Inspector!");
        if (playerStatus == null)
            Debug.LogWarning("PlayerStatus missing on Player!");
        if (gameOverUI == null)
            Debug.LogWarning("GameOver UI not assigned!");
    }

    void Update()
    {
        if (player == null || playerStatus == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            if (!playerStatus.isWearingHelmet)
            {
                // اللاعب مكشوف → اجري وراه
                agent.SetDestination(player.position);
            }
            else
            {
                // اللاعب لابس خوذة → وقف مكانك
                agent.ResetPath();
            }
        }
    }

    // ============================================================
    //   لما العدو يلمس اللاعب → Game Over
    // ============================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // شغّل شاشة الاتقفاش
            if (gameOverUI != null)
                gameOverUI.SetActive(true);

            // وقف حركة العدو
            if (agent != null)
                agent.ResetPath();

            // إعادة تحميل السين بعد ثانيتين
            Invoke(nameof(RestartScene), restartDelay);
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
