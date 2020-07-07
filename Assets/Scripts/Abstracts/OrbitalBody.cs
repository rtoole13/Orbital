using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class OrbitalBody : MonoBehaviour
{
    public Vector2 startVelocity;

    private float _argumentOfPeriapsis;
    private Vector2 _OrbitalVelocity;
    private float _eccentricity;
    private float _eccentricAnomaly;
    private Vector3 _eccentricityVector;
    private float _flightPathAngle;
    private GravitySource _gravitySource;
    private Vector2[] _hyperbolicAsymptotes;
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
    private bool _clockWiseOrbit = false;
    private float _semimajorAxis;
    private float _semimajorAxisReciprocal;
    private float _semiminorAxis;
    private float _specificOrbitalEnergy;
    private Vector3 _specificRelativeAngularMomentum;
    private float _specificRelativeAngularMomentumMagnitude;
    private float _timeSinceEpoch;
    private float _trueAnomaly;
    private Mechanics.Globals.TrajectoryType _trajectoryType;
    protected Rigidbody2D body;
    protected bool _updatingIteratively = true;

    private Vector2 lastPosition;
    private float hyperbolicVelocityDiffThreshold = 0.02f; // If abs(position-this-frame - position-last-frame)/dt > this, vel is close enough to hyperbolic excess vel, update that way
    private float hyperbolicExcessVelocityApproxThreshold = 1.5f; // If calculated velocity - hyperbolic excess vel < this, check above
    private bool nearHyperbolicAsymptote = false;

    public delegate void OnOrbitCalculation();
    public event OnOrbitCalculation OnOrbitCalculationEvent;

    #region GETSET
    public GravitySource CurrentGravitySource
    {
        get { return _gravitySource; }
        protected set { _gravitySource = value; }
    }
    public bool UpdatingIteratively
    {
        get { return _updatingIteratively; }
        protected set { _updatingIteratively = value; }
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
            return UpdatingIteratively ? body.velocity : OrbitalVelocityToWorld;
        }
    }
    
    public float CurrentSourceDistance
    {
        get { return (Position - CurrentGravitySource.Position).magnitude; }
    }

    public bool ClockWiseOrbit
    {
        get { return _clockWiseOrbit; }
        private set { _clockWiseOrbit = value; }
    }
    public float FlightPathAngle
    {
        get { return _flightPathAngle; }
        private set { _flightPathAngle = value; }
    }

    public Vector2[] HyperbolicAsymptotes
    {
        get { return _hyperbolicAsymptotes; }
        private set { _hyperbolicAsymptotes = value; }
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
            return Mechanics.Body.StandardGravityParameter(CurrentGravitySource.Mass);
        }
    }

    public Vector3 SpecificRelativeAngularMomentum
    {
        get { return _specificRelativeAngularMomentum; }
        private set
        {
            _specificRelativeAngularMomentum = value;
            _specificRelativeAngularMomentumMagnitude = value.magnitude;
        }
    }

    public float SpecificRelativeAngularMomentumMagnitude
    {
        get { return _specificRelativeAngularMomentumMagnitude; }
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
                _trajectoryType = Mechanics.Globals.TrajectoryType.Ellipse;
            }
            else
            {
                // == 1 more or less impossible, ignore parabola
                _trajectoryType = Mechanics.Globals.TrajectoryType.Hyperbola;
            }
        }
    }

    public Mechanics.Globals.TrajectoryType TrajectoryType
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

    public float SemimajorAxisReciprocal
    {
        get { return _semimajorAxisReciprocal; }
        protected set { _semimajorAxisReciprocal = value; }
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

    public Vector2 OrbitalDirectionToWorld
    {
        get
        {
            Vector2 dir = Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, ClockWiseOrbit);
            return dir.RotateVector(ArgumentOfPeriapsis);
        }
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
    protected virtual void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        SpecificRelativeAngularMomentum = Mechanics.Trajectory.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        ClockWiseOrbit = SpecificRelativeAngularMomentum.z < 0;
        EccentricityVector = Mechanics.Trajectory.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, SpecificRelativeAngularMomentum, CurrentGravitySource.Mass);
        SemimajorAxis = Mechanics.Trajectory.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, CurrentGravitySource.Mass);
        SemimajorAxisReciprocal = 1f / SemimajorAxis;
        SemiminorAxis = Mechanics.Trajectory.SemiminorAxis(SemimajorAxis, Eccentricity);
        SpecificOrbitalEnergy = Mechanics.Trajectory.SpecificOrbitalEnergy(CurrentGravitySource.Mass, SemimajorAxis);
        ArgumentOfPeriapsis = Mechanics.Trajectory.ArgumentOfPeriapse(EccentricityVector, sourceRelativePosition);

        // Epoch parameters
        TimeSinceEpoch = 0f;
        MeanMotion = Mechanics.Trajectory.MeanMotion(CurrentGravitySource.Mass, SemimajorAxis);
        if (TrajectoryType == Mechanics.Globals.TrajectoryType.Ellipse)
        {
            CalculateEllipticalOrbitParameters(sourceRelativePosition, sourceRelativeVelocity);
        }
        else
        {
            CalculateHyperbolicOrbitParameters(sourceRelativePosition, sourceRelativeVelocity);
        }
        OrbitalPosition = Mechanics.Trajectory.OrbitalPosition(OrbitalRadius, TrueAnomaly, ClockWiseOrbit);
        lastPosition = OrbitalPosition;

        OnOrbitCalculationEvent(); // Invoke orbit calculation event, triggering things like trajectory drawing.
    }

    private void CalculateEllipticalOrbitParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        OrbitalPeriod = Mechanics.Trajectory.OrbitalPeriod(MeanMotion);
        EccentricAnomaly = Mechanics.Trajectory.EccentricAnomalyAtEpoch(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass, EccentricityVector);
        TrueAnomaly = Mechanics.Trajectory.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
        MeanAnomalyAtEpoch = Mechanics.Trajectory.MeanAnomalyAtEpoch(EccentricAnomaly, Eccentricity);
        MeanAnomaly = MeanAnomalyAtEpoch;
        OrbitalRadius = Mechanics.Trajectory.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
        FlightPathAngle = Mechanics.Trajectory.FlightPathAngle(Eccentricity, TrueAnomaly);
        OrbitalSpeed = Mechanics.Trajectory.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis);
    }

    private void CalculateHyperbolicOrbitParameters(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        nearHyperbolicAsymptote = false;
        HyperbolicExcessSpeed = Mechanics.HyperbolicTrajectory.ExcessVelocity(CurrentGravitySource.Mass, SemimajorAxis);
        TrueAnomalyOfAsymptote = Mechanics.HyperbolicTrajectory.TrueAnomalyOfAsymptote(Eccentricity, ClockWiseOrbit);
        HyperbolicAsymptotes = Mechanics.HyperbolicTrajectory.Asymptotes(TrueAnomalyOfAsymptote, ClockWiseOrbit);
        TrueAnomaly = Mechanics.HyperbolicTrajectory.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, SemimajorAxis, Eccentricity);
        EccentricAnomaly = Mechanics.HyperbolicTrajectory.HyperbolicAnomalyAtEpoch(TrueAnomaly, Eccentricity);
        MeanAnomalyAtEpoch = Mechanics.Trajectory.MeanAnomalyAtEpoch(EccentricAnomaly, Eccentricity);
        MeanAnomaly = MeanAnomalyAtEpoch;
        float recalculatedEccentricAnomaly = Mechanics.KeplerMethod.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        float recalculatedTrueAnomaly = Mechanics.HyperbolicTrajectory.TrueAnomaly(Eccentricity, recalculatedEccentricAnomaly, ClockWiseOrbit);
        float calculatedOrbitalRadius = Mechanics.Trajectory.OrbitalRadius(Eccentricity, SemimajorAxis, recalculatedTrueAnomaly);
        if (CalculatedRadiusUnstable(calculatedOrbitalRadius, sourceRelativePosition.magnitude))
        {
            nearHyperbolicAsymptote = true;
            OrbitalRadius = sourceRelativePosition.magnitude;
        }
        else
        {
            OrbitalRadius = calculatedOrbitalRadius;
        }
        FlightPathAngle = Mechanics.Trajectory.FlightPathAngle(Eccentricity, TrueAnomaly);
        OrbitalSpeed = Mechanics.Trajectory.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis);
        OrbitalVelocity = OrbitalSpeed * Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, ClockWiseOrbit);
    }
    #endregion PHYSICS

    #region GENERAL

    protected virtual void UpdateDeterministically(){
        if (CurrentGravitySource == null)
            return;
        
        if (TrajectoryType == Mechanics.Globals.TrajectoryType.Ellipse)
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
        MeanAnomaly = Mechanics.KeplerMethod.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch, true);
        EccentricAnomaly = Mechanics.KeplerMethod.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        TrueAnomaly = Mechanics.Trajectory.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
        FlightPathAngle = Mechanics.Trajectory.FlightPathAngle(Eccentricity, TrueAnomaly);
        OrbitalRadius = Mechanics.Trajectory.OrbitalRadius(Eccentricity, SemimajorAxis, TrueAnomaly);
        lastPosition = OrbitalPosition;
        OrbitalPosition = Mechanics.Trajectory.OrbitalPosition(OrbitalRadius, TrueAnomaly, ClockWiseOrbit);
        OrbitalSpeed = Mechanics.Trajectory.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis);
        OrbitalVelocity = OrbitalSpeed * Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, ClockWiseOrbit);
        transform.position = OrbitalPositionToWorld;
    }

    protected void UpdateHyperbolically()
    {
        TimeSinceEpoch += Time.fixedDeltaTime;
        MeanAnomaly = Mechanics.KeplerMethod.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch, false);
        EccentricAnomaly = Mechanics.KeplerMethod.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        float calculatedTrueAnomaly = Mechanics.HyperbolicTrajectory.TrueAnomaly(Eccentricity, EccentricAnomaly, ClockWiseOrbit);
        float calculatedOrbitalRadius = Mechanics.Trajectory.OrbitalRadius(Eccentricity, SemimajorAxis, calculatedTrueAnomaly);
        lastPosition = OrbitalPosition;
        if (nearHyperbolicAsymptote)
        {
            UpdateHyperbolicallyNearAsymptote();
            if (Vector2.Dot(OrbitalPosition, OrbitalVelocity) < 0f)
            {
                nearHyperbolicAsymptote = CalculatedRadiusUnstable(calculatedOrbitalRadius, OrbitalRadius);
            }
        }
        else
        {
            Vector2 calculatedPosition = Mechanics.Trajectory.OrbitalPosition(calculatedOrbitalRadius, calculatedTrueAnomaly, ClockWiseOrbit);
            Vector2 deltaPosition = calculatedPosition - lastPosition;
            float measuredSpeed = deltaPosition.magnitude / Time.fixedDeltaTime;
            float calculatedFpA = Mechanics.Trajectory.FlightPathAngle(Eccentricity, calculatedTrueAnomaly);

            bool angularMomentumIrregular = (Vector2.Dot(calculatedPosition, deltaPosition) > 0f) // Only checking when moving away from current gravity source
                ? !AngularMomentumConserved(calculatedOrbitalRadius, measuredSpeed, calculatedFpA)
                : false;

            if (angularMomentumIrregular)
            {
                nearHyperbolicAsymptote = true;
                UpdateHyperbolicallyNearAsymptote();                
            }
            else
            {
                TrueAnomaly = calculatedTrueAnomaly;
                FlightPathAngle = Mechanics.Trajectory.FlightPathAngle(Eccentricity, TrueAnomaly);
                OrbitalRadius = calculatedOrbitalRadius;
                OrbitalPosition = Mechanics.Trajectory.OrbitalPosition(OrbitalRadius, TrueAnomaly, ClockWiseOrbit);
                OrbitalSpeed = Mechanics.Trajectory.OrbitalSpeed(CurrentGravitySource.Mass, OrbitalRadius, SemimajorAxis);
                OrbitalVelocity = OrbitalSpeed * Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, ClockWiseOrbit);
            }
        }
        transform.position = OrbitalPositionToWorld;
    }

    private void UpdateHyperbolicallyNearAsymptote()
    {
        Vector3 relativePosition = Position - CurrentGravitySource.Position;
        Vector2 nearestAsymptote = TrueAnomaly < 0
            ? -HyperbolicAsymptotes[0]
            : HyperbolicAsymptotes[1];
        Vector3 estimatedRelativeVelocity = OrbitalSpeed * nearestAsymptote;
        OrbitalSpeed = Mechanics.Trajectory.OrbitalSpeed(CurrentGravitySource.Mass, relativePosition.magnitude, SemimajorAxis);
        TrueAnomaly = Mechanics.HyperbolicTrajectory.TrueAnomaly(relativePosition, estimatedRelativeVelocity, SemimajorAxis, Eccentricity);
        FlightPathAngle = Mechanics.Trajectory.FlightPathAngle(Eccentricity, TrueAnomaly);
        OrbitalVelocity = OrbitalSpeed * Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, ClockWiseOrbit);
        OrbitalPosition += OrbitalVelocity * Time.fixedDeltaTime;
        OrbitalRadius = OrbitalPosition.magnitude;
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
        if (Mathf.Abs(SpecificRelativeAngularMomentumMagnitude - calculatedSpecificAngularMomentumMag) > 0.5f) //Arbitrary cutoff to prevent calculated radius blowing up to infinity
        {
            return false;
        }
        return true;
    }

    public Vector2 WorldPositionToPerifocalPosition(Vector2 position)
    {
        position = CurrentGravitySource != null
            ? position - CurrentGravitySource.Position
            : position;
        return position.RotateVector(-ArgumentOfPeriapsis);
    }
    
    #endregion GENERAL

    protected virtual void OnDrawGizmos()
    {
        if (CurrentGravitySource == null || body == null)
            return;

        // Draw velocityVector
        //Gizmos.color = Color.red;
        //Vector2 dir = Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, ClockWiseOrbit);
        //Gizmos.DrawRay(Position, 5f * dir.RotateVector(ArgumentOfPeriapsis));

        if (TrajectoryType == Mechanics.Globals.TrajectoryType.Hyperbola)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(CurrentGravitySource.Position - SemimajorAxis * (Vector2)EccentricityVector, 1000f * HyperbolicAsymptotes[0].RotateVector(ArgumentOfPeriapsis));
            Gizmos.DrawRay(CurrentGravitySource.Position - SemimajorAxis * (Vector2)EccentricityVector, 1000f * HyperbolicAsymptotes[1].RotateVector(ArgumentOfPeriapsis));
        }
    }
}
