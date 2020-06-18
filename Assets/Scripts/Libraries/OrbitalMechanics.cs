using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalMechanics
{
    public static class Body
    {
        public static float StandardGravityParameter(float mass)
        {
            return Globals.GRAVITATIONALCONSTANT * mass;
        }
    }

    public static class Globals
    {
        public const float GRAVITATIONALCONSTANT = 1f;
        public enum TrajectoryType
        {
            Ellipse = 0,
            Hyperbola = 1
            //Circle = 2,
            //Parabola = 3,
        }
    }

    public static class Trajectory
    {
        public static float SpecificOrbitalEnergy(float mainMass, float orbitalMass, float semimajorAxis)
        {
            return -1f * Body.StandardGravityParameter(mainMass + orbitalMass) / (2f * semimajorAxis);
        }

        public static Vector2 GravitationalForceAtPosition(this GravitySource gravitySource, Vector2 position, float mass)
        {
            Vector2 distance = gravitySource.Position - position;
            float forceMagnitude = Globals.GRAVITATIONALCONSTANT * gravitySource.Mass * mass / Vector2.SqrMagnitude(distance);
            Vector2 force = forceMagnitude * distance.normalized;
            return force;
        }

        public static Vector3 EccentricityVector(Vector3 relativePosition, Vector3 relativeVelocity, float bodyMass)
        {
            return Vector3.Cross(relativeVelocity, SpecificRelativeAngularMomentum(relativePosition, relativeVelocity)) / Body.StandardGravityParameter(bodyMass) - relativePosition.normalized;
        }

        public static Vector3 EccentricityVector(Vector3 relativePosition, Vector3 relativeVelocity, Vector3 specificRelativeAngularMomentum, float bodyMass)
        {
            return Vector3.Cross(relativeVelocity, specificRelativeAngularMomentum) / Body.StandardGravityParameter(bodyMass) - relativePosition.normalized;
        }

        public static float SemimajorAxis(float orbitalRadius, float velocitySq, float mainMass)
        {
            float denom = (2 / orbitalRadius) - (velocitySq / (Globals.GRAVITATIONALCONSTANT * mainMass));
            return 1 / denom;
        }

        public static float SemimajorAxis(Vector3 relativePosition, float velocitySq, float mainMass)
        {
            float denom = (2 / relativePosition.magnitude) - (velocitySq / (Globals.GRAVITATIONALCONSTANT * mainMass));
            return 1 / denom;
        }

        public static float TrueAnomaly(Vector3 relativePosition, Vector3 relativeVelocity, Vector3 eccentricityVector)
        {
            float eccentricity = eccentricityVector.magnitude;
            float nu = eccentricity == 0f
                ? 0f // If circular orbit, true anomaly to be considered angle b/w position vector and (1,0,0)
                : Mathf.Acos(Vector2.Dot(eccentricityVector, relativePosition) / (eccentricityVector.magnitude * relativePosition.magnitude));

            if (Vector2.Dot(relativePosition, relativeVelocity) < 0f)
            {
                nu = 2 * Mathf.PI - nu;
            }
            return nu;
        }

        public static float TrueAnomaly(float eccentricity, float eccentricAnomaly, Vector3 specificRelativeAngularMomentum)
        {
            if (eccentricity == 0f)
                return eccentricAnomaly;

            // Angle b/w relative position vector and eccentricityVectory
            float sinNu = Mathf.Sqrt(1f - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(eccentricAnomaly);
            float cosNu = Mathf.Cos(eccentricAnomaly) - eccentricity;
            float nu = Mathf.Atan2(sinNu, cosNu);

            // Move [-pi, pi] range to [0, 2pi]
            float twoPi = 2f * Mathf.PI;
            nu = (nu + twoPi) % twoPi;
            return nu;
        }

        public static float EccentricAnomalyAtEpoch(Vector3 relativePosition, Vector3 relativeVelocity, float bodyMass, Vector3 eccentricityVector)
        {
            float trueAnomaly = TrueAnomaly(relativePosition, relativeVelocity, eccentricityVector);
            float eccentricity = eccentricityVector.magnitude;
            float E = Mathf.Atan2(Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(trueAnomaly), eccentricity + Mathf.Cos(trueAnomaly));

            return MathUtilities.Modulo(E, 2f * Mathf.PI);
        }

        public static float EccentricAnomalyAtEpoch(float trueAnomaly, float eccentricity)
        {
            float E = Mathf.Atan2(Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(trueAnomaly), eccentricity + Mathf.Cos(trueAnomaly));
            return MathUtilities.Modulo(E, 2f * Mathf.PI);
        }

        public static Vector3 SpecificRelativeAngularMomentum(Vector3 relativePosition, Vector3 relativeVelocity)
        {
            return Vector3.Cross(relativePosition, relativeVelocity);
        }

        public static float RadiusOfInfluence(float semimajorAxis, float orbitingMass, float mainMass)
        {
            return Mathf.Abs(semimajorAxis) * Mathf.Pow(orbitingMass / mainMass, 2f / 5f);
        }

        public static float OrbitalSpeed(float mainMass, float orbitalRadius, float semimajorAxis)
        {
            return Mathf.Sqrt(Globals.GRAVITATIONALCONSTANT * mainMass * ((2f / orbitalRadius) - (1f / semimajorAxis)));
        }

        public static Vector2 OrbitalDirection(float trueAnomaly, float flightPathAngle, bool clockWise)
        {
            float psi = trueAnomaly + Mathf.PI / 2 - flightPathAngle;
            float sin = clockWise
                ? -Mathf.Sin(psi)
                : Mathf.Sin(psi);

            return new Vector2(Mathf.Cos(psi), sin);
        }

        public static float FlightPathAngle(float eccentricity, float trueAnomaly)
        {
            float phi, denom;
            if (eccentricity >= 1f)
            {
                // Hyperbolic
                denom = 1f + eccentricity * Mathf.Cos(trueAnomaly);
                phi = Mathf.Atan(eccentricity * Mathf.Sin(trueAnomaly) / denom); // Atan?
            }
            else
            {
                denom = Mathf.Sqrt(1f + Mathf.Pow(eccentricity, 2) + 2f * eccentricity * Mathf.Cos(trueAnomaly));
                float num = 1f + eccentricity * Mathf.Cos(trueAnomaly);
                phi = Mathf.Acos(num / denom);
                phi = trueAnomaly > Mathf.PI
                    ? phi *= -1f
                    : phi;
            }
            return phi;
        }

        public static float ArgumentOfPeriapse(Vector3 eccentricityVector, Vector3 relativePosition)
        {
            float eccentricity = eccentricityVector.magnitude;
            if (eccentricity != 0f)
            {
                return Mathf.Atan2(eccentricityVector.y, eccentricityVector.x);
            }
            // If circular orbit, argument of periapse to be considered angle b/w position vector and (1,0,0)
            return Mathf.Atan2(relativePosition.y, relativePosition.x);
        }

        public static float SemiminorAxis(float semimajorAxis, float eccentricity)
        {
            if (eccentricity >= 1)
            {
                // Hyperbolic
                return -1f * semimajorAxis * Mathf.Sqrt(Mathf.Pow(eccentricity, 2) - 1);
            }
            return semimajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)); ;
        }

        public static float OrbitalPeriod(float meanMotion)
        {
            //FIXME: Meaningless for Hyperbolic orbit.
            return 2 * Mathf.PI / meanMotion;
        }

        public static float OrbitalPeriod(float semimajorAxis, float mainMass)
        {
            return 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(semimajorAxis, 3) / Body.StandardGravityParameter(mainMass));
        }

        public static float OrbitalRadius(float eccentricity, float semimajorAxis, float trueAnomaly)
        {
            // Always ends up positive because of the negative convention of semimajorAxis for hyperbolas
            float denom = 1f + (eccentricity * Mathf.Cos(trueAnomaly));
            float num = 1f - Mathf.Pow(eccentricity, 2);
            return semimajorAxis * num / denom;
        }

        public static Vector2 OrbitalPosition(float orbitalRadius, float trueAnomaly, bool clockWise)
        {
            float sin = clockWise
                ? -Mathf.Sin(trueAnomaly)
                : Mathf.Sin(trueAnomaly);
            return orbitalRadius * new Vector2(Mathf.Cos(trueAnomaly), sin);
        }

        public static float EccentricAnomalyAtEpoch(float orbitalDistance, float eccentricity, float semimajorAxis)
        {
            if (semimajorAxis < 0) // Hyperbolic
            {
                float semilatusRectum = SemilatusRectum(semimajorAxis, eccentricity);
                float coshE = (1f / eccentricity) * (1f - orbitalDistance / semilatusRectum) + (orbitalDistance * eccentricity) / semilatusRectum;
                return Mathf.Log(coshE + Mathf.Sqrt(Mathf.Pow(coshE, 2) - 1f));
            }
            return Mathf.Acos(Mathf.Clamp((-1f / eccentricity) * ((orbitalDistance / semimajorAxis) - 1f), -1f, 1f));
        }

        public static float MeanAnomalyAtEpoch(float eccentricAnomaly, float eccentricity)
        {
            if (eccentricity >= 1)
            {
                return eccentricity * MathUtilities.Sinh(eccentricAnomaly) - eccentricAnomaly;
            }
            return eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly);
        }

        public static float MeanAnomalyAtEpoch(float orbitalDistance, float eccentricity, float semimajorAxis)
        {
            float eccentricAnomaly = EccentricAnomalyAtEpoch(orbitalDistance, eccentricity, semimajorAxis);
            return eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly);
        }

        public static float MeanMotion(float bodyMass, float semimajorAxis)
        {
            float absSemimajorAxis;
            if (semimajorAxis < 0)
            {
                absSemimajorAxis = -1f * semimajorAxis;
            }
            else
            {
                absSemimajorAxis = semimajorAxis;
            }
            return Mathf.Sqrt(Body.StandardGravityParameter(bodyMass) / Mathf.Pow(absSemimajorAxis, 3));
        }

        public static float SemilatusRectum(float semimajorAxis, float eccentricity)
        {
            // Works out to be positive for ellipses and hyperbolas (e < 1 and a >= 0 vs e >= 1 and a < 0, respectively)
            return semimajorAxis * (1f - Mathf.Pow(eccentricity, 2));
        }
    }

    public static class HyperbolicTrajectory
    {
        public static Vector2[] Asymptotes(float trueAnomalyOfAsymptote, bool clockWise)
        {
            Vector2[] asymptotes = new Vector2[2];
            float sin = Mathf.Sin(trueAnomalyOfAsymptote);
            asymptotes[0] = new Vector2(Mathf.Cos(trueAnomalyOfAsymptote), -sin);
            asymptotes[1] = new Vector2(Mathf.Cos(trueAnomalyOfAsymptote), sin);
            return asymptotes;
        }

        public static float ExcessVelocity(float mass, float semimajorAxis)
        {
            return Mathf.Sqrt(-Body.StandardGravityParameter(mass) / semimajorAxis);
        }

        public static float TrueAnomalyOfAsymptote(float eccentricity, bool clockWise)
        {
            float nuInf = Mathf.Acos(-1f / eccentricity);
            return nuInf;
        }

        public static float HyperbolicAnomalyAtEpoch(float trueAnomaly, float eccentricity)
        {
            float sqrt = Mathf.Sqrt((eccentricity - 1f) / (eccentricity + 1f));
            return 2f * MathUtilities.ArcTanh(sqrt * Mathf.Tan(trueAnomaly / 2f));
        }

        public static float TrueAnomaly(Vector3 relativePosition, Vector3 relativeVelocity, float semimajorAxis, float eccentricity)
        {
            float cosNu = (Trajectory.SemilatusRectum(semimajorAxis, eccentricity) / relativePosition.magnitude - 1f) / eccentricity;
            float nu = Mathf.Acos(Mathf.Clamp(cosNu, -1, 1));
            if (Vector3.Dot(relativePosition, relativeVelocity) < 0f)
                nu *= -1f;
            return nu;
        }

        public static float TrueAnomaly(float eccentricity, float hyperbolicEccentricAnomaly, bool clockWise)
        {
            float nu = 2f * Mathf.Atan(MathUtilities.HalfTanh(hyperbolicEccentricAnomaly) * Mathf.Sqrt((eccentricity + 1f) / (eccentricity - 1f)));

            // move [-pi, pi] range to [0, 2pi]
            //float twoPi = 2f * Mathf.PI;
            //nu = (nu + twoPi) % twoPi; 
            return nu;
        }

        public static float OrbitalRadius(float eccentricity, float semimajorAxis, float hyperbolicEccentricAnomaly)
        {
            return semimajorAxis * (1f - eccentricity * MathUtilities.Cosh(hyperbolicEccentricAnomaly));
        }
    }

    public static class KeplerMethod
    {
        public static float MeanAnomaly(float meanAnomalyAtEpoch, float meanMotion, float timeSinceEpoch, bool modulate)
        {
            // Updated mean anomaly, assuming mean anomaly at epoch has been calculated!
            float M = meanAnomalyAtEpoch + meanMotion * timeSinceEpoch;

            if (modulate)
                M = MathUtilities.Modulo(M, 2 * Mathf.PI);
            return M;
        }

        public static float MeanAnomaly(float meanAnomalyAtEpoch, float bodyMass, float semimajorAxis, float timeSinceEpoch)
        {
            // Updated mean anomaly, assuming mean anomaly at epoch has been calculated!
            float M = meanAnomalyAtEpoch + Trajectory.MeanMotion(bodyMass, semimajorAxis) * timeSinceEpoch;
            return MathUtilities.Modulo(M, 2 * Mathf.PI);
        }

        public static float EccentricAnomaly(float meanAnomaly, float eccentricity, int maxIterations)
        {
            if (eccentricity == 0f)
                return meanAnomaly;

            // Newton's method
            int currentIter = 0;
            float E = meanAnomaly;
            if (eccentricity >= 1)
            {
                // Hyperbola
                while (true)
                {
                    currentIter += 1;
                    if (currentIter > maxIterations)
                        break;

                    float deltaE = (eccentricity * MathUtilities.Sinh(E) - E - meanAnomaly) / (eccentricity * MathUtilities.Cosh(E) - 1f);
                    E -= deltaE;
                    if (Mathf.Abs(deltaE) < 1e-6)
                        break;
                }
            }
            else
            {
                // Ellipse

                float lastDeltaE = 0f;
                float deltaE = 0f;
                while (true)
                {
                    currentIter += 1;
                    if (currentIter > maxIterations)
                        break;
                    lastDeltaE = deltaE;
                    deltaE = (E - eccentricity * Mathf.Sin(E) - meanAnomaly) / (1f - eccentricity * Mathf.Cos(E));
                    if (deltaE * lastDeltaE <= 0)
                        break;

                    if (eccentricity > 0.95)
                    {
                        Debug.Log(deltaE);
                    }
                    E -= deltaE;
                    if (Mathf.Abs(deltaE) < 1e-6)
                        break;
                }
            }
            return E;
        }
    }
}

