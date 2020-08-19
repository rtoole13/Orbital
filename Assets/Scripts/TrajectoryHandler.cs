using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody))]
public class TrajectoryHandler : MonoBehaviour
{
    public GameObject trajectoryObjectPrefab;
    public Gradient trajectoryGradient;

    public GameObject intersectionObjectPrefab;

    private GameObject trajectoryObject;
    private TrajectoryPlotter trajectoryPlotter;
    private OrbitalBody orbitalBody;

    private List<SourceIntersections> sourceIntersections;
    private List<Color> sourceIntersectionColors;
    private int sourceIntersectionColorCount;
    private List<GameObject> sourceIntersectionObjectSpriteObjects;


    // DEBUG

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
        sourceIntersectionObjectSpriteObjects = new List<GameObject>();
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
        PlotNearestSourceIntersections();
    }

    private void ResetNearestSourceIntersections()
    {
        sourceIntersections.Clear();
        for (int i = 0; i < sourceIntersectionObjectSpriteObjects.Count; i++)
        {
            Destroy(sourceIntersectionObjectSpriteObjects[i]);
        }
        sourceIntersectionObjectSpriteObjects.Clear();
    }

    private void PlotNearestSourceIntersections()
    {
        if (!(orbitalBody is GravityAffected))
            return;

        // Clear previous source intersections
        ResetNearestSourceIntersections();

        // Update sources considered candidates for intersection
        GetNearbySourceIntersections();

        for (int i = 0; i < sourceIntersections.Count; i++)
        {
            Color intersectionColor = sourceIntersectionColors[MathUtilities.IntModulo(i, sourceIntersectionColorCount)];
            SourceIntersections thisSourceIntersections = sourceIntersections[i];
            for (int j = 0; j < thisSourceIntersections.SegmentIntersections.Count; j++)
            {
                GameObject intersectionObject = Instantiate(intersectionObjectPrefab, thisSourceIntersections.SegmentIntersections[j].ClosestPoint, Quaternion.identity);
                SpriteRenderer spriteRenderer = intersectionObject.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = sourceIntersectionColors[i];
                sourceIntersectionObjectSpriteObjects.Add(intersectionObject);
                Vector2 destination = (thisSourceIntersections.SegmentIntersections[j].ClosestPoint - orbitalBody.CurrentGravitySource.Position).RotateVector(-orbitalBody.ArgumentOfPeriapsis);
                Debug.LogFormat("trajHand trueAnom: {0}", orbitalBody.TrueAnomaly * Mathf.Rad2Deg);
                float timeOfFlight = OrbitalMechanics.UniversalVariableMethod.CalculateTimeOfFlight(orbitalBody.OrbitalPosition, orbitalBody.OrbitalVelocity, destination, orbitalBody.EccentricityVector, orbitalBody.CurrentGravitySource.Mass);
                IEnumerator timeOfFlightCalc = Timer(timeOfFlight);
                StartCoroutine(timeOfFlightCalc);
            }
        }
    }

    private void GetNearbySourceIntersections()
    {
        sourceIntersections.Clear();
        List<GravitySource> sourcesInSystem = orbitalBody.CurrentGravitySource.OrbitalBodies;
        for (int i = 0; i < sourcesInSystem.Count; i++)
        {
            GravitySource source = sourcesInSystem[i];
            TrajectoryHandler trajectoryHandler = source.TrajectoryHandler;
            if (trajectoryHandler == null)
                continue;
            List<SegmentIntersection> sourceSegmentIntersections = GetClosestPointOfSourceIntersections(trajectoryHandler);
            if (SourceHasNearbySourceIntersections(sourceSegmentIntersections, source))
            {
                sourceIntersections.Add(new SourceIntersections(sourceSegmentIntersections, source));
            }
        }
    }

    private bool SourceHasNearbySourceIntersections(List<SegmentIntersection> nearestSourceIntersections, GravitySource source)
    {
        for (int j = 0; j < nearestSourceIntersections.Count; j++)
        {
            if (nearestSourceIntersections[j].MinDistSq < source.RadiusOfInfluenceSq)
                return true;
        }
        return false;
    }

    public Vector3[] GetVertices(bool inWorldCoordinates)
    {
        return trajectoryPlotter.GetVertices(inWorldCoordinates);
    }

    private List<SegmentIntersection> GetClosestPointOfSourceIntersections(TrajectoryHandler otherTrajectoryHandler)
    {
        List<SegmentIntersection> intersections = MathUtilities.GetClosestPointsBetweenPolygons(GetVertices(true), otherTrajectoryHandler.GetVertices(true));
        return intersections;
    }

    public struct SourceIntersections
    {
        // Really more of a "possible intersection" sort of deal..
        public SourceIntersections(List<SegmentIntersection> segmentIntersections, GravitySource source)
        {
            SegmentIntersections = segmentIntersections;
            Source = source;
        }
        public List<SegmentIntersection> SegmentIntersections { get; }
        public GravitySource Source { get; }

    }

    //DEBUG
    private IEnumerator Timer(float time)
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(time);
        Debug.LogFormat("{0} seconds executed!", time);
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
