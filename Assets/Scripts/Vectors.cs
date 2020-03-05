using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vectors
{
   public static Vector2 RotateVector(this Vector2 vector, float angle)
    {
        return new Vector2(vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle), vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle));
    }
}
