using UnityEngine;

[CreateAssetMenu(menuName = "VR/CassetteTapeData")]
public class TapeData : ScriptableObject
{
    public string tapeName;
    public AudioClip clip;
    [Tooltip("Optional override length in seconds")]
    public float lengthOverride = 0f;

    public float ClipLength => (lengthOverride > 0f) ? lengthOverride : (clip ? clip.length : 0f);
}
