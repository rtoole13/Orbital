using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNodeHandler : MonoBehaviour
{
    public GameObject nodePrefab;

    
    private Camera mainCamera;
    private bool isActive = false;
    private Ship ship;
    private List<ManeuverNode> plannedManeuvers;
    private ManeuverNode selectedNode;
    private GameObject instantiatedNode;

    #region UNITY
    private void Awake()
    {
        mainCamera = Camera.main;

        ship = GetComponentInParent<Ship>();
        if (ship == null)
            throw new UnityException(string.Format("{0}'s ManeuverNodeHandler must have a 'Ship' component on its parent!", name));

        ManeuverNode[] maneuverNodes = nodePrefab.GetComponentsInChildren<ManeuverNode>();
        if (maneuverNodes.Length != 1)
        {
            throw new UnityException(string.Format("{0}'s ManeuverNodeHandler must have a prefab with a single ManeuverNode component on it selected!", name));
        }
        
        // FIXME: Should probably check for SelectionHitBoxHandler too

        plannedManeuvers = new List<ManeuverNode>();
        ObjectSelector.OnObjectSelectionEvent += ObjectSelectionChanged;
    }

    private void OnDisable()
    {
        ObjectSelector.OnObjectSelectionEvent -= ObjectSelectionChanged;
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (!isActive)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            if (instantiatedNode == null)
                instantiatedNode = Instantiate(nodePrefab);
            if (!instantiatedNode.activeInHierarchy)
                instantiatedNode.SetActive(true);
        }
        if (Input.GetMouseButton(1))
        {
            Vector2 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            float trueAnomaly = CalculateTrueAnomalyOfPosition(ship.WorldPositionToPerifocalPosition(clickPosition));
            float orbitalRadius = OrbitalMechanics.OrbitalRadius(ship.Eccentricity, ship.SemimajorAxis, trueAnomaly);
            Vector2 orbitalPosition = OrbitalMechanics.OrbitalPosition(orbitalRadius, trueAnomaly, ship.ClockWiseOrbit);
            instantiatedNode.transform.position = OrbitalPositionToWorld(orbitalPosition);
        }
        if (Input.GetMouseButtonUp(1))
        {
            
        }

    }
    #endregion UNITY

    #region GENERAL
    //private ManeuverNode raycastSelectManeuverNode()
    //{

    //}

    private float CalculateTrueAnomalyOfPosition(Vector2 position)
    {
        Vector2 periapse = new Vector2(1f, 0f);
        float angle = Vector2.SignedAngle(periapse, position) * Mathf.Deg2Rad;
        float twoPi = 2f * Mathf.PI;
        return (angle + twoPi) % twoPi;
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
            return;
        }
        else
        {
            Ship newlySelectedShip = newlySelectedObject.GetComponent<Ship>();
            isActive = (ship == newlySelectedShip);
        }

        if (instantiatedNode != null)
            instantiatedNode.SetActive(false);
    }

    #endregion GENERAL
    //private void OnDrawGizmos()
    //{
    //    if (!isActive)
    //        return;
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(OrbitalPositionToWorld(), 1f);
    //}
}
