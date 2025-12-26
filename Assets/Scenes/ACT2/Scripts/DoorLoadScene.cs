using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorLoadScene : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnDoorPressed);
    }

    void OnDoorPressed(SelectEnterEventArgs args)
    {
        SceneManager.LoadScene(sceneName);
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnDoorPressed);
    }
}
