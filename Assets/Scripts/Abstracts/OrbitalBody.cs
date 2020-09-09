using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class OrbitalBody : MonoBehaviour
{
    public Vector2 startVelocity;

    private float _flightPathAngle;
    private GravitySource _gravitySource;

    [SerializeField]
    private float _mass = 1.0f;
    private Vector2 _orbitalPosition;
    private float _orbitalRadius;
    private float _orbitalSpeed;
    private Vector2 _orbitalVelocity;
    private float _timeSinceEpoch;
    private TrajectoryHandler _trajectoryHandler;
    private float _trueAnomaly;
    private bool _updatingIteratively = true;

    protected Rigidbody2D body;
    protected UniversalVariableSolver trajectorySolver;

    public delegate void OnOrbitCalculation();
    public event OnOrbitCalculation OnOrbitCalculationEvent;

    #region GETSET
    public Trajectory Trajectory { get; private set; }

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

    public float FlightPathAngle
    {
        get { return _flightPathAngle; }
        private set { _flightPathAngle = value; }
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
            return OrbitalPosition.RotateVector(Trajectory.ArgumentOfPeriapsis) + position;
        }
    }

    public Vector2 OrbitalVelocityToWorld
    {   
        get 
        {
            Vector2 velocity = CurrentGravitySource != null
                ? CurrentGravitySource.Velocity
                : Vector2.zero;
            return OrbitalVelocity.RotateVector(Trajectory.ArgumentOfPeriapsis) + velocity;
        }
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
            Vector2 dir = Mechanics.Trajectory.OrbitalDirection(TrueAnomaly, FlightPathAngle, Trajectory.ClockWiseOrbit);
            return dir.RotateVector(Trajectory.ArgumentOfPeriapsis);
        }
    }

    public TrajectoryHandler TrajectoryHandler {
        get { return _trajectoryHandler; }
        private set { _trajectoryHandler = value; }
    }

    #endregion GETSET

    #region UNITY
    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.velocity = startVelocity;

        TrajectoryHandler = GetComponent<TrajectoryHandler>();
    }

    protected virtual void Start()
    {
        Trajectory = new Trajectory();
        trajectorySolver = new UniversalVariableSolver();
    }

    #endregion UNITY

    #region PHYSICS
    protected virtual void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        Trajectory.CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource);

        // Initialize Solver
        trajectorySolver.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, CurrentGravitySource.Mass, Trajectory);
        
        // Epoch parameters
        TimeSinceEpoch = 0f;

        UpdateStateVectorsBySolver();
        
        // Invoke orbit calculation event, triggering things like trajectory drawing
        OnOrbitCalculationEvent(); 
    }

    public Vector2 PredictPosition(float timeOfFlight)
    {
        Vector2 relativePosition = Position;
        Vector2 relativeVelocity = Velocity;
        if (CurrentGravitySource != null)
        {
            relativePosition -= CurrentGravitySource.Position;
            relativeVelocity -= CurrentGravitySource.Velocity;
        }
        // Initialize Solver
        UniversalVariableSolver tempTrajSolver = new UniversalVariableSolver();
        tempTrajSolver.InitializeSolver(relativePosition, relativeVelocity, CurrentGravitySource.Mass, Trajectory);

        // Solve for new position given time of flight
        tempTrajSolver.UpdateStateVariables(timeOfFlight);
        return tempTrajSolver.CalculatedPosition.RotateVector(Trajectory.ArgumentOfPeriapsis) + CurrentGravitySource.Position;
    }

    #endregion PHYSICS

    #region GENERAL

    protected virtual void UpdateDeterministically(){
        if (CurrentGravitySource == null)
            return;

        TimeSinceEpoch += Time.fixedDeltaTime;
        trajectorySolver.UpdateStateVariables(TimeSinceEpoch);
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
        return position.RotateVector(-Trajectory.ArgumentOfPeriapsis);
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
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(uvmSolver.CalculatedPosition.RotateVector(ArgumentOfPeriapsis), 1f);

        //if (Trajectory.TrajectoryType == Mechanics.Globals.TrajectoryType.Hyperbola)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawRay(CurrentGravitySource.Position - Trajectory.SemimajorAxis * (Vector2)Trajectory.EccentricityVector, 1000f * trajectorySolver.HyperbolicAsymptotes[0].RotateVector(Trajectory.ArgumentOfPeriapsis));
        //    Gizmos.DrawRay(CurrentGravitySource.Position - Trajectory.SemimajorAxis * (Vector2)Trajectory.EccentricityVector, 1000f * trajectorySolver.HyperbolicAsymptotes[1].RotateVector(Trajectory.ArgumentOfPeriapsis));
        //}
    }
}
