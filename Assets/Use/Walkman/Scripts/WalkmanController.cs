using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(AudioSource))]
public class WalkmanController : MonoBehaviour
{
    [Header("Audio / Slot")]
    public AudioSource playerSource;
    public Transform tapeSlot;
    public AudioClip insertSfx;
    public bool autoPlayOnInsert = true;

    [Header("LED")]
    public Light statusLED;
    public Color idleColor = Color.black;
    public Color playingColor = Color.red;

    Tape currentTape;
    XRSocketInteractor socket;

    void Awake()
    {
        if (!playerSource)
            playerSource = GetComponent<AudioSource>();

        if (tapeSlot)
            socket = tapeSlot.GetComponent<XRSocketInteractor>();

        if (statusLED)
        {
            statusLED.color = idleColor;
            statusLED.enabled = false;
        }
    }

    void OnEnable()
    {
        if (socket != null)
        {
            socket.selectEntered.AddListener(OnSocketSelectEntered);
            socket.selectExited.AddListener(OnSocketSelectExited);
        }
    }

    void OnDisable()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnSocketSelectEntered);
            socket.selectExited.RemoveListener(OnSocketSelectExited);
        }
    }

    // ------------------------ Socket Events ------------------------

    void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        XRBaseInteractable interactable = args.interactableObject as XRBaseInteractable;
        if (!interactable) return;

        Tape tape = interactable.GetComponent<Tape>();
        if (tape != null)
            InsertTape(tape);
    }

    void OnSocketSelectExited(SelectExitEventArgs args)
    {
        if (args.isCanceled) return;

        XRBaseInteractable interactable = args.interactableObject as XRBaseInteractable;
        if (!interactable) return;

        Tape tape = interactable.GetComponent<Tape>();
        if (tape != null && tape == currentTape)
            ClearTapeState();
    }

    // ------------------------ Tape Handling ------------------------

    void InsertTape(Tape tape)
    {
        if (!tape) return;

        currentTape = tape;

        // Snap into place & disable physics
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

        playerSource.clip = tape.tapeClip;

        if (insertSfx)
            AudioSource.PlayClipAtPoint(insertSfx, transform.position);

        if (autoPlayOnInsert)
            Play();
    }

    void ClearTapeState()
    {
        playerSource.Stop();
        playerSource.clip = null;

        SetLED(false);

        if (currentTape != null)
        {
            Rigidbody rb = currentTape.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            currentTape.transform.SetParent(null, true);
        }

        currentTape = null;
    }

    // ------------------------ LED ------------------------

    void SetLED(bool playing)
    {
        if (!statusLED) return;

        statusLED.enabled = playing;
        statusLED.color = playing ? playingColor : idleColor;
    }

    // ------------------------ Playback ------------------------

    public void Play()
    {
        if (playerSource.clip == null) return;
        if (playerSource.isPlaying) return;

        if (playerSource.time > 0f)
            playerSource.UnPause();
        else
            playerSource.Play();

        SetLED(true);
    }

    public void Pause()
    {
        if (playerSource.isPlaying)
        {
            playerSource.Pause();
            SetLED(false);
        }
    }

    public void PlayPauseToggle()
    {
        if (!playerSource.clip) return;

        if (playerSource.isPlaying)
            Pause();
        else
            Play();
    }

    public void StopPlayback()
    {
        playerSource.Stop();
        playerSource.time = 0f;
        SetLED(false);
    }

    public void OnPlayPausePressed() => PlayPauseToggle();
    public void OnStopPressed() => StopPlayback();
}
