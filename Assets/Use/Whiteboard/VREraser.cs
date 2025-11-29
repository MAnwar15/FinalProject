using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VREraser : MonoBehaviour
{
    public void Clear()
    {
        GameObject parent = GameObject.Find("AllLines");

        if (parent != null)
        {
            foreach (Transform child in parent.transform)
                GameObject.Destroy(child.gameObject);
        }
    }
}
