using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : MonoBehaviour
{
    [SerializeField]
    private float _mass = 1.0f;
    private GravitySource _gravitySource;
    private float semimajorAxis;
    private float semiminorAxis;
    private Rigidbody2D body;

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
            {
                return Mathf.Infinity;
            }
            Vector3 diff = CurrentGravitySource.transform.position - this.transform.position;
            return diff.magnitude;
        }
    }

    public Vector3 SourceRelativePosition
    {
        get
        {
            if (CurrentGravitySource == null)
            {
                return Vector3.positiveInfinity;
            }
            return this.transform.position - CurrentGravitySource.transform.position;
        }
    }

    public Vector3 SourceRelativeVelocity
    {
        get
        {
            if (CurrentGravitySource == null)
            {
                return new Vector3(body.velocity.x, body.velocity.y, 0f);
            }
            Vector3 thisVelocity = new Vector3(body.velocity.x, body.velocity.y, 0f);
            return thisVelocity - CurrentGravitySource.Velocity;
        }

    }
    public float SourceDistanceSquared
    {
        get
        {
            if (CurrentGravitySource == null)
            {
                return Mathf.Infinity;
            }
            Vector3 diff = CurrentGravitySource.transform.position - this.transform.position;
            return diff.sqrMagnitude;
        }
    }

    public float StandardGravityParameter
    {
        get
        {
            if (CurrentGravitySource == null)
            {
                return 0f;
            }
            return CurrentGravitySource.GRAVITYCONSTRANT * (CurrentGravitySource.Mass + Mass);
        }
    }

    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    protected float CalculateSemimajorAxis()
    {
        if (!CurrentGravitySource)
            return Mathf.Infinity;
        float denom = (2 / SourceDistance) - (body.velocity.sqrMagnitude / (CurrentGravitySource.GRAVITYCONSTRANT * CurrentGravitySource.Mass));
        return 1 / denom;
    }

    protected float CalculateEccentricity()
    {
        return Mathf.Sqrt(1f + (2 * CalculateSpecificOrbitalEnergy() * CalculateSpecificRelativeAngularMomentum().sqrMagnitude) / Mathf.Pow(StandardGravityParameter, 2));
    }

    protected Vector3 CalculateSpecificRelativeAngularMomentum()
    {
        return Vector3.Cross(SourceRelativePosition, SourceRelativeVelocity);
    }

    protected Vector3 CalculateAscendingNodeDir()
    {
        return Vector3.Cross(Vector3.forward, CalculateSpecificRelativeAngularMomentum());
    }

    protected Vector3 CalculateEccentricityVector()
    {
        Vector3 relativePosition = SourceRelativePosition;
        return (Vector3.Cross(SourceRelativeVelocity, CalculateSpecificRelativeAngularMomentum()) / StandardGravityParameter) - (relativePosition / relativePosition.magnitude);
    }

    protected float CalculateSpecificOrbitalEnergy()
    {
        float a = CalculateSemimajorAxis();
        return -1f * StandardGravityParameter / (2f * a);
    }

    protected float CalculateArgumentOfPeriapse()
    {
        Vector3 eccentricityVector = CalculateEccentricityVector();
        return Mathf.Acos(Vector3.Dot(Vector3.right, eccentricityVector) / (1f * eccentricityVector.magnitude));
    }
    private float CalculateTrueAnomoly()
    {
        return 0f;
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
}
