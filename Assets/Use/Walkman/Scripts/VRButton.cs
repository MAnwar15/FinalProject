using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class VRButton : MonoBehaviour
{
    [Header("Button Settings")]
    public UnityEvent onPress;
    public AudioClip sfx;
    public float pressDepth = 0.012f;
    public float animSpeed = 12f;

    Vector3 restLocal;
    Vector3 pressedLocal;
    bool animating = false;
    AudioSource sfxSource;

    XRBaseInteractable interactable;

    void Awake()
    {
        restLocal = transform.localPosition;
        pressedLocal = restLocal + Vector3.down * pressDepth;

        // Prepare SFX source
        if (sfx != null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.spatialBlend = 1f;
            sfxSource.playOnAwake = false;
        }

        interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(_ => Press());
    }

    public void Press()
    {
        if (!animating)
            StartCoroutine(AnimatePress());
    }

    IEnumerator AnimatePress()
    {
        animating = true;

        if (sfxSource && sfx) sfxSource.PlayOneShot(sfx);
        onPress?.Invoke();

        // press-in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            transform.localPosition = Vector3.Lerp(restLocal, pressedLocal, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // release
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            transform.localPosition = Vector3.Lerp(pressedLocal, restLocal, t);
            yield return null;
        }

        animating = false;
    }
}
