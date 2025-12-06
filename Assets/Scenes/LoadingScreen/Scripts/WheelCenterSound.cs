using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WheelCenterSound : MonoBehaviour
{
    public AudioSource hornSound;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Start()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrabCenter);
    }

    void OnGrabCenter(SelectEnterEventArgs args)
    {
        if (hornSound != null)
            hornSound.Play();
    }
}
