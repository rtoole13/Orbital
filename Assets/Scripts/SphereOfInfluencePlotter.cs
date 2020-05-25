using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereOfInfluencePlotter : LinePlotter
{
    private GravitySource gravitySource;
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
        GetComponentInParent<OrbitalBody>().OnOrbitCalculationEvent += DrawCircle; // Hack to listen to base class event
    }

    protected override void Start()
    {
        base.Start();
        DrawCircle();
        SetDisplay(false);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        OrbitalBody orbitalBody = GetComponentInParent<OrbitalBody>();
        if (orbitalBody != null) // Prevent error on end of play
            orbitalBody.OnOrbitCalculationEvent -= DrawCircle; // Hack to listen to base class event
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            SetDisplay(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt))
            SetDisplay(true);
    }
    #endregion UNITY
    

    private void DrawCircle()
    {
        lineRenderer.useWorldSpace = false; // Important for having SOI follow object
        if (Radius == Mathf.Infinity)
            return;

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * Radius, Mathf.Cos(angle) * Radius, zepth);
            points[i] = transform.worldToLocalMatrix * vertex;
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }
}
