using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody))]
public class TrajectoryPlotter : LinePlotter
{
    private OrbitalBody orbitalBody;
    private float eccentricityTolerance = 0.05f;

    #region GETSET
    public float SemimajorAxis
    {
        get { return orbitalBody.SemimajorAxis; }

    }
    public float SemiminorAxis
    {
        get { return orbitalBody.SemiminorAxis; }
    }
    #endregion GETSET

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        orbitalBody = GetComponent<OrbitalBody>();
    }
    
    private void Update()
    {
        if (orbitalBody.CurrentGravitySource == null)
            return;

        if (orbitalBody.Eccentricity >= 1f)
        {
            BuildHyperbola();
        }
        else
        {
            BuildEllipse();
        }
    }
    #endregion UNITY

    private void BuildEllipse()
    {
        if ((SemimajorAxis < orbitalBody.CurrentGravitySource.Radius) || (SemiminorAxis < orbitalBody.CurrentGravitySource.Radius)){
            //Ignoring when on surface-ish
            return;
        }
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * SemimajorAxis, Mathf.Cos(angle) * SemiminorAxis, zepth);
            vertex = TranslateVector(vertex, new Vector3(-SemimajorAxis * orbitalBody.Eccentricity, 0, 0));
            points[i] = RotateVertex(vertex, orbitalBody.ArgumentOfPeriapsis) + orbitalBody.CurrentGravitySource.transform.position;
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }

    private void BuildHyperbola()
    {
        
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2f  - 1f;
            Vector3 vertex = new Vector3(SemimajorAxis * MathUtilities.Cosh(angle), SemiminorAxis * MathUtilities.Sinh(angle), 1f);
            vertex = TranslateVector(vertex, new Vector3(-SemimajorAxis * orbitalBody.Eccentricity, 0, 0));
            points[i] = RotateVertex(vertex, orbitalBody.ArgumentOfPeriapsis) + orbitalBody.CurrentGravitySource.transform.position;
        }
        lineRenderer.positionCount = segments;
        lineRenderer.SetPositions(points);
    }

    #region GIZMOS
    private void OnDrawGizmos()
    {
        if (orbitalBody == null || orbitalBody.CurrentGravitySource == null)
            return;
        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(orbitalBody.CurrentGravitySource.transform.position, SemimajorAxis * (1f - orbitalBody.Eccentricity) * orbitalBody.EccentricityVector.normalized);
        
    }
    #endregion GIZMOS
}
