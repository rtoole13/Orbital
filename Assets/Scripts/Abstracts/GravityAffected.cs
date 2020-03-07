using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : MonoBehaviour, ICameraTrackable
{
    [SerializeField]
    private float _mass = 1.0f;
    private GravitySource _gravitySource;
    private Vector3 _specificRelativeAngularMomentum;
    private Vector3 _eccentricityVector;
    private Vector2 _orbitalPosition;
    private float _trueAnomaly;
    private float _eccentricity;
    private float _specificOrbitalEnergy;
    private float _argumentOfPeriapsis;
    private float _semimajorAxis;
    private float _semiminorAxis;
    private float _period;
    private float _orbitalPeriod;
    private float _meanAnomalyAtEpoch;
    private float _meanAnomaly;
    private float _eccentricAnomaly;
    private float _meanMotion;
    private float _timeSinceEpoch;
    private float _hillRadius; // FIXME: Current hill radius. If extend beyond this, gravitySource drops back to parent gravity source.
    
    private List<Vector2> nonGravitationalForces;
    private enum TrajectoryType
    {
        Ellipse = 0,
        Hyperbola = 1
    }
    private TrajectoryType currentTrajectoryType;
    private bool updateIteratively = true;
    private Vector2 deterministicVelocity;

    protected Rigidbody2D body;
    protected bool nonGravitationalForcesAdded = true;
    protected bool determineTrajectory = false;

    //GIZMOS VARS
    private Vector2 lastEpochPos = Vector2.zero;
    private Vector2 currentEpochPosition = Vector2.zero;
    private bool canUpdateEpochs = true;
    private float elapsedEpochTime = 0f;
    private Vector2 trajectoryPosition = Vector2.zero;

    #region GETSET
    public float Mass {
        get { return _mass; }
        private set { _mass = value; }
    }

    public GravitySource CurrentGravitySource
    {
        get { return _gravitySource; }
        private set { _gravitySource = value; }
    }

    public float SourceDistance
    {
        get { 
            if (CurrentGravitySource == null)
                return Mathf.Infinity;
            Vector3 diff = CurrentGravitySource.transform.position - transform.position;
            return diff.magnitude;
        }
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
        private set
        {
            _specificRelativeAngularMomentum = value;
        }
    }

    public Vector3 EccentricityVector
    {
        get { return _eccentricityVector; }
        private set
        {
            _eccentricityVector = value;
            _eccentricity = _eccentricityVector.magnitude;
            if (_eccentricity < 1f)
            {
                currentTrajectoryType = TrajectoryType.Ellipse;
            }
            else
            {
                // == 1 more or less impossible, ignore parabola
                currentTrajectoryType = TrajectoryType.Hyperbola;
            }
        }
    }

    public float Eccentricity
    {
        get { return _eccentricity; }
    }

    public float SpecificOrbitalEnergy
    {
        get { return _specificOrbitalEnergy; }
        private set { _specificOrbitalEnergy = value; }

    }
    
    public float ArgumentOfPeriapsis
    {
        get { return _argumentOfPeriapsis; }
        private set { _argumentOfPeriapsis = value; }
    }

    public float SemimajorAxis
    {
        get { return _semimajorAxis; }
        private set { _semimajorAxis = value; }
    }

    public float SemiminorAxis
    {
        get { return _semiminorAxis; }
        private set { _semiminorAxis = value; }
    }

    public float Period
    {
        get { return _period; }
        private set { _period = value; }
    }

    public Vector2 OrbitalPosition
    {
        get { return _orbitalPosition; }
        private set { _orbitalPosition = value; }
    }

    public float OrbitalPeriod
    {
        get { return _orbitalPeriod; }
        private set { _orbitalPeriod = value; }
    }

    public float MeanMotion
    {
        get { return _meanMotion; }
        private set { _meanMotion = value; }
    }

    public float MeanAnomalyAtEpoch
    {
        get { return _meanAnomalyAtEpoch; }
        private set { _meanAnomalyAtEpoch = value; }
    }

    public float MeanAnomaly
    {
        get { return _meanAnomaly; }
        private set { _meanAnomaly = value; }
    }

    public float EccentricAnomaly
    {
        get { return _eccentricAnomaly; }
        private set { _eccentricAnomaly = value; }
    }

    public float TimeSinceEpoch
    {
        get { return _timeSinceEpoch; }
        private set { _timeSinceEpoch = value; }
    }

    public float TrueAnomaly
    {
        get { return _trueAnomaly; }
        private set { _trueAnomaly = value; }
    }
    #endregion GETSET

    #region UNITY
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        nonGravitationalForces = new List<Vector2>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }

    protected void FixedUpdate()
    {
        if (CurrentGravitySource == null)
            return;

        if (updateIteratively)
        {
            // Apply forces, regular rigidbody stuff.
            UpdateIteratively();
        }
        else
        {
            UpdateByTrajectory();
        }

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        CurrentGravitySource = source.GetGravitySource();
        source.AddAffectedBody(this);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        CurrentGravitySource = null;
        source.RemoveAffectedBody(this);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // On Collision w/ an object, stop deterministic trajectory update (normal force)
        Debug.Log("Collision!");
        SwitchToIterativeUpdate();
    }

    private void SwitchToDeterministicUpdate()
    {
        // Switching from iterative to deterministic trajectory update
        if (!updateIteratively)
            return;

        updateIteratively = false;
        body.isKinematic = true;
        CalculateOrbitalParameters();
        CalculateEpochParameters();
    }

    private void SwitchToIterativeUpdate()
    {
        // Switching from deterministic trajectory to iterative update
        if (updateIteratively)
            return;

        updateIteratively = true;
        body.isKinematic = false;
        body.velocity = deterministicVelocity.RotateVector(ArgumentOfPeriapsis);
    }

    #endregion UNITY

    #region DETERMINISTIC
    // Basically, rigidbody.iskinematic = true;
    private void UpdateByTrajectory()
    {
        if (nonGravitationalForcesAdded)
        {
            SwitchToIterativeUpdate();
            return;
        }

        TimeSinceEpoch = (TimeSinceEpoch + Time.fixedDeltaTime) % OrbitalPeriod;
        MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        if (MeanAnomaly >= 0f)
        {
            EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
            TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
            transform.position = OrbitalPositionToWorld(OrbitalMechanics.OrbitalPosition(Eccentricity, SemimajorAxis, TrueAnomaly));
            deterministicVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
        }
        
    }

    #endregion DETERMINISTIC

    #region ITERATIVE
    // Basically, rigidbody.iskinematic = false;
    private void UpdateIteratively()
    {
        //Add other forces. Collisions?
        /*
        if (nonGravitationalForces.Count == 0 && currentTrajectoryType == TrajectoryType.Ellipse)
        {
            nonGravitationalForcesAdded = false;
            SwitchToDeterministicUpdate();
            return;
        }
        */

        Vector2 gravitationalForce = CurrentGravitySource.CalculateGravitationalForceAtPosition(transform.position, Mass);
        body.AddForce(gravitationalForce);
        ApplyNonGravitationalForces();
        if (Input.GetMouseButtonUp(0))
        {
            CalculateOrbitalParameters();
            CalculateEpochParameters();
        }
        if (Input.GetMouseButton(1))
        {
            CalculateOrbitalParameters();
            CalculateEpochParameters();
        }
        TimeSinceEpoch = (TimeSinceEpoch + Time.fixedDeltaTime) % OrbitalPeriod;
        MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        if (MeanAnomaly >= 0f) //FIXME this check shouldnt exist.
        {
            EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
            TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
            trajectoryPosition = OrbitalPositionToWorld(OrbitalMechanics.OrbitalPosition(Eccentricity, SemimajorAxis, TrueAnomaly));
            //Debug.Log("Old: " + CalculateVelocityFromOrbitalParameters());
            //Debug.Log("New: " + OrbitalMechanics.CalculateVelocityFromOrbitalParameters(SpecificRelativeAngularMomentum, SourceRelativePosition, CurrentGravitySource.Mass, SemimajorAxis));
        }
    }
    #endregion ITERATIVE

    #region PHYSICS
    public void CalculateOrbitalParameters()
    {
        // This should only be called from the iterative update method and only once before switching to trajectory update.
        SpecificRelativeAngularMomentum = OrbitalMechanics.SpecificRelativeAngularMomentum(SourceRelativePosition, SourceRelativeVelocity);
        EccentricityVector = OrbitalMechanics.EccentricityVector(SourceRelativePosition, SourceRelativeVelocity, SpecificRelativeAngularMomentum, CurrentGravitySource.Mass, Mass);
        SemimajorAxis = OrbitalMechanics.SemimajorAxis(SourceDistance, body.velocity.sqrMagnitude, CurrentGravitySource.Mass);
        SemiminorAxis = OrbitalMechanics.SemiminorAxis(SemimajorAxis, Eccentricity);
        SpecificOrbitalEnergy = OrbitalMechanics.SpecificOrbitalEnergy(CurrentGravitySource.Mass, Mass, SemimajorAxis);
        ArgumentOfPeriapsis = OrbitalMechanics.ArgumentOfPeriapse(EccentricityVector);
    }
    
    public void CalculateEpochParameters()
    {
        TimeSinceEpoch = 0f;
        MeanMotion = OrbitalMechanics.MeanMotion(CurrentGravitySource.Mass, Mass, SemimajorAxis);
        OrbitalPeriod = OrbitalMechanics.OrbitalPeriod(MeanMotion);
        EccentricAnomaly = OrbitalMechanics.EccentricAnomalyAtEpoch(SourceRelativePosition, SourceRelativeVelocity, CurrentGravitySource.Mass, Mass, Eccentricity);
        MeanAnomalyAtEpoch = OrbitalMechanics.MeanAnomalyAtEpoch(EccentricAnomaly, Eccentricity);
        MeanAnomaly = MeanAnomalyAtEpoch;
        TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
        deterministicVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
    }

    public void AddExternalForce(Vector2 forceVector)
    {
        nonGravitationalForcesAdded = true;
        nonGravitationalForces.Add(forceVector);   
    }

    private void ApplyNonGravitationalForces()
    {
        for (int i = 0; i < nonGravitationalForces.Count; i++)
        {
            body.AddForce(nonGravitationalForces[i]);
        }
        nonGravitationalForces.Clear();
    }
    #endregion PHYSICS
    #region GENERAL
    private Vector2 OrbitalPositionToWorld(Vector2 orbitalPosition)
    {
        return orbitalPosition.RotateVector(ArgumentOfPeriapsis) + CurrentGravitySource.Position;
    }
    #endregion GENERAL  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(lastEpochPos, new Vector3(.5f, .5f, .5f));
        Gizmos.color = Color.green;
        Gizmos.DrawCube(currentEpochPosition, new Vector3(.5f, .5f, .5f));
        float updateInterval = 0.05f;
        if (TimeSinceEpoch > 0 && TimeSinceEpoch < updateInterval && canUpdateEpochs)
        {
            lastEpochPos = currentEpochPosition + CurrentGravitySource.Position;
            currentEpochPosition = body.position + CurrentGravitySource.Position;
            canUpdateEpochs = false;
            elapsedEpochTime = 0f;
        }
        elapsedEpochTime += Time.fixedDeltaTime;
        if (elapsedEpochTime > updateInterval + 1f)
            canUpdateEpochs = true;

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(trajectoryPosition, new Vector3(.5f, .5f, .5f));
    }
}
