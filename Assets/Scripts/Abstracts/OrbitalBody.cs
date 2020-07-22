using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class OrbitalBody : MonoBehaviour
{
    public Vector2 startVelocity;

    private float _argumentOfPeriapsis;
    private float _eccentricity;
    private Vector3 _eccentricityVector;
    private float _flightPathAngle;
    private GravitySource _gravitySource;
    private Vector2[] _hyperbolicAsymptotes;
    private float _hyperbolicExcessVelocity;
    private float _trueAnomalyOfAsymptote;
    [SerializeField]
    private float _mass = 1.0f;
    private float _orbitalPeriod;
    private Vector2 _orbitalPosition;
    private float _orbitalRadius;
    private float _orbitalSpeed;
    private Vector2 _orbitalVelocity;
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
    private bool _updatingIteratively = true;
    protected Rigidbody2D body;
    protected KeplerSolver trajectorySolver;
    protected UniversalVariableSolver uvmSolver;

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
        get { return _orbitalVelocity; }
        protected set { _orbitalVelocity = value; }
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
        trajectorySolver = new KeplerSolver();
        //trajectorySolver = new UniversalVariableSolver();
        uvmSolver = new UniversalVariableSolver();
    }

    protected virtual void Start()
    {
    }

    #endregion UNITY

    #region PHYSICS
    protected virtual void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        // Get Basic Orbital Elements
        SpecificRelativeAngularMomentum = Mechanics.Trajectory.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        ClockWiseOrbit = SpecificRelativeAngularMomentum.z < 0;
        EccentricityVector = Mechanics.Trajectory.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, SpecificRelativeAngularMomentum, CurrentGravitySource.Mass);
        SemimajorAxis = Mechanics.Trajectory.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, CurrentGravitySource.Mass);
        SemimajorAxisReciprocal = 1f / SemimajorAxis;
        SemiminorAxis = Mechanics.Trajectory.SemiminorAxis(SemimajorAxis, Eccentricity);
        SpecificOrbitalEnergy = Mechanics.Trajectory.SpecificOrbitalEnergy(CurrentGravitySource.Mass, SemimajorAxis);
        ArgumentOfPeriapsis = Mechanics.Trajectory.ArgumentOfPeriapse(EccentricityVector, sourceRelativePosition);

        // Initialize Solver
        trajectorySolver.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass, SpecificRelativeAngularMomentum, EccentricityVector, SemimajorAxis);
        uvmSolver.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass, SpecificRelativeAngularMomentum, EccentricityVector, SemimajorAxis);

        // Epoch parameters
        TimeSinceEpoch = 0f;

        UpdateStateVectorsBySolver();

        // Invoke orbit calculation event, triggering things like trajectory drawing
        OnOrbitCalculationEvent(); 
    }
    #endregion PHYSICS

    #region GENERAL

    protected virtual void UpdateDeterministically(){
        if (CurrentGravitySource == null)
            return;

        TimeSinceEpoch += Time.fixedDeltaTime;
        trajectorySolver.UpdateStateVariables(TimeSinceEpoch);
        uvmSolver.UpdateStateVariables(Time.fixedDeltaTime);
        Debug.LogFormat("kepler: {0}, uvm: {1}", trajectorySolver.CalculatedPosition, uvmSolver.CalculatedPosition);
        UpdateStateVectorsBySolver();
    }

    private void UpdateStateVectorsBySolver()
    {
        // Pull parameters from solver
        OrbitalPosition = trajectorySolver.CalculatedPosition;
        OrbitalRadius = trajectorySolver.CalculatedRadius;
        OrbitalSpeed = trajectorySolver.CalculatedSpeed;
        OrbitalVelocity = trajectorySolver.CalculatedVelocity;
        FlightPathAngle = trajectorySolver.FlightPathAngle;
        TrueAnomaly = trajectorySolver.TrueAnomaly;
        
        // Transform from perifocal to world position
        transform.position = OrbitalPositionToWorld;
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
