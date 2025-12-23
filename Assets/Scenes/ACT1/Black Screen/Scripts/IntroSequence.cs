using UnityEngine;
using TMPro;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;   // for fade out
    public GameObject introUI;
    public TextMeshProUGUI actText;

    [Header("Audio")]
    public AudioSource introAudio;

    [Header("Timing")]
    public float holdDuration = 3f;   // time before fade
    public float fadeDuration = 1.5f; // fade out time

    void Start()
    {
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        // Show instantly
        introUI.SetActive(true);
        canvasGroup.alpha = 1f;

        // Play sound
        if (introAudio != null)
            introAudio.Play();

        // Wait full duration (no fade)
        yield return new WaitForSeconds(holdDuration);

        // Fade OUT only
        yield return StartCoroutine(FadeOut());

        // Hide UI
        introUI.SetActive(false);
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
