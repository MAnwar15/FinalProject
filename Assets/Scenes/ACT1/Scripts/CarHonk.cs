using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CarHonk : MonoBehaviour
{
    public AudioSource hornAudio;
    public float delayBeforeLoading = 2f; // 2 seconds dramatic delay
    public string loadingSceneName = "LoadingScene";

    private bool hasHonked = false;

    // This method will be called when the player interacts
    public void Honk()
    {
        if (!hasHonked)
        {
            hasHonked = true;
            hornAudio.Play();
            StartCoroutine(GoToLoadingSceneAfterDelay());
        }
    }

    private IEnumerator GoToLoadingSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoading);
        SceneManager.LoadScene(loadingSceneName);
    }
}
