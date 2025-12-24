using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class IntroSequence : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public GameObject introUI;

    [Header("Audio")]
    public AudioSource introAudio;
    public AudioMixer audioMixer;

    [Header("Timing")]
    public float holdDuration = 3f;
    public float fadeDuration = 1.5f;

    void Start()
    {
        // HARD MUTE game audio
        audioMixer.SetFloat("GameVol", -80f);

        StartCoroutine(IntroRoutine());
    }


    IEnumerator IntroRoutine()
    {
        introUI.SetActive(true);
        canvasGroup.alpha = 1f;

        introAudio.Play();

        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(FadeOut());

        introUI.SetActive(false);

        // Restore game audio
        audioMixer.SetFloat("GameVol", 0f);
    }

    IEnumerator FadeOut()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
