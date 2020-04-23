using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : OrbitalBody
{
    protected bool nonGravitationalForcesAdded = false;

    private bool recentlyChangedSource = false;
    private float sourceChangeInterval = .2f;
    private List<Vector2> nonGravitationalForces;
    private List<GravitySource> possibleGravitySources;

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        nonGravitationalForces = new List<Vector2>();
        possibleGravitySources = new List<GravitySource>();
    }

    protected override void Start()
    {
        base.Start();
        UpdatingIteratively = false;
        body.isKinematic = true;
        DeterministicSystem system = FindObjectOfType<DeterministicSystem>();
        //InitializeNewOrbit(system.PrimarySource.GetGravitySourceAtPosition(Position, false));
        CurrentGravitySource = system.PrimarySource.GetGravitySourceAtPosition(Position, false);
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.transform.position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.startVelocity;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }

    protected virtual void Update(){}

    protected virtual void FixedUpdate()
    {
        UpdateCurrentGravitySource();
        if (UpdatingIteratively)
        {
            if (!nonGravitationalForcesAdded)
            {
                SwitchToDeterministicUpdate();
                UpdateDeterministically();
                return;
            }
            UpdateIteratively();
        }
        else
        {
            if (nonGravitationalForcesAdded)
            {
                SwitchToIterativeUpdate();
                UpdateIteratively();
                return;
            }
            UpdateDeterministically();
        }
    }

    private void SwitchToDeterministicUpdate()
    {
        body.isKinematic = true;
        InitializeNewOrbit(CurrentGravitySource);
        UpdatingIteratively = false;
    }

    private void SwitchToIterativeUpdate()
    {
        body.isKinematic = false;
        UpdatingIteratively = true;
        body.velocity = OrbitalVelocityToWorld;
    }

    private void UpdateIteratively()
    {
        // Gravitational force
        Vector2 gravitationalForce = CurrentGravitySource.CalculateGravitationalForceAtPosition(transform.position, Mass);
        body.AddForce(gravitationalForce);
        
        // Other forces
        ApplyNonGravitationalForces();

        Vector2 relVel = Velocity - CurrentGravitySource.Velocity;  // world vel - newSource.vel
        Vector2 relPos = Position - CurrentGravitySource.Position; // world pos - newSource.pos
        CalculateMinimalOrbitalParameters(relVel, relPos);
    }

    
    #endregion UNITY

    #region PHYSICS

    public void AddExternalForce(Vector2 forceVector)
    {
        if (Time.timeScale != 1f) // FIXME: Potential float comparison issue
        {
            Debug.LogFormat("Force applied! Dropping time warp from {0}x to 1x.", Time.timeScale);
            Time.timeScale = 1f;
        }
        
        nonGravitationalForcesAdded = true;
        nonGravitationalForces.Add(forceVector);
    }

    private void ApplyNonGravitationalForces()
    {
        for (int i = 0; i < nonGravitationalForces.Count; i++)
        {
            //body.AddForce(nonGravitationalForces[i], ForceMode2D.Impulse);
            body.AddForce(nonGravitationalForces[i]);
        }
        nonGravitationalForces.Clear();
        nonGravitationalForcesAdded = false;
    }
    
    protected bool LeavingSphereOfInfluence()
    {
        if (CurrentGravitySource == null)
            return false;

        if (CurrentGravitySource.CurrentGravitySource == null)
            return false;
        if (OrbitalRadius < CurrentGravitySource.RadiusOfInfluence)
        {
            return false;
        }
        return true;
    }

    protected void InitializeNewOrbit(GravitySource newSource)
    {
        Vector2 relVel = Velocity - newSource.Velocity;  // world vel - newSource.vel
        Vector2 relPos = Position - newSource.Position; // world pos - newSource.pos
        CurrentGravitySource = newSource;
        CalculateOrbitalParametersFromStateVectors(relPos, relVel);
        //Debug.LogFormat("SemimajorAxis: {0}", SemimajorAxis);
        //Debug.LogFormat("Eccentricity: {0}", Eccentricity);
        //Debug.LogFormat("EccentricAnomaly: {0}", EccentricAnomaly);
        //Debug.LogFormat("MeanAnomaly: {0}", MeanAnomaly);
        //Debug.LogFormat("OrbitalRadius: {0}", OrbitalRadius);
        //Debug.LogFormat("OrbitalPosition: {0}", OrbitalPosition);
        recentlyChangedSource = true;
        IEnumerator recentSourceChangeCoroutine = ChangeSourceTimer();
        StartCoroutine(recentSourceChangeCoroutine);
    }

    private void UpdateCurrentGravitySource()
    {
        if (recentlyChangedSource)
            return;

        if (LeavingSphereOfInfluence() && CurrentGravitySource.CurrentGravitySource != null) // Leaving current source
        {
            Debug.LogFormat("{0}'s leaving {1}'s sphere of influence. Entering {2}'s.", gameObject.name, CurrentGravitySource.name, CurrentGravitySource.CurrentGravitySource.name);
            InitializeNewOrbit(CurrentGravitySource.CurrentGravitySource);
        }
        else
        {
            GravitySource dominantGravitySource = CurrentGravitySource.GetGravitySourceAtPosition(Position, true);
            if (dominantGravitySource != CurrentGravitySource)
            {
                Debug.LogFormat("{0}'s leaving {1}'s sphere of influence. Entering {2}'s.", gameObject.name, CurrentGravitySource.name, dominantGravitySource.name);
                InitializeNewOrbit(dominantGravitySource);
            }
        }
    }
    #endregion PHYSICS

    private IEnumerator ChangeSourceTimer()
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(sourceChangeInterval);
        recentlyChangedSource = false;
    }
}

