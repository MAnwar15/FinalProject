using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SteeringWheelVR : MonoBehaviour
{
    public Transform wheelModel;
    public List<XRGrabInteractable> grabPoints;
    public float maxAngle = 180f;

    private XRBaseInteractor hand;

    void OnEnable()
    {
        foreach (var g in grabPoints)
        {
            g.selectEntered.AddListener(OnGrab);
            g.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        foreach (var g in grabPoints)
        {
            g.selectEntered.RemoveListener(OnGrab);
            g.selectExited.RemoveListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        hand = args.interactorObject as XRBaseInteractor;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        hand = null;
    }

    void Update()
    {
        if (hand != null)
        {
            // موقع اليد بالنسبة لمحور دوران العجلة
            Vector3 localHandPos = transform.InverseTransformPoint(hand.transform.position);

            // حساب زاوية الدوران حول Z
            float angle = Mathf.Atan2(localHandPos.y, localHandPos.x) * Mathf.Rad2Deg;

            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);

            // الدوران حول محور Z فقط
            wheelModel.localRotation = Quaternion.Euler(0, 0, -angle);
        }
    }
}
