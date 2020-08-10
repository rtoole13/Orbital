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

    [SerializeField]
    private TrajectoryHandler debugTrajectoryTarget;

    private List<SourceIntersections> sourceIntersections;
    private List<Color> sourceIntersectionColors;
    private int sourceIntersectionColorCount;

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

        sourceIntersections = new List<SourceIntersections>();

        // Initialize source intersection colors
        // FIXME THIS IS TEMPORARY
        sourceIntersectionColors = new List<Color>();
        sourceIntersectionColors.Add(Color.red);
        sourceIntersectionColors.Add(Color.green);
        sourceIntersectionColors.Add(Color.blue);
        sourceIntersectionColorCount = sourceIntersectionColors.Count;
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

        // Plot nearest intersections
        PlotNearestSourceIntersectionss();
    }

    private void PlotNearestSourceIntersectionss()
    {
        if (!(orbitalBody is GravityAffected))
            return;

        // Update sources considered candidates for intersection
        GetNearbySourceIntersectionss();

        for (int i = 0; i < sourceIntersections.Count; i++)
        {
            Color intersectionColor = sourceIntersectionColors[MathUtilities.IntModulo(i, sourceIntersectionColorCount)];
            SourceIntersections thisSourceIntersections = sourceIntersections[i];
            for (int j = 0; j < thisSourceIntersections.WorldPositions.Count; j++)
            {
                Debug.Log(thisSourceIntersections.WorldPositions[j]);
            }
        }
    }

    private void GetNearbySourceIntersectionss()
    {
        sourceIntersections.Clear();
        List<GravitySource> sourcesInSystem = orbitalBody.CurrentGravitySource.OrbitalBodies;
        for (int i = 0; i < sourcesInSystem.Count; i++)
        {
            GravitySource source = sourcesInSystem[i];
            TrajectoryHandler trajectoryHandler = source.TrajectoryHandler;
            if (trajectoryHandler == null)
                continue;
            List<Vector2> sourceSourceIntersectionss = GetClosestPointOfSourceIntersections(trajectoryHandler);
            if (SourceHasNearbySourceIntersections(sourceSourceIntersectionss, source))
            {
                sourceIntersections.Add(new SourceIntersections(sourceSourceIntersectionss, source));
            }
        }
    }

    private bool SourceHasNearbySourceIntersections(List<Vector2> nearestSourceIntersectionss, GravitySource source)
    {
        for (int j = 0; j < nearestSourceIntersectionss.Count; j++)
        {
            float distSq = (nearestSourceIntersectionss[j] - source.Position).sqrMagnitude;
            if (distSq < source.RadiusOfInfluenceSq)
                return true;
        }
        return false;
    }

    public Vector3[] GetVertices(bool inWorldCoordinates)
    {
        return trajectoryPlotter.GetVertices(inWorldCoordinates);
    }

    private List<Vector2> GetClosestPointOfSourceIntersections(TrajectoryHandler otherTrajectoryHandler)
    {
        List<Vector2> intersections = MathUtilities.GetClosestPointsBetweenPolygons(GetVertices(true), otherTrajectoryHandler.GetVertices(true));
        return intersections;
    }

    public struct SourceIntersections
    {
        // Really more of a "possible intersection" sort of deal..
        public SourceIntersections(List<Vector2> worldPositions, GravitySource source)
        {
            WorldPositions = worldPositions;
            Source = source;
        }
        public List<Vector2> WorldPositions { get; }
        public GravitySource Source { get; }

    }

    //private void OnDrawGizmos()
    //{
    //    if (trajectoryPlotter == null)
    //        return;

    //    if (trajectoryPlotter.GetVertexCount() == 0)
    //        return;

    //    if (debugTrajectoryTarget == null)
    //        return;

    //    List<Vector2> intersections = GetClosestPointOfSourceIntersections(debugTrajectoryTarget);
    //    Gizmos.color = Color.green;
    //    for (int i = 0; i < intersections.Count; i++)
    //    {
    //        Gizmos.DrawSphere(intersections[i], 1f);
    //    }
    //}
}
