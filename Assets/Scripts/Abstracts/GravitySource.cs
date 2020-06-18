using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;

public abstract class GravitySource : OrbitalBody
{
    private float _radiusOfInfluence;
    private float _radiusOfInfluenceSq;
    private int _sourceRank = 0;

    [SerializeField]
    private List<GravitySource> _orbitalBodies;    
    
    #region GETSET
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

    protected virtual void Update(){}
    protected virtual void FixedUpdate(){}
    #endregion UNITY
    #region PHYSICS

    protected override void CalculateOrbitalParametersFromStateVectors(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {
        base.CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
        RadiusOfInfluence = CurrentGravitySource == null
            ? Mathf.Infinity
            : Mechanics.Trajectory.RadiusOfInfluence(SemimajorAxis, Mass, CurrentGravitySource.Mass);
    }

    public Vector2 CalculateGravitationalForceAtPosition(Vector2 position, float mass) //DEPRECATED, remove in favor of OrbitalMechanics method
    {
        Vector2 distance = (Vector2)transform.position - position;
        float forceMagnitude = Mechanics.Globals.GRAVITATIONALCONSTANT * Mass * mass / Vector2.SqrMagnitude(distance);
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

    public int CalculateSourceRank(int count)
    {
        if (CurrentGravitySource == null)
        {
            return count;
        }
        return CurrentGravitySource.CalculateSourceRank(count + 1);
    }
    #endregion GENERAL
}
