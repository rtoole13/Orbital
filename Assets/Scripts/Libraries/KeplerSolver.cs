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
    private float orbitalPeriod;
    private int maxNewtonianMethodIterations = 6;


    private float _flightPathAngle;

    // Hyperbolic parameters
    private bool nearHyperbolicAsymptote;
    private float hyperbolicExcessSpeed;
    private float trueAnomalyOfAsymptote;
    private Vector2[] hyperbolicAsymptotes;
    

    public KeplerSolver(float _eccentricAnomaly = 0f)
    {
        eccentricAnomaly = _eccentricAnomaly;
        meanAnomaly = 0f;
        meanAnomalyAtEpoch = 0f;
        meanMotion = 0f;
    }

    public override void InitializeSolver(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float sourceMass)
    {
        // Initialize orbital parameters
        base.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, sourceMass);

        // Initialize Kepler elements
        InitializeKeplerElements(sourceRelativePosition, sourceRelativeVelocity);
    }

    public override void InitializeSolver(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float _sourceMass, Vector3 _specificRelativeAngularMomentum, Vector3 eccentricityVector, float _semimajorAxis)
    {
        // Initialize orbital parameters
        base.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, _sourceMass, _specificRelativeAngularMomentum, eccentricityVector, _semimajorAxis);

        // Initialize Kepler elements
        InitializeKeplerElements(sourceRelativePosition, sourceRelativeVelocity);
    }

    private void InitializeKeplerElements(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        // Initialize Kepler elements
        meanMotion = OrbitalMechanics.KeplerMethod.MeanMotion(sourceMass, semimajorAxis);
        if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            orbitalPeriod = OrbitalMechanics.KeplerMethod.OrbitalPeriod(meanMotion);
            InitializeEllipticalParameters(sourceRelativePosition, sourceRelativeVelocity);

        }
        else if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Hyperbola)
        {
            orbitalPeriod = Mathf.Infinity;
            InitializeHyperbolicParameters(sourceRelativePosition, sourceRelativeVelocity);
        }
        else
        {
            orbitalPeriod = Mathf.Infinity;
            InitializeParabolicParameters();
        }
    }

    private void InitializeEllipticalParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomalyAtEpoch(sourceRelativePosition, sourceRelativeVelocity, sourceMass, EccentricityVector);
        TrueAnomaly = OrbitalMechanics.KeplerMethod.TrueAnomaly(eccentricity, eccentricAnomaly);
        meanAnomalyAtEpoch = OrbitalMechanics.KeplerMethod.MeanAnomalyAtEpoch(eccentricAnomaly, eccentricity);
        meanAnomaly = meanAnomalyAtEpoch;

        // Calculate Position
        CalculatedRadius = OrbitalMechanics.Trajectory.OrbitalRadius(eccentricity, semimajorAxis, TrueAnomaly);
        CalculatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(CalculatedRadius, TrueAnomaly, clockWiseOrbit);

        // Update Velocity by vis-viva
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, semimajorAxis);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, clockWiseOrbit);
    }

    private void InitializeHyperbolicParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        nearHyperbolicAsymptote = false;
        hyperbolicExcessSpeed = OrbitalMechanics.HyperbolicTrajectory.ExcessVelocity(sourceMass, semimajorAxis);
        trueAnomalyOfAsymptote = OrbitalMechanics.HyperbolicTrajectory.TrueAnomalyOfAsymptote(eccentricity, clockWiseOrbit);
        hyperbolicAsymptotes = OrbitalMechanics.HyperbolicTrajectory.Asymptotes(trueAnomalyOfAsymptote, clockWiseOrbit);
        TrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, semimajorAxis, eccentricity);
        eccentricAnomaly = OrbitalMechanics.HyperbolicTrajectory.HyperbolicAnomalyAtEpoch(TrueAnomaly, eccentricity);
        meanAnomalyAtEpoch = OrbitalMechanics.KeplerMethod.MeanAnomalyAtEpoch(eccentricAnomaly, eccentricity);
        meanAnomaly = meanAnomalyAtEpoch;

        float recalculatedEccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomaly(meanAnomaly, eccentricity, 6);
        float recalculatedTrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(eccentricity, recalculatedEccentricAnomaly, clockWiseOrbit);
        float estimatedOrbitalRadius = OrbitalMechanics.Trajectory.OrbitalRadius(eccentricity, semimajorAxis, recalculatedTrueAnomaly);
        if (CalculatedRadiusUnstable(estimatedOrbitalRadius, sourceRelativePosition.magnitude))
        {
            nearHyperbolicAsymptote = true;
            CalculatedRadius = sourceRelativePosition.magnitude;
        }
        else
        {
            CalculatedRadius = estimatedOrbitalRadius;
        }
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, TrueAnomaly);
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, semimajorAxis);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, clockWiseOrbit);
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

        if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            UpdateStateVariablesElliptically(timeOfFlight);
        }
        else if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Hyperbola)
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
        timeOfFlight %= orbitalPeriod;
        meanAnomaly = OrbitalMechanics.KeplerMethod.MeanAnomaly(meanAnomalyAtEpoch, meanMotion, timeOfFlight, true);
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomaly(meanAnomaly, eccentricity, maxNewtonianMethodIterations);

        // Update TrueAnomaly
        TrueAnomaly = OrbitalMechanics.KeplerMethod.TrueAnomaly(eccentricity, eccentricAnomaly);

        // Calculate Position
        CalculatedRadius = OrbitalMechanics.Trajectory.OrbitalRadius(eccentricity, semimajorAxis, TrueAnomaly);
        CalculatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(CalculatedRadius, TrueAnomaly, clockWiseOrbit);

        // Update Velocity by vis-viva
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, semimajorAxis);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, clockWiseOrbit);
    }

    private void UpdateStateVariablesHyperbolically(float timeOfFlight)
    {
        meanAnomaly = OrbitalMechanics.KeplerMethod.MeanAnomaly(meanAnomalyAtEpoch, meanMotion, timeOfFlight, false);
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomaly(meanAnomaly, eccentricity, 6);
        float estimatedTrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(eccentricity, eccentricAnomaly, clockWiseOrbit);
        float estimatedOrbitalRadius = OrbitalMechanics.Trajectory.OrbitalRadius(eccentricity, semimajorAxis, estimatedTrueAnomaly);
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
            Vector2 estimatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(estimatedOrbitalRadius, estimatedTrueAnomaly, clockWiseOrbit);
            Vector2 estimatedDeltaPosition = estimatedPosition - CalculatedPosition;
            float estimatedSpeed = estimatedDeltaPosition.magnitude / Time.fixedDeltaTime;
            float estimatedFlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, estimatedTrueAnomaly);

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
                CalculatedPosition = OrbitalMechanics.Trajectory.OrbitalPosition(CalculatedRadius, TrueAnomaly, clockWiseOrbit);
                CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, semimajorAxis);
                CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, clockWiseOrbit);
            }
        }
    }

    private void UpdateHyperbolicallyNearAsymptote()
    {
        Vector2 nearestAsymptote = TrueAnomaly < 0
            ? -hyperbolicAsymptotes[0]
            : hyperbolicAsymptotes[1];
        Vector3 estimatedRelativeVelocity = CalculatedSpeed * nearestAsymptote;
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(sourceMass, CalculatedRadius, semimajorAxis);
        TrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(CalculatedPosition, estimatedRelativeVelocity, semimajorAxis, eccentricity);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, clockWiseOrbit);
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
        if (Mathf.Abs(specificRelativeAngularMomentum.magnitude - calculatedSpecificAngularMomentumMag) > 0.5f) //Arbitrary cutoff to prevent calculated radius blowing up to infinity
        {
            return false;
        }
        return true;
    }
}