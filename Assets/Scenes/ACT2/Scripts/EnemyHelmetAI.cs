using UnityEngine;
using UnityEngine.AI;

public class EnemyHelmetAI : MonoBehaviour
{
    public Transform player;          // اللاعب
    public float detectionRange = 10f;

    private PlayerStatus playerStatus;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player != null)
        {
            playerStatus = player.GetComponent<PlayerStatus>();
        }

        // تحذير لو حاجة مش متعينة
        if (agent == null)
            Debug.LogWarning("NavMeshAgent missing on enemy!");
        if (player == null)
            Debug.LogWarning("Player not assigned in Inspector!");
        if (playerStatus == null)
            Debug.LogWarning("PlayerStatus missing on Player!");
    }

    void Update()
    {
        // فحص Null عشان ما يحصلش Exception
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
}
