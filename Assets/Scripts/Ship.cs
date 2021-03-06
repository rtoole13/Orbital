﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ship : GravityAffected, ICameraTrackable
{
    public float fullThrust = 1f;

    private bool _stabilityAssistEnabled = false;
    private ShipSystems.StabilityAssistMode _stabilityAssistMode = ShipSystems.StabilityAssistMode.Hold;
    
    [SerializeField]
    [Range(0.1f, 2f)]
    private float rotationAccel = 0.5f;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float rotationRateCap = 2f;
    private float rotationRate = 0f;
    private float rotationDampVel = 0;
    private float rotationDampTime = .5f;

    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float thrustAccel = 0.5f;
    private float normalizedThrust = 0f;
    private bool thrusting = false;

    private float stabilityAssistAccel = 0.3f;

    #region GETSET
    public bool StabilityAssistEnabled
    {
        get { return _stabilityAssistEnabled; }
        private set { _stabilityAssistEnabled = value; }
    }
    
    public ShipSystems.StabilityAssistMode StabilityAssistMode
    {
        get { return _stabilityAssistMode; }
        private set { _stabilityAssistMode = value; }
    }

    #endregion GETSET
    #region UNITY
    
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FixedUpdate()
    {
        Rotate();
        ApplyThrust();

        base.FixedUpdate();
    }

    #endregion UNITY

    #region CALLBACKS
    public void DecrementRotationRate()
    {
        rotationRate -= rotationAccel;
    }

    public void IncrementRotationRate()
    {
        rotationRate += rotationAccel;
    }

    public void ToggleStabilityAssist()
    {
        StabilityAssistEnabled = !StabilityAssistEnabled;
        ChangeStabilityAssistMode((int)ShipSystems.StabilityAssistMode.Hold);
    }

    public void ChangeStabilityAssistMode(int newValue)
    {
        StabilityAssistMode = (ShipSystems.StabilityAssistMode)newValue;
    }

    public void ResetThrust()
    {
        normalizedThrust = 0f;
        thrusting = false;
    }

    public void ThrottleMax()
    {
        if (Time.timeScale != 1f)
        {
            Debug.Log("Cannot apply thrust while time warping!");
        }
        normalizedThrust = 1f;
    }

    public void ThrottleUp()
    {
        if (Time.timeScale != 1f)
        {
            Debug.Log("Cannot apply thrust while time warping!");
        }
        normalizedThrust += thrustAccel;
    }

    public void ThrottleDown()
    {
        normalizedThrust -= thrustAccel;
    }
    
    public void ExecuteInstantBurn(Vector2 deltaVelocity)
    {
        // Specifically velocity in world coordinates!
        Vector2 newVelocity = OrbitalVelocityToWorld + deltaVelocity;
        SetDirection(newVelocity.normalized, true);

        Vector2 relVel = newVelocity - CurrentGravitySource.Velocity;
        Vector2 relPos = Position - CurrentGravitySource.Position; // world pos - newSource.pos
        CalculateOrbitalParametersFromStateVectors(relPos, relVel);
    }

    #endregion CALLBACKS

    private void Rotate()
    {
        rotationRate = Mathf.Clamp(rotationRate, -rotationRateCap, rotationRateCap);

        if (!StabilityAssistEnabled)
        {
            // Rotate freely
            transform.Rotate(Vector3.forward, rotationRate);
            return;
        }

        // Stability assist
        if (StabilityAssistMode == ShipSystems.StabilityAssistMode.Hold)
        {
            // Hold position
            rotationRate = Mathf.SmoothDamp(rotationRate, 0f, ref rotationDampVel, rotationDampTime);
            transform.Rotate(Vector3.forward, rotationRate);
            return;
        }
        else if (StabilityAssistMode == ShipSystems.StabilityAssistMode.Prograde)
        {
            // Rotate to prograde
            float sign = Mathf.Sign(Vector2.SignedAngle(transform.up, OrbitalDirectionToWorld));
            rotationRate += (sign * stabilityAssistAccel);
        }
        else if (StabilityAssistMode == ShipSystems.StabilityAssistMode.Retrograde)
        {
            // Rotate to retrograde
            float sign = Mathf.Sign(Vector2.SignedAngle(transform.up, OrbitalDirectionToWorld));
            rotationRate -= (sign * stabilityAssistAccel);
        }
        else if (StabilityAssistMode == ShipSystems.StabilityAssistMode.RadialIn)
        {
            // Rotate to radial in
            Vector2 dir = OrbitalPosition.RotateVector(Trajectory.ArgumentOfPeriapsis).normalized;
            float sign = Vector2.SignedAngle(transform.up, -dir);
            rotationRate += (sign * stabilityAssistAccel);
        }
        else
        {
            // Rotate to radial out
            Vector2 dir = OrbitalPosition.RotateVector(Trajectory.ArgumentOfPeriapsis).normalized;
            float sign = Vector2.SignedAngle(transform.up, dir);
            rotationRate += (sign * stabilityAssistAccel);
        }
        rotationRate = Mathf.Clamp(rotationRate, -rotationRateCap, rotationRateCap);
        transform.Rotate(Vector3.forward, rotationRate);
    }

    private void SetDirection(Vector2 direction, bool killRotation)
    {
        Quaternion rotationQuaternion = Quaternion.FromToRotation(transform.up, direction);
        transform.rotation = rotationQuaternion * transform.rotation;
        if (killRotation)
            rotationRate = 0;
    }

    private void ApplyThrust()
    {
        normalizedThrust = Mathf.Clamp(normalizedThrust, 0f, 1f);
        if (normalizedThrust <= 0f)
        {
            thrusting = false;
            return;
        }
        thrusting = true;
        AddExternalForce(fullThrust * normalizedThrust * transform.up);
    }

    protected override void TimeScaleAdjusted(float newTimeScale)
    {
        ResetThrust();
        base.TimeScaleAdjusted(newTimeScale);
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (body == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(Position, 5f * transform.up);


        Gizmos.color = Color.red;
        Gizmos.DrawRay(Position, 5f * OrbitalDirectionToWorld);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Position, 5f * OrbitalPosition.RotateVector(Trajectory.ArgumentOfPeriapsis).normalized);
    }
}


