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
    private float _flightPathAngle;
    [SerializeField]
    private GravitySource _gravitySource;
    private float _currentSphereOfInfluence;
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
    private float _orbitalSpeed;
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
    private bool clockWiseOrbit = false;
    private float hyperbolicVelocityDiffThreshold = 0.02f; // If abs(position-this-frame - position-last-frame)/dt > this, vel is close enough to hyperbolic excess vel, update that way
    private float hyperbolicExcessVelocityApproxThreshold = 1.5f; // If calculated velocity - hyperbolic excess vel < this, check above
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

    public float FlightPathAngle
    {
        get { return _flightPathAngle; }
        private set { _flightPathAngle = value; }
    }
    public float RadiusOfInfluence
    {
        get { return _currentSphereOfInfluence; }
        private set { _currentSphereOfInfluence = value; }
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

    public float OrbitalSpeed
    {
        get { return _orbitalSpeed; }
        private set { _orbitalSpeed = value; }
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
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.Position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.Velocity;
        OrbitalSpeed = sourceRelativeVelocity.magnitude;
        SpecificRelativeAngularMomentum = OrbitalMechanics.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        clockWiseOrbit = SpecificRelativeAngularMomentum.z < 0;
        EccentricityVector = OrbitalMechanics.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, SpecificRelativeAngularMomentum, CurrentGravitySource.Mass);
        SemimajorAxis = OrbitalMechanics.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, CurrentGravitySource.Mass);
        SemiminorAxis = OrbitalMechanics.SemiminorAxis(SemimajorAxis, Eccentricity);
        RadiusOfInfluence = CurrentGravitySource == null
            ? Mathf.Infinity
            : OrbitalMechanics.RadiusOfInfluence(SemimajorAxis, Mass, CurrentGravitySource.Mass);

        SpecificOrbitalEnergy = OrbitalMechanics.SpecificOrbitalEnergy(CurrentGravitySource.Mass, Mass, SemimajorAxis);
        ArgumentOfPeriapsis = OrbitalMechanics.ArgumentOfPeriapse(EccentricityVector);
        
        // Epoch parameters
        TimeSinceEpoch = 0f;
        MeanMotion = OrbitalMechanics.MeanMotion(CurrentGravitySource.Mass, SemimajorAxis);
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
            TrueAnomalyOfAsymptote = OrbitalMechanics.TrueAnomalyOfAsymptote(Eccentricity, clockWiseOrbit);
            HyperbolicAsymptote = OrbitalMechanics.HyperbolicAsymptote(TrueAnomalyOfAsymptote, clockWiseOrbit);
            TrueAnomaly = OrbitalMechanics.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass);
            EccentricAnomaly = OrbitalMechanics.HyperbolicAnomaly(TrueAnomaly, Eccentricity);
            OrbitalVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis); //BROKE
        }
        MeanAnomalyAtEpoch = OrbitalMechanics.MeanAnomalyAtEpoch(EccentricAnomaly, Eccentricity);
        MeanAnomaly = MeanAnomalyAtEpoch;
        OrbitalRadius = OrbitalMechanics.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
        lastPosition = OrbitalPosition;
        OrbitalPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly, clockWiseOrbit);
    }
    #endregion PHYSICS

    #region GENERAL

    protected void UpdateDeterministically(){
        // Only expected to be called at some time after CalculateEpochParameters
        if (CurrentGravitySource == null)
            return;

        if (LeavingSphereOfInfluence())
            LeaveSphereOfInfluence();
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
        OrbitalPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly, clockWiseOrbit);
        OrbitalSpeed = OrbitalMechanics.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis);
        FlightPathAngle = OrbitalMechanics.FlightPathAngle(Eccentricity, TrueAnomaly);

        //Debug.Log("MeanAnomaly: " + MeanAnomaly);
        //Debug.Log("EccentricAnomaly: " + EccentricAnomaly);
        transform.position = OrbitalPositionToWorld;
    }

    protected void UpdateHyperbolically()
    {
        TimeSinceEpoch += Time.fixedDeltaTime;
        MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        TrueAnomaly = OrbitalMechanics.HyperbolicTrueAnomaly(Eccentricity, EccentricAnomaly, clockWiseOrbit); //FIXME check CW?
        FlightPathAngle = OrbitalMechanics.FlightPathAngle(Eccentricity, TrueAnomaly);
        lastPosition = OrbitalPosition;
        if (!approachingHyperbolicExcessVelocity)
        {
            float calculatedOrbitalRadius = OrbitalMechanics.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
            Vector2 calculatedPosition = OrbitalMechanics.OrbitalPosition(calculatedOrbitalRadius, TrueAnomaly, clockWiseOrbit);
            float measuredSpeed = (calculatedPosition - lastPosition).magnitude / Time.fixedDeltaTime;
            float calculatedSpeed = OrbitalMechanics.OrbitalSpeed(CurrentGravitySource.Mass, calculatedOrbitalRadius, SemimajorAxis);
            if ((Mathf.Abs(calculatedSpeed - HyperbolicExcessSpeed) < hyperbolicExcessVelocityApproxThreshold) && Mathf.Abs(measuredSpeed - calculatedSpeed) > hyperbolicVelocityDiffThreshold)
            {
                // Setting speed to hyperbolic excess velocity, updating from that
                approachingHyperbolicExcessVelocity = true;
                OrbitalVelocity = calculatedSpeed * HyperbolicAsymptote;
                OrbitalPosition += OrbitalVelocity * Time.fixedDeltaTime;
            }
            else
            {
                OrbitalRadius = calculatedOrbitalRadius;
                OrbitalPosition = OrbitalMechanics.OrbitalPosition(OrbitalRadius, TrueAnomaly, clockWiseOrbit);
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

    protected bool LeavingSphereOfInfluence()
    {
        if (CurrentGravitySource.CurrentGravitySource == null)
            return false;

        if (OrbitalRadius < CurrentGravitySource.RadiusOfInfluence)
        {
            return false;
        }
        Debug.LogFormat("Leaving {0}'s sphere of influence. Entering {1}'s.", CurrentGravitySource.name, CurrentGravitySource.CurrentGravitySource.name);
        return true;
    }

    protected void LeaveSphereOfInfluence()
    {
        CurrentGravitySource = CurrentGravitySource.CurrentGravitySource;
        CalculateOrbitalParametersFromStateVectors();
    }

    #endregion GENERAL
    private void OnDrawGizmos()
    {
        if (CurrentGravitySource == null || body == null)
            return;
        Gizmos.color = Color.green;

        // Draw radius of SOI
        Gizmos.DrawWireSphere(Position, RadiusOfInfluence);

        // Draw velocityVector
        Gizmos.color = Color.red;
        //Vector2 dir = new Vector2(Mathf.Cos(FlightPathAngle), Mathf.Sin(FlightPathAngle));
        Vector2 dir = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
        Gizmos.DrawRay(Position, dir.RotateVector(ArgumentOfPeriapsis));
    }
}
