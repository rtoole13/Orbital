using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeplerSolver : Solver
{
    // Kepler variables
    private float eccentricAnomaly;
    private float meanAnomaly;
    private float meanAnomalyAtEpoch;
    private float meanMotion;
    private int maxNewtonianMethodIterations = 6;


    private float _flightPathAngle;

    // Hyperbolic parameters
    private bool nearHyperbolicAsymptote;
    

    public KeplerSolver(float _eccentricAnomaly = 0f)
    {
        eccentricAnomaly = _eccentricAnomaly;
        meanAnomaly = 0f;
        meanAnomalyAtEpoch = 0f;
        meanMotion = 0f;
    }

    public override void InitializeSolver(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float sourceMass, Trajectory trajectory)
    {
        // Initialize orbital parameters
        base.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, sourceMass, trajectory);

        // Initialize Kepler elements
        InitializeKeplerElements(sourceRelativePosition, sourceRelativeVelocity);
    }
    
    private void InitializeKeplerElements(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        // Initialize Kepler elements
        meanMotion = OrbitalMechanics.KeplerMethod.MeanMotion(sourceMass, Trajectory.SemimajorAxis);
        if (Trajectory.TrajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            InitializeEllipticalParameters(sourceRelativePosition, sourceRelativeVelocity);

        }
        else if (Trajectory.TrajectoryType == OrbitalMechanics.Globals.TrajectoryType.Hyperbola)
        {
            InitializeHyperbolicParameters(sourceRelativePosition, sourceRelativeVelocity);
        }
        else
        {
            InitializeParabolicParameters();
        }
    }

    private void InitializeEllipticalParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomalyAtEpoch(sourceRelativePosition, sourceRelativeVelocity, sourceMass, Trajectory.EccentricityVector);
        TrueAnomaly = OrbitalMechanics.KeplerMethod.TrueAnomaly(Trajectory.Eccentricity, eccentricAnomaly);
        meanAnomalyAtEpoch = OrbitalMechanics.KeplerMethod.MeanAnomalyAtEpoch(eccentricAnomaly, Trajectory.Eccentricity);
        meanAnomaly = meanAnomalyAtEpoch;

        // Calculate Position
        CalculatedRadius = OrbitalMechanics.Trajectory.OrbitalRadius(Trajectory.Eccentricity, Trajectory.SemimajorAxis, TrueAnomaly);
        CalculatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(CalculatedRadius, TrueAnomaly, Trajectory.ClockWiseOrbit);

        // Update Velocity by vis-viva
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, Trajectory.SemimajorAxis);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(Trajectory.Eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, Trajectory.ClockWiseOrbit);
    }

    private void InitializeHyperbolicParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        nearHyperbolicAsymptote = false;
        TrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, Trajectory.SemimajorAxis, Trajectory.Eccentricity);
        eccentricAnomaly = OrbitalMechanics.HyperbolicTrajectory.HyperbolicAnomalyAtEpoch(TrueAnomaly, Trajectory.Eccentricity);
        meanAnomalyAtEpoch = OrbitalMechanics.KeplerMethod.MeanAnomalyAtEpoch(eccentricAnomaly, Trajectory.Eccentricity);
        meanAnomaly = meanAnomalyAtEpoch;

        float recalculatedEccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomaly(meanAnomaly, Trajectory.Eccentricity, 6);
        float recalculatedTrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(Trajectory.Eccentricity, recalculatedEccentricAnomaly, Trajectory.ClockWiseOrbit);
        float estimatedOrbitalRadius = OrbitalMechanics.Trajectory.OrbitalRadius(Trajectory.Eccentricity, Trajectory.SemimajorAxis, recalculatedTrueAnomaly);
        if (CalculatedRadiusUnstable(estimatedOrbitalRadius, sourceRelativePosition.magnitude))
        {
            nearHyperbolicAsymptote = true;
            CalculatedRadius = sourceRelativePosition.magnitude;
        }
        else
        {
            CalculatedRadius = estimatedOrbitalRadius;
        }
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(Trajectory.Eccentricity, TrueAnomaly);
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, Trajectory.SemimajorAxis);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, Trajectory.ClockWiseOrbit);
    }

    private void InitializeParabolicParameters()
    {
        throw new System.NotImplementedException("Kepler parabolic solver not implemented!");
    }

    public void UpdateStateVariables(float timeOfFlight)
    {
        //LastPosition = CalculatedPosition;
        //LastRadius = CalculatedRadius;
        //LastSpeed = CalculatedSpeed;
        //LastVelocity = CalculatedVelocity;

        if (Trajectory.TrajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            UpdateStateVariablesElliptically(timeOfFlight);
        }
        else if (Trajectory.TrajectoryType == OrbitalMechanics.Globals.TrajectoryType.Hyperbola)
        {
            UpdateStateVariablesHyperbolically(timeOfFlight);
        }
        else
        {
            UpdateStateVariablesParabolically();
        }
    }

    private void UpdateStateVariablesElliptically(float timeOfFlight)
    {
        // Update eccentricAnomaly
        timeOfFlight %= Trajectory.Period;
        meanAnomaly = OrbitalMechanics.KeplerMethod.MeanAnomaly(meanAnomalyAtEpoch, meanMotion, timeOfFlight, true);
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomaly(meanAnomaly, Trajectory.Eccentricity, maxNewtonianMethodIterations);

        // Update TrueAnomaly
        TrueAnomaly = OrbitalMechanics.KeplerMethod.TrueAnomaly(Trajectory.Eccentricity, eccentricAnomaly);

        // Calculate Position
        CalculatedRadius = OrbitalMechanics.Trajectory.OrbitalRadius(Trajectory.Eccentricity, Trajectory.SemimajorAxis, TrueAnomaly);
        CalculatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(CalculatedRadius, TrueAnomaly, Trajectory.ClockWiseOrbit);

        // Update Velocity by vis-viva
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, Trajectory.SemimajorAxis);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(Trajectory.Eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, Trajectory.ClockWiseOrbit);
    }

    private void UpdateStateVariablesHyperbolically(float timeOfFlight)
    {
        meanAnomaly = OrbitalMechanics.KeplerMethod.MeanAnomaly(meanAnomalyAtEpoch, meanMotion, timeOfFlight, false);
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomaly(meanAnomaly, Trajectory.Eccentricity, 6);
        float estimatedTrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(Trajectory.Eccentricity, eccentricAnomaly, Trajectory.ClockWiseOrbit);
        float estimatedOrbitalRadius = OrbitalMechanics.Trajectory.OrbitalRadius(Trajectory.Eccentricity, Trajectory.SemimajorAxis, estimatedTrueAnomaly);
        if (nearHyperbolicAsymptote)
        {
            UpdateHyperbolicallyNearAsymptote();
            if (Vector2.Dot(CalculatedPosition, CalculatedVelocity) < 0f)
            {
                nearHyperbolicAsymptote = CalculatedRadiusUnstable(estimatedOrbitalRadius, CalculatedRadius);
            }
        }
        else
        {
            Vector2 estimatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(estimatedOrbitalRadius, estimatedTrueAnomaly, Trajectory.ClockWiseOrbit);
            Vector2 estimatedDeltaPosition = estimatedPosition - CalculatedPosition;
            float estimatedSpeed = estimatedDeltaPosition.magnitude / Time.fixedDeltaTime;
            float estimatedFlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(Trajectory.Eccentricity, estimatedTrueAnomaly);

            bool angularMomentumIrregular = (Vector2.Dot(estimatedPosition, estimatedDeltaPosition) > 0f) // Only checking when moving away from current gravity source
                ? !AngularMomentumConserved(estimatedOrbitalRadius, estimatedSpeed, estimatedFlightPathAngle)
                : false;

            if (angularMomentumIrregular)
            {
                nearHyperbolicAsymptote = true;
                UpdateHyperbolicallyNearAsymptote();
            }
            else
            {
                TrueAnomaly = estimatedTrueAnomaly;
                FlightPathAngle = estimatedFlightPathAngle;
                
                CalculatedRadius = estimatedOrbitalRadius;
                CalculatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(CalculatedRadius, TrueAnomaly, Trajectory.ClockWiseOrbit);
                CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, Trajectory.SemimajorAxis);
                CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, Trajectory.ClockWiseOrbit);
            }
        }
    }

    private void UpdateHyperbolicallyNearAsymptote()
    {
        Vector2 nearestAsymptote = TrueAnomaly < 0
            ? -Trajectory.HyperbolicAsymptotes[0]
            : Trajectory.HyperbolicAsymptotes[1];
        Vector3 estimatedRelativeVelocity = CalculatedSpeed * nearestAsymptote;
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, Trajectory.SemimajorAxis);
        TrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(CalculatedPosition, estimatedRelativeVelocity, Trajectory.SemimajorAxis, Trajectory.Eccentricity);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(Trajectory.Eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, Trajectory.ClockWiseOrbit);
        CalculatedPosition += CalculatedVelocity * Time.fixedDeltaTime;
        CalculatedRadius = CalculatedPosition.magnitude;
    }

    private void UpdateStateVariablesParabolically()
    {
        throw new System.NotImplementedException("Kepler parabolic solver not implemented!");
    }

    

    private bool CalculatedRadiusUnstable(float orbitalRadiusA, float orbitalRadiusB)
    {
        // Radius A & B calculated through different means
        if (Mathf.Abs(orbitalRadiusA - orbitalRadiusB) > 0.01f) //Arbitrary cutoff to prevent calculated radius blowing up to infinity
        {
            return true;
        }
        return false;
    }

    private bool AngularMomentumConserved(float calculatedPosition, float calculatedSpeed, float calculatedflightPathAngle)
    {
        float calculatedSpecificAngularMomentumMag = calculatedPosition * calculatedSpeed * Mathf.Cos(calculatedflightPathAngle);
        // Radius A & B calculated through different means
        if (Mathf.Abs(Trajectory.SpecificRelativeAngularMomentum.magnitude - calculatedSpecificAngularMomentumMag) > 0.5f) //Arbitrary cutoff to prevent calculated radius blowing up to infinity
        {
            return false;
        }
        return true;
    }
}