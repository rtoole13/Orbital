using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityAffected, ICameraTrackable
{
    public float fullThrust = 1f;

    [SerializeField]
    [Range(0.1f,2f)]
    private float rotationAccel = 0.5f;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float rotationRateCap = 2f;
    private float rotationRate = 0f;

    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float thrustAccel = 0.5f;
    private float normalizedThrust = 0f;
    private bool thrusting = false;
    #region UNITY
    /*
    private void Start()
    {
        if (CurrentGravitySource != null)
        {
            body.velocity = startVelocity + CurrentGravitySource.startVelocity;
            return;
        }
        body.velocity = startVelocity;
    }
    */
    protected override void FixedUpdate()
    {
        Rotate();
        ApplyThrust();
        
        base.FixedUpdate();
    }

    #endregion UNITY

    private void Rotate()
    {
        if (Input.GetKey(KeyCode.T))
        {
            rotationRate = 0;
            return;
        }

        if (Input.GetKey(KeyCode.A))
        {
            rotationRate += rotationAccel;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotationRate -= rotationAccel;
        }
        rotationRate = Mathf.Clamp(rotationRate, -rotationRateCap, rotationRateCap);
        transform.Rotate(Vector3.forward, rotationRate);
    }

    private void ApplyThrust()
    {
        if (Input.GetKey(KeyCode.X))
        {
            normalizedThrust = 0f;
            thrusting = false;
            return;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            normalizedThrust = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            normalizedThrust += thrustAccel;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            normalizedThrust -= thrustAccel;
        }
        normalizedThrust = Mathf.Clamp(normalizedThrust, 0f, 1f);
        if (normalizedThrust <= 0f)
        {
            thrusting = false;
            return;
        }
        thrusting = true;
        AddExternalForce(fullThrust * normalizedThrust * transform.up);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!thrusting)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Position, fullThrust * normalizedThrust * transform.up);
    }
}


