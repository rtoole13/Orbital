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
        // returns the component of vector perpendicular to direction
        return vector - vector.Projection(direction);
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
}
