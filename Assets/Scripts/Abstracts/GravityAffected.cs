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

    public GravitySource gravitySource
    {
        get { return _gravitySource; }
        private set { _gravitySource = value; }
    }

    public float SourceDistance
    {
        get { 
            if (gravitySource == null)
            {
                return Mathf.Infinity;
            }
            Vector3 diff = gravitySource.transform.position - this.transform.position;
            return diff.magnitude;
        }
    }

    public float SourceDistanceSquared
    {
        get
        {
            if (gravitySource == null)
            {
                return Mathf.Infinity;
            }
            Vector3 diff = gravitySource.transform.position - this.transform.position;
            return diff.sqrMagnitude;
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    protected float calculateSemimajorAxis()
    {
        if (!gravitySource)
            return Mathf.Infinity;
        float denom = (2 / SourceDistance) - (body.velocity.sqrMagnitude / (gravitySource.GRAVITYCONSTRANT * gravitySource.Mass));
        return 1 / denom;
    }

    protected float calculateSemiminorAxis()
    {
        float num = Mathf.Pow(body.transform.position.y, 2) * Mathf.Pow(calculateSemimajorAxis(), 2);
        float denom = 1 - Mathf.Pow(body.transform.position.x, 2);
        return Mathf.Sqrt(num / denom);
    }

    private float calculateTrueAnomoly()
    {
        return 0f;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        gravitySource = source.GetGravitySource();
        source.AddAffectedBody(this);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        gravitySource = null;
        source.RemoveAffectedBody(this);
    }
}
