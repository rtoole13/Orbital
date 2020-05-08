using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    public Vector2 velocity;
    public float trueAnomaly;
    public int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
    public float hitRadius;

    private List<ManeuverNode> maneuverNodes;

    [SerializeField]
    private SpriteRenderer nodeSprite;

    [SerializeField]
    private Animator tangentialVectorAnimator;
    private SpriteRenderer tangentialVectorSprite;

    [SerializeField]
    private Animator orthogonalVectorAnimator;
    private SpriteRenderer orthogonalVectorSprite;

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
            throw new UnityException(string.Format("Expecting ManeuverNode to have a SpriterRenderer on a child object"));

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
        nodeSprite.enabled = true;
    }

    public void HideNode()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].HideNode();

        }
        nodeSprite.enabled = false;
    }

    public void ClearNodes()
    {
        for (int i = 0; i < maneuverNodes.Count; i++)
        {
            maneuverNodes[i].ClearNodes();
        }
        maneuverNodes.Clear();
    }

    private void HideSprites()
    {
        nodeSprite.enabled = false;
        //tangentialVectorAnimator.
        nodeSprite.enabled = false;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
