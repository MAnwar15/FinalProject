using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class TapeBehaviour : MonoBehaviour
{
    public TapeData data;
    public Transform snapAnchor; // alignment point
    [HideInInspector] public Transform originalParent;

    Rigidbody rb;
    XRGrabInteractable grab;

    public bool IsInserted { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        originalParent = transform.parent;
    }

    public void OnInsert(Transform slotTransform)
    {
        IsInserted = true;

        // temporarily disable grabbing while inserted
        if (grab)
        {
            grab.enabled = false;
            grab.interactionLayers = 0; // make ungrabbable
        }

        rb.isKinematic = true;
        transform.SetParent(slotTransform, true);

        // align using snapAnchor if assigned
        if (snapAnchor)
        {
            transform.localPosition = -snapAnchor.localPosition;
            transform.localRotation = Quaternion.Inverse(snapAnchor.localRotation);
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void OnEject()
    {
        IsInserted = false;

        // restore physics and grabbing
        rb.isKinematic = false;
        transform.SetParent(originalParent, true);

        // Wait one frame before enabling grab again to avoid timing bug
        StartCoroutine(ReenableGrabNextFrame());
    }

    System.Collections.IEnumerator ReenableGrabNextFrame()
    {
        yield return null; // wait one frame
        if (grab)
        {
            grab.interactionLayers = ~0; // restore all layers
            grab.enabled = true;
        }
    }
}
