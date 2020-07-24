using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityAffected : OrbitalBody
{
    protected bool nonGravitationalForcesAdded = false;

    private bool recentlyChangedSource = false;
    private float sourceChangeInterval = .2f;
    private List<Vector2> nonGravitationalForces;
    private List<GravitySource> possibleGravitySources;

    public delegate void GravitySourceChanged();
    public GravitySourceChanged GravitySourceChangedEvent;

    private Vector2 universalPostion;
    private float x;
    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        nonGravitationalForces = new List<Vector2>();
        possibleGravitySources = new List<GravitySource>();
        TimeController.TimeScaleChangeEvent += TimeScaleAdjusted;
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
        x = 0f;
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }

    protected virtual void OnDisable()
    {
        TimeController.TimeScaleChangeEvent -= TimeScaleAdjusted;
    }

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
            //OrbitalMechanics.UniversalVariableMethod.UniversalVariable(ref x, Time.fixedDeltaTime, CurrentGravitySource.Mass, SemimajorAxis, OrbitalRadius, OrbitalPosition, OrbitalVelocity);
            //float z = Mathf.Pow(x, 2) / SemimajorAxis;
            //float S = OrbitalMechanics.UniversalVariableMethod.StumpffS(x, SemimajorAxis);
            //float C = OrbitalMechanics.UniversalVariableMethod.StumpffC(z);

            //float f = OrbitalMechanics.UniversalVariableMethod.VariableF(x, OrbitalRadius, C);
            //float g = OrbitalMechanics.UniversalVariableMethod.VariableG(Time.fixedDeltaTime, x, CurrentGravitySource.Mass, S);
            //universalPostion = OrbitalMechanics.UniversalVariableMethod.OrbitalPosition(f, g, OrbitalPosition, OrbitalVelocity);

            //float fPrime = OrbitalMechanics.UniversalVariableMethod.VariableFprime(CurrentGravitySource.Mass, OrbitalRadius, universalPostion.magnitude, x, z, S);
            //float gPrime = OrbitalMechanics.UniversalVariableMethod.VariableGprime(x, universalPostion.magnitude, C);
            //Vector2 universalVelocity = OrbitalMechanics.UniversalVariableMethod.OrbitalVelocity(fPrime, gPrime, OrbitalPosition, OrbitalVelocity);

            UpdateDeterministically();
            //Debug.LogFormat("gravA x: {0}", x);
            //Debug.LogFormat("f: {0}, g: {1}", f, g);
            //Debug.LogFormat("uP: {0}, cP: {1}, uV: {2}, cV: {3}", universalPostion, OrbitalPosition, universalVelocity, OrbitalVelocity);
            //Debug.LogFormat("uP: ({0}, {1}), cP: ({2}, {3})", universalPostion.x, universalPostion.y, OrbitalPosition.x, OrbitalPosition.y);
            //Debug.LogFormat("uV: ({0}, {1}), cV: ({2}, {3})", universalVelocity.x, universalVelocity.y, OrbitalVelocity.x, OrbitalVelocity.y);
        }
    }

    private void SwitchToDeterministicUpdate()
    {
        nonGravitationalForces.Clear();
        nonGravitationalForcesAdded = false;
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
        CalculateOrbitalParametersFromStateVectors(relPos, relVel);
    }

    protected virtual void TimeScaleAdjusted(float newTimeScale)
    {
        if (UpdatingIteratively && newTimeScale != 1f)
        {
            Debug.LogFormat("Time scale adjusted. {0} switching to deterministic update.", name);
            SwitchToDeterministicUpdate();
        }
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
        recentlyChangedSource = true;

        GravitySourceChangedEvent(); // invoke event
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

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(universalPostion.RotateVector(ArgumentOfPeriapsis), 1f);
    }
}

