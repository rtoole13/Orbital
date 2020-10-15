using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;
public class ManeuverNode : MonoBehaviour
{
    //[HideInInspector]
    //public float timeToIntersect = 0f;

    [Range(0.0f, 0.5f)]
    public float minimumDeltaVelocity;

    public GameObject trajectoryObjectPrefab;
    private GameObject trajectoryObject;
    private TrajectoryPlotter trajectoryPlotter;
    private IntersectionCalculator intersectionCalculator;
    private Ship ship;
    private Gradient nodeOutlineGradient;
    private Color nodeOutlineBaseColor;
    private Vector2 _deltaOrbitalVelocity;
    private float _trueAnomaly;
    private Vector2 orbitalPosition;
    private Vector2 orbitalDirection;
    private float orbitalSpeed;
    private Vector2 orthogonalDirection;
    private int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
    private Trajectory trajectory;

    private List<ManeuverNode> maneuverNodes;
    private CircleCollider2D nodeCollider;

    [SerializeField]
    private SpriteRenderer nodeSprite;
    private CirclePlotter nodeOutline;

    [SerializeField]
    private ManeuverVectorHandler tangentialForwardVectorHandler;

    [SerializeField]
    private ManeuverVectorHandler orthogonalOutVectorHandler;

    [SerializeField]
    private ManeuverVectorHandler tangentialBackwardVectorHandler;

    [SerializeField]
    private ManeuverVectorHandler orthogonalInVectorHandler;

