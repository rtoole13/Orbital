using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeplerSolver : Solver
{
    private float eccentricAnomaly;
    private float meanAnomaly;
    private float meanAnomalyAtEpoch;
    private float meanMotion;
    private float orbitalPeriod;
    private int maxNewtonianMethodIterations = 6;

    private float _flightPathAngle;
    public KeplerSolver(float _eccentricAnomaly = 0f)
    {
        eccentricAnomaly = _eccentricAnomaly;
        meanAnomaly = 0f;
        meanAnomalyAtEpoch = 0f;
        meanMotion = 0f;
    }

    public void UpdateStateVariables(OrbitalMechanics.Globals.TrajectoryType trajectoryType, float timeOfFlight, float eccentricity, float semimajorAxis, bool clockWiseOrbit, float mainMass)
    {
        if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            UpdateStateVariablesElliptically(timeOfFlight, eccentricity, semimajorAxis, clockWiseOrbit, mainMass);
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

    public void UpdateStateVariablesElliptically(float timeOfFlight, float eccentricity, float semimajorAxis, bool clockWiseOrbit, float mainMass)
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
        CalculatedSpeed = OrbitalMechanics.Trajectory.OrbitalSpeed(mainMass, CalculatedRadius, semimajorAxis);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, TrueAnomaly);
        CalculatedVelocity = CalculatedSpeed * OrbitalMechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, clockWiseOrbit);
    }

    public void UpdateStateVariablesHyperbolically()
    {
        throw new System.NotImplementedException("Kepler hyperbolic solver not implemented!");
    }

    public void UpdateStateVariablesParabolically()
    {
        throw new System.NotImplementedException("Kepler parabolic solver not implemented!");
    }
}