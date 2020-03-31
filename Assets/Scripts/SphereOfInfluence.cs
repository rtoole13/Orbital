using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereOfInfluence : MonoBehaviour
{
    private GravitySource gravitySource;
    private CircleCollider2D influenceCollider;
    
    #region GETSET
    #endregion GETSET

    void Awake()
    {
        gravitySource = GetComponentInParent<GravitySource>();

        //Find ROI collider
        influenceCollider = GetComponent<CircleCollider2D>();
        if (influenceCollider == null || !influenceCollider.isTrigger)
        {
            throw new UnityException(string.Format("{0}'s circle collider must be isTrigger!", gameObject.name));
        }
    }

    private void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GravityAffected enteringObject = collision.GetComponent<GravityAffected>();
        if (enteringObject == null)
            return;
        Debug.LogFormat("Entering {0}", gravitySource.name);
        enteringObject.EnterSphereOfInfluence(gravitySource);
    }
    
    public void UpdateRadius(float radius)
    {
        if (radius == Mathf.Infinity)
        {
            influenceCollider.radius = 0.0001f;
            if (!influenceCollider.enabled)
                influenceCollider.enabled = false;

            return;
        }
        if (!influenceCollider.enabled)
            influenceCollider.enabled = true;
        
        influenceCollider.radius = radius / gravitySource.transform.localScale.z;
    }
}
