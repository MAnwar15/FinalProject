using UnityEngine;
using UnityEngine.Video;

public class VideoToggleMultiple : MonoBehaviour
{
    public VideoPlayer[] videoPlayers;
    private bool isOn = false;

    public void ToggleVideo()
    {
        isOn = !isOn;

        foreach (VideoPlayer vp in videoPlayers)
        {
            if (vp == null) continue;

            if (isOn)
            {
                vp.Stop();
                vp.time = 0;
                vp.Play();
            }
            else
            {
                vp.Stop();
            }
        }
    }

    public bool IsVideoOn()
    {
        return isOn;
    }
}
