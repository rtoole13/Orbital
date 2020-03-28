using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : OrbitalBody
{

    protected bool nonGravitationalForcesAdded = true;
    
    //GIZMOS VARS
    private List<Vector2> nonGravitationalForces;
    private bool canUpdateEpochs = true;
    private Vector2 currentEpochPosition = Vector2.zero;
    private float elapsedEpochTime = 0f;
    private Vector2 lastEpochPos = Vector2.zero;
    private Vector2 trajectoryPosition = Vector2.zero;

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        nonGravitationalForces = new List<Vector2>();
    }

    protected override void Start()
    {
        base.Start();
        updateIteratively = false;
        body.isKinematic = true;
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.Position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.startVelocity;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }
    protected virtual void Update(){

    }

    private void FixedUpdate()
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
            UpdateDeterministically();
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // On Collision w/ an object, stop deterministic trajectory update (normal force)
        SwitchToIterativeUpdate();
    }

    private void SwitchToDeterministicUpdate()
    {
        // Switching from iterative to deterministic trajectory update
        if (!updateIteratively)
            return;

        //CalculateOrbitalParametersFromStateVectors();
        updateIteratively = false;
        body.isKinematic = true;
    }

    private void SwitchToIterativeUpdate()
    {
        // Switching from deterministic trajectory to iterative update
        if (updateIteratively)
            return;

        updateIteratively = true;
        body.isKinematic = false;
        body.velocity = OrbitalVelocity.RotateVector(ArgumentOfPeriapsis);
    }

    #endregion UNITY

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
            //CalculateOrbitalParametersFromStateVectors();
        }
        if (Input.GetMouseButton(1))
        {
            //CalculateOrbitalParametersFromStateVectors();
        }
        TimeSinceEpoch = (TimeSinceEpoch + Time.fixedDeltaTime) % OrbitalPeriod;
        MeanAnomaly = OrbitalMechanics.MeanAnomaly(MeanAnomalyAtEpoch, MeanMotion, TimeSinceEpoch);
        if (MeanAnomaly >= 0f) //FIXME this check shouldnt exist.
        {
            EccentricAnomaly = OrbitalMechanics.EccentricAnomaly(MeanAnomaly, Eccentricity, 6);
            //TrueAnomaly = OrbitalMechanics.TrueAnomaly(Eccentricity, EccentricAnomaly, SpecificRelativeAngularMomentum);
            //OrbitalPosition = OrbitalMechanics.OrbitalPosition(Eccentricity, SemimajorAxis, TrueAnomaly);
            //OrbitalVelocity = OrbitalMechanics.OrbitalVelocity(MeanMotion, EccentricAnomaly, Eccentricity, SemimajorAxis);
            //trajectoryPosition = OrbitalPositionToWorld;
            //Debug.Log("Old: " + CalculateVelocityFromOrbitalParameters());
            //Debug.Log("New: " + OrbitalMechanics.CalculateVelocityFromOrbitalParameters(SpecificRelativeAngularMomentum, SourceRelativePosition, CurrentGravitySource.Mass, SemimajorAxis));
        }
    }
    #endregion ITERATIVE

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
    #endregion PHYSICS


    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawCube(lastEpochPos, new Vector3(.5f, .5f, .5f));
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawCube(currentEpochPosition, new Vector3(.5f, .5f, .5f));
    //    float updateInterval = 0.05f;
    //    if (TimeSinceEpoch > 0 && TimeSinceEpoch < updateInterval && canUpdateEpochs)
    //    {
    //        lastEpochPos = currentEpochPosition + CurrentGravitySource.Position;
    //        currentEpochPosition = body.position + CurrentGravitySource.Position;
    //        canUpdateEpochs = false;
    //        elapsedEpochTime = 0f;
    //    }
    //    elapsedEpochTime += Time.fixedDeltaTime;
    //    if (elapsedEpochTime > updateInterval + 1f)
    //        canUpdateEpochs = true;

    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawCube(trajectoryPosition, new Vector3(.5f, .5f, .5f));
    //}
}
