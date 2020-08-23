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

    private void RemoveIntersectionObjectPair(GameObject intersectionObjectA, GameObject intersectionObjectB)
    {
        sourceIntersectionObjectSpriteObjects.Remove(intersectionObjectA);
        sourceIntersectionObjectSpriteObjects.Remove(intersectionObjectB);
        Destroy(intersectionObjectA);
        Destroy(intersectionObjectB);
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
                // Plot current object's future position
                GameObject intersectionObjectA = Instantiate(intersectionObjectPrefab, thisSourceIntersections.SegmentIntersections[j].ClosestPoint, Quaternion.identity);
                SpriteRenderer spriteRenderer = intersectionObjectA.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = sourceIntersectionColors[i];
                sourceIntersectionObjectSpriteObjects.Add(intersectionObjectA);

                // Calculate time of flight of current object to destination
                Vector2 worldDestination = thisSourceIntersections.SegmentIntersections[j].ClosestPoint;
                Vector2 localDestination = (worldDestination - orbitalBody.CurrentGravitySource.Position).RotateVector(-orbitalBody.ArgumentOfPeriapsis);
                float timeOfFlight = OrbitalMechanics.UniversalVariableMethod.CalculateTimeOfFlight(orbitalBody.OrbitalPosition, orbitalBody.OrbitalVelocity, localDestination, orbitalBody.EccentricityVector, orbitalBody.CurrentGravitySource.Mass);
                 
                // Plot this source object's position at timeOfFlight
                Vector2 predictedWorldPosition = thisSourceIntersections.Source.PredictPosition(timeOfFlight);
                GameObject intersectionObjectB = InitiateIntersectionObject(predictedWorldPosition, sourceIntersectionColors[i]);


                //Debug
                IEnumerator timeOfFlightCalc = TimerToPosition(timeOfFlight, thisSourceIntersections.Source, intersectionObjectA, intersectionObjectB);
                StartCoroutine(timeOfFlightCalc);
            }
        }
    }

    private GameObject InitiateIntersectionObject(Vector2 worldPosition, Color color)
    {
        GameObject intersectionObjectB = Instantiate(intersectionObjectPrefab, worldPosition, Quaternion.identity);
        SpriteRenderer spriteRenderer = intersectionObjectB.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = color;
        sourceIntersectionObjectSpriteObjects.Add(intersectionObjectB);
        return intersectionObjectB;
    }

    private void UpdateIntersectionObject(GravitySource source, GameObject intersectionObjectA, GameObject intersectionObjectB)
    {
        if (intersectionObjectA == null || intersectionObjectB == null)
            return;

        // Calculate time of flight to next closest point
        if (orbitalBody.TrajectoryType != OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            RemoveIntersectionObjectPair(intersectionObjectA, intersectionObjectB);
            return;
        }

        // Returns to this position in one period.
        float timeOfFlight = orbitalBody.OrbitalPeriod;

        // Update intersection object's position
        intersectionObjectB.transform.position = source.PredictPosition(timeOfFlight);

        // Kick off another coroutine for tracking time til intersection point.
        IEnumerator timeOfFlightCalc = TimerToPosition(timeOfFlight, source, intersectionObjectA, intersectionObjectB);
        StartCoroutine(timeOfFlightCalc);
    }

    private IEnumerator TimerToPosition(float time, GravitySource source, GameObject intersectionObjectA, GameObject intersectionObjectB)
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(time);

        // Recalculate time of flight for orbitalBody to reach orbitalBodyPosition and then source's predicted position
        UpdateIntersectionObject(source, intersectionObjectA, intersectionObjectB);
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
