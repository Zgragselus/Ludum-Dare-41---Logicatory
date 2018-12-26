using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public GameObject to;
    public float gravityEffect = 0.1f;
    public int lineSteps = 4;
    public Material lineMaterial;
    Vector3[] points;
    LineRenderer line;
    
    void Rebuild(Vector3 windOffset)
    {
        float dist = (transform.position - to.transform.position).magnitude;
        Vector3 mid = transform.position + (to.transform.position - transform.position) * 0.5f + new Vector3(0.0f, -1.0f, 0.0f) * gravityEffect * dist + windOffset;

        for (int i = 0; i < lineSteps; i++)
        {
            float time = (float)i / (float)(lineSteps - 1);
            points[i] = GetPoint(transform.position, mid, to.transform.position, time);
        }

        line.positionCount = lineSteps;
        for (int i = 0; i < lineSteps; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }

	void Start ()
    {
        if (to != null)
        {
            line = gameObject.AddComponent<LineRenderer>();
            points = new Vector3[lineSteps];

            line.sortingLayerName = "OnTop";
            line.sortingOrder = 5;
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.useWorldSpace = true;
            line.material = lineMaterial;

            Rebuild(new Vector3(0.0f, 0.0f, 0.0f));
        }
    }

    /*void OnDrawGizmosSelected()
    {
        if (to != null)
        {
            Handles.color = Color.white;
            Vector3 lineStart = transform.position;
            float dist = (transform.position - to.transform.position).magnitude;
            Vector3 mid = transform.position + (to.transform.position - transform.position) * 0.5f + new Vector3(0.0f, -1.0f, 0.0f) * gravityEffect * dist;
            for (int i = 1; i < lineSteps; i++)
            {
                float time = (float)i / (float)(lineSteps - 1);
                Vector3 lineEnd = GetPoint(transform.position, mid, to.transform.position, time);
                Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }
        }
    }*/

    // Update is called once per frame
    void Update ()
    {
        if (to != null)
        {
            Rebuild(new Vector3(0.0f, 0.0f, 0.0f));
        }
    }

    Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
    }
}
