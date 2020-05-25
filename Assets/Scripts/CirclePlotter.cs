using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class CirclePlotter : LinePlotter
{
    private float radius;
    #region GETSET
    #endregion GETSET

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        radius = transform.localScale.x / 2f;
    }
    protected override void Start()
    {
        DrawCircle();
    }

    //protected void Update()
    //{
    //    DrawCircle();
    //}

    #endregion UNITY

    private void DrawCircle()
    {
        lineRenderer.useWorldSpace = false; // Important for having SOI follow object
        if (radius == Mathf.Infinity || radius == 0)
            return;

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius, zepth);
            points[i] = transform.worldToLocalMatrix * vertex;
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }
}
