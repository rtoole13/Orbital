﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(GravityAffected))]
public class TrajectoryPlotter : MonoBehaviour
{
    LineRenderer lineRenderer;
    GravityAffected orbitalBody;
    private float _semiminorAxis;
    private Vector2 _center;
    private float eccentricityTolerance = 0.05f;

    [Range(3, 36)]
    public int segments;

    #region GETSET
    public Vector2 Center
    {
        get { return _center; }
        private set { _center = value; }
    }
    public float SemimajorAxis
    {
        get { return orbitalBody.SemimajorAxis; }

    }
    public float SemiminorAxis
    {
        get { return _semiminorAxis; }
        private set { _semiminorAxis = value; }
    }
    #endregion GETSET

    #region UNITY
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        orbitalBody = GetComponent<GravityAffected>();
        //BuildEllipse();
    }

    private void Update()
    {
        if (orbitalBody.Eccentricity >= 1f)
            return;
        BuildEllipse();
    }
    #endregion UNITY

    private void BuildEllipse()
    {
        SemiminorAxis = CalculateSemiminorAxis();
        if ((orbitalBody.SemimajorAxis < orbitalBody.CurrentGravitySource.Radius) || (SemiminorAxis < orbitalBody.CurrentGravitySource.Radius)){
            //Ignoring when on surface-ish
            return;
        }

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * SemimajorAxis, Mathf.Cos(angle) * SemiminorAxis, 0);
            if (orbitalBody.SpecificRelativeAngularMomentum.z < 0)
            {
                //WORKS REAL GOOD
                points[i] = TranslateVector(vertex, new Vector3(-orbitalBody.Eccentricity * SemimajorAxis * Mathf.Cos(orbitalBody.ArgumentOfPeriapsis), 
                                                                -orbitalBody.Eccentricity * SemimajorAxis * Mathf.Sin(orbitalBody.ArgumentOfPeriapsis), 0));
            }
            else
            {
                points[i] = TranslateVector(vertex, new Vector3(-orbitalBody.Eccentricity * SemimajorAxis * Mathf.Cos(orbitalBody.ArgumentOfPeriapsis), 
                                                                -orbitalBody.Eccentricity * SemimajorAxis * Mathf.Sin(orbitalBody.ArgumentOfPeriapsis), 0));
            }
            
            //points[i] = RotateVertex(vertex, orbitalBody.ArgumentOfPeriapsis);
        }

        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }

    private float CalculateSemiminorAxis()
    {
        if (Mathf.Abs(orbitalBody.Eccentricity - 1f) < eccentricityTolerance)
        {
            return 0f;
        }
        return SemimajorAxis * Mathf.Sqrt(1 - Mathf.Pow(orbitalBody.Eccentricity, 2));
    }

    private Vector3 RotateVertex(Vector3 vertex, float angle)
    {
        return new Vector3(vertex.x * Mathf.Cos(angle) - vertex.y * Mathf.Sin(angle),
                       vertex.x * Mathf.Sin(angle) + vertex.y * Mathf.Cos(angle), 0);
    }
    
    private Vector3 TranslateVector(Vector3 vertex, Vector3 distance)
    {
        return new Vector3(vertex.x + distance.x, vertex.y + distance.y, 0);
    }

    #region GIZMOS
    private void OnDrawGizmos()
    {
        if (orbitalBody == null || orbitalBody.CurrentGravitySource == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(Vector3.zero, SemimajorAxis * orbitalBody.EccentricityVector);
        
    }
    #endregion GIZMOS
}