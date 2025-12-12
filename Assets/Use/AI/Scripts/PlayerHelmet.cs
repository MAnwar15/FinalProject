using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlayerHelmet : MonoBehaviour
{
    public bool isInvisible = false;
    public XRSocketInteractor headSocket;

    void OnEnable()
    {
        headSocket.selectEntered.AddListener(OnInserted);
        headSocket.selectExited.AddListener(OnRemoved);
    }

    void OnDisable()
    {
        headSocket.selectEntered.RemoveListener(OnInserted);
        headSocket.selectExited.RemoveListener(OnRemoved);
    }

    void OnInserted(SelectEnterEventArgs arg)
    {
        if (arg.interactableObject.transform == transform)
            isInvisible = true;
    }

    void OnRemoved(SelectExitEventArgs arg)
    {
        if (arg.interactableObject.transform == transform)
            isInvisible = false;
    }
}
