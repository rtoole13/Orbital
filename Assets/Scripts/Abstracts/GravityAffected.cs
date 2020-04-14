using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : OrbitalBody
{

    protected bool nonGravitationalForcesAdded = true;

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
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.Position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.startVelocity;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }
    protected virtual void Update()
    {

    }

    private void FixedUpdate()
    {
        if (CurrentGravitySource == null) //FIXME remove w/e bandaid this is..
            return;

        UpdateCurrentGravitySource();
            

        UpdateDeterministically();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // On Collision w/ an object, stop deterministic trajectory update (normal force)
        SwitchToIterativeUpdate();
    }

    private void SwitchToDeterministicUpdate()
    {
        // Switching from iterative to deterministic trajectory update
        if (!UpdatingIteratively)
            return;

        //CalculateOrbitalParametersFromStateVectors();
        UpdatingIteratively = false;
        body.isKinematic = true;
    }

    private void SwitchToIterativeUpdate()
    {
        // Switching from deterministic trajectory to iterative update
        if (UpdatingIteratively)
            return;

        UpdatingIteratively = true;
        body.isKinematic = false;
        body.velocity = OrbitalVelocity.RotateVector(ArgumentOfPeriapsis);
    }

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
            //CalculateOrbitalParametersFromStateVectors();
        }
        if (Input.GetMouseButton(1))
        {
            //CalculateOrbitalParametersFromStateVectors();
        }
        TimeSinceEpoch = (TimeSinceEpoch + Time.fixedDeltaTime) % OrbitalPeriod;
        //MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        //if (MeanAnomaly >= 0f) //FIXME this check shouldnt exist.
        //{
        //    EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
        //    //TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
        //    //OrbitalPosition = OrbitalMechanics.OrbitalPosition(Eccentricity, SemimajorAxis, TrueAnomaly);
        //    //OrbitalVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
        //    //trajectoryPosition = OrbitalPositionToWorld;
        //    //Debug.Log("Old: " + CalculateVelocityFromOrbitalParameters());
        //    //Debug.Log("New: " + OrbitalMechanics.CalculateVelocityFromOrbitalParameters(SpecificRelativeAngularMomentum, SourceRelativePosition, CurrentGravitySource.Mass, SemimajorAxis));
        //}
    }

    #endregion UNITY

    #region PHYSICS

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

    protected bool LeavingSphereOfInfluence()
    {
        if (CurrentGravitySource.CurrentGravitySource == null)
            return false;
        if (OrbitalRadius < CurrentGravitySource.RadiusOfInfluence)
        {
            return false;
        }
        return true;
    }

    public void EnterSphereOfInfluence(GravitySource newSource)
    {
        //if (newSource == CurrentGravitySource)
        //    return;

        if (possibleGravitySources.Contains(newSource))
            return;

        possibleGravitySources.Add(newSource);
    }

    public void InitializeNewOrbit(GravitySource newSource)
    {
        Vector2 relVel = Velocity - newSource.Velocity;  // world vel - newSource.vel
        Vector2 relPos = Position - newSource.Position; // world pos - newSource.pos
        CurrentGravitySource = newSource;
        CalculateOrbitalParametersFromStateVectors(relPos, relVel);
        recentlyChangedSource = true;
        Debug.Log(relVel.magnitude);
        IEnumerator recentSourceChangeCoroutine = ChangeSourceTimer();
        StartCoroutine(recentSourceChangeCoroutine);
    }

    private bool UpdateCurrentGravitySource()
    {
        if (recentlyChangedSource)
            return false;

        if (LeavingSphereOfInfluence()) // Leaving current source
        {
            if (CurrentGravitySource.CurrentGravitySource == null)
                return false;

            Debug.LogFormat("{0}'s leaving {1}'s sphere of influence. Entering {2}'s.", gameObject.name, CurrentGravitySource.name, CurrentGravitySource.CurrentGravitySource.name);
            InitializeNewOrbit(CurrentGravitySource.CurrentGravitySource);
            return true;
        }

        if (possibleGravitySources.Count == 0) // No new SOIs encountered this frame
            return false;

        GravitySource gravitySource = possibleGravitySources[0];   
        for (int i = 1; i < possibleGravitySources.Count; i++)
        {
            GravitySource thisGravitySource = possibleGravitySources[i];
            if (thisGravitySource.SourceRank > gravitySource.SourceRank)
            {
                gravitySource = thisGravitySource;
            }
        }
        possibleGravitySources.Clear();
        if (gravitySource == CurrentGravitySource)
        {
            return false;
        }
        if ((gravitySource.Position - Position).magnitude >= gravitySource.RadiusOfInfluence)
        {
            return false;
        }
        Debug.LogFormat("{0}'s leaving {1}'s sphere of influence. Entering {2}'s.", gameObject.name, CurrentGravitySource.name, gravitySource.name);
        InitializeNewOrbit(gravitySource);
        return true;
    }
    #endregion PHYSICS

    private IEnumerator ChangeSourceTimer()
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(sourceChangeInterval);
        recentlyChangedSource = false;
    }
}

