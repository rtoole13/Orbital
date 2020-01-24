using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(GravityAffected))]
public class Ellipse : MonoBehaviour
{
    LineRenderer lineRenderer;
    GravityAffected orbitalBody;

    [Range(3, 36)]
    public int segments;
    public float semimajorAxis;
    public float semiminorAxis;

    [Range(0, 2*Mathf.PI)]
    public float angleToPeriapse;
    public Vector2 translation;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        orbitalBody = GetComponent<GravityAffected>();
        //BuildEllipse();
    }
    
    private void BuildEllipse()
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * semimajorAxis, Mathf.Cos(angle) * semiminorAxis, 0);
            vertex = RotateVertex(vertex);
            points[i] = TranslateVertex(vertex);
            //points[i] = vertex;
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }

    /*
    private void OnValidate()
    {
        BuildEllipse();
    }
    */
    private void Update()
    {
        BuildEllipse();
        Debug.Log(calculateSemiminorAxis());
    }

    private float calculateSemimajorAxis()
    {
        if (!orbitalBody.gravitySource)
            return Mathf.Infinity;
        Rigidbody2D body = orbitalBody.GetComponent<Rigidbody2D>();
        float denom = 2 / orbitalBody.SourceDistance - body.velocity.sqrMagnitude / (orbitalBody.gravitySource.GRAVITYCONSTRANT * orbitalBody.gravitySource.Mass);
        return 1 / denom;
    }

    private float calculateSemiminorAxis()
    {
        Rigidbody2D body = orbitalBody.GetComponent<Rigidbody2D>();
        float num = Mathf.Pow(body.transform.position.y, 2) * Mathf.Pow(calculateSemimajorAxis(), 2);
        float denom = 1 - Mathf.Pow(body.transform.position.x, 2);
        return Mathf.Sqrt(num / denom);
    }

    private float calculateTrueAnomoly()
    {
        return 0f;
    }

    private Vector3 RotateVertex(Vector3 vertex)
    {
        return new Vector3(vertex.x * Mathf.Cos(angleToPeriapse) - vertex.y * Mathf.Sin(angleToPeriapse),
                       vertex.x * Mathf.Sin(angleToPeriapse) + vertex.y * Mathf.Cos(angleToPeriapse), 0);
    }

    private Vector3 TranslateVertex(Vector3 vertex)
    {
        return new Vector3(vertex.x + translation.x, vertex.y + translation.y, 0);
    }
}
