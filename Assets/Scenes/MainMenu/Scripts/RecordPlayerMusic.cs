using UnityEngine;

public class RecordPlayerMusic : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null)
            audioSource.Play();
    }
}
