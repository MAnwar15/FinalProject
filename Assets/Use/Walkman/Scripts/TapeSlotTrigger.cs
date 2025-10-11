using UnityEngine;

public class TapeSlotTrigger : MonoBehaviour
{
    public WalkmanPlayer walkman;

    void OnTriggerEnter(Collider other)
    {
        var tape = other.GetComponent<TapeBehaviour>();
        if (tape != null && walkman != null && walkman.currentTape == null)
        {
            // require player to let go before insertion (optional)
            walkman.InsertTape(tape);
        }
    }
}
