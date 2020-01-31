using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(GravityAffected))]
public class Ellipse : MonoBehaviour
{
    LineRenderer lineRenderer;
    GravityAffected orbitalBody;
    private float _semimajorAxis;
    private float _semiminorAxis;
    private float eccentricityTolerance = 0.05f;
    private float _argumentOfPeriapsis;
    private float eccentricity;

    public float SemimajorAxis
    {
        get
        {
            return _semimajorAxis;
        }
        private set
        {
            _semimajorAxis = value;
        }
    }
    public float SemiminorAxis
    {
        get
        {
            return _semiminorAxis;
        }
        private set
        {
            _semiminorAxis = value;
        }
    }

    public float ArgumentOfPeriapsis
    {
        get
        {
            return orbitalBody.CalculateArgumentOfPeriapse();
        }
    }

    [Range(3, 36)]
    public int segments;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        orbitalBody = GetComponent<GravityAffected>();
        //BuildEllipse();
    }
    
    private void BuildEllipse()
    {
        
        SemimajorAxis = orbitalBody.CalculateSemimajorAxis();
        CalculateSemiminorAxis();
        Debug.Log(ArgumentOfPeriapsis);
        if ((SemimajorAxis < orbitalBody.CurrentGravitySource.Radius) || (SemiminorAxis < orbitalBody.CurrentGravitySource.Radius)){
            //Ignoring when on surface-ish
            return;
        }

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * SemimajorAxis, Mathf.Cos(angle) * SemiminorAxis, 0);
            vertex = TranslateVector(vertex, new Vector3(eccentricity * SemimajorAxis, 0, 0));
            points[i] = RotateVertex(vertex, ArgumentOfPeriapsis + Mathf.PI);
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }

    private void Update()
    {
        BuildEllipse();
    }

    private void CalculateSemiminorAxis()
    {
        eccentricity = orbitalBody.CalculateEccentricityVector().magnitude;
        if (Mathf.Abs(eccentricity - 1f) < eccentricityTolerance)
        {
            SemiminorAxis = 0;
            return;
        }
        SemiminorAxis = SemimajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2));
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
}
