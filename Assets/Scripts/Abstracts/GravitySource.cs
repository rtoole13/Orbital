using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public abstract class GravitySource : OrbitalBody
{
    private float _radiusOfInfluence;
    private int _sourceRank = 0;
    private CircleCollider2D bodyCollider;
    private SphereOfInfluence sphereOfInfluence;
    private List<GravityAffected> gravityAffectedObjects = new List<GravityAffected>();

    #region GETSET

    public float Radius
    {
        get { return bodyCollider.radius; }
    }

    public float RadiusOfInfluence
    {
        get { return _radiusOfInfluence; }
        private set {
            _radiusOfInfluence = value;
            sphereOfInfluence.UpdateRadius(value);
        }
    }

    public int SourceRank
    {
        get { return _sourceRank; }
        private set { _sourceRank = value; }
    }
    #endregion GETSET

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        
        // Get bodyCollider
        bodyCollider = GetComponent<CircleCollider2D>();
        if (bodyCollider.isTrigger)
        {
            throw new UnityException(string.Format("{0}'s circle collider must not be isTrigger!", gameObject.name));
        }
        sphereOfInfluence = GetComponentInChildren<SphereOfInfluence>();
        if (sphereOfInfluence == null)
        {
            throw new UnityException(string.Format("{0} must have a SphereOfInfluence child!", gameObject.name));
        }
        SourceRank = CalculateSourceRank(0);
    }

    protected override void Start()
    {
        base.Start();
        if (CurrentGravitySource == null)
            return;
        updateIteratively = false;
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.Position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.startVelocity;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }

    private void FixedUpdate()
    {
        UpdateDeterministically();
    }

    #endregion UNITY

    public int CalculateSourceRank(int count)
    {
        if (CurrentGravitySource == null)
        {
            return count;
        }
        return CurrentGravitySource.CalculateSourceRank(count + 1);
    }
    protected override void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        base.CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
        RadiusOfInfluence = CurrentGravitySource == null
            ? Mathf.Infinity
            : OrbitalMechanics.RadiusOfInfluence(SemimajorAxis, Mass, CurrentGravitySource.Mass);
    }

    public Vector2 CalculateGravitationalForceAtPosition(Vector2 position, float mass) //DEPRECATED, remove in favor of OrbitalMechanics method
    {
        Vector2 distance = (Vector2)bodyCollider.transform.position - position;
        float forceMagnitude = OrbitalMechanics.GRAVITATIONALCONSTANT * Mass * mass / Vector2.SqrMagnitude(distance);
        Vector2 force = forceMagnitude * distance.normalized;
        return force;
    }

    public void AddAffectedBody(GravityAffected body)
    {
        gravityAffectedObjects.Add(body);
    }

    public void RemoveAffectedBody(GravityAffected body)
    {
        gravityAffectedObjects.Remove(body);
    }

    protected override void OnDrawGizmos()
    {
        if (CurrentGravitySource == null || body == null)
            return;

        // Draw SOI
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Position, RadiusOfInfluence);
        base.OnDrawGizmos();
    }
}
