using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class GravitySource : MonoBehaviour, IGravitySource
{
    [SerializeField]
    private float _mass = 5.0f;

    public float Mass
    {
        get { return _mass; }
        private set { _mass = value; }
    }


    public readonly float GRAVITYCONSTRANT = 1.0f;
    //public float GRAVITYCONSTRANT = 1.0f;

    private CircleCollider2D gravityCollider;
    private List<GravityAffected> gravityAffecteObjects = new List<GravityAffected>(); 

    private void Start()
    {
        // Get gravityCollider
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            CircleCollider2D collider = colliders[i];
            if (collider.isTrigger)
            {
                gravityCollider = collider;
                break;
            }
        }
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < gravityAffecteObjects.Count; i++)
        {
            GravityAffected affectedObject = gravityAffecteObjects[i];
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
        gravityAffecteObjects.Add(body);
    }

    public void RemoveAffectedBody(GravityAffected body)
    {
        gravityAffecteObjects.Remove(body);
    }

    public GravitySource GetGravitySource()
    {
        return this;
    }
}
