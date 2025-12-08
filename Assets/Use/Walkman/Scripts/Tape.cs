using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Tape : MonoBehaviour
{
    [Tooltip("Audio that this cassette contains.")]
    public AudioClip tapeClip;

    [Tooltip("Optional name for editor.")]
    public string tapeName;
}
