using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory
{
    private Vector3 _eccentricityVector;

    public float ArgumentOfPeriapsis { get; private set; }
    public bool ValidOrbit { get; private set; }
    public bool ClockWiseOrbit { get; private set; }
    public float Eccentricity { get; private set; }
    public Vector3 EccentricityVector
    {
        get { return _eccentricityVector; }
        private set
        {
            _eccentricityVector = value;
            Eccentricity = _eccentricityVector.magnitude;
            TrajectoryType = (Eccentricity < 1f)
                ? OrbitalMechanics.Globals.TrajectoryType.Ellipse
                : OrbitalMechanics.Globals.TrajectoryType.Hyperbola;
        }
    }
    public Vector2[] HyperbolicAsymptotes { get; private set; }
    public float HyperbolicExcessSpeed { get; private set; }
    public float Period { get; private set; }
    public float SemimajorAxis { get; private set; }
    public float SemiminorAxis { get; private set; }
    public float SpecificOrbitalEnergy { get; private set; }
    public Vector3 SpecificRelativeAngularMomentum { get; private set; }
    public OrbitalMechanics.Globals.TrajectoryType TrajectoryType { get; private set; }
    public float TrueAnomalyOfAsymptote { get; private set; }
    
   
    #region CONSTRUCTORS
    public Trajectory()
    {
        ValidOrbit = false;
    }

    public Trajectory(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float sourceMass)
    {
        ValidOrbit = false;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity, sourceMass);
    }
    #endregion CONSTRUCTORS

    public void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float sourceMass)
    {
        SpecificRelativeAngularMomentum = OrbitalMechanics.Trajectory.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        ClockWiseOrbit = SpecificRelativeAngularMomentum.z < 0;
        EccentricityVector = OrbitalMechanics.Trajectory.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, SpecificRelativeAngularMomentum, sourceMass);
        SemimajorAxis = OrbitalMechanics.Trajectory.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, sourceMass);
        SemiminorAxis = OrbitalMechanics.Trajectory.SemiminorAxis(SemimajorAxis, Eccentricity);
        SpecificOrbitalEnergy = OrbitalMechanics.Trajectory.SpecificOrbitalEnergy(sourceMass, SemimajorAxis);
        ArgumentOfPeriapsis = OrbitalMechanics.Trajectory.ArgumentOfPeriapse(EccentricityVector, sourceRelativePosition);

        if (TrajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            Period = OrbitalMechanics.Trajectory.OrbitalPeriod(SemimajorAxis, sourceMass);
        }
        else
        {
            HyperbolicExcessSpeed = OrbitalMechanics.HyperbolicTrajectory.ExcessVelocity(sourceMass, SemimajorAxis);
            TrueAnomalyOfAsymptote = OrbitalMechanics.HyperbolicTrajectory.TrueAnomalyOfAsymptote(Eccentricity, ClockWiseOrbit);
            HyperbolicAsymptotes = OrbitalMechanics.HyperbolicTrajectory.Asymptotes(TrueAnomalyOfAsymptote, ClockWiseOrbit);
            Period = Mathf.Infinity;
        }
        ValidOrbit = true;

    }

    
}
