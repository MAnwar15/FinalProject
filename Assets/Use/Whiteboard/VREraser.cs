using UnityEngine;
using System.Collections.Generic;

public class VREraser : MonoBehaviour
{
    public float eraseRadius = 0.05f;

    private void OnTriggerStay(Collider other)
    {
        LineRenderer line = other.GetComponent<LineRenderer>();
        if (line == null) return;

        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < line.positionCount; i++)
        {
            Vector3 p = line.GetPosition(i);
            float dist = Vector3.Distance(p, transform.position);

            if (dist > eraseRadius)
                newPoints.Add(p);
        }

        // if all points wiped, destroy
        if (newPoints.Count < 2)
        {
            Destroy(line.gameObject);
            return;
        }

        line.positionCount = newPoints.Count;
        line.SetPositions(newPoints.ToArray());
    }
}
