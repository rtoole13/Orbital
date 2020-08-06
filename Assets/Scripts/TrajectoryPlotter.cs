using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPlotter : LinePlotter
{
    public void BuildEllipticalTrajectory(float semimajorAxis, float semiminorAxis, float eccentricity, float argumentOfPeriapse)
    {
        lineRenderer.loop = true;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = new Vector3(Mathf.Sin(angle) * semimajorAxis, Mathf.Cos(angle) * semiminorAxis, zepth);
            vertex = TranslateVector(vertex, new Vector3(-semimajorAxis * eccentricity, 0, 0));
            points[i] = transform.worldToLocalMatrix * RotateVertex(vertex, argumentOfPeriapse);
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }

    public void BuildHyperbolicTrajectory(float semimajorAxis, float semiminorAxis, float eccentricity, float argumentOfPeriapse)
    {
        lineRenderer.loop = false;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = (((float)i / (float)segments) * 2f - 1f) * Mathf.PI;
            Vector3 vertex = new Vector3(semimajorAxis * MathUtilities.Cosh(angle), semiminorAxis * MathUtilities.Sinh(angle), 1f);
            vertex = TranslateVector(vertex, new Vector3(-semimajorAxis * eccentricity, 0, 0));
            points[i] = transform.worldToLocalMatrix * RotateVertex(vertex, argumentOfPeriapse);
        }
        lineRenderer.positionCount = segments;
        lineRenderer.SetPositions(points);
    }

    public void SetGradient(Gradient gradient)
    {
        lineRenderer.colorGradient = gradient;
    }
}
