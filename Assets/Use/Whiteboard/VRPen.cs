using System.Collections.Generic;
using UnityEngine;

public class VRPen : MonoBehaviour
{
    public Transform tip; // The pen tip
    public float width = 0.01f;
    public LayerMask whiteboardLayer;
    public Color penColor = Color.black;

    private LineRenderer line;
    private List<Vector3> points = new List<Vector3>();
    private bool isDrawing = false;

    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = penColor;
        line.endColor = penColor;
        line.startWidth = width;
        line.endWidth = width;
    }

    void Update()
    {
        if (isDrawing)
        {
            Vector3 tipPos = tip.position;

            // Only add points if the pen is moving a little
            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], tipPos) > 0.005f)
            {
                points.Add(tipPos);
                line.positionCount = points.Count;
                line.SetPosition(points.Count - 1, tipPos);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & whiteboardLayer) != 0)
        {
            isDrawing = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & whiteboardLayer) != 0)
        {
            isDrawing = false;
        }
    }
}
