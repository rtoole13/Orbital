using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
    #region VECTORS
    public static Vector2 RotateVector(this Vector2 vector, float angle)
    {
        // Radians
        return new Vector2(vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle), vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle));
    }

    public static Vector2 Projection(this Vector2 vector, Vector2 direction)
    {
        // Projects vector onto direction, returning the component of vector parallel to direction
        float dotNum = Vector2.Dot(vector, direction);
        float dotDenom = Vector2.Dot(direction, direction);
        return (dotNum / dotDenom) * direction;
    }

    public static Vector2 Rejection(this Vector2 vector, Vector2 direction)
    {
        // Returns the component of vector perpendicular to direction
        return vector - vector.Projection(direction);
    }

    #endregion VECTORS
    #region SCALARS
    public static float Modulo(float number, float modulus)
    {
        return number - (modulus * Mathf.Floor(number / modulus));
    }

    public static int IntModulo(int number, int modulus)
    {
        return number - (modulus * (number / modulus));
    }

    public static float RescaleFloat(float x, float currentMin, float currentMax, float newMin, float newMax)
    {
        float currentRange = currentMax - currentMin;
        float newRange = newMax - newMin;
        return (x - currentMin) * (newRange / currentRange) + newMin;
    }

    public static float Cosh(float value)
    {
        return (Mathf.Exp(value) + Mathf.Exp(-value)) / 2f;
    }

    public static float ArcCosh(float value)
    {
        return Mathf.Log(value + Mathf.Sqrt(Mathf.Pow(value, 2) - 1f));
    }

    public static float Sinh(float value)
    {
        return (Mathf.Exp(value) - Mathf.Exp(-value)) / 2f;
    }

    public static float ArcSinh(float value)
    {
        return Mathf.Log(value + Mathf.Sqrt(Mathf.Pow(value, 2) + 1f));

    }

    public static float Tanh(float value)
    {
        return Sinh(value) / Cosh(value);
    }

    public static float ArcTanh(float value)
    {
        return 0.5f * (Mathf.Log(1f + value) - Mathf.Log(1f - value));
    }

    public static float HalfTanh(float value)
    {
        return Sinh(value) / (Cosh(value) + 1f);
    }

    #endregion
    #region GEOMETRY
    public static SegmentIntersection GetLineIntersection(Vector2 pointA, Vector2 pointB, Vector2 pointC, Vector2 pointD)
    {
        // Return point where Line 1 (pointA and point B) intersects with Line 2 (pointC and pointD) by means of SegmentIntersection struct.
        // Also returns the shortest distance between pointA vs Line 2 and pointB vs Line 2
        // Line 1 x: pointA.x + t * (pointB.x - pointA.x)
        // Line 1 y: pointA.y + t * (pointB.y - pointA.y)

        // Line 2 x: pointC.x + u * (pointD.x - pointC.x)
        // Line 2 y: pointC.y + u * (pointD.y - pointC.y)

        float t, u;
        float detNumeratorT, detDenom, detNumeratorU;
        detNumeratorT = (pointA.x - pointC.x) * (pointC.y - pointD.y) - (pointA.y - pointC.y) * (pointC.x - pointD.x);
        detDenom = (pointA.x - pointB.x) * (pointC.y - pointD.y) - (pointA.y - pointB.y) * (pointC.x - pointD.x);
        if (detDenom == 0) // Could be coincident. Should perhaps check
            return new SegmentIntersection(false, Vector2.positiveInfinity, 0f, pointA);

        detNumeratorU = (pointA.x - pointB.x) * (pointA.y - pointC.y) - (pointA.y - pointB.y) * (pointA.x - pointC.x);

        t = detNumeratorT / detDenom;
        u = -detNumeratorU / detDenom;
        
        Vector2 diff = pointD - pointC;
        Vector2 intersectionPoint = pointC + u * diff;
        bool onSegment = (t >= 0f && t <= 1f && u >= 0f && u <= 1f);
        float minDist;
        Vector2 closestPoint;
        if (!onSegment)
        {
            // Calculate distance of pointA, pointB, and the midpoint to Line 2 verts
            // NOTE: Vector rejection will not work in this case, considering it would measure distance to the LINE, not segment.
            Vector2 midpoint = 0.5f * (pointA + pointB);
            float distA = Mathf.Min((pointA - pointC).magnitude, (pointA - pointD).magnitude);
            float distB = Mathf.Min((pointB - pointC).magnitude, (pointB - pointD).magnitude);
            float distMid = Mathf.Min((midpoint - pointC).magnitude, (midpoint - pointD).magnitude);

            minDist = distA;
            closestPoint = pointA;
            if (distB < distA)
            {
                minDist = distB;
                closestPoint = pointB;
            }
            if (distMid < distB)
            {
                minDist = distMid;
                closestPoint = midpoint;
            }
        }
        else
        {
            minDist = 0f;
            closestPoint = intersectionPoint;
        }

        
        return new SegmentIntersection(onSegment, intersectionPoint, minDist, closestPoint);
    }

    public static List<SegmentIntersection> GetPolygonIntersections(Vector3[] polygonVerticesA, Vector3[] polygonVerticesB)
    {
        // Should add some validation here
        List<SegmentIntersection> intersections = new List<SegmentIntersection>();
        for (int i = 0; i < polygonVerticesA.Length - 1; i++)
        {
            for (int j = 0; j < polygonVerticesB.Length - 1; j++)
            {
                SegmentIntersection intersection = GetLineIntersection(polygonVerticesA[i], polygonVerticesA[i + 1], polygonVerticesB[j], polygonVerticesB[j + 1]);
                if (!intersection.OnSegment)
                    continue;

                intersections.Add(intersection);
            }
        }
        return intersections;
    }

    public static List<SegmentIntersection> GetClosestPointsBetweenPolygons(Vector3[] polygonVerticesA, Vector3[] polygonVerticesB)
    {
        // Returns a list of intersections as Vector2s. Or, in the case of no intersection the vertex in polygonVerticesA closest to polygonB
        List<SegmentIntersection> intersections = new List<SegmentIntersection>();
        float minDist = Mathf.Infinity;
        Debug.LogFormat("countA: {0}, countB: {1}", polygonVerticesA.Length, polygonVerticesB.Length);
        SegmentIntersection closestIntersection = new SegmentIntersection(false, polygonVerticesA[0], Mathf.Infinity, polygonVerticesA[0]);
        for (int i = 0; i < polygonVerticesA.Length - 1; i++)
        {
            for (int j = 0; j < polygonVerticesB.Length - 1; j++)
            {
                SegmentIntersection intersection = GetLineIntersection(polygonVerticesA[i], polygonVerticesA[i + 1], polygonVerticesB[j], polygonVerticesB[j + 1]);
                if (!intersection.OnSegment)
                {
                    // Not on segment
                    // Consider possibility that this is the closest segment interaction
                    // 
                    if (intersection.MinDist < minDist) 
                    {
                        minDist = intersection.MinDist;
                        closestIntersection = intersection;
                    }
                    continue;
                }

                intersections.Add(intersection);
            }
        }
        if (intersections.Count == 0)
            intersections.Add(closestIntersection); // No intersections. Take vertex in A closest to B

        Debug.Log(intersections[0].MinDist);
        return intersections;

    }
    public static Vector3[] TestCircle(Vector3 center, float radius, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            Vector3 vertex = radius * new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f);
            points[i] = vertex + center;
        }
        points[segments] = points[0];
        return points;
    }

    #endregion GEOMETRY
    #region OTHER
    public static Vector2 GetClosestPoint(float semimajorA, float semiminorA, float semimajorB, float semiminorB)
    {
        Vector2 point;
        if (semimajorA >= 0 && semimajorB >= 0)
        {
            // ellipse v ellipse
            point = ClosestPointEllipseToEllipse(semimajorA, semiminorA, semimajorB, semiminorB);
        }
        else if (semimajorA >= 0 && semimajorB < 0)
        {
            //ellipse v hyperbola
            point = ClosestPointEllipseToHyperbola(semimajorA, semiminorB, semimajorB, semiminorB);
        }
        else if ((semimajorA < 0 && semimajorB >= 0))
        {
            //hyperbola v ellipse
            point = ClosestPointEllipseToHyperbola(semimajorB, semiminorB, semimajorA, semiminorA);
        }
        else
        {
            //hyperbola v hyperbola
            point = ClosestPointHyperbolaToHyperbola(semimajorA, semiminorA, semimajorB, semiminorB);

        }
        return point;
    }

    public static Vector2 ClosestPointEllipseToEllipse(float semimajorA, float semiminorA, float semimajorB, float semiminorB)
    {
        throw new System.NotImplementedException("Not yet implemented");
    }

    public static Vector2 ClosestPointEllipseToHyperbola(float ellipseA, float ellipseB, float hyperbolaA, float hyperbolaB)
    {
        throw new System.NotImplementedException("Not yet implemented");
    }

    public static Vector2 ClosestPointHyperbolaToHyperbola(float hyperbolaA, float hyperbolaB, float hyperbolaC, float hyperbolaD)
    {
        throw new System.NotImplementedException("Not yet implemented");
    }
    #endregion
}

public struct SegmentIntersection
{
    public SegmentIntersection(bool onSegment, Vector2 point, float minDist, Vector2 closestPoint)
    {
        OnSegment = onSegment;
        Point = point;
        MinDist = minDist;
        MinDistSq = minDist * minDist;
        ClosestPoint = closestPoint;
    }
    public bool OnSegment { get; }
    public Vector2 Point { get; }
    public float MinDist { get; }
    public float MinDistSq { get; }
    public Vector2 ClosestPoint { get; }
}
