using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OrbitalBody : MonoBehaviour
{

    private float _argumentOfPeriapsis;
    private Vector2 _deterministicVelocity;
    private float _eccentricity;
    private float _eccentricAnomaly;
    private Vector3 _eccentricityVector;
    [SerializeField]
    private GravitySource _gravitySource; //UNDO this asap
    private float _hillRadius; // FIXME: Current hill radius. If extend beyond this, gravitySource drops back to parent gravity source.
    [SerializeField]
    private float _mass = 1.0f;
    private float _meanAnomaly;
    private float _meanAnomalyAtEpoch;
    private float _meanMotion;
    private float _orbitalPeriod;
    private Vector2 _orbitalPosition;
    private float _period;
    private float _semimajorAxis;
    private float _semiminorAxis;
    private float _specificOrbitalEnergy;
    private Vector3 _specificRelativeAngularMomentum;
    private float _timeSinceEpoch;
    private OrbitalMechanics.TrajectoryType _trajectoryType;
    private float _trueAnomaly;

    protected Rigidbody2D body;
    protected bool updateIteratively = true;

    #region GETSET
    public GravitySource CurrentGravitySource
    {
        get { return _gravitySource; }
        protected set { _gravitySource = value; }
    }
    public float Mass
    {
        get { return _mass; }
        private set { _mass = value; }
    }

    public float SourceDistance
    {
        get
        {
            if (CurrentGravitySource == null)
                return Mathf.Infinity;
            Vector3 diff = CurrentGravitySource.transform.position - transform.position;
            return diff.magnitude;
        }
    }

    public Vector2 DeterministicVelocity
    {
        get { return _deterministicVelocity; }
        protected set { _deterministicVelocity = value; }
    }

    public Vector3 SourceRelativePosition
    {
        get
        {
            if (CurrentGravitySource == null)
                return Vector3.positiveInfinity;
            return transform.position - CurrentGravitySource.transform.position;
        }
    }

    public Vector3 SourceRelativeVelocity
    {
        get
        {
            if (CurrentGravitySource == null)
                return new Vector3(body.velocity.x, body.velocity.y, 0f);
            Vector3 thisVelocity = new Vector3(body.velocity.x, body.velocity.y, 0f);
            return thisVelocity - CurrentGravitySource.Velocity;
        }

    }
    public float SourceDistanceSquared
    {
        get
        {
            if (CurrentGravitySource == null)
                return Mathf.Infinity;
            Vector3 diff = CurrentGravitySource.transform.position - this.transform.position;
            return diff.sqrMagnitude;
        }
    }

    public float StandardGravityParameter
    {
        get
        {
            if (CurrentGravitySource == null)
                return 0f;
            return OrbitalMechanics.StandardGravityParameter(CurrentGravitySource.Mass, Mass);
        }
    }

    public Vector3 SpecificRelativeAngularMomentum
    {
        get { return _specificRelativeAngularMomentum; }
        protected set { _specificRelativeAngularMomentum = value; }
    }

    public Vector3 EccentricityVector
    {
        get { return _eccentricityVector; }
        protected set
        {
            _eccentricityVector = value;
            _eccentricity = _eccentricityVector.magnitude;
            if (_eccentricity < 1f)
            {
                _trajectoryType = OrbitalMechanics.TrajectoryType.Ellipse;
            }
            else
            {
                // == 1 more or less impossible, ignore parabola
                _trajectoryType = OrbitalMechanics.TrajectoryType.Hyperbola;
            }
        }
    }

    public OrbitalMechanics.TrajectoryType TrajectoryType
    {
        get { return _trajectoryType; }
        protected set { _trajectoryType = value; }
    }

    public float Eccentricity
    {
        get { return _eccentricity; }
    }

    public float SpecificOrbitalEnergy
    {
        get { return _specificOrbitalEnergy; }
        protected set { _specificOrbitalEnergy = value; }

    }

    public float ArgumentOfPeriapsis
    {
        get { return _argumentOfPeriapsis; }
        protected set { _argumentOfPeriapsis = value; }
    }

    public float SemimajorAxis
    {
        get { return _semimajorAxis; }
        protected set { _semimajorAxis = value; }
    }

    public float SemiminorAxis
    {
        get { return _semiminorAxis; }
        protected set { _semiminorAxis = value; }
    }

    public Vector2 OrbitalPosition
    {
        get { return _orbitalPosition; }
        protected set { _orbitalPosition = value; }
    }

    public float OrbitalPeriod
    {
        get { return _orbitalPeriod; }
        protected set { _orbitalPeriod = value; }
    }

    public float MeanMotion
    {
        get { return _meanMotion; }
        protected set { _meanMotion = value; }
    }

    public float MeanAnomalyAtEpoch
    {
        get { return _meanAnomalyAtEpoch; }
        protected set { _meanAnomalyAtEpoch = value; }
    }

    public float MeanAnomaly
    {
        get { return _meanAnomaly; }
        protected set { _meanAnomaly = value; }
    }

    public float EccentricAnomaly
    {
        get { return _eccentricAnomaly; }
        protected set { _eccentricAnomaly = value; }
    }

    public float TimeSinceEpoch
    {
        get { return _timeSinceEpoch; }
        protected set { _timeSinceEpoch = value; }
    }

    public float TrueAnomaly
    {
        get { return _trueAnomaly; }
        protected set { _trueAnomaly = value; }
    }
    #endregion GETSET

    #region UNITY
    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }
    #endregion UNITY

    #region PHYSICS
    protected void CalculateOrbitalParameters()
    {
        // This should only be called from the iterative update method and only once before switching to trajectory update.
        SpecificRelativeAngularMomentum = OrbitalMechanics.SpecificRelativeAngularMomentum(SourceRelativePosition, SourceRelativeVelocity);
        EccentricityVector = OrbitalMechanics.EccentricityVector(SourceRelativePosition, SourceRelativeVelocity, SpecificRelativeAngularMomentum, CurrentGravitySource.Mass, Mass);
        SemimajorAxis = OrbitalMechanics.SemimajorAxis(SourceDistance, body.velocity.sqrMagnitude, CurrentGravitySource.Mass);
        SemiminorAxis = OrbitalMechanics.SemiminorAxis(SemimajorAxis, Eccentricity);
        SpecificOrbitalEnergy = OrbitalMechanics.SpecificOrbitalEnergy(CurrentGravitySource.Mass, Mass, SemimajorAxis);
        ArgumentOfPeriapsis = OrbitalMechanics.ArgumentOfPeriapse(EccentricityVector);
    }

    protected void CalculateEpochParameters()
    {
        TimeSinceEpoch = 0f;
        MeanMotion = OrbitalMechanics.MeanMotion(CurrentGravitySource.Mass, Mass, SemimajorAxis);
        OrbitalPeriod = OrbitalMechanics.OrbitalPeriod(MeanMotion);
        EccentricAnomaly = OrbitalMechanics.EccentricAnomalyAtEpoch(SourceRelativePosition, SourceRelativeVelocity, CurrentGravitySource.Mass, Mass, Eccentricity);
        MeanAnomalyAtEpoch = OrbitalMechanics.MeanAnomalyAtEpoch(EccentricAnomaly, Eccentricity);
        MeanAnomaly = MeanAnomalyAtEpoch;
        TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
        DeterministicVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
        Debug.Log(OrbitalPositionToWorld(OrbitalMechanics.OrbitalPosition(Eccentricity, SemimajorAxis, TrueAnomaly)));
        Debug.Log(CurrentGravitySource.Velocity);
    }
    #endregion PHYSICS

    #region GENERAL
    protected Vector2 OrbitalPositionToWorld(Vector2 orbitalPosition)
    {
        return orbitalPosition.RotateVector(ArgumentOfPeriapsis) + CurrentGravitySource.Position;
    }
    #endregion GENERAL  
}
