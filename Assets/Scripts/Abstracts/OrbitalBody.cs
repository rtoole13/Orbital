using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OrbitalBody : MonoBehaviour
{
    public Vector2 startVelocity;

    private float _argumentOfPeriapsis;
    private Vector2 _OrbitalVelocity;
    private float _eccentricity;
    private float _eccentricAnomaly;
    private Vector3 _eccentricityVector;
    [SerializeField]
    private GravitySource _gravitySource; //UNDO this asap
    private float _hillRadius; // FIXME: Current hill radius. If extend beyond this, gravitySource drops back to parent gravity source.
    private Vector2 _hyperbolicAsymptote;
    private float _hyperbolicExcessVelocity;
    private float _trueAnomalyOfAsymptote;
    [SerializeField]
    private float _mass = 1.0f;
    private float _meanAnomaly;
    private float _meanAnomalyAtEpoch;
    private float _meanMotion;
    private float _orbitalPeriod;
    private Vector2 _orbitalPosition;
    private float _orbitalRadius;
    private float _period;
    private float _semimajorAxis;
    private float _semiminorAxis;
    private float _specificOrbitalEnergy;
    private Vector3 _specificRelativeAngularMomentum;
    private float _timeSinceEpoch;
    private float _trueAnomaly;
    private OrbitalMechanics.TrajectoryType _trajectoryType;

    protected Rigidbody2D body;
    protected bool updateIteratively = true;

    private Vector2 lastPosition;
    private float hyperbolicVelocityDiffThreshold = 0.02f; // If abs(position-this-frame - position-last-frame)/dt > this, vel is close enough to hyperbolic excess vel, update that way
    private float hyperbolicExcessVelocityApproxThreshold = 0.6f; // If calculated velocity - hyperbolic excess vel < this, check above
    private bool approachingHyperbolicExcessVelocity = false;

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

    public Vector2 Position
    {
        // Whether iterative or deterministic, this position will be updating
        get { return body.position; }
    }

    public Vector2 Velocity
    {
        // If iterative, body.velocity is dynamic, otherwise calculate world velocity from orbital vel
        get
        {
            return updateIteratively ? body.velocity : OrbitalVelocityToWorld;
        }
    }

    public Vector2 HyperbolicAsymptote
    {
        get { return _hyperbolicAsymptote; }
        private set { _hyperbolicAsymptote = value; }
    }
    public float HyperbolicExcessSpeed
    {
        get { return _hyperbolicExcessVelocity; }
        private set { _hyperbolicExcessVelocity = value; }
    }

    public float TrueAnomalyOfAsymptote
    {
        get { return _trueAnomalyOfAsymptote; }
        private set { _trueAnomalyOfAsymptote = value; }
    }

    public float SourceDistance
    {
        get
        {
            if (CurrentGravitySource == null)
                return Mathf.Infinity;
            Vector3 diff = CurrentGravitySource.body.position - body.position;
            return diff.magnitude;
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
            return OrbitalMechanics.StandardGravityParameter(CurrentGravitySource.Mass);
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
        private set { _orbitalPosition = value; }
    }

    public float OrbitalRadius
    {
        get { return _orbitalRadius; }
        private set { _orbitalRadius = value; }
    }
    public Vector2 OrbitalVelocity
    {
        get { return _OrbitalVelocity; }
        protected set { _OrbitalVelocity = value; }
    }

    public Vector2 OrbitalPositionToWorld
    {
        get
        {
            Vector2 position = CurrentGravitySource != null
                ? CurrentGravitySource.Position
                : Vector2.zero;
            return OrbitalPosition.RotateVector(ArgumentOfPeriapsis) + position;
        }
    }

    public Vector2 OrbitalVelocityToWorld
    {
        
        get 
        {
            Vector2 velocity = CurrentGravitySource != null
                ? CurrentGravitySource.Velocity
                : Vector2.zero;
            return OrbitalVelocity.RotateVector(ArgumentOfPeriapsis) + velocity;
        }
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
        set { _trueAnomaly = value; }
    }
    #endregion GETSET

    #region UNITY
    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.velocity = startVelocity;
    }

    protected virtual void Start()
    {
    }
    #endregion UNITY

    #region PHYSICS
    protected void CalculateOrbitalParametersFromStateVectors()
    {
        // This should only be called from the iterative update method and only once before switching to trajectory update.
        Vector3 sourceRelativePosition = (Vector3)body.position - (Vector3)CurrentGravitySource.Position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.Velocity;
        SpecificRelativeAngularMomentum = OrbitalMechanics.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        EccentricityVector = OrbitalMechanics.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, SpecificRelativeAngularMomentum, CurrentGravitySource.Mass);
        SemimajorAxis = OrbitalMechanics.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, CurrentGravitySource.Mass);
        SemiminorAxis = OrbitalMechanics.SemiminorAxis(SemimajorAxis, Eccentricity);
        SpecificOrbitalEnergy = OrbitalMechanics.SpecificOrbitalEnergy(CurrentGravitySource.Mass, Mass, SemimajorAxis);
        ArgumentOfPeriapsis = OrbitalMechanics.ArgumentOfPeriapse(EccentricityVector);

        // Epoch parameters
        TimeSinceEpoch = 0f;
        MeanMotion = OrbitalMechanics.MeanMotion(CurrentGravitySource.Mass, SemimajorAxis);
        MeanAnomalyAtEpoch = OrbitalMechanics.MeanAnomalyAtEpoch(EccentricAnomaly, Eccentricity);
        MeanAnomaly = MeanAnomalyAtEpoch;
        if (TrajectoryType == OrbitalMechanics.TrajectoryType.Ellipse)
        {
            OrbitalPeriod = OrbitalMechanics.OrbitalPeriod(MeanMotion);
            EccentricAnomaly = OrbitalMechanics.EccentricAnomalyAtEpoch(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass, Eccentricity);
            TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
            OrbitalVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
        }
        else
        {
            approachingHyperbolicExcessVelocity = false;
            HyperbolicExcessSpeed = OrbitalMechanics.HyperbolicExcessVelocity(CurrentGravitySource.Mass, SemimajorAxis);
            TrueAnomalyOfAsymptote = OrbitalMechanics.TrueAnomalyOfAsymptote(Eccentricity, SpecificRelativeAngularMomentum.z < 0);
            HyperbolicAsymptote = OrbitalMechanics.HyperbolicAsymptote(TrueAnomalyOfAsymptote);
            TrueAnomaly = OrbitalMechanics.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass);
            EccentricAnomaly = OrbitalMechanics.HyperbolicAnomaly(TrueAnomaly, Eccentricity);
            OrbitalVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis); //BROKE
        }
        OrbitalRadius = OrbitalMechanics.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
        lastPosition = OrbitalPosition;
        OrbitalPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly);
    }
    #endregion PHYSICS

    #region GENERAL

    protected void UpdateDeterministically(){
        // Only expected to be called at some time after CalculateEpochParameters
        if (CurrentGravitySource == null)
            return;
        if (TrajectoryType == OrbitalMechanics.TrajectoryType.Ellipse)
        {
            UpdateElliptically();
        }
        else
        {
            UpdateHyperbolically();
        }
    }

    protected void UpdateElliptically()
    {
        TimeSinceEpoch = (TimeSinceEpoch + Time.fixedDeltaTime) % OrbitalPeriod;
        MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
        OrbitalRadius = OrbitalMechanics.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
        lastPosition = OrbitalPosition;
        OrbitalVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
        OrbitalPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly);
        transform.position = OrbitalPositionToWorld;
    }

    protected void UpdateHyperbolically()
    {
        TimeSinceEpoch += Time.fixedDeltaTime;
        MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        TrueAnomaly = OrbitalMechanics.HyperbolicTrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum.z > 0f);
        lastPosition = OrbitalPosition;
        if (!approachingHyperbolicExcessVelocity)
        {
            float calculatedOrbitalRadius = OrbitalMechanics.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
            Vector2 calculatedPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly);
            float measuredSpeed = (calculatedPosition - lastPosition).magnitude / Time.fixedDeltaTime;
            float calculatedSpeed = OrbitalMechanics.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis);
            if ((Mathf.Abs(calculatedSpeed - HyperbolicExcessSpeed) < hyperbolicExcessVelocityApproxThreshold) && Mathf.Abs(measuredSpeed - calculatedSpeed) > hyperbolicVelocityDiffThreshold)
            {
                approachingHyperbolicExcessVelocity = true;
                // Setting speed to hyperbolic excess velocity, updating from that
                OrbitalVelocity = calculatedSpeed * HyperbolicAsymptote;
                OrbitalPosition += OrbitalVelocity * Time.fixedDeltaTime;
            }
            else
            {
                OrbitalRadius = calculatedOrbitalRadius;
                OrbitalPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly);
                //OrbitalVelocity = OrbitalMechanics.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis); //FIXME NEED DIRECTION
            }
        }
        else
        {
            OrbitalVelocity = HyperbolicExcessSpeed * HyperbolicAsymptote;
            OrbitalPosition += OrbitalVelocity * Time.fixedDeltaTime;
        }

        transform.position = OrbitalPositionToWorld;
    }
    #endregion GENERAL
    private void OnDrawGizmos()
    {
        if (CurrentGravitySource == null)
            return;
        Gizmos.color = Color.green;
        //Vector2 dir = new Vector2(Mathf.Cos(flightPathAngle), Mathf.Sin(flightPathAngle)).normalized;
        //Gizmos.DrawRay(transform.position, dir.RotateVector(ArgumentOfPeriapsis) * 2f);
    }
}
