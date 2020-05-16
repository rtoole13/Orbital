using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    [Range(0.0f, 0.5f)]
    public float minimumDeltaVelocity;

    public GameObject trajectoryObjectPrefab;
    private GameObject trajectoryObject;
    private TrajectoryPlotter trajectoryPlotter;
    private Ship ship;
    private Vector2 _deltaOrbitalVelocity;
    private float _trueAnomaly;
    private Vector2 orbitalDirection;
    private float orbitalSpeed;
    private Vector2 worldVelocity;
    private Vector2 orthogonalDirection;
    private int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
    private Orbit orbit;
    public float hitRadius;

    private List<ManeuverNode> maneuverNodes;

    [SerializeField]
    private SpriteRenderer nodeSprite;

    [SerializeField]
    private ManeuverVectorHandler tangentialVectorHandler;

    [SerializeField]
    private ManeuverVectorHandler orthogonalVectorHandler;

    private float _hitRadiusSq;

    #region GETSET
    public float HitRadiusSq
    {
        get { return _hitRadiusSq; }
        private set { _hitRadiusSq = value; }
    }
    public float TrueAnomaly
    {
        get { return _trueAnomaly; }
        private set { _trueAnomaly = value; }
    }
    public Vector2 DeltaOrbitalVelocity
    {
        get { return _deltaOrbitalVelocity; }
        private set { _deltaOrbitalVelocity = value; }
    }
    #endregion
    #region UNITY
    private void Awake()
    {
        if (nodeSprite == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a SpriterRenderer on a child object!"));
        
        if (tangentialVectorHandler == null || orthogonalVectorHandler == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a ManeuverVectorHandler on two on child objects!"));

        // Event listeners for velocity mag change
        tangentialVectorHandler.DeltaVelocityAdjustedEvent += AdjustVelocityTangentially;
        orthogonalVectorHandler.DeltaVelocityAdjustedEvent += AdjustVelocityOrthogonally;

        orbit = new Orbit();

        HitRadiusSq = hitRadius * hitRadius;
        maneuverNodes = new List<ManeuverNode>();
    }

    private void OnDisable()
    {
        // Event listeners for velocity mag change
        tangentialVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityTangentially;
        orthogonalVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityOrthogonally;
    }

    #endregion
    #region GENERAL
    private void AdjustVelocityTangentially(float velMag)
    {
        DeltaOrbitalVelocity += velMag * orbitalDirection;
        UpdateOrbit();
    }

    private void AdjustVelocityOrthogonally(float velMag)
    {
        DeltaOrbitalVelocity += velMag * orthogonalDirection;
        UpdateOrbit();
    }
    //Initialize(trueAnomaly, orbitalDirection, worldDirection, ship, ship.CurrentGravitySource);
    public void Initialize(float _trueAnomaly, Ship _ship)
    {
        ship = _ship;
        UpdateParameters(_trueAnomaly);
    }

    public void UpdateParameters(float trueAnomaly)
    {
        TrueAnomaly = trueAnomaly;
        orbitalDirection = CalculateOrbitalDirection(trueAnomaly);
        orthogonalDirection = orbitalDirection.RotateVector(-Mathf.PI / 2);

        float orbitalRadius = OrbitalMechanics.OrbitalRadius(ship.Eccentricity, ship.SemimajorAxis, trueAnomaly);
        orbitalSpeed = OrbitalMechanics.OrbitalSpeed(ship.CurrentGravitySource.Mass, orbitalRadius, ship.SemimajorAxis);

        Vector2 orbitalPosition = OrbitalMechanics.OrbitalPosition(orbitalRadius, trueAnomaly, ship.ClockWiseOrbit);

        // Direction and position in world coordinate space
        Vector2 worldPosition = OrbitalPositionToWorld(orbitalPosition);
        Vector2 worldDirection = orbitalDirection.RotateVector(ship.ArgumentOfPeriapsis);

        Vector2 orbitalVelocity = orbitalSpeed * orbitalDirection;
        worldVelocity = orbitalVelocity.RotateVector(ship.ArgumentOfPeriapsis);
        UpdateTransform(worldPosition, worldDirection);
        UpdateOrbit();
    }

    private void UpdateTransform(Vector2 worldPosition, Vector2 worldDirection)
    {
        Quaternion rotationQuaternion = Quaternion.FromToRotation(transform.up, worldDirection);
        transform.position = worldPosition;
        transform.rotation = rotationQuaternion * transform.rotation;
        tangentialVectorHandler.UpdateDirection(worldDirection);
        orthogonalVectorHandler.UpdateDirection(worldDirection.RotateVector(-Mathf.PI / 2));

        DeltaOrbitalVelocity = rotationQuaternion * DeltaOrbitalVelocity;
    }

    private Vector2 WorldPositionFromTrueAnomaly(float trueAnomaly)
    {
        float orbitalRadius = OrbitalMechanics.OrbitalRadius(ship.Eccentricity, ship.SemimajorAxis, trueAnomaly);
        Vector2 orbitalPosition = OrbitalMechanics.OrbitalPosition(orbitalRadius, trueAnomaly, ship.ClockWiseOrbit);
        return OrbitalPositionToWorld(orbitalPosition);
    }

    private Vector2 CalculateOrbitalDirection(float trueAnomaly)
    {
        float flightPathAngle = OrbitalMechanics.FlightPathAngle(ship.Eccentricity, trueAnomaly);
        return OrbitalMechanics.OrbitalDirection(trueAnomaly, flightPathAngle, ship.ClockWiseOrbit);

    }

    private Vector2 OrbitalPositionToWorld(Vector2 perifocalPosition)
    {
        Vector2 translation = ship.CurrentGravitySource != null
            ? ship.CurrentGravitySource.Position
            : Vector2.zero;
        return perifocalPosition.RotateVector(ship.ArgumentOfPeriapsis) + translation;
    }

    private Vector2 OrbitalVelocityToWorld(Vector2 perifocalVelocity)
    {
        Vector2 velocity = ship.CurrentGravitySource != null
                ? ship.CurrentGravitySource.Velocity
                : Vector2.zero;
        return perifocalVelocity.RotateVector(ship.ArgumentOfPeriapsis) + velocity;
    }

    public void ShowNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ShowNode();
        }
        ShowSprites();
        if (trajectoryObject != null)
            trajectoryObject.SetActive(true);
    }

    public void HideNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].HideNode();

        }
        HideSprites();
        if (trajectoryObject != null)
            trajectoryObject.SetActive(false);
    }

    private void HideSprites()
    {
        nodeSprite.enabled = false;
        tangentialVectorHandler.HideVector();
        orthogonalVectorHandler.HideVector();
    }

    private void ShowSprites()
    {
        nodeSprite.enabled = true;
        tangentialVectorHandler.ShowVector();
        orthogonalVectorHandler.ShowVector();
    }

    public void ClearNodes()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ClearNodes();
        }
        maneuverNodes.Clear();
    }

    public void ToggleManeuverExecution(bool executeManeuverMode)
    {
        nodeSprite.color = executeManeuverMode
            ? Color.red
            : Color.green;
    }

    private void UpdateOrbit()
    {
        
        if (DeltaOrbitalVelocity.magnitude < minimumDeltaVelocity)
        {
            return;
        }
        
        if (trajectoryObjectPrefab == null)
        {
            Debug.Log("No trajectory object prefab selected!");
            return;
        }
        
        if (trajectoryObject == null)
        {
            // Instantiate prefab if null
            trajectoryObject = Instantiate(trajectoryObjectPrefab);
            trajectoryPlotter = trajectoryObject.GetComponent<TrajectoryPlotter>();
        }
        trajectoryObject.transform.parent = ship.CurrentGravitySource.transform;
        trajectoryObject.transform.position = trajectoryObject.transform.parent.position;

        // Specifically velocity in world coordinates!
        Vector2 relVel = worldVelocity + DeltaOrbitalVelocity.RotateVector(ship.ArgumentOfPeriapsis) - ship.CurrentGravitySource.Velocity;
        Vector2 relPos = (Vector2)transform.position - ship.CurrentGravitySource.Position; // world pos - newSource.pos
        orbit.CalculateOrbitalParametersFromStateVectors(relPos, relVel, ship.CurrentGravitySource.Mass);
        if (orbit.TrajectoryType == OrbitalMechanics.TrajectoryType.Ellipse)
        {
            trajectoryPlotter.BuildEllipticalTrajectory(orbit.SemimajorAxis, orbit.SemiminorAxis, orbit.Eccentricity, orbit.ArgumentOfPeriapsis);
        }
        else
        {
            trajectoryPlotter.BuildHyperbolicTrajectory(orbit.SemimajorAxis, orbit.SemiminorAxis, orbit.Eccentricity, orbit.ArgumentOfPeriapsis);
        }
    }
    #endregion

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, hitRadius);
    //}
}
