using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Rock : MonoBehaviour
{
    public RockConfig config;

    public static event Action<Vector3> OnRockThrown;

    Rigidbody rb;
    bool thrown = false;
    bool landed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectExited.AddListener(OnSelectExited);
            Debug.Log($"[Rock] Subscribed to XRGrabInteractable on {name}");
        }
    }

    void OnDisable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"[Rock] OnSelectExited velocity = {rb.linearVelocity.magnitude}");
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            thrown = true;
            landed = false;
            Debug.Log($"[Rock] Thrown at {transform.position}");
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (thrown && !landed)
        {
            landed = true;
            Debug.Log($"[Rock] Landed at {transform.position} -> notifying enemies");
            OnRockThrown?.Invoke(transform.position);
        }
    }
}
