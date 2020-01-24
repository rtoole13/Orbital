using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : MonoBehaviour
{
    [SerializeField]
    private float _mass = 1.0f;

    [HideInInspector]
    public GravitySource gravitySource;

    public float Mass {
        get { return _mass; }
        private set { _mass = value; }
    }

    public float SourceDistance
    {
        get { 
            if (gravitySource == null)
            {
                return Mathf.Infinity;
            }
            Vector3 diff = gravitySource.transform.position - this.transform.position;
            return diff.magnitude;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        gravitySource = source.GetGravitySource();
        source.AddAffectedBody(this);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        gravitySource = null;
        source.RemoveAffectedBody(this);
    }
}
