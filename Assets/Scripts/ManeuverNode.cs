using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    private Vector2 deltaOrbitalVelocity;
    private float trueAnomaly;
    private Vector2 orbitalDirection;
    private Vector2 orthogonalDirection;
    private int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
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
        get { return trueAnomaly; }
    }
    public Vector2 DeltaOrbitalVelocity
    {
        get { return deltaOrbitalVelocity; }
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
        deltaOrbitalVelocity += velMag * orbitalDirection;
    }

    private void AdjustVelocityOrthogonally(float velMag)
    {
        deltaOrbitalVelocity += velMag * orthogonalDirection;
    }

    public void UpdateValues(float _trueAnomaly, Vector2 _orbitalDirection, Vector2 worldDirection)
    {
        trueAnomaly = _trueAnomaly;
        orbitalDirection = _orbitalDirection;
        orthogonalDirection = orbitalDirection.RotateVector(-Mathf.PI / 2);
        tangentialVectorHandler.UpdateDirection(worldDirection);
        orthogonalVectorHandler.UpdateDirection(worldDirection.RotateVector(-Mathf.PI / 2));
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

    #endregion

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, hitRadius);
    //}
}
