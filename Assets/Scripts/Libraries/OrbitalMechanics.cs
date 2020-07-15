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
            Hyperbola = 1,
            Parabola = 2,
        }
        //public enum UpdateMethods
        //{
        //    Kepler = 0,
        //    UniversalVariable = 1,
        //}
    }

    public static class Trajectory
    {
        public static float SpecificOrbitalEnergy(float mainMass, float semimajorAxis)
        {
            return -1f * Body.StandardGravityParameter(mainMass) / (2f * semimajorAxis);
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
            // Vis-viva equation, "living force"
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

        public static float FlightPathAngle(float specificRelativeAngularMomentum, Vector2 orbitalPosition, Vector2 orbitalVelocity)
        {
            float radius = orbitalPosition.magnitude;
            float speed = orbitalVelocity.magnitude;

            return Mathf.Acos(specificRelativeAngularMomentum / (radius * speed)) * Mathf.Sign(Vector2.Dot(orbitalPosition, orbitalVelocity));
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

    public static class ParabolicTrajectory
    {
        //STUFF
    }

    public static class KeplerMethod
    {
        public static float TrueAnomaly(float eccentricity, float eccentricAnomaly)
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
            float trueAnomaly = Trajectory.TrueAnomaly(relativePosition, relativeVelocity, eccentricityVector);
            float eccentricity = eccentricityVector.magnitude;
            float E = Mathf.Atan2(Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(trueAnomaly), eccentricity + Mathf.Cos(trueAnomaly));

            return MathUtilities.Modulo(E, 2f * Mathf.PI);
        }

        public static float EccentricAnomalyAtEpoch(float trueAnomaly, float eccentricity)
        {
            float E = Mathf.Atan2(Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2)) * Mathf.Sin(trueAnomaly), eccentricity + Mathf.Cos(trueAnomaly));
            return MathUtilities.Modulo(E, 2f * Mathf.PI);
        }

        public static float OrbitalPeriod(float meanMotion)
        {
            //FIXME: Meaningless for Hyperbolic orbit.
            return 2 * Mathf.PI / meanMotion;
        }

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
            float M = meanAnomalyAtEpoch + MeanMotion(bodyMass, semimajorAxis) * timeSinceEpoch;
            return MathUtilities.Modulo(M, 2 * Mathf.PI);
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
                float deltaE = 0f;
                while (true)
                {
                    currentIter += 1;
                    if (currentIter > maxIterations)
                        break;
                    deltaE = (E - eccentricity * Mathf.Sin(E) - meanAnomaly) / (1f - eccentricity * Mathf.Cos(E));

                    E -= deltaE;
                    if (Mathf.Abs(deltaE) < 1e-6)
                        break;
                }
            }
            return E;
        }

        public static float EccentricAnomalyAtEpoch(float orbitalDistance, float eccentricity, float semimajorAxis)
        {
            if (semimajorAxis < 0) // Hyperbolic
            {
                float semilatusRectum = Trajectory.SemilatusRectum(semimajorAxis, eccentricity);
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
    }

    public static class UniversalVariableMethod
    {
        public static float Xdot(float mainMass, float orbitalRadius)
        {
            return Body.StandardGravityParameter(mainMass) / orbitalRadius;
        }
        
        public static float slopeTimeVsX(float mainMass, float orbitalRadius)
        {
            return orbitalRadius / Body.StandardGravityParameter(mainMass);
        }

        public static float OrbitalRadius(float semimajorAxis, float eccentricity, float x, float constantOfIntegration)
        {
            return semimajorAxis * (1f + eccentricity * (x + constantOfIntegration) / Mathf.Sqrt(semimajorAxis));
        }

        public static float StumpffC(float z)
        {
            if (Mathf.Abs(z) < 1e-3)
            {
                // C = 1/2! - z/4! + z^2/6! - ...
                return 0.5f - z / 24f;
            }

            if (z > 0)
            {
                return (1f - Mathf.Cos(Mathf.Sqrt(z))) / z;
            }
            return (1f - MathUtilities.Cosh(Mathf.Sqrt(-z))) / z;
            
        }

        public static float StumpffS(float z)
        {
            if (Mathf.Abs(z) < 1e-3)
            {
                // S = 1/3! - z/5! + z^2/7! - ...
                return (1f / 6f) - z / 120f;
            }

            if (z > 0)
            {
                return (Mathf.Sqrt(z) - Mathf.Sin(Mathf.Sqrt(z))) / Mathf.Pow(z, 3f / 2f);
            }
            float negZ = -z;
            return (MathUtilities.Sinh(Mathf.Sqrt(negZ)) - Mathf.Sqrt(negZ)) / Mathf.Pow(negZ, 3f / 2f);
        }

        public static float StumpffS(float x, float semimajorAxis)
        {
            float z = Mathf.Pow(x, 2) / semimajorAxis;
            return StumpffS(z);
        }

        public static void UniversalVariable(ref float x, float timeOfFlight, float mainMass, float semimajorAxis, float orbitalRadius, Vector2 orbitalPosition, Vector2 orbitalVelocity)
        {
            // Heavily pulled from Fundamentals of Astrodynamics by Bate et. al.

            // Constants
            int maxIterations = 6;
            float sqrtMu = Mathf.Sqrt(Body.StandardGravityParameter(mainMass));
            float rDotV = Vector2.Dot(orbitalPosition, orbitalVelocity);
            float rDotVbyRootMu = rDotV / sqrtMu;
            float z, c, s;
            // Initialize
            float t = 0;
            float dTdX = 0;
            int currentIter = 0;
            float deltaX = Mathf.Infinity;
            while (true)
            {
                currentIter += 1;
                if (currentIter > maxIterations)
                    break;

                z = Mathf.Pow(x, 2) / semimajorAxis;
                c = StumpffC(z);
                s = StumpffS(z);
                // Xn+1 = Xn + (t - tn)/(dt/dX)|X=Xn
                t = rDotVbyRootMu * Mathf.Pow(x, 2) * c + (1f - (orbitalRadius / semimajorAxis)) * Mathf.Pow(x, 3) * s + orbitalRadius * x; // Note that this is actually mu * t
                dTdX = Mathf.Pow(x, 2) * c + rDotVbyRootMu * x * (1f - z * s) + orbitalRadius * (1f - z * c); // Note that this is actually mu * dTdX, mu cancels
                deltaX = (timeOfFlight - t) / dTdX;
                x += deltaX;
                if (Mathf.Abs(deltaX) < 1e-6)
                    break;
            }
            //Debug.LogFormat("z: {0}, c: {1}, s: {2}, iter: {3}", z, c, s, currentIter);
            //Debug.LogFormat("x: {0}, iter: {1}", x, currentIter);
        }

        // FORMULATED IN TERMS OF STATE VARS
        //public static float VariableF(float semimajorAxis, float orbitalRadius, float x)
        //{
        //    if (semimajorAxis < 0)
        //        semimajorAxis *= -1f;
        //    return 1f - (semimajorAxis / orbitalRadius) * (1f - Mathf.Cos(x / Mathf.Sqrt(semimajorAxis)));
        //}

        // FORMULATED IN TERMS OF C
        public static float VariableF(float x, float orbitalRadius, float constantC)
        {
            return 1f - (Mathf.Pow(x, 2) / orbitalRadius) * constantC;
        }

        public static float VariableG(float timeOfFlight, float x, float mainMass, float stumpffS)
        {
            float sqrtMu = Mathf.Sqrt(Body.StandardGravityParameter(mainMass));
            return timeOfFlight - (Mathf.Pow(x, 3) / sqrtMu) * stumpffS;
        }

        public static Vector2 OrbitalPosition(float f, float g, Vector2 initialPosition, Vector2 initialVelocity)
        {
            return f * initialPosition + g * initialVelocity;
        }

        public static Vector2 OrbitalVelocity(float fPrime, float gPrime, Vector2 initialPosition, Vector2 initialVelocity)
        {
            //IMPORTANT NOTE: Must be calculated after orbital position is determined, as calculation of fPrime and gPrime depend on the updated position
            return fPrime * initialPosition + gPrime * initialVelocity;
        }

        public static float VariableFprime(float mainMass, float initialOrbitalRadius, float orbitalRadius, float x, float z, float stumpffS)
        {
            float sqrtMu = Mathf.Sqrt(Body.StandardGravityParameter(mainMass));
            return (sqrtMu / (initialOrbitalRadius * orbitalRadius)) * x * (z * stumpffS - 1f);
        }

        public static float VariableGprime(float x, float orbitalRadius, float stumpffC)
        {
            return 1f - (Mathf.Pow(x, 2) * stumpffC / orbitalRadius);
        }
    }
}

