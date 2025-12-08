using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnemyHelmetAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;

    public GameObject gameOverUI;
    public float restartDelay = 2f;

    public List<AudioClip> gameOverSounds;  // ✔ قائمة الأصوات
    private AudioSource audioSource;        // ✔ مصدر الصوت

    private PlayerStatus playerStatus;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (player != null)
            playerStatus = player.GetComponent<PlayerStatus>();

        if (gameOverUI == null)
            Debug.LogWarning("GameOver UI not assigned!");

        if (audioSource == null)
            Debug.LogWarning("No AudioSource found on enemy!");
    }

    void Update()
    {
        if (player == null || playerStatus == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            if (!playerStatus.isWearingHelmet)
                agent.SetDestination(player.position);   // يجري
            else
                agent.ResetPath();                       // يقف
        }
    }

    // ============================================================
    //   Game Over لما يلمس اللاعب
    // ============================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // UI
            if (gameOverUI != null)
                gameOverUI.SetActive(true);

            // وقف العدو
            if (agent != null)
                agent.ResetPath();

            // ✔ تشغيل كل الأصوات
            PlayAllSounds();

            // إعادة السين
            Invoke(nameof(RestartScene), restartDelay);
        }
    }

    void PlayAllSounds()
    {
        if (audioSource == null || gameOverSounds.Count == 0) return;

        foreach (var clip in gameOverSounds)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
