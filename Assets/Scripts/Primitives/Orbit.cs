using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;

public class Orbit
{
    private Vector3 _eccentricityVector;

    public float ArgumentOfPeriapsis { get; private set; }
    public bool ValidOrbit { get; private set; }
    public bool ClockWiseOrbit { get; private set; }
    public float Eccentricity { get; private set; }
    public Vector3 EccentricityVector {
        get { return _eccentricityVector; }
        private set {
            _eccentricityVector = value;
            Eccentricity = _eccentricityVector.magnitude;
            TrajectoryType = (Eccentricity < 1f)
                ? Mechanics.Globals.TrajectoryType.Ellipse
                : Mechanics.Globals.TrajectoryType.Hyperbola;
        }
    }
    public Vector2[] HyperbolicAsymptotes { get; private set; }
    public float HyperbolicExcessSpeed { get; private set; }
    public float Period { get; private set; }
    public float SemimajorAxis { get; private set; }
    public float SemiminorAxis { get; private set; }
    public Mechanics.Globals.TrajectoryType TrajectoryType { get; private set; }
    public float TrueAnomalyOfAsymptote { get; private set; }

    #region CONSTRUCTORS
    public Orbit()
    {
        ValidOrbit = false;
    }

    public Orbit(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float sourceMass)
    {
        ValidOrbit = false;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity, sourceMass);
    }

    #endregion
    public void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float sourceMass)
    {
        Vector3 specificRelativeAngularMomentum = Mechanics.Trajectory.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        ClockWiseOrbit = specificRelativeAngularMomentum.z < 0;
        EccentricityVector = Mechanics.Trajectory.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, specificRelativeAngularMomentum, sourceMass);
        SemimajorAxis = Mechanics.Trajectory.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, sourceMass);
        SemiminorAxis = Mechanics.Trajectory.SemiminorAxis(SemimajorAxis, Eccentricity);
        ArgumentOfPeriapsis = Mechanics.Trajectory.ArgumentOfPeriapse(EccentricityVector, sourceRelativePosition);

        // 
        if (TrajectoryType == Mechanics.Globals.TrajectoryType.Ellipse)
        {
            Period = Mechanics.Trajectory.OrbitalPeriod(SemimajorAxis, sourceMass);
        }
        else
        {
            HyperbolicExcessSpeed = Mechanics.HyperbolicTrajectory.ExcessVelocity(sourceMass, SemimajorAxis);
            TrueAnomalyOfAsymptote = Mechanics.HyperbolicTrajectory.TrueAnomalyOfAsymptote(Eccentricity, ClockWiseOrbit);
            HyperbolicAsymptotes = Mechanics.HyperbolicTrajectory.Asymptotes(TrueAnomalyOfAsymptote, ClockWiseOrbit);
            Period = Mathf.Infinity;
        }
        ValidOrbit = true;
    }
}
