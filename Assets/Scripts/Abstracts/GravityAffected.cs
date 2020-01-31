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
    private float _eccentricity;
    private float _specificOrbitalEnergy;
    private float _argumentOfPeriapsis;
    private float _semimajorAxis;

    private Rigidbody2D body;

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
            Vector3 diff = CurrentGravitySource.transform.position - this.transform.position;
            return diff.magnitude;
        }
    }
    
    public Vector3 SourceRelativePosition
    {
        get
        {
            if (CurrentGravitySource == null)
                return Vector3.positiveInfinity;
            return this.transform.position - CurrentGravitySource.transform.position;
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
    #endregion GETSET

    #region UNITY
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        CalculateOrbitalParameters();
        /*
        Debug.Log("SOE: " + SpecificOrbitalEnergy);
        Debug.Log("SRAM: " + SpecificRelativeAngularMomentum);
        Debug.Log("E vec: " + EccentricityVector);
        Debug.Log("a: " + SemimajorAxis);
        */
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
    #endregion UNITY

    #region PHYSICS
    public void CalculateOrbitalParameters()
    {
        SpecificRelativeAngularMomentum = CalculateSpecificRelativeAngularMomentum();
        EccentricityVector = CalculateEccentricityVector();
        SemimajorAxis = CalculateSemimajorAxis();
        SpecificOrbitalEnergy = CalculateSpecificOrbitalEnergy();
        ArgumentOfPeriapsis = CalculateArgumentOfPeriapse();

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
        if (SpecificRelativeAngularMomentum.z > 0)
        {
            return 2 * Mathf.PI - omega;
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

    /*
    private float CalculateTrueAnomoly()
    {
        return 0f;
    }
    */
    #endregion PHYSICS
}
