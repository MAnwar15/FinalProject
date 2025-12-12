using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyHelmetAI : MonoBehaviour
{
    // 🔥 GameOver يحصل مرة واحدة فقط حتى لو 100 عدو لمسوا اللاعب
    public static bool gameOverTriggered = false;

    public Transform player;
    public float detectionRange = 10f;

    public GameObject gameOverUI;
    public float restartDelay = 2f;

    public AudioClip[] gameOverSounds; // ✔ الأصوات
    private AudioSource audioSource;

    private PlayerStatus playerStatus;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (player != null)
            playerStatus = player.GetComponent<PlayerStatus>();

        if (gameOverUI != null)
            gameOverUI.SetActive(false); // يخليها مخفية في البداية
    }

    void Update()
    {
        if (player == null || playerStatus == null || agent == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 👀 العدو يشوف اللاعب لو مش لابس الخوذة
        if (distance <= detectionRange)
        {
            if (!playerStatus.isWearingHelmet)
                agent.SetDestination(player.position);
            else
                agent.ResetPath(); // لو لابس خوذة → يقف
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 🔥 يمنع تكرار الجيم أوفر مهما كان عدد الأعداء
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;

        // 🔥 إظهار شاشة الجيم أوفر مرة واحدة
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // إيقاف حركة العدو
        if (agent != null)
            agent.ResetPath();

        // تشغيل كل الأصوات مرة واحدة
        PlayAllSounds();

        // إعادة تحميل المشهد
        Invoke(nameof(RestartScene), restartDelay);
    }

    void PlayAllSounds()
    {
        if (audioSource == null || gameOverSounds.Length == 0)
            return;

        foreach (AudioClip clip in gameOverSounds)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
