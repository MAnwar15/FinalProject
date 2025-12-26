using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroVideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource musicSource;
    public string nextSceneName = "MainMenu";

    void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        if (musicSource != null)
        {
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
