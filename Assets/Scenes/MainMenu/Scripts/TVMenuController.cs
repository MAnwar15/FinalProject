using UnityEngine;

public class TVMenuController : MonoBehaviour
{
    public GameObject menuCanvas;     // Your UI canvas
    public GameObject screenOnImage;  // Optional: screen image

    public void TurnOnTV()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(true);

        if (screenOnImage != null)
            screenOnImage.SetActive(true);

        Debug.Log("TV turned on!");
    }
}
