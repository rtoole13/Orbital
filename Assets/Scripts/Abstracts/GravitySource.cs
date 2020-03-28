using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public abstract class GravitySource : OrbitalBody
{
    private CircleCollider2D bodyCollider;
    private List<GravityAffected> gravityAffectedObjects = new List<GravityAffected>();

    #region GETSET

    public float Radius
    {
        get { return bodyCollider.radius; }
    }
    #endregion GETSET

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        // Get gravityCollider
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            CircleCollider2D collider = colliders[i];
            if (!collider.isTrigger)
            {
                bodyCollider = collider;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        if (CurrentGravitySource == null)
            return;
        updateIteratively = false;
        Vector3 sourceRelativePosition = (Vector3)Position - (Vector3)CurrentGravitySource.Position;
        Vector3 sourceRelativeVelocity = (Vector3)body.velocity - (Vector3)CurrentGravitySource.startVelocity;
        Debug.LogFormat("{0}'s source vel: {1}", gameObject.name, CurrentGravitySource.startVelocity);
        CalculateOrbitalParametersFromStateVectors(sourceRelativePosition, sourceRelativeVelocity);
    }

    private void FixedUpdate()
    {
        UpdateDeterministically();
    }

    #endregion UNITY

    public Vector2 CalculateGravitationalForceAtPosition(Vector2 position, float mass) //DEPRECATED, remove in favor of OrbitalMechanics method
    {
        Vector2 distance = (Vector2)bodyCollider.transform.position - position;
        float forceMagnitude = OrbitalMechanics.GRAVITATIONALCONSTANT * Mass * mass / Vector2.SqrMagnitude(distance);
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
}
