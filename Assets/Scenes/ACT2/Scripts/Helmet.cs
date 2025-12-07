using UnityEngine;

public class Helmet : MonoBehaviour
{
    [Header("Player Status")]
    public PlayerStatus playerStatus; // ربط اللاعب

    [Header("Socket / Attach Point")]
    public Transform helmetSocket;     // مكان تركيب الخوذة (XR Socket أو Empty GameObject)

    [Header("Settings")]
    public float equipDistance = 0.1f; // المسافة اللي تعتبر فيها الخوذة لابسة

    void Update()
    {
        // لو الخوذة قريبة من الـ Socket → اللاعب لابس الخوذة
        float distance = Vector3.Distance(transform.position, helmetSocket.position);

        if (distance <= equipDistance)
        {
            playerStatus.isWearingHelmet = true;
        }
        else
        {
            playerStatus.isWearingHelmet = false;
        }
    }
}
