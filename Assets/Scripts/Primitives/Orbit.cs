using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit
{
    private Vector3 _eccentricityVector;

    public bool ValidOrbit { get; private set; }
    public float ArgumentOfPeriapsis { get; private set; }
    public bool ClockWiseOrbit { get; private set; }
    public float Eccentricity { get; private set; }
    public Vector3 EccentricityVector {
        get { return _eccentricityVector; }
        private set {
            _eccentricityVector = value;
            Eccentricity = _eccentricityVector.magnitude;
            TrajectoryType = (Eccentricity < 1f)
                ? OrbitalMechanics.TrajectoryType.Ellipse
                : OrbitalMechanics.TrajectoryType.Hyperbola;
        }
    }
    public Vector2[] HyperbolicAsymptotes { get; private set; }
    public float HyperbolicExcessSpeed { get; private set; }
    public float Period { get; private set; }
    public float SemimajorAxis { get; private set; }
    public float SemiminorAxis { get; private set; }
    public OrbitalMechanics.TrajectoryType TrajectoryType { get; private set; }
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
        Vector3 specificRelativeAngularMomentum = OrbitalMechanics.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        ClockWiseOrbit = specificRelativeAngularMomentum.z < 0;
        EccentricityVector = OrbitalMechanics.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, specificRelativeAngularMomentum, sourceMass);
        SemimajorAxis = OrbitalMechanics.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, sourceMass);
        SemiminorAxis = OrbitalMechanics.SemiminorAxis(SemimajorAxis, Eccentricity);
        ArgumentOfPeriapsis = OrbitalMechanics.ArgumentOfPeriapse(EccentricityVector, sourceRelativePosition);

        // 
        if (TrajectoryType == OrbitalMechanics.TrajectoryType.Ellipse)
        {
            Period = OrbitalMechanics.OrbitalPeriod(SemimajorAxis, sourceMass);
        }
        else
        {
            HyperbolicExcessSpeed = OrbitalMechanics.HyperbolicExcessVelocity(sourceMass, SemimajorAxis);
            TrueAnomalyOfAsymptote = OrbitalMechanics.TrueAnomalyOfAsymptote(Eccentricity, ClockWiseOrbit);
            HyperbolicAsymptotes = OrbitalMechanics.HyperbolicAsymptotes(TrueAnomalyOfAsymptote, ClockWiseOrbit);
            Period = Mathf.Infinity;
        }
        ValidOrbit = true;
    }
}
