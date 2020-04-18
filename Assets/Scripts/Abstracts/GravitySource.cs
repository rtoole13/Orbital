﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public abstract class GravitySource : OrbitalBody
{
    private float _radiusOfInfluence;
    private float _radiusOfInfluenceSq;
    private int _sourceRank = 0;
    private CircleCollider2D bodyCollider;

    private List<GravityAffected> gravityAffectedObjects = new List<GravityAffected>();

    [SerializeField]
    private List<GravitySource> _orbitalBodies;

    #region GETSET

    public float Radius
    {
        get { return bodyCollider.radius; }
    }

    public float RadiusOfInfluence
    {
        get { return _radiusOfInfluence; }
        private set
        {
            _radiusOfInfluence = value;
            _radiusOfInfluenceSq = Mathf.Pow(value, 2);
        }
    }

    public float RadiusOfInfluenceSq
    {
        get { return _radiusOfInfluenceSq; }
    }

    public int SourceRank
    {
        get { return _sourceRank; }
        private set { _sourceRank = value; }
    }

    public List<GravitySource> OrbitalBodies
    {
        get { return _orbitalBodies; }
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
        UpdatingIteratively = false;
        if (CurrentGravitySource == null)
            return;
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.transform.position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.startVelocity;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }

    protected override void Start()
    {
        
        base.Start();
    }

    protected virtual void Update()
    {
        CheckCurrentOrbitingObjects();
        CheckForNewOrbitingObjects();
    }

    private void FixedUpdate()
    {
    }

    #endregion UNITY
    #region PHYSICS

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
    #endregion PHYSICS
    #region GENERAL
    public void InitializeSystem(GravitySource parent)
    {
        CurrentGravitySource = parent;
        if (parent == null)
        {
            SourceRank = 0;
        }
        else
        {
            SourceRank = CurrentGravitySource.SourceRank + 1;
        }
        for (int i = 0; i < OrbitalBodies.Count; i++)
        {
            OrbitalBodies[i].InitializeSystem(this);
        }
    }

    public void UpdateSystem()
    {
        UpdateDeterministically();
        for (int i = 0; i < OrbitalBodies.Count; i++)
        {
            GravitySource gravitySource = OrbitalBodies[i];
            if (gravitySource == null || !gravitySource.isActiveAndEnabled)
                continue;
            gravitySource.UpdateSystem();
        }
        //for (int i = 0; i < OrbitalBodies.Count; i++)
        //{
        //    OrbitalBodies[i].UpdateSystem();
        //}
    }

    public GravitySource GetGravitySourceAtPosition(Vector2 position, bool firstLevel)
    {
        // Given a position, get the most influential gravity source.
        // firstLevel defines whether to recurse through gravitySources or not
        
        for (int i = 0; i < OrbitalBodies.Count; i++)
        {
            GravitySource gravitySource = OrbitalBodies[i];
            float distSq = Vector2.SqrMagnitude(gravitySource.Position - position);
            if (distSq < gravitySource.RadiusOfInfluenceSq)
            {
                if (firstLevel)
                {
                    return gravitySource;
                }
                else
                {
                    return gravitySource.GetGravitySourceAtPosition(position, false);
                }
            }
        }
        return this;
    }

    public void CheckForNewOrbitingObjects()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Position, RadiusOfInfluence);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            GravityAffected gravityAffected = collider.GetComponent<GravityAffected>();
            if (gravityAffected == null)
                continue;

            float dist = (gravityAffected.Position - Position).magnitude;
            if (dist <= RadiusOfInfluence && !gravityAffectedObjects.Contains(gravityAffected))
            {
                gravityAffected.EnterSphereOfInfluence(this);
                gravityAffectedObjects.Add(gravityAffected);
            }
        }
    }

    public void CheckCurrentOrbitingObjects()
    {
        for (int i = 0; i < gravityAffectedObjects.Count; i++)
        {
            GravityAffected gravityAffected = gravityAffectedObjects[i];
            float dist = (gravityAffected.Position - Position).magnitude;
            if (dist > RadiusOfInfluence)
            {
                gravityAffectedObjects.Remove(gravityAffected);
            }
        }
    }

    public int CalculateSourceRank(int count)
    {
        if (CurrentGravitySource == null)
        {
            return count;
        }
        return CurrentGravitySource.CalculateSourceRank(count + 1);
    }
    #endregion GENERAL

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
