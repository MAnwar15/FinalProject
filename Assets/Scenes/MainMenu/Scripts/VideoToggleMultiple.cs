using UnityEngine;
using UnityEngine.Video;

public class VideoToggleMultiple : MonoBehaviour
{
    public VideoPlayer[] videoPlayers;  // Assign all your screens
    private bool isPlaying = false;

    public void ToggleVideo()
    {
        if (videoPlayers.Length == 0) return;

        isPlaying = !isPlaying; // toggle state

        foreach (VideoPlayer vp in videoPlayers)
        {
            if (vp == null) continue;

            if (isPlaying)
                vp.Play();
            else
                vp.Pause();
        }

        Debug.Log("Videos " + (isPlaying ? "Playing" : "Paused"));
    }
}
