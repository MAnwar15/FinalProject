using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Patrol: move between waypoints
public class PatrolState : IEnemyState
{
    EnemyAI e;
    int idx = 0;

    public PatrolState(EnemyAI enemy) { e = enemy; }

    public void Enter()
    {
        Debug.Log($"[Enemy] {e.name} entering Patrol");
        if (e.waypoints.Length > 0)
            e.Agent.SetDestination(e.waypoints[0].position);
    }

    public void Exit() { }

    public void Tick()
    {
        if (e.waypoints.Length == 0) return;

        if (!e.Agent.pathPending && e.Agent.remainingDistance < 0.5f)
        {
            idx = (idx + 1) % e.waypoints.Length;
            e.Agent.SetDestination(e.waypoints[idx].position);
            Debug.Log($"[Enemy] {e.name} patrol -> waypoint {idx}");
        }
    }
}

// Chase: chase the player
public class ChaseState : IEnemyState
{
    EnemyAI e;

    public ChaseState(EnemyAI enemy) { e = enemy; }

    public void Enter()
    {
        Debug.Log($"[Enemy] {e.name} entering Chase");
    }

    public void Exit() { }

    public void Tick()
    {
        if (e == null || e.Target == null) return;
        e.Agent.SetDestination(e.Target.position);

        float dist = Vector3.Distance(e.transform.position, e.Target.position);
        if (dist <= e.killDistance)
        {
            Debug.Log($"[Enemy] {e.name} reached player. Kill!");
            e.OnReachPlayer();
        }
    }
}

// Investigate: go to rock position for a while
public class InvestigateState : IEnemyState
{
    EnemyAI e;
    Vector3 pos;
    float startTime;

    public InvestigateState(EnemyAI enemy, Vector3 position)
    {
        e = enemy;
        pos = position;
    }

    public void Enter()
    {
        Debug.Log($"[Enemy] {e.name} entering Investigate at {pos}");
        e.Agent.SetDestination(pos);
        startTime = Time.time;
    }

    public void Exit() { }

    public void Tick()
    {
        // If reached pos or timeout, go back to patrol
        if (!e.Agent.pathPending && e.Agent.remainingDistance < 1f)
        {
            if (Time.time - startTime > e.investigateDuration)
            {
                e.EndInvestigation();
            }
        }
        else if (Time.time - startTime > e.investigateDuration + 3f)
        {
            // force end after max time
            e.EndInvestigation();
        }
    }
}

// Dead state
public class DeadState : IEnemyState
{
    EnemyAI e;
    public DeadState(EnemyAI enemy) { e = enemy; }

    public void Enter()
    {
        Debug.Log($"[Enemy] {e.name} entering Dead");
        e.Agent.isStopped = true;
        if (e.Animator != null) e.Animator.SetTrigger("Die");
    }

    public void Exit() { }

    public void Tick() { }
}
