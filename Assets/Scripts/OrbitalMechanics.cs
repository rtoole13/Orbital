﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OrbitalMechanics
{
    public static float GRAVITATIONALCONSTANT = 1f;
    public enum TrajectoryType
    {
        Ellipse = 0,
        Hyperbola = 1
    }
    #region GENERAL
    public static float StandardGravityParameter(float massA, float massB)
    {
        return GRAVITATIONALCONSTANT * (massA + massB);
    }

    public static Vector2 GravitationalForceAtPosition(this GravitySource gravitySource, Vector2 position, float mass)
    {
        Vector2 distance = gravitySource.Position - position;
        float forceMagnitude = OrbitalMechanics.GRAVITATIONALCONSTANT * gravitySource.Mass * mass / Vector2.SqrMagnitude(distance);
        Vector2 force = forceMagnitude * distance.normalized;
        return force;
    }

    #endregion GENERAL

    #region STATEVECTORS
    public static Vector3 SpecificRelativeAngularMomentum(Vector3 relativePosition, Vector3 relativeVelocity)
    {
        return Vector3.Cross(relativePosition, relativeVelocity);
    }

    public static Vector3 EccentricityVector(Vector3 relativePosition, Vector3 relativeVelocity, float mainMass, float orbitalMass)
    {
        return Vector3.Cross(relativeVelocity, SpecificRelativeAngularMomentum(relativePosition, relativeVelocity)) / StandardGravityParameter(mainMass, orbitalMass) - relativePosition.normalized;
    }

    public static Vector3 EccentricityVector(Vector3 relativePosition, Vector3 relativeVelocity, Vector3 specificRelativeAngularMomentum, float mainMass, float orbitalMass)
    {
        return Vector3.Cross(relativeVelocity, specificRelativeAngularMomentum) / StandardGravityParameter(mainMass, orbitalMass) - relativePosition.normalized;
    }

    public static float SemimajorAxis(float orbitalRadius, float velocitySq, float mainMass)
    {
        float denom = (2 / orbitalRadius) - (velocitySq / (GRAVITATIONALCONSTANT * mainMass));
        return 1 / denom;
    }

    public static float SemimajorAxis(Vector3 relativePosition, float velocitySq, float mainMass)
    {
        float denom = (2 / relativePosition.magnitude) - (velocitySq / (GRAVITATIONALCONSTANT * mainMass));
        return 1 / denom;
    }

    public static float TrueAnomaly(Vector3 relativePosition, Vector3 relativeVelocity, float mainMass, float orbitalMass)
    {
        Vector3 eccentricityVector = EccentricityVector(relativePosition, relativeVelocity, mainMass, orbitalMass);
        float nu = Mathf.Acos(Vector2.Dot(eccentricityVector, relativePosition) / (eccentricityVector.magnitude * relativePosition.magnitude));

        if (Vector2.Dot(relativePosition, relativeVelocity) < 0f)
        {
            nu = 2 * Mathf.PI - nu;
        }
        return nu;
    }

    public static float EccentricAnomalyAtEpoch(Vector3 relativePosition, Vector3 relativeVelocity, float mainMass, float orbitalMass, float eccentricity)
    {
        float trueAnomaly = TrueAnomaly(relativePosition, relativeVelocity, mainMass, orbitalMass);
        float E = Mathf.Atan2(Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(trueAnomaly), eccentricity + Mathf.Cos(trueAnomaly));

        float modulus = 2f * Mathf.PI;
        return E - (modulus * Mathf.Floor(E / modulus)); //FIXME: PROPER MODULUS OPERATOR -- MAKE GENERIC
    }

    public static float EccentricAnomalyAtEpoch(float trueAnomaly, float eccentricity)
    {
        float E = Mathf.Atan2(Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(trueAnomaly), eccentricity + Mathf.Cos(trueAnomaly));

        float modulus = 2f * Mathf.PI;
        return E - (modulus * Mathf.Floor(E / modulus)); //FIXME: PROPER MODULUS OPERATOR -- MAKE GENERIC
    }

    #endregion STATEVECTORS

    #region ORBITALELEMENTS
    public static Vector2 OrbitalVelocity(Vector3 specificRelativeAngularMomentum, Vector3 orbitalRadius, float mainMass, float semimajorAxis)
    {
        // NOTE: Specific relative angular momentum must have been calculated at a previous point, when accel was constant.

        // Get direction
        Vector2 direction = Vector3.Cross(specificRelativeAngularMomentum, orbitalRadius);

        // Get speed via vis-viva
        float speed = Mathf.Sqrt(GRAVITATIONALCONSTANT * mainMass * ((2 / orbitalRadius.magnitude) - (1 / semimajorAxis)));

        return speed * direction;
    }

    public static Vector2 OrbitalVelocity(float meanMotion, float eccentricAnomaly, float eccentricity, float semimajorAxis)
    {
        float deltaE = meanMotion / (1f - (eccentricity * Mathf.Cos(eccentricAnomaly)));
        return new Vector2(-Mathf.Sin(eccentricAnomaly), Mathf.Cos(eccentricAnomaly) * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2))) * semimajorAxis * deltaE;

    }
    public static float SpecificOrbitalEnergy(float mainMass, float orbitalMass, float semimajorAxis)
    {
        return -1f * StandardGravityParameter(mainMass, orbitalMass) / (2f * semimajorAxis);
    }

    public static float ArgumentOfPeriapse(Vector3 eccentricityVector)
    {
        return Mathf.Atan2(eccentricityVector.y, eccentricityVector.x);
    }

    public static float SemiminorAxis(float semimajorAxis, float eccentricity)
    {
        if (eccentricity >= 1)
        {
            return -1f * semimajorAxis * Mathf.Sqrt(Mathf.Pow(eccentricity, 2) - 1);
        }
        return semimajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)); ;
    }

    public static float TrueAnomaly(float eccentricity, float eccentricAnomaly, Vector3 specificRelativeAngularMomentum)
    {
        // Angle b/w relative position vector and eccentricityVectory
        float sinNu = Mathf.Sqrt(1f - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(eccentricAnomaly);
        float cosNu = Mathf.Cos(eccentricAnomaly) - eccentricity;
        float nu = Mathf.Atan2(sinNu, cosNu);

        if (specificRelativeAngularMomentum.z < 0f)
            nu = 2 * Mathf.PI - nu; //CW orbit when switched from iterative

        // move [-pi, pi] range to [0, 2pi]
        float twoPi = 2f * Mathf.PI;
        nu = (nu + twoPi) % twoPi; //FIXME Shouldn't work in the CW orbit case
        return nu;
    }

    public static float OrbitalPeriod(float meanMotion)
    {
        return 2 * Mathf.PI / meanMotion;
    }

    public static float OrbitalRadius(float eccentricity, float trueAnomaly, float semimajorAxis)
    {
        float num = 1f - Mathf.Pow(eccentricity, 2);
        float denom = 1f + (eccentricity * Mathf.Cos(trueAnomaly));

        return semimajorAxis * num / denom;
    }

    public static Vector2 OrbitalPosition(float eccentricity, float semimajorAxis, float trueAnomaly)
    {
        //position in both cases is identical

        //From TrueAnomaly -> radius -> Cartesian
        float orbitalRadius = OrbitalRadius(eccentricity, trueAnomaly, semimajorAxis);

        //Convert to polar coordinates
        return orbitalRadius * new Vector2(Mathf.Cos(trueAnomaly), Mathf.Sin(trueAnomaly));

        //FROM EccentricAnomaly directly to Cartesian
        //return new Vector2(Mathf.Cos(EccentricAnomaly) - Eccentricity, Mathf.Sin(EccentricAnomaly) * Mathf.Sqrt(1 - Mathf.Pow(Eccentricity, 2))) * SemimajorAxis; 
    }

    public static float EccentricAnomalyAtEpoch(Vector3 relativePosition, float eccentricity, float semimajorAxis)
    {
        return Mathf.Acos(Mathf.Clamp((-1f / eccentricity) * ((relativePosition.magnitude / semimajorAxis) - 1f), -1f, 1f));

    }

    public static float EccentricAnomalyAtEpoch(float orbitalDistance, float eccentricity, float semimajorAxis)
    {
        return Mathf.Acos(Mathf.Clamp((-1f / eccentricity) * ((orbitalDistance / semimajorAxis) - 1f), -1f, 1f));

    }

    public static float MeanAnomalyAtEpoch(float eccentricAnomaly, float eccentricity)
    {
        return eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly);
    }

    public static float MeanAnomalyAtEpoch(float orbitalDistance, float eccentricity, float semimajorAxis)
    {
        float eccentricAnomaly = EccentricAnomalyAtEpoch(orbitalDistance, eccentricity, semimajorAxis);
        return eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly);
    }

    public static float MeanMotion(float mainMass, float orbitalMass, float semimajorAxis)
    {
        return Mathf.Sqrt(StandardGravityParameter(mainMass, orbitalMass) / Mathf.Pow(semimajorAxis, 3));
    }

    public static float MeanAnomaly(float meanAnomalyAtEpoch, float meanMotion, float timeSinceEpoch)
    {
        // Updated mean anomaly, assuming mean anomaly at epoch has been calculated!
        return meanAnomalyAtEpoch + meanMotion * timeSinceEpoch;
    }

    public static float MeanAnomaly(float meanAnomalyAtEpoch, float mainMass, float orbitalMass, float semimajorAxis, float timeSinceEpoch)
    {
        // Updated mean anomaly, assuming mean anomaly at epoch has been calculated!
        return meanAnomalyAtEpoch + MeanMotion(mainMass, orbitalMass, semimajorAxis) * timeSinceEpoch;
    }

    public static float EccentricAnomaly(float meanAnomaly, float eccentricity, int maxIterations)
    {
        int currentIter = 0;
        float E = meanAnomaly;
        while (true)
        {
            currentIter += 1;
            if (currentIter > maxIterations)
                break;

            float deltaE = (E - eccentricity * Mathf.Sin(E) - meanAnomaly) / (1f - eccentricity * Mathf.Cos(E));
            E -= deltaE;
            if (Mathf.Abs(deltaE) < 1e-6)
                break;
        }
        return E;
    }
    #endregion ORBITALELEMENTS
}
