using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrajectoryPlotter))]
public class IntersectionCalculator : MonoBehaviour
{
    public GameObject intersectionObjectPrefab;

    private TrajectoryPlotter trajectoryPlotter;

    private GravitySource currentGravitySource;
    private List<SourceIntersections> sourceIntersections;
    private List<Color> sourceIntersectionColors;
    private int sourceIntersectionColorCount;

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
    }

    #endregion UNITY
    
    public void PlotNearestSourceIntersections(OrbitalBody orbitalBody)
    {
        // Called by TrajectoryHandlers AND by coroutines on intersection update
        PlotNearestSourceIntersections(orbitalBody, orbitalBody.OrbitalPosition, orbitalBody.OrbitalVelocity, 0f, orbitalBody.Trajectory);
    }

    public void PlotNearestSourceIntersections(OrbitalBody orbitalBody, Vector2 orbitalPosition, Vector2 orbitalVelocity, float timeToPosition, Trajectory trajectory)
    {
        // orbitalPosition/Velocity are not necessarily equivalent to orbitalBody.orbitalPosition/Velocity (ManeuverNode predicted trajectories for instance)

        // Update sources considered candidates for intersection
        UpdateNearbySourceIntersections(trajectory.ParentGravitySource);

        for (int i = 0; i < sourceIntersections.Count; i++)
        {
            Color intersectionColor = sourceIntersectionColors[MathUtilities.IntModulo(i, sourceIntersectionColorCount)];
            SourceIntersections sourceIntersectionsEntry = sourceIntersections[i];
            for (int j = 0; j < sourceIntersectionsEntry.SegmentIntersections.Length; j++)
            {
                PredictIntersection(sourceIntersectionsEntry, j, orbitalBody, orbitalPosition, orbitalVelocity, timeToPosition, trajectory, intersectionColor);
            }
        }
    }

    private void PredictIntersection(SourceIntersections sourceIntersectionsEntry, int segmentIndex, OrbitalBody orbitalBody, Vector2 orbitalPosition, Vector2 orbitalVelocity, float timeToPosition, Trajectory trajectory, Color intersectionColor)
    {
        

        // Calculate time of flight of current object to destination
        Vector2 worldDestination = sourceIntersectionsEntry.SegmentIntersections[segmentIndex].ClosestPoint;
        Vector2 localDestination = (worldDestination - trajectory.ParentGravitySource.Position).RotateVector(-trajectory.ArgumentOfPeriapsis);
        Debug.LogFormat("pos: {0}, pos-passed: {1},  dest: {2}", orbitalBody.OrbitalPosition, orbitalPosition, localDestination);
        float timeOfFlight = timeToPosition + OrbitalMechanics.UniversalVariableMethod.CalculateTimeOfFlight(orbitalPosition, orbitalVelocity, localDestination, trajectory.EccentricityVector, trajectory.ParentGravitySource.Mass);
        Debug.LogFormat("time of flight: {0}", timeOfFlight);
        if (float.IsNaN(timeOfFlight)){
            // timeOfFlight not properly calculated. hide sprites.
            sourceIntersectionsEntry.HideIntersectionObjects(segmentIndex);
            return;
        }

        // Plot current object's future position
        sourceIntersectionsEntry.InitiateIntersectionSprite(segmentIndex, sourceIntersectionsEntry.SegmentIntersections[segmentIndex].ClosestPoint, intersectionColor, true);

        // Plot this source object's position at timeOfFlight
        Vector2 predictedWorldPosition = sourceIntersectionsEntry.Source.PredictPosition(timeOfFlight);
        sourceIntersectionsEntry.InitiateIntersectionSprite(segmentIndex, predictedWorldPosition, intersectionColor, false);

        StartNewIntersectionCoroutine(orbitalBody, timeOfFlight, trajectory, sourceIntersectionsEntry, segmentIndex);
    }

    private void PredictIntersection(SourceIntersections sourceIntersectionsEntry, int segmentIndex, OrbitalBody orbitalBody, Vector2 orbitalPosition, Vector2 orbitalVelocity, float timeToPosition, Trajectory trajectory)
    {
        // Called on completion of coroutine solely. Maintain current sprite color
        SpriteRenderer spriteRenderer = sourceIntersectionsEntry.baseIntersectionSprites[segmentIndex].GetComponentInChildren<SpriteRenderer>();
        PredictIntersection(sourceIntersectionsEntry, segmentIndex, orbitalBody, orbitalPosition, orbitalVelocity, timeToPosition, trajectory, spriteRenderer.color);
    }

    private void StartNewIntersectionCoroutine(OrbitalBody orbitalBody, float timeOfFlight, Trajectory trajectory, SourceIntersections sourceIntersectionsEntry, int segmentIndex)
    {
        IEnumerator timeOfFlightEnum = TimerToPosition(orbitalBody, timeOfFlight, trajectory, sourceIntersectionsEntry, segmentIndex);
        StartCoroutine(timeOfFlightEnum);
        
        // Adding reference to this source intersections entry
        sourceIntersectionsEntry.intersectionCoroutines[segmentIndex] = timeOfFlightEnum;
    }

    private void UpdateIntersectionSprites(OrbitalBody orbitalBody, Trajectory trajectory, SourceIntersections sourceIntersectionsEntry, int segmentIndex)
    {
        // Calculate time of flight to next closest point -- FIXME: Still needed?????
        if (trajectory.TrajectoryType != OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {

            sourceIntersectionsEntry.HideIntersectionObjects(segmentIndex);
            return;
        }
        // Use trajectory period as timeOfFlight
        float timeOfFlight = trajectory.Period;
        return;
        Vector2 predictedWorldPosition = sourceIntersectionsEntry.Source.PredictPosition(timeOfFlight);
        sourceIntersectionsEntry.InitiateIntersectionSprite(segmentIndex, predictedWorldPosition, false);
        //StartNewIntersectionCoroutine(orbitalBody, trajectory.Period, trajectory, sourceIntersectionsEntry, segmentIndex);


        //Debug.LogFormat("Period: {0}", trajectory.Period);
        PredictIntersection(sourceIntersectionsEntry, segmentIndex, orbitalBody, orbitalBody.OrbitalPosition, orbitalBody.OrbitalVelocity, 0, trajectory);
    }

    private IEnumerator TimerToPosition(OrbitalBody orbitalBody, float time, Trajectory trajectory, SourceIntersections sourceIntersectionsEntry, int segmentIndex)
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(time);

        // Recalculate time of flight for orbitalBody to reach orbitalBodyPosition and then source's predicted position
        UpdateIntersectionSprites(orbitalBody, trajectory, sourceIntersectionsEntry, segmentIndex);
    }

    private void UpdateNearbySourceIntersections(GravitySource gravitySource)
    {
        if (gravitySource != currentGravitySource)
        {
            // Gravity source changed. Re-initialize source intersections
            currentGravitySource = gravitySource;
            InitializeSourceIntersections();
            return;
        }
        // Update source intersection entries
        for (int i = 0; i < sourceIntersections.Count; i++)
        {
            SourceIntersections thisSourceIntersectionEntry = sourceIntersections[i];
            
            // Hide all intersection sprites
            thisSourceIntersectionEntry.HideIntersectionObjects();
            // Stop coroutines running for each existing segment intersection
            for (int j = 0; j < thisSourceIntersectionEntry.IntersectionCount; j++)
            {
                IEnumerator thisRoutine = thisSourceIntersectionEntry.intersectionCoroutines[j];
                if (thisRoutine == null)
                    continue;
                StopCoroutine(thisRoutine);
            }
            // Update this entry with now valid intersection objects
            SegmentIntersection[] validIntersections = GetValidSegmentIntersections(thisSourceIntersectionEntry.Source);
            thisSourceIntersectionEntry.UpdateSegmentIntersections(validIntersections);
        }
    }

    private void InitializeSourceIntersections()
    {
        ResetNearestSourceIntersections();
        for (int i = 0; i < currentGravitySource.OrbitalBodies.Count; i++)
        {
            GravitySource source = currentGravitySource.OrbitalBodies[i];
            SegmentIntersection[] validIntersections = GetValidSegmentIntersections(source);
            sourceIntersections.Add(new SourceIntersections(validIntersections, source, intersectionObjectPrefab));
        }
    }
    private void StopIntersectionCoroutines()
    {
        for (int i = 0; i < sourceIntersections.Count; i++)
        {
            SourceIntersections thisSourceIntersectionEntry = sourceIntersections[i];
            for (int j = 0; j < thisSourceIntersectionEntry.IntersectionCount; j++)
            {
                StopCoroutine(thisSourceIntersectionEntry.intersectionCoroutines[j]);
            }
        }
    }

    private SegmentIntersection[] GetValidSegmentIntersections(GravitySource source)
    {
        TrajectoryHandler trajectoryHandler = source.TrajectoryHandler;
        if (trajectoryHandler == null)
        {
            return new SegmentIntersection[0];
        }

        List<SegmentIntersection> intersections = MathUtilities.GetClosestPointsBetweenPolygons(trajectoryPlotter.GetVertices(true), trajectoryHandler.GetVertices(true));
        Debug.Log(intersections.Count);
        List<SegmentIntersection> validIntersections = new List<SegmentIntersection>();
        for (int j = 0; j < intersections.Count; j++)
        {
            if (intersections[j].MinDistSq < source.RadiusOfInfluenceSq)
                validIntersections.Add(intersections[j]);
        }
        return validIntersections.ToArray();
    }

    private void ResetNearestSourceIntersections()
    {
        StopIntersectionCoroutines();
        for (int i = 0; i < sourceIntersections.Count; i++)
        {
            sourceIntersections[i].Destroy();
        }
        sourceIntersections.Clear();
    }

    public class SourceIntersections
    {
        // Really more of a "possible intersection" sort of deal..
        public IEnumerator[] intersectionCoroutines;
        public GameObject[] baseIntersectionSprites;
        public GameObject[] targetIntersectionSprites;
        public SegmentIntersection[] SegmentIntersections { get; private set; }
        public GravitySource Source { get; }

        public int IntersectionCount { get; private set; }
        public SourceIntersections(SegmentIntersection[] segmentIntersections, GravitySource source, GameObject intersectionObjectPrefab)
        {
            SegmentIntersections = segmentIntersections;
            IntersectionCount = segmentIntersections.Length;
            intersectionCoroutines = new IEnumerator[IntersectionCount];
            Source = source;

            baseIntersectionSprites = new GameObject[2];
            targetIntersectionSprites = new GameObject[2];
            for (int i = 0; i < baseIntersectionSprites.Length; i++)
            {
                AddIntersectionObject(i, baseIntersectionSprites, intersectionObjectPrefab);
                AddIntersectionObject(i, targetIntersectionSprites, intersectionObjectPrefab);
            }
        }

        private void AddIntersectionObject(int index, GameObject[] intersectionObjectArray, GameObject intersectionObjectPrefab)
        {
            GameObject intersectionObject = Instantiate(intersectionObjectPrefab);
            intersectionObject.SetActive(false);
            intersectionObjectArray[index] = intersectionObject;
        }

        public void InitiateIntersectionSprite(int index, Vector2 worldPosition, bool isBase)
        {
            GameObject[] array = isBase
                ? baseIntersectionSprites
                : targetIntersectionSprites;

            GameObject thisSpriteObject = array[index];
            thisSpriteObject.transform.position = worldPosition;
            thisSpriteObject.SetActive(true);
        }

        public void InitiateIntersectionSprite(int index, Vector2 worldPosition, Color color, bool isBase)
        {
            GameObject[] array = isBase
                ? baseIntersectionSprites
                : targetIntersectionSprites;

            GameObject thisSpriteObject = array[index];
            thisSpriteObject.transform.position = worldPosition;

            SpriteRenderer spriteRenderer = thisSpriteObject.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.color = color;
            thisSpriteObject.SetActive(true);
        }

        public void HideIntersectionObjects(int i)
        {
            baseIntersectionSprites[i].SetActive(false);
            targetIntersectionSprites[i].SetActive(false);
        }

        public void HideIntersectionObjects()
        {
            for (int i = 0; i < baseIntersectionSprites.Length; i++)
            {
                baseIntersectionSprites[i].SetActive(false);
                targetIntersectionSprites[i].SetActive(false);
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < baseIntersectionSprites.Length; i++)
            {
                Object.Destroy(baseIntersectionSprites[i]);
                Object.Destroy(targetIntersectionSprites[i]);
            }
        }

        public void UpdateSegmentIntersections(SegmentIntersection[] segmentIntersections)
        {
            SegmentIntersections = segmentIntersections;
            IntersectionCount = segmentIntersections.Length;
            intersectionCoroutines = new IEnumerator[IntersectionCount];
        }
    }
}
