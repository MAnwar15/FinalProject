using UnityEngine;

public class WalkmanButton : MonoBehaviour
{
    public enum ButtonType { PlayPause, Rewind, Eject }
    public WalkmanPlayer walkman;
    public ButtonType type;
    public float rewindSeconds = 5f;

    // call this from an XR interaction event (OnActivate / OnSelectEntered) or a physical press script
    public void Press()
    {
        if (walkman == null) return;
        switch (type)
        {
            case ButtonType.PlayPause: walkman.PlayPauseToggle(); break;
            case ButtonType.Rewind: walkman.Rewind(rewindSeconds); break;
            case ButtonType.Eject: walkman.EjectTape(); break;
        }
    }
}
