using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public abstract class GravitySource : MonoBehaviour, IGravitySource
{
    [SerializeField]
    private float _mass = 5.0f;
    private Rigidbody2D body;

    public float Mass
    {
        get { return _mass; }
        private set { _mass = value; }
    }

    public Vector3 Velocity
    {
        get { return new Vector3(body.velocity.x, body.velocity.y, 0f); }
    }

    public readonly float GRAVITYCONSTRANT = 1.0f;
    //public float GRAVITYCONSTRANT = 1.0f;

    private CircleCollider2D gravityCollider;
    private CircleCollider2D bodyCollider;
    private List<GravityAffected> gravityAffectedObjects = new List<GravityAffected>(); 

    public float Radius
    {
        get
        {
            return bodyCollider.radius;
        }
    }
    private void Awake()
    {
        // Get gravityCollider
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            CircleCollider2D collider = colliders[i];
            if (collider.isTrigger)
            {
                gravityCollider = collider;
            }
            else
            {
                bodyCollider = collider;
            }
        }

        // Get rigidbody
        body = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        
    }
    
    private void FixedUpdate()
    {
        for (int i = 0; i < gravityAffectedObjects.Count; i++)
        {
            GravityAffected affectedObject = gravityAffectedObjects[i];
            Rigidbody2D rigidbody = affectedObject.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(CalculateForceAtPosition(rigidbody.transform.position, affectedObject.Mass));
        }
    }

    private Vector2 CalculateForceAtPosition(Vector2 position, float mass)
    {
        Vector2 distance = (Vector2)gravityCollider.transform.position - position;
        float forceMagnitude = GRAVITYCONSTRANT * Mass * mass / Vector2.SqrMagnitude(distance);
        Vector2 force = forceMagnitude * distance.normalized;
        return force;
    }
    
    public void AddAffectedBody(GravityAffected body)
    {
        gravityAffectedObjects.Add(body);
    }

    public void RemoveAffectedBody(GravityAffected body)
    {
        gravityAffectedObjects.Remove(body);
    }

    public GravitySource GetGravitySource()
    {
        return this;
    }
}
