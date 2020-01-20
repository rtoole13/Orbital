using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public abstract class GravityAffected : MonoBehaviour
{
    [SerializeField]
    private float _mass = 1.0f;

    public float Mass {
        get { return _mass; }
        private set { _mass = value; }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        source.AddAffectedBody(this);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        IGravitySource source = collider.gameObject.GetComponent<IGravitySource>();
        source.RemoveAffectedBody(this);
    }
}
