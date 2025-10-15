using UnityEngine;

public interface IEnemyState
{
    void Enter();
    void Exit();
    void Tick();
}

public class EnemyStateMachine : MonoBehaviour
{
    IEnemyState current;

    public void SetState(IEnemyState next)
    {
        if (current != null) current.Exit();
        current = next;
        if (current != null) current.Enter();
    }

    void Update()
    {
        if (current != null) current.Tick();
    }
}