    #region GETSET
    public float TrueAnomaly
    {
        get { return _trueAnomaly; }
        private set { _trueAnomaly = value; }
    }
    public Vector2 DeltaVelocity
    {
        get { return _deltaOrbitalVelocity; }
        private set { _deltaOrbitalVelocity = value; }
    }
    #endregion
    #region UNITY
    private void Awake()
    {
        nodeCollider = GetComponent<CircleCollider2D>();
        if (nodeCollider == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a Circle collider on it!"));

        if (nodeSprite == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a SpriterRenderer on a child object!"));

        nodeOutline = nodeSprite.GetComponent<CirclePlotter>();
        if (nodeOutline == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a CirclePlotter on its child object w/ a SpriteRenderer!"));

        if (tangentialForwardVectorHandler == null || tangentialBackwardVectorHandler == null || orthogonalOutVectorHandler == null || orthogonalInVectorHandler == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a ManeuverVectorHandler on each of four child objects!"));

        // Event listeners for velocity mag change
        tangentialForwardVectorHandler.DeltaVelocityAdjustedEvent += AdjustVelocityTangentiallyForward;
        tangentialBackwardVectorHandler.DeltaVelocityAdjustedEvent += AdjustVelocityTangentiallyBackward;
        orthogonalOutVectorHandler.DeltaVelocityAdjustedEvent += AdjustVelocityOrthogonallyOutward;
        orthogonalInVectorHandler.DeltaVelocityAdjustedEvent += AdjustVelocityOrthogonallyInward;

        trajectory = new Trajectory();
        maneuverNodes = new List<ManeuverNode>();
    }

    private void OnDisable()
    {
        // Event listeners for velocity mag change
        tangentialForwardVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityTangentiallyForward;
        tangentialBackwardVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityTangentiallyBackward;
        orthogonalOutVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityOrthogonallyOutward;
        orthogonalInVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityOrthogonallyInward;
    }

    private void OnDestroy()
    {
        // Event listeners for velocity mag change
        tangentialForwardVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityTangentiallyForward;
        tangentialBackwardVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityTangentiallyBackward;
        orthogonalOutVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityOrthogonallyOutward;
        orthogonalInVectorHandler.DeltaVelocityAdjustedEvent -= AdjustVelocityOrthogonallyInward;

        if (trajectoryObject != null)
            Destroy(trajectoryObject);
        Destroy(gameObject);
    }

    #endregion
    #region GENERAL
    private void AdjustVelocityTangentiallyForward(float velMag)
    {
        DeltaVelocity += velMag * orbitalDirection;
        UpdateOrbit();
    }

    private void AdjustVelocityTangentiallyBackward(float velMag)
    {
        DeltaVelocity -= velMag * orbitalDirection;
        UpdateOrbit();
    }

    private void AdjustVelocityOrthogonallyOutward(float velMag)
    {
        DeltaVelocity += velMag * orthogonalDirection;
        UpdateOrbit();
    }

    private void AdjustVelocityOrthogonallyInward(float velMag)
    {
        DeltaVelocity -= velMag * orthogonalDirection;
        UpdateOrbit();
    }

    public void Initialize(float _trueAnomaly, Ship _ship, Gradient _nodeOutlineGradient)
    {
        ship = _ship;
        transform.parent = ship.CurrentGravitySource.transform;
        nodeOutlineGradient = _nodeOutlineGradient;
        nodeOutlineBaseColor = nodeOutlineGradient.Evaluate(0);
        nodeOutline.SetGradient(nodeOutlineGradient);
        UpdateParameters(_trueAnomaly);
    }

    public void UpdateParameters(float trueAnomaly)
    {
        TrueAnomaly = trueAnomaly;
        orbitalDirection = CalculateOrbitalDirection(trueAnomaly);
        orthogonalDirection = orbitalDirection.RotateVector(-Mathf.PI / 2);

        float orbitalRadius = Mechanics.Trajectory.OrbitalRadius(ship.Trajectory.Eccentricity, ship.Trajectory.SemimajorAxis, trueAnomaly);
        orbitalSpeed = Mechanics.Trajectory.OrbitalSpeed(ship.CurrentGravitySource.Mass, orbitalRadius, ship.Trajectory.SemimajorAxis);

        orbitalPosition = Mechanics.Trajectory.OrbitalPosition(orbitalRadius, trueAnomaly, ship.Trajectory.ClockWiseOrbit);

        // Direction and position in world coordinate space
        Vector2 worldPosition = OrbitalPositionToWorld(orbitalPosition);
        Vector2 worldDirection = orbitalDirection.RotateVector(ship.Trajectory.ArgumentOfPeriapsis);
        UpdateTransform(worldPosition, worldDirection);
        UpdateOrbit();
    }

    private void UpdateTransform(Vector2 worldPosition, Vector2 worldDirection)
    {
        Quaternion rotationQuaternion = Quaternion.FromToRotation(transform.up, worldDirection);
        transform.position = worldPosition;
        transform.rotation = rotationQuaternion * transform.rotation;
        tangentialForwardVectorHandler.UpdateDirection(worldDirection);
        tangentialBackwardVectorHandler.UpdateDirection(-worldDirection);
        orthogonalOutVectorHandler.UpdateDirection(worldDirection.RotateVector(-Mathf.PI / 2));
        orthogonalInVectorHandler.UpdateDirection(worldDirection.RotateVector(Mathf.PI / 2));

        DeltaVelocity = rotationQuaternion * DeltaVelocity;
    }

    private Vector2 WorldPositionFromTrueAnomaly(float trueAnomaly)
    {
        float orbitalRadius = Mechanics.Trajectory.OrbitalRadius(ship.Trajectory.Eccentricity, ship.Trajectory.SemimajorAxis, trueAnomaly);
        Vector2 orbitalPosition = Mechanics.Trajectory.OrbitalPosition(orbitalRadius, trueAnomaly, ship.Trajectory.ClockWiseOrbit);
        return OrbitalPositionToWorld(orbitalPosition);
    }

    private Vector2 CalculateOrbitalDirection(float trueAnomaly)
    {
        float flightPathAngle = Mechanics.Trajectory.FlightPathAngle(ship.Trajectory.Eccentricity, trueAnomaly);
        return Mechanics.Trajectory.OrbitalDirection(trueAnomaly, flightPathAngle, ship.Trajectory.ClockWiseOrbit);

    }

    private Vector2 OrbitalPositionToWorld(Vector2 perifocalPosition)
    {
        Vector2 translation = ship.CurrentGravitySource != null
            ? ship.CurrentGravitySource.Position
            : Vector2.zero;
        return perifocalPosition.RotateVector(ship.Trajectory.ArgumentOfPeriapsis) + translation;
    }

    private Vector2 OrbitalVelocityToWorld(Vector2 perifocalVelocity)
    {
        Vector2 velocity = ship.CurrentGravitySource != null
                ? ship.CurrentGravitySource.Velocity
                : Vector2.zero;
        return perifocalVelocity.RotateVector(ship.Trajectory.ArgumentOfPeriapsis) + velocity;
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
        nodeOutline.SetDisplay(false);
        tangentialForwardVectorHandler.HideVector();
        tangentialBackwardVectorHandler.HideVector();
        orthogonalOutVectorHandler.HideVector();
        orthogonalInVectorHandler.HideVector();
    }

    private void ShowSprites()
    {
        nodeSprite.enabled = true;
        nodeOutline.SetDisplay(true);
        tangentialForwardVectorHandler.ShowVector();
        tangentialBackwardVectorHandler.ShowVector();
        orthogonalOutVectorHandler.ShowVector();
        orthogonalInVectorHandler.ShowVector();
    }

    public void ClearNodes()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ClearNodes();
        }
        maneuverNodes.Clear();
    }

    public void SetManeuverExecution(bool executeManeuverMode)
    {
        tangentialBackwardVectorHandler.SetExecuting(executeManeuverMode, nodeOutlineBaseColor);
        tangentialForwardVectorHandler.SetExecuting(executeManeuverMode, nodeOutlineBaseColor);
        orthogonalOutVectorHandler.SetExecuting(executeManeuverMode, nodeOutlineBaseColor);
        orthogonalInVectorHandler.SetExecuting(executeManeuverMode, nodeOutlineBaseColor);
    }

    private void UpdateOrbit()
    {

        if (DeltaVelocity.magnitude < minimumDeltaVelocity)
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

            // Pass ship to intersection calculator
            intersectionCalculator = trajectoryObject.GetComponent<IntersectionCalculator>();
            //intersectionCalculator.SetOrbitalBody(ship);
        }
        trajectoryObject.transform.parent = ship.CurrentGravitySource.transform;
        trajectoryObject.transform.position = trajectoryObject.transform.parent.position;

        // Specifically velocity in world coordinates!
        Vector2 newOrbitalVelocity = orbitalSpeed * orbitalDirection + DeltaVelocity;
        Vector2 relVel = newOrbitalVelocity.RotateVector(-ship.Trajectory.ArgumentOfPeriapsis);
        Vector2 relPos = (Vector2)transform.position - ship.CurrentGravitySource.Position; // world pos - newSource.pos
        trajectory.CalculateOrbitalParametersFromStateVectors(relPos, relVel, ship.CurrentGravitySource);
        if (trajectory.TrajectoryType == Mechanics.Globals.TrajectoryType.Ellipse)
        {
            trajectoryPlotter.BuildEllipticalTrajectory(trajectory.SemimajorAxis, trajectory.SemiminorAxis, trajectory.Eccentricity, trajectory.ArgumentOfPeriapsis);
        }
        else
        {
            trajectoryPlotter.BuildHyperbolicTrajectory(trajectory.SemimajorAxis, trajectory.SemiminorAxis, trajectory.Eccentricity, trajectory.ArgumentOfPeriapsis);
        }
        float timeToNode = OrbitalMechanics.UniversalVariableMethod.CalculateTimeOfFlight(ship.OrbitalPosition, ship.OrbitalVelocity, orbitalPosition, ship.Trajectory.EccentricityVector, ship.Trajectory.ParentGravitySource.Mass);
        Debug.LogFormat("calculated orbitalVelocity: {0}", newOrbitalVelocity);
        intersectionCalculator.PlotNearestSourceIntersections(ship, orbitalPosition, newOrbitalVelocity, timeToNode, trajectory);
    }
    
    public void RecalculateIntersections()
    {
        if (intersectionCalculator == null)
            return;
        Vector2 newOrbitalVelocity = orbitalSpeed * orbitalDirection + DeltaVelocity;
        intersectionCalculator.PlotNearestSourceIntersections(ship, orbitalPosition, newOrbitalVelocity, trajectory.Period, trajectory);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (nodeCollider == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, nodeCollider.radius);
        Gizmos.DrawRay(OrbitalPositionToWorld(orbitalPosition), orbitalDirection * 10f);
    }
}
