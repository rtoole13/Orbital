using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : MonoBehaviour
{
    [SerializeField]
    private float _mass = 1.0f;
    private GravitySource _gravitySource;
    private Vector3 _specificRelativeAngularMomentum;
    private Vector3 _eccentricityVector;
    private Vector2 _orbitalPosition;
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

    private List<Vector2> nonGravitationalForces;
    
    private enum TrajectoryType
    {
        Ellipse = 0,
        Hyperbola = 1
    }
    private TrajectoryType currentTrajectoryType;
    private Rigidbody2D body;

    protected bool nonGravitationalForcesAdded = true;
    protected bool determineTrajectory = false;
    
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
            return CurrentGravitySource.GRAVITYCONSTRANT * (CurrentGravitySource.Mass + Mass);
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
    #endregion GETSET

    #region UNITY
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        nonGravitationalForces = new List<Vector2>();
    }

    protected virtual void Update()
    {


        //if (adjustTrajectory)
        //UpdateTrajectory();

        /*
        CalculateOrbitalParameters();
        Debug.Log("E vec: " + EccentricityVector);
        Debug.Log("a: " + SemimajorAxis);
        Debug.Log("SOE: " + SpecificOrbitalEnergy);
        Debug.Log("SRAM: " + SpecificRelativeAngularMomentum);
        
        */
    }

    protected void FixedUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            nonGravitationalForcesAdded = false;
            //determineTrajectory = true;
        }
        else
        {
            nonGravitationalForcesAdded = true;
        }

        if (nonGravitationalForcesAdded)
        {
            if (body.isKinematic)
                body.isKinematic = false;

            //Apply forces, regular rigidbody stuff.
            UpdatePositionIteratively();
            determineTrajectory = true;
        }
        else
        {
            body.isKinematic = true;
            if (determineTrajectory)
            {
                // First frame switching from iterative update to trajectory update
                determineTrajectory = false;
                CalculateOrbitalParameters();
            }
            UpdatePositionByTrajectory();
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
        //On Collision w/ an object, stop deterministic trajectory update (normal force)
        nonGravitationalForcesAdded = true;
    }
    #endregion UNITY

    #region PHYSICS
    private void UpdatePositionByTrajectory()
    {
        if (CurrentGravitySource == null)
            return;

        UpdateMeanAnomaly();
        if (MeanAnomaly >= 0f)
        {
            Debug.Log("wee");
            UpdateEccentricAnomaly();
            transform.position = RotateVertex(CalculateOrbitalPosition(), ArgumentOfPeriapsis);
        }
        else
        {
            UpdatePositionIteratively();
        }
            
            
        /*
        float actualX = temp.x * Mathf.Cos(ArgumentOfPeriapsis) - temp.y * Mathf.Sin(ArgumentOfPeriapsis);
        float actualY = temp.y * Mathf.Sin(ArgumentOfPeriapsis) + temp.x * Mathf.Sin(ArgumentOfPeriapsis);
        Debug.Log("calcX: " + actualX + ", calcY: " + actualY);
        Debug.Log("actuX: " + transform.position.x + ", actuy: " + transform.position.y);
        
        float trueAnomaly = CalculateTrueAnomaly();
        OrbitalPosition = new Vector2(SemimajorAxis * Mathf.Cos(trueAnomaly), SemiminorAxis * Mathf.Sin(trueAnomaly));
        Debug.Log(CalculateVelocityFromOrbitalParameters());
        transform.position = TransformByGravitationalSourcePoint(OrbitalPosition);
        */
        //body.velocity = CalculateVelocityFromOrbitalParameters();
    }

    private void UpdatePositionIteratively()
    {
        if (CurrentGravitySource == null)
            return;

        Vector2 gravitationalForce = CurrentGravitySource.CalculateGravitationalForceAtPosition(transform.position, Mass);
        body.AddForce(gravitationalForce);

        //Add other forces. Collisions?
        ApplyNonGravitationalForces();
    }

    public void CalculateOrbitalParameters()
    {
        // This should only be called from the iterative update method and only once before switching to trajectory update.
        SpecificRelativeAngularMomentum = CalculateSpecificRelativeAngularMomentum();
        EccentricityVector = CalculateEccentricityVector();
        SemimajorAxis = CalculateSemimajorAxis();
        SemiminorAxis = CalculateSemiminorAxis();
        SpecificOrbitalEnergy = CalculateSpecificOrbitalEnergy();

        TimeSinceEpoch = 0f;
        MeanMotion = CalculateMeanMotion();
        OrbitalPeriod = CalculateOrbitalPeriod();
        EccentricAnomaly = CalculateEccentricAnomalyAtEpoch();
        MeanAnomalyAtEpoch = CalculateMeanAnomalyAtEpoch();
        MeanAnomaly = MeanAnomalyAtEpoch;
        ArgumentOfPeriapsis = CalculateArgumentOfPeriapse();

    }

    public Vector2 CalculateVelocityFromOrbitalParameters()
    {
        // get direction
        Vector2 direction = Vector3.Cross(SpecificRelativeAngularMomentum, SourceRelativePosition);

        // get speed via vis-viva
        float speed = Mathf.Sqrt(CurrentGravitySource.GRAVITYCONSTRANT * CurrentGravitySource.Mass * ((2 / SourceDistance) - (1 / SemimajorAxis)));

        return speed * direction;
    }

    public Vector3 CalculateSpecificRelativeAngularMomentum()
    {
        return Vector3.Cross(SourceRelativePosition, SourceRelativeVelocity);
    }

    public Vector3 CalculateEccentricityVector()
    {
        Vector3 relativePosition = SourceRelativePosition;
        return Vector3.Cross(SourceRelativeVelocity, SpecificRelativeAngularMomentum) / StandardGravityParameter - relativePosition.normalized;
    }

    public float CalculateSpecificOrbitalEnergy()
    {
        return -1f * StandardGravityParameter / (2f * SemimajorAxis);
    }

    public float CalculateArgumentOfPeriapse()
    {
        float omega = Mathf.Atan2(EccentricityVector.y, EccentricityVector.x);
        
        if (SpecificRelativeAngularMomentum.z < 0)
        {
            return 2*Mathf.PI - omega;
        }
        
        return omega;
    }

    public float CalculateSemimajorAxis()
    {
        if (!CurrentGravitySource)
            return Mathf.Infinity;
        float denom = (2 / SourceDistance) - (body.velocity.sqrMagnitude / (CurrentGravitySource.GRAVITYCONSTRANT * CurrentGravitySource.Mass));
        return 1 / denom;
    }

    public float CalculateSemiminorAxis()
    {
        if (!CurrentGravitySource)
            return Mathf.Infinity;

        if (currentTrajectoryType == TrajectoryType.Hyperbola)
        {
            
            return -1f * SemimajorAxis * Mathf.Sqrt(Mathf.Pow(Eccentricity, 2) - 1);
        }
        return SemimajorAxis * Mathf.Sqrt(1 - Mathf.Pow(Eccentricity, 2)); ;
    }

    public float CalculateTrueAnomaly()
    {
        // Angle b/w relative position vector and eccentricityVectory
        float nu = Mathf.Atan2(SourceRelativePosition.y, SourceRelativePosition.x) - Mathf.Atan2(EccentricityVector.y, EccentricityVector.x);

        
        if (SpecificRelativeAngularMomentum.z < 0)
        {

            return 2 * Mathf.PI - nu;
        }

        // Adjust to be b/w 0 and 2pi
        nu = (nu + 2 * Mathf.PI) % (2 * Mathf.PI);
        if (SpecificRelativeAngularMomentum.z < 0)
        {
            return 2 * Mathf.PI - nu;
        }
        return nu;
    }

    public float CalculateOrbitalPeriod()
    {
        return 2 * Mathf.PI / MeanMotion;
    }

    public Vector2 CalculateOrbitalPosition()
    {
        return new Vector2(Mathf.Cos(EccentricAnomaly) - Eccentricity, Mathf.Sin(EccentricAnomaly) * Mathf.Sqrt(1 - Mathf.Pow(Eccentricity, 2))) * SemimajorAxis;

    }
    public float CalculateEccentricAnomalyAtEpoch()
    {
        return Mathf.Acos((-1f / Eccentricity) * ((SourceDistance / SemimajorAxis) - 1f));
    }

    public float CalculateMeanAnomalyAtEpoch()
    {
        return EccentricAnomaly - Eccentricity * Mathf.Sin(EccentricAnomaly);
    }

    public float CalculateMeanMotion()
    {
        return Mathf.Sqrt(StandardGravityParameter / Mathf.Pow(SemimajorAxis, 3));
    }

    public void UpdateMeanAnomaly()
    {
        var time = (TimeSinceEpoch + Time.deltaTime) % OrbitalPeriod;
        TimeSinceEpoch = time; //Fixed delta time??
        MeanAnomaly = MeanAnomalyAtEpoch + MeanMotion * TimeSinceEpoch;
    }

    public void UpdateEccentricAnomaly()
    {
        float E = MeanAnomaly;
        while (true)
        {
            float deltaE = (E - Eccentricity * Mathf.Sin(E) - MeanAnomaly) / (1f - Eccentricity * Mathf.Cos(E));
            E -= deltaE;
            if (Mathf.Abs(deltaE) < 1e-6)
                break;
        }
        EccentricAnomaly = E;
    }

    private Vector2 RotateVertex(Vector2 vertex, float angle)
    {
        return new Vector2(vertex.x * Mathf.Cos(angle) - vertex.y * Mathf.Sin(angle),
                       vertex.x * Mathf.Sin(angle) + vertex.y * Mathf.Cos(angle));
    }

    private Vector2 TransformByGravitationalSourcePoint(Vector2 position)
    {
        position = RotateVertex(position, ArgumentOfPeriapsis);
        Vector2 offset = new Vector2(Mathf.Cos(ArgumentOfPeriapsis), Mathf.Sin(ArgumentOfPeriapsis)) * -1f * Eccentricity * SemimajorAxis;
        return position + offset;
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
        nonGravitationalForcesAdded = false;
    }
    #endregion PHYSICS
}
