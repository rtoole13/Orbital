using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    public Vector2 velocity;
    public float trueAnomaly;
    public int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
    public float hitRadius;
    public List<ManeuverNode> maneuverNodes;

    private float _hitRadiusSq;
    private SpriteRenderer spriteRenderer;

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
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            throw new UnityException(string.Format("Expecting ManeuverNode to have a SpriterRenderer or have a child with a SpriteRenderer"));

        HitRadiusSq = hitRadius * hitRadius;
        maneuverNodes = new List<ManeuverNode>();
    }

    #endregion
    #region GENERAL
    public void ShowNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ShowNode();
        }
        spriteRenderer.enabled = true;
    }

    public void HideNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].HideNode();

        }
        spriteRenderer.enabled = false;
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
