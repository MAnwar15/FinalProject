using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VRSceneTimer : MonoBehaviour
{
    [Header("Scene Settings")]
    public string targetSceneName; // Name of the scene to load
    public float waitDuration = 5f; // Time in seconds before loading

    void Start()
    {
        StartCoroutine(LoadTargetSceneAfterDelay());
    }

    IEnumerator LoadTargetSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitDuration);
        SceneManager.LoadScene(targetSceneName);
    }
}
