using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrajectoryPlotter))]
public class IntersectionCalculator : MonoBehaviour
{
    public GameObject intersectionObjectPrefab;

    private TrajectoryPlotter trajectoryPlotter;
    
    private List<SourceIntersections> sourceIntersections;
    private List<Color> sourceIntersectionColors;
    private int sourceIntersectionColorCount;
    private List<GameObject> sourceIntersectionObjectSpriteObjects;

    #region UNITY
    private void Awake()
    {
        trajectoryPlotter = GetComponent<TrajectoryPlotter>();
        sourceIntersections = new List<SourceIntersections>();

        // Initialize source intersection colors
        // FIXME THIS IS TEMPORARY
        sourceIntersectionColors = new List<Color>
        {
            Color.red,
            Color.green,
            Color.blue
        };
        sourceIntersectionColorCount = sourceIntersectionColors.Count;
        sourceIntersectionObjectSpriteObjects = new List<GameObject>();

    }

    #endregion UNITY
    
    public void PlotNearestSourceIntersections(Vector2 orbitalPosition, Vector2 orbitalVelocity, Trajectory trajectory)
    {
        // Clear previous source intersections
        ResetNearestSourceIntersections();

        // Update sources considered candidates for intersection
        GetNearbySourceIntersections(trajectory.ParentGravitySource);

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
                Vector2 localDestination = (worldDestination - trajectory.ParentGravitySource.Position).RotateVector(-trajectory.ArgumentOfPeriapsis);
                float timeOfFlight = OrbitalMechanics.UniversalVariableMethod.CalculateTimeOfFlight(orbitalPosition, orbitalVelocity, localDestination, trajectory.EccentricityVector, trajectory.ParentGravitySource.Mass);
                
                // Plot this source object's position at timeOfFlight
                Vector2 predictedWorldPosition = thisSourceIntersections.Source.PredictPosition(timeOfFlight);
                GameObject intersectionObjectB = InitiateIntersectionObject(predictedWorldPosition, sourceIntersectionColors[i]);


                //Debug
                IEnumerator timeOfFlightCalc = TimerToPosition(timeOfFlight, thisSourceIntersections.Source, trajectory, intersectionObjectA, intersectionObjectB);
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

    private void UpdateIntersectionObject(OrbitalBody otherObject, Trajectory trajectory, GameObject intersectionObjectA, GameObject intersectionObjectB)
    {
        if (intersectionObjectA == null || intersectionObjectB == null)
            return;

        // Calculate time of flight to next closest point
        if (trajectory.TrajectoryType != OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            RemoveIntersectionObjectPair(intersectionObjectA, intersectionObjectB);
            return;
        }

        // Returns to this position in one period.
        float timeOfFlight = trajectory.Period;

        // Update intersection object's position
        intersectionObjectB.transform.position = otherObject.PredictPosition(timeOfFlight);

        // Kick off another coroutine for tracking time til intersection point.
        IEnumerator timeOfFlightCalc = TimerToPosition(timeOfFlight, otherObject, trajectory, intersectionObjectA, intersectionObjectB);
        StartCoroutine(timeOfFlightCalc);
    }

    private IEnumerator TimerToPosition(float time, OrbitalBody otherObject, Trajectory trajectory, GameObject intersectionObjectA, GameObject intersectionObjectB)
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(time);

        // Recalculate time of flight for orbitalBody to reach orbitalBodyPosition and then source's predicted position
        UpdateIntersectionObject(otherObject, trajectory, intersectionObjectA, intersectionObjectB);
    }

    private void GetNearbySourceIntersections(GravitySource gravitySource)
    {
        sourceIntersections.Clear();
        List<GravitySource> sourcesInSystem = gravitySource.OrbitalBodies;
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

    private List<SegmentIntersection> GetClosestPointOfSourceIntersections(TrajectoryHandler otherTrajectoryHandler)
    {
        List<SegmentIntersection> intersections = MathUtilities.GetClosestPointsBetweenPolygons(trajectoryPlotter.GetVertices(true), otherTrajectoryHandler.GetVertices(true));
        return intersections;
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
}
