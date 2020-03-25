using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
    #region VECTORS
    public static Vector2 RotateVector(this Vector2 vector, float angle)
    {
        return new Vector2(vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle), vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle));
    }
    #endregion VECTORS
    #region SCALARS
    public static float Modulo(float number, float modulus)
    {
        return number - (modulus * Mathf.Floor(number / modulus));
    }
    public static float RescaleFloat(float x, float currentMin, float currentMax, float newMin, float newMax)
    {
        float currentRange = currentMax - currentMin;
        float newRange = newMax - newMin;
        return (x - currentMin) * (newRange / currentRange) + newMin;
    }
    #endregion 
}
