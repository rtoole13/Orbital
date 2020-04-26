using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [SerializeField]
    private GameObject stabilityAssistUI;
    private TMP_Dropdown stabilityAssistDropdown;
    private ShipSystems.StabilityAssistMode stabilityAssistMode = ShipSystems.StabilityAssistMode.Hold;
    private bool stabilityAssist = false;

    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        stabilityAssistDropdown = stabilityAssistUI.GetComponentInChildren<TMP_Dropdown>();
        stabilityAssistDropdown.onValueChanged.AddListener(ChangeStabilityAssist);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        stabilityAssistDropdown.onValueChanged.RemoveListener(ChangeStabilityAssist);
    }

    private void Update()
    {
        ToggleStabilityAssist();
    }

    protected override void FixedUpdate()
    {
        Rotate();
        ApplyThrust();
        
        base.FixedUpdate();
    }

    #endregion UNITY

    private void ToggleStabilityAssist()
    {
        if (!Input.GetKeyDown(KeyCode.T))
            return;

        stabilityAssist = !stabilityAssist;
        stabilityAssistUI.SetActive(stabilityAssist);
    }

    private void Rotate()
    {

        if (stabilityAssist)
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
            ResetThrust();
            return;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            if (Time.timeScale != 1f) // FIXME: Potential float comparison issue
            {
                Debug.Log("Cannot apply thrust while time warping!");
                return;
            }
            normalizedThrust = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Time.timeScale != 1f) // FIXME: Potential float comparison issue
            {
                Debug.Log("Cannot apply thrust while time warping!");
                return;
            }
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

    private void ResetThrust()
    {
        normalizedThrust = 0f;
        thrusting = false;
    }

    protected override void TimeScaleAdjusted(float newTimeScale)
    {
        ResetThrust();
        base.TimeScaleAdjusted(newTimeScale);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!thrusting)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Position, fullThrust * normalizedThrust * transform.up);
    }

    public void ChangeStabilityAssist(int newValue)
    {
        stabilityAssistMode = (ShipSystems.StabilityAssistMode)newValue;
    }
}


