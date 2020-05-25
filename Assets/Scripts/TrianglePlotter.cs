using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class TrianglePlotter : LinePlotter
{
    public float baseToHeightRatio;
    public float halfHeight;
    private float width;
    
    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        halfHeight *= transform.localScale.y / 2f;
        width = halfHeight * baseToHeightRatio;
    }
    protected override void Start()
    {
        DrawTriangle();
    }

    protected void Update()
    {
        DrawTriangle();
    }

    #endregion UNITY

    private void DrawTriangle()
    {
        Debug.Log(halfHeight);
        lineRenderer.useWorldSpace = false; // Important for having SOI follow object

        Vector3[] points = new Vector3[4];
        points[0] = new Vector3(0, halfHeight, zepth);
        points[1] = new Vector3(width, -halfHeight, zepth);
        points[2] = new Vector3(-width, -halfHeight, zepth);
        points[3] = points[0];
        lineRenderer.positionCount = 4;
        lineRenderer.SetPositions(points);
    }
}
