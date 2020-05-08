using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNodeHandler : MonoBehaviour
{
    public GameObject nodePrefab;

    [Range(1,5)]
    public int maneuverNodeMaxCount = 2;

    private Camera mainCamera;
    private bool isActive = false;
    private Ship ship;
    private List<ManeuverNode> plannedManeuvers;
    private ManeuverNode selectedNode;
    private int maneuverLayerMask;

    #region UNITY
    private void Awake()
    {
        mainCamera = Camera.main;

        ship = GetComponentInParent<Ship>();
        if (ship == null)
            throw new UnityException(string.Format("{0}'s ManeuverNodeHandler must have a 'Ship' component on its parent!", name));

        ManeuverNode[] maneuverNodeScripts = nodePrefab.GetComponentsInChildren<ManeuverNode>();
        if (maneuverNodeScripts.Length != 1)
        {
            throw new UnityException(string.Format("{0}'s ManeuverNodeHandler must have a prefab with a single ManeuverNode component on it selected!", name));
        }

        // FIXME: Should probably check for SelectionHitBoxHandler too

        maneuverLayerMask = LayerMask.GetMask("ObjectSelection");
        plannedManeuvers = new List<ManeuverNode>();
        ObjectSelector.OnObjectSelectionEvent += ObjectSelectionChanged;
    }

    private void OnDisable()
    {
        ObjectSelector.OnObjectSelectionEvent -= ObjectSelectionChanged;
    }
    
    void Update()
    {
        if (!isActive)
            return;

        if (Input.GetMouseButtonUp(1))
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            selectedNode = SelectManeuverNode(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButton(1))
        {
            // May need a null selectedNode check. If so, something's prob set up wrong
            float trueAnomaly = CalculateTrueAnomalyOfWorldPosition(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            selectedNode.transform.position = WorldPositionFromTrueAnomaly(trueAnomaly);
        }
        

    }
    #endregion UNITY

    #region GENERAL
    private ManeuverNode SelectManeuverNode(Vector2 mousePosition)
    {
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            float distSq = Vector2.SqrMagnitude(mousePosition - (Vector2)plannedManeuvers[i].transform.position);
            if (distSq < thisNode.HitRadiusSq)
                return thisNode;
        }
        return CreateManeuverNode(mousePosition);
    }

    private ManeuverNode CreateManeuverNode(Vector2 position)
    {
        float trueAnomaly = CalculateTrueAnomalyOfWorldPosition(position);
        Vector2 worldPosition = WorldPositionFromTrueAnomaly(trueAnomaly);

        ManeuverNode newNode;
        if (plannedManeuvers.Count >= maneuverNodeMaxCount)
        {
            //Effectively pop first node - DELETING all child nodes
            newNode = plannedManeuvers[0];
            newNode.ClearNodes();
            plannedManeuvers.RemoveAt(0);
            newNode.transform.position = worldPosition;
        }
        else
        {
            GameObject newNodeObject = Instantiate(nodePrefab, worldPosition, Quaternion.identity);
            newNode = newNodeObject.GetComponent<ManeuverNode>();
        }
        newNode.trueAnomaly = trueAnomaly;
        plannedManeuvers.Add(newNode);
        Debug.Log(plannedManeuvers.Count);
        Debug.Log(plannedManeuvers[0].spriteRenderer);
        return newNode;
    }

    private float CalculateTrueAnomalyOfWorldPosition(Vector2 worldPosition)
    {
        // Angle b/w from eccentricity vector and vector pointing from CurrentGravitySource to position
        Vector2 perifocalPosition = ship.WorldPositionToPerifocalPosition(worldPosition);
        Vector2 periapse = new Vector2(1f, 0f);
        float angle = Vector2.SignedAngle(periapse, perifocalPosition) * Mathf.Deg2Rad;
        float twoPi = 2f * Mathf.PI;
        return (angle + twoPi) % twoPi;
    }

    public Vector2 WorldPositionFromTrueAnomaly(float trueAnomaly)
    {
        float orbitalRadius = OrbitalMechanics.OrbitalRadius(ship.Eccentricity, ship.SemimajorAxis, trueAnomaly);
        Vector2 orbitalPosition = OrbitalMechanics.OrbitalPosition(orbitalRadius, trueAnomaly, ship.ClockWiseOrbit);
        return OrbitalPositionToWorld(orbitalPosition);
    }

    private Vector2 OrbitalPositionToWorld(Vector2 perifocalPosition)
    {
        Vector2 translation = ship.CurrentGravitySource != null
            ? ship.CurrentGravitySource.Position
            : Vector2.zero;
        return perifocalPosition.RotateVector(ship.ArgumentOfPeriapsis) + translation;
    }

    private void ObjectSelectionChanged(GameObject newlySelectedObject)
    {
        if (newlySelectedObject == null)
        {
            isActive = false;
        }
        else
        {
            Ship newlySelectedShip = newlySelectedObject.GetComponent<Ship>();
            isActive = (ship == newlySelectedShip);
        }

        if (isActive)
        {
            ShowNodes();
        }
        else
        {
            HideNodes();
        }
    }

    private void ShowNodes()
    {
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            thisNode.ShowNode();
        }
    }

    private void HideNodes()
    {
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            thisNode.HideNode();
        }
    }

    #endregion GENERAL
}
