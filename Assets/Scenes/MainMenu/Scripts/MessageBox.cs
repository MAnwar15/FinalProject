using UnityEngine;

public class MessageBox : MonoBehaviour
{
    public GameObject messageCanvas;

    public void CloseMessage()
    {
        messageCanvas.SetActive(false);
    }
}
