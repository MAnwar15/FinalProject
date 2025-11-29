using System.Collections.Generic;
using UnityEngine;

public class VRPen : MonoBehaviour
{
    public Transform tip;
    public float width = 0.01f;
    public LayerMask whiteboardLayer;
    public Color penColor = Color.black;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();
    private bool isDrawing = false;

    void Update()
    {
        if (isDrawing)
        {
            Vector3 tipPos = tip.position;

            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], tipPos) > 0.003f)
            {
                AddPoint(tipPos);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & whiteboardLayer) != 0)
        {
            StartNewLine();
            isDrawing = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & whiteboardLayer) != 0)
        {
            isDrawing = false;
            points.Clear();
        }
    }

    void StartNewLine()
    {
        GameObject parent = GameObject.Find("AllLines");
        if (parent == null)
            parent = new GameObject("AllLines");

        GameObject lineObj = new GameObject("Line Stroke");
        lineObj.transform.SetParent(parent.transform);

        currentLine = lineObj.AddComponent<LineRenderer>();

        currentLine.material = new Material(Shader.Find("Sprites/Default"));
        currentLine.startColor = penColor;
        currentLine.endColor = penColor;
        currentLine.startWidth = width;
        currentLine.endWidth = width;
        currentLine.positionCount = 0;

        points.Clear();
    }
    void AddPoint(Vector3 point)
    {
        points.Add(point);
        currentLine.positionCount = points.Count;
        currentLine.SetPosition(points.Count - 1, point);
    }
}
