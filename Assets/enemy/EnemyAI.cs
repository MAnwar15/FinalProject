using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStateMachine))]
public class EnemyAI : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform Target; // player transform (assign Player root)
    public float killDistance = 1.2f;
    public float investigateDuration = 4f;
    public Animator Animator;

    [Header("Detection")]
    public EnemyDetection detection;

    [HideInInspector] public NavMeshAgent Agent;
    EnemyStateMachine fsm;
    IEnemyState patrolState;
    IEnemyState chaseState;
    IEnemyState deadState;

    IEnemyState currentInvestigate;

    int health = 100;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<EnemyStateMachine>();
    }

    void Start()
    {
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        deadState = new DeadState(this);

        // initial state
        fsm.SetState(patrolState);

        Rock.OnRockThrown += HandleRockThrown;
        Debug.Log($"[EnemyAI] {name} started, subscribed to Rock.OnRockThrown");
    }

    void OnDestroy()
    {
        Rock.OnRockThrown -= HandleRockThrown;
    }

    void Update()
    {
        // simple sight check
        if (Target != null && detection != null)
        {
            if (detection.CanSeePlayer(Target))
            {
                // switch to chase state
                TargetSpotted();
            }
        }
    }

    void TargetSpotted()
    {
        // set player as target and chase
        fsm.SetState(chaseState);
        Debug.Log($"[EnemyAI] {name} spotted player -> chasing");
    }

    void HandleRockThrown(Vector3 rockPos)
    {
        // If rock within radius, investigate
        float d = Vector3.Distance(transform.position, rockPos);
        if (d <= (configAttractRadius())) // uses config or fallback
        {
            currentInvestigate = new InvestigateState(this, rockPos);
            fsm.SetState(currentInvestigate);
            Debug.Log($"[EnemyAI] {name} investigating rock at {rockPos}");
        }
    }

    float configAttractRadius()
    {
        // try to find RockConfig from a rock in the scene as a quick config; fallback to 8
        var rock = FindObjectOfType<Rock>();
        if (rock != null && rock.config != null) return rock.config.attractRadius;
        return 8f;
    }

    // Called by ChaseState when close enough
    public void OnReachPlayer()
    {
        // kill player
        var playerStatus = Target.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.Kill();
            Debug.Log($"[EnemyAI] {name} killed the player");
            GameManager.Instance.OnPlayerKilled();
        }

        // go to dead or reset
        fsm.SetState(deadState);
    }

    public void EndInvestigation()
    {
        // resume patrol
        fsm.SetState(patrolState);
        Debug.Log($"[EnemyAI] {name} ended investigation -> patrol");
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log($"[EnemyAI] {name} took {dmg} damage -> hp {health}");
        if (health <= 0)
        {
            fsm.SetState(deadState);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerStatus>();
        if (player != null && player.IsAlive)
        {
            Debug.Log($"[EnemyAI] Triggered with player {player.name}");
            OnReachPlayer();
        }
    }

}
