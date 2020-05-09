using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    private Vector2 velocity;
    private float trueAnomaly;
    private Vector2 orbitalDirection;
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
    #endregion
    #region UNITY
    public void Awake()
    {
        if (nodeSprite == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a SpriterRenderer on a child object!"));

        if (tangentialVectorHandler == null || orthogonalVectorHandler == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a ManeuverVectorHandler on two on child objects!"));

        HitRadiusSq = hitRadius * hitRadius;
        maneuverNodes = new List<ManeuverNode>();
    }

    #endregion
    #region GENERAL

    public void UpdateValues(float _trueAnomaly, Vector2 _orbitalDirection)
    {
        trueAnomaly = _trueAnomaly;
        orbitalDirection = _orbitalDirection;
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
        tangentialVectorHandler.HideSprite();
        orthogonalVectorHandler.HideSprite();
    }

    private void ShowSprites()
    {
        nodeSprite.enabled = true;
        tangentialVectorHandler.ShowSprite();
        orthogonalVectorHandler.ShowSprite();
    }

    public void ClearNodes()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ClearNodes();
        }
        maneuverNodes.Clear();
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
