using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(AudioSource))]
public class SpeakerController : MonoBehaviour
{
    [Header("Speaker Settings")]
    public AudioSource audioSource;
    public Transform tapeSlot;
    public AudioClip insertSfx;
    public bool autoPlayOnInsert = true;

    Tape currentTape;
    XRSocketInteractor socket;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (tapeSlot) socket = tapeSlot.GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        if (socket)
        {
            socket.selectEntered.AddListener(OnTapeInserted);
            socket.selectExited.AddListener(OnTapeRemoved);
        }
    }

    void OnDisable()
    {
        if (socket)
        {
            socket.selectEntered.RemoveListener(OnTapeInserted);
            socket.selectExited.RemoveListener(OnTapeRemoved);
        }
    }

    // ------------------------ Socket Events ------------------------

    void OnTapeInserted(SelectEnterEventArgs args)
    {
        XRBaseInteractable interactable = args.interactableObject as XRBaseInteractable;
        if (!interactable) return;

        Tape tape = interactable.GetComponent<Tape>();
        if (tape != null)
            InsertTape(tape);
    }

    void OnTapeRemoved(SelectExitEventArgs args)
    {
        if (args.isCanceled) return;

        XRBaseInteractable interactable = args.interactableObject as XRBaseInteractable;
        if (!interactable) return;

        Tape tape = interactable.GetComponent<Tape>();
        if (tape != null && tape == currentTape)
            ClearTape();
    }

    // ------------------------ Tape Logic ------------------------

    void InsertTape(Tape tape)
    {
        if (!tape) return;

        currentTape = tape;

        // Snap & disable physics
        tape.transform.SetParent(tapeSlot, false);
        tape.transform.localPosition = Vector3.zero;
        tape.transform.localRotation = Quaternion.identity;

        Rigidbody rb = tape.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        audioSource.clip = tape.tapeClip;

        if (insertSfx)
            AudioSource.PlayClipAtPoint(insertSfx, transform.position);

        if (autoPlayOnInsert)
            audioSource.Play();
    }

    void ClearTape()
    {
        audioSource.Stop();
        audioSource.clip = null;

        if (currentTape != null)
        {
            Rigidbody rb = currentTape.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;   // re-enable physics
                rb.useGravity = true;     // restore gravity
            }

            currentTape.transform.SetParent(null, true);
        }

        currentTape = null;
    }
}
