using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    [Range(0.0f, 0.5f)]
    public float minimumDeltaVelocity;

    private Ship ship;
    private Vector2 _deltaOrbitalVelocity;
    private float _trueAnomaly;
    private Vector2 orbitalDirection;
    private Vector2 orthogonalDirection;
    private int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
    private Orbit orbit;
    private TrajectoryPlotter trajectoryPlotter;
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
    }
    public Vector2 DeltaOrbitalVelocity
    {
        get { return _deltaOrbitalVelocity; }
    }
    #endregion
    #region UNITY
    private void Awake()
    {
        if (nodeSprite == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a SpriterRenderer on a child object!"));

        trajectoryPlotter = GetComponent<TrajectoryPlotter>();
        if (trajectoryPlotter == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a TrajectoryPlotter!"));

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
        _deltaOrbitalVelocity += velMag * orbitalDirection;
        UpdateOrbit();
    }

    private void AdjustVelocityOrthogonally(float velMag)
    {
        _deltaOrbitalVelocity += velMag * orthogonalDirection;
        UpdateOrbit();
    }
    //Initialize(trueAnomaly, orbitalDirection, worldDirection, ship, ship.CurrentGravitySource);
    public void Initialize(float _trueAnomaly, Vector2 _orbitalDirection, Vector2 worldDirection, Ship _ship)
    {
        ship = _ship;
        UpdateValues(_trueAnomaly, _orbitalDirection, worldDirection);
    }

    public void UpdateValues(float _trueAnomaly, Vector2 _orbitalDirection, Vector2 worldDirection)
    {
        this._trueAnomaly = _trueAnomaly;
        orbitalDirection = _orbitalDirection;
        orthogonalDirection = orbitalDirection.RotateVector(-Mathf.PI / 2);
        tangentialVectorHandler.UpdateDirection(worldDirection);
        orthogonalVectorHandler.UpdateDirection(worldDirection.RotateVector(-Mathf.PI / 2));
        UpdateOrbit();
    }

    public void ShowNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ShowNode();
        }
        ShowSprites();
    }

    public void HideNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].HideNode();

        }
        HideSprites();
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
        // Specifically velocity in world coordinates!
        Vector2 relVel = ship.OrbitalVelocityToWorld + DeltaOrbitalVelocity - ship.CurrentGravitySource.Velocity;
        Vector2 relPos = ship.Position - ship.CurrentGravitySource.Position; // world pos - newSource.pos
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
