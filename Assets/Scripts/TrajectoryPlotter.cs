using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(OrbitalBody))]
public class TrajectoryPlotter : MonoBehaviour
{
    private float[] lineWidthRange;
    private float currentLineWidth;
    private LineRenderer lineRenderer;
    private OrbitalBody orbitalBody;
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
        get { return orbitalBody.SemiminorAxis; }
    }
    #endregion GETSET

    #region UNITY
    private void Awake()
    {
        lineWidthRange = new float[2];
        lineWidthRange[0] = 0.5f;
        lineWidthRange[1] = 5f;
        lineRenderer = GetComponent<LineRenderer>();
        orbitalBody = GetComponent<OrbitalBody>();
        CameraController.OrthographicSizeChangeEvent += AdjustLineThickness;
    }

    private void Start()
    {
        
    }

    private void OnDisable()
    {
        CameraController.OrthographicSizeChangeEvent -= AdjustLineThickness;
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
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * SemimajorAxis, Mathf.Cos(angle) * SemiminorAxis, 1f);
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
                       vertex.x * Mathf.Sin(angle) + vertex.y * Mathf.Cos(angle), vertex.z);
    }
    
    private Vector3 TranslateVector(Vector3 vertex, Vector3 distance)
    {
        return new Vector3(vertex.x + distance.x, vertex.y + distance.y, vertex.z);
    }

    private void AdjustLineThickness(float minOrthoSize, float maxOrthoSize, float targetOrthoSize)
    {
        float newLineWidth = MathUtilities.RescaleFloat(targetOrthoSize, minOrthoSize, maxOrthoSize, lineWidthRange[0], lineWidthRange[1]);
        lineRenderer.startWidth = lineRenderer.endWidth = newLineWidth;
    }

    #region GIZMOS
    private void OnDrawGizmos()
    {
        if (orbitalBody == null || orbitalBody.CurrentGravitySource == null)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(orbitalBody.CurrentGravitySource.transform.position, SemimajorAxis * (1f - orbitalBody.Eccentricity) * orbitalBody.EccentricityVector.normalized);
        
    }
    #endregion GIZMOS
}
