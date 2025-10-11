using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WalkmanPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform tapeSlot; // transform used as parent for inserted tape
    public AudioSource audioSource; // spatial AudioSource on the Walkman
    public Transform spoolLeft, spoolRight; // visuals
    [Header("SFX")]
    public AudioClip insertSfx, ejectSfx, clickSfx;

    [Header("Settings")]
    public float spoolSpeed = 1f;
    public float ejectForce = 0.6f;

    [HideInInspector] public TapeBehaviour currentTape;
    bool isPlaying = false;
    Coroutine rewindRoutine = null;

    void Update()
    {
        if (currentTape == null)
        {
            // stop any remaining spin and slowly reset if you like
            return;
        }

        if (isPlaying)
        {
            // normal playback spin
            float spin = spoolSpeed * 360f * Time.deltaTime;
            if (spoolLeft) spoolLeft.Rotate(Vector3.forward, spin, Space.Self);
            if (spoolRight) spoolRight.Rotate(Vector3.forward, spin, Space.Self);
        }
    }
    public void InsertTape(TapeBehaviour tape)
    {
        if (currentTape != null) return;
        currentTape = tape;
        currentTape.OnInsert(tapeSlot);
        PlayOneShot(insertSfx);

        if (tape.data != null && tape.data.clip != null)
        {
            audioSource.clip = tape.data.clip;
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }
    }
    public void EjectTape()
    {
        if (currentTape != null)
        {
            // Detach the tape
            currentTape.transform.parent = null;

            // Apply ejection force
            Rigidbody tapeRigidbody = currentTape.GetComponent<Rigidbody>();
            if (tapeRigidbody != null)
            {
                tapeRigidbody.isKinematic = false;
                tapeRigidbody.AddForce(transform.up * ejectForce, ForceMode.Impulse);
            }

            // Enable grabbing
            XRGrabInteractable grabInteractable = currentTape.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = true;
            }

            // Clear the current tape reference
            currentTape = null;
        }
    }
    public void PlayPauseToggle()
    {
        if (audioSource.clip == null) { PlayOneShot(clickSfx); return; }
        if (isPlaying) Stop();
        else Play();
    }

    public void Play()
    {
        if (audioSource.clip == null) return;
        audioSource.Play();
        isPlaying = true;
        PlayOneShot(clickSfx);
    }

    public void Stop()
    {
        audioSource.Stop();
        isPlaying = false;
        PlayOneShot(clickSfx);
    }

    public void Rewind(float seconds = 5f)
    {
        if (audioSource.clip == null) return;
        if (rewindRoutine != null) StopCoroutine(rewindRoutine);
        rewindRoutine = StartCoroutine(RewindCoroutine(seconds));
    }

    IEnumerator RewindCoroutine(float seconds)
    {
        if (!audioSource.isPlaying) audioSource.Play();
        float target = Mathf.Max(0f, audioSource.time - seconds);
        while (audioSource.time > target)
        {
            audioSource.time = Mathf.Max(target, audioSource.time - Time.deltaTime * 8f);
            // fast spool visual
            if (spoolLeft) spoolLeft.Rotate(Vector3.forward, -1000f * Time.deltaTime, Space.Self);
            if (spoolRight) spoolRight.Rotate(Vector3.forward, 1000f * Time.deltaTime, Space.Self);
            yield return null;
        }
        PlayOneShot(clickSfx);
        rewindRoutine = null;
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
