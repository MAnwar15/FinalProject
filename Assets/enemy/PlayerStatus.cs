using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public bool IsAlive { get; private set; } = true;

    public void Kill()
    {
        if (!IsAlive) return;
        IsAlive = false;
        Debug.Log("[PlayerStatus] Player killed - disabling interactions");
        // disable XR interaction components or locomotion here:
        var rigs = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.XRBaseController>();
        foreach (var r in rigs) r.enabled = false;

        // alternatively, disable locomotion, smooth locomotion scripts, etc.
    }
}
