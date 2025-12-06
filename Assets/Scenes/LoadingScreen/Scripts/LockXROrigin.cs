using UnityEngine;

public class LockXROrigin : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}
