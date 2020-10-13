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
    private IntersectionCalculator intersectionCalculator;
    private OrbitalBody orbitalBody;


    #region GETSET
    public float SemimajorAxis
    {
        get { return orbitalBody.Trajectory.SemimajorAxis; }

    }
    public float SemiminorAxis
    {
        get { return orbitalBody.Trajectory.SemiminorAxis; }
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
        if (trajectoryPlotter == null)
            throw new UnityException("Expecting trajectory prefab to have a TrajectoryPlotter script");
        trajectoryPlotter.SetGradient(trajectoryGradient);
        intersectionCalculator = trajectoryObject.GetComponent<IntersectionCalculator>();
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
        if (orbitalBody.Trajectory.Eccentricity >= 1f)
        {
            trajectoryPlotter.BuildHyperbolicTrajectory(SemimajorAxis, SemiminorAxis, orbitalBody.Trajectory.Eccentricity, orbitalBody.Trajectory.ArgumentOfPeriapsis);
        }
        else
        {
            trajectoryPlotter.BuildEllipticalTrajectory(SemimajorAxis, SemiminorAxis, orbitalBody.Trajectory.Eccentricity, orbitalBody.Trajectory.ArgumentOfPeriapsis);
        }

        // Plot nearest intersections
        if (intersectionCalculator != null && orbitalBody is GravityAffected)
            intersectionCalculator.PlotNearestSourceIntersections(orbitalBody);
    }

    public Vector3[] GetVertices(bool inWorldCoordinates)
    {
        return trajectoryPlotter.GetVertices(inWorldCoordinates);
    }
}
