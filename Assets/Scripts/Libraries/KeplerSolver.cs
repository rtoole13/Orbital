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
        meanAnomalyAtEpoch = OrbitalMechanics.KeplerMethod.MeanAnomalyAtEpoch(eccentricAnomaly, eccentricity);
        meanAnomaly = meanAnomalyAtEpoch;
    }

    private void InitializeEllipticalParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        eccentricAnomaly = OrbitalMechanics.KeplerMethod.EccentricAnomalyAtEpoch(sourceRelativePosition, sourceRelativeVelocity, sourceMass, EccentricityVector);
        TrueAnomaly = OrbitalMechanics.KeplerMethod.TrueAnomaly(eccentricity, eccentricAnomaly);
    }

    private void InitializeHyperbolicParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        nearHyperbolicAsymptote = false;
        hyperbolicExcessSpeed = OrbitalMechanics.HyperbolicTrajectory.ExcessVelocity(sourceMass, semimajorAxis);
        trueAnomalyOfAsymptote = OrbitalMechanics.HyperbolicTrajectory.TrueAnomalyOfAsymptote(eccentricity, clockWiseOrbit);
        hyperbolicAsymptotes = OrbitalMechanics.HyperbolicTrajectory.Asymptotes(trueAnomalyOfAsymptote, clockWiseOrbit);
        TrueAnomaly = OrbitalMechanics.HyperbolicTrajectory.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, semimajorAxis, eccentricity);
        eccentricAnomaly = OrbitalMechanics.HyperbolicTrajectory.HyperbolicAnomalyAtEpoch(TrueAnomaly, eccentricity);
    }

    private void InitializeParabolicParameters()
    {
        throw new System.NotImplementedException("Kepler parabolic solver not implemented!");
    }

    public void UpdateStateVariables(float timeOfFlight)
    {
        if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            UpdateStateVariablesElliptically(timeOfFlight);
        }
        else if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Hyperbola)
        {
            UpdateStateVariablesHyperbolically();
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

    private void UpdateStateVariablesHyperbolically()
    {
        throw new System.NotImplementedException("Kepler hyperbolic solver not implemented!");
    }

    private void UpdateStateVariablesParabolically()
    {
        throw new System.NotImplementedException("Kepler parabolic solver not implemented!");
    }
}