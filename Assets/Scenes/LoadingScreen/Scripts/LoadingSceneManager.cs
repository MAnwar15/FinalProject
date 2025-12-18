using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public string sceneToLoad = "Act2";
    public float minimumLoadingTime = 40f; // 40 seconds min stay in loading scene

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        float startTime = Time.time;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false; // Prevent automatic switch

        // Wait until the scene is done loading (progress reaches 0.9)
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                // Ensure minimum loading time has passed
                float elapsedTime = Time.time - startTime;
                if (elapsedTime >= minimumLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }
}
