using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereOfInfluencePlotter : LinePlotter
{
    private GravitySource gravitySource;
    private bool display = true;
    #region GETSET
    public float Radius
    {
        get { return gravitySource.RadiusOfInfluence; }
    }
    #endregion GETSET

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        gravitySource = GetComponentInParent<GravitySource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            ToggleDisplay();

        if (display)
            DrawCircle();
    }
    #endregion UNITY

    private void ToggleDisplay()
    {
        display = !display;
        if (!display)
            lineRenderer.positionCount = 0;
    }

    private void DrawCircle()
    {
        if (Radius == Mathf.Infinity)
            return;

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * Radius, Mathf.Cos(angle) * Radius, zepth);
            points[i] = vertex + gravitySource.transform.position;
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }
}
