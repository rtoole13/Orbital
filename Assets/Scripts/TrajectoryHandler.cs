using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody))]
public class TrajectoryHandler : MonoBehaviour
{
    public GameObject trajectoryObjectPrefab;
    public Gradient trajectoryGradient;

    private GameObject trajectoryObject;
    private TrajectoryPlotter trajectoryPlotter;
    private OrbitalBody orbitalBody;
    private float eccentricityTolerance = 0.05f;

    #region GETSET
    public float SemimajorAxis
    {
        get { return orbitalBody.SemimajorAxis; }

    }
    public float SemiminorAxis
    {
        get { return orbitalBody.SemiminorAxis; }
    }

    #endregion GETSET
    #region UNITY
    private void Awake()
    {
        orbitalBody = GetComponent<OrbitalBody>();
        orbitalBody.OnOrbitCalculationEvent += UpdateTrajectory;

        // Instantiate prefab if null
        trajectoryObject = Instantiate(trajectoryObjectPrefab);
        trajectoryPlotter = trajectoryObject.GetComponent<TrajectoryPlotter>();
        trajectoryPlotter.SetGradient(trajectoryGradient);
    }

    private void OnDisable()
    {
        orbitalBody.OnOrbitCalculationEvent -= UpdateTrajectory;
    }
    
    #endregion UNITY

    private void UpdateTrajectory()
    {
        if (orbitalBody.CurrentGravitySource == null)
            return;
        
        trajectoryObject.transform.parent = orbitalBody.CurrentGravitySource.transform;
        trajectoryObject.transform.position = trajectoryObject.transform.parent.position;
        if (orbitalBody.Eccentricity >= 1f)
        {
            trajectoryPlotter.BuildHyperbolicTrajectory(SemimajorAxis, SemiminorAxis, orbitalBody.Eccentricity, orbitalBody.ArgumentOfPeriapsis);
        }
        else
        {
            trajectoryPlotter.BuildEllipticalTrajectory(SemimajorAxis, SemiminorAxis, orbitalBody.Eccentricity, orbitalBody.ArgumentOfPeriapsis);
        }
    }
}
