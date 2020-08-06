using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics = OrbitalMechanics;

public class ManeuverNodeHandler : MonoBehaviour
{
    public GameObject nodePrefab;

    [Range(1,5)]
    public int maneuverNodeMaxCount = 2;

    public Gradient nodeOutlineGradient;

    private Camera mainCamera;
    private bool isActive = false;
    private bool executeManeuversBool = false;
    private bool activelyDraggingNode = false;
    private Ship ship;
    private List<ManeuverNode> plannedManeuvers;
    private ManeuverNode selectedNode;
    private int nodeLayerMask;
    private int vectorLayerMask;
    private ManeuverVectorHandler selectedManeuverVector;
    private float lastTrueAnomalyCalculated;


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

        nodeLayerMask = LayerMask.GetMask("ManeuverSelection");
        vectorLayerMask = LayerMask.GetMask("ManeuverVectorSelection");
        plannedManeuvers = new List<ManeuverNode>();
        ObjectSelector.OnObjectSelectionEvent += ObjectSelectionChanged;
        ship.GravitySourceChangedEvent += ShipGravitySourceChanged;
        lastTrueAnomalyCalculated = ship.TrueAnomaly;
    }

    private void OnDisable()
    {
        ObjectSelector.OnObjectSelectionEvent -= ObjectSelectionChanged;
        ship.GravitySourceChangedEvent -= ShipGravitySourceChanged;
    }
    
    void Update()
    {
        if (!isActive)
            return;

        HandleNodeVectorAdjustment();
        HandleNodePosition();
        HandleManeuverExecution();
    }
    #endregion UNITY

    #region GENERAL
    void HandleManeuverExecution()
    {
        if (!activelyDraggingNode && Input.GetKeyDown(KeyCode.M)) 
        {
            SetManeuverNodeExecution(!executeManeuversBool);
        }

        if (!executeManeuversBool)
            return;


        float currentTrueAnomaly = ship.TrueAnomaly;
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            if (AngleInRange(thisNode.TrueAnomaly, lastTrueAnomalyCalculated, currentTrueAnomaly))
            {
                ExecuteManeuver(thisNode);
                return;
            }
        }
        lastTrueAnomalyCalculated = currentTrueAnomaly;
    }

    bool AngleInRange(float val, float min, float max)
    {
        if (max < min && val <= max)
        {
            // wrapping from 2pi to 0
            return true;
        }
        else if (val >= min && val <= max){
            return true;
        }
        return false;
    }

    void HandleNodePosition()
    {
        if (Input.GetMouseButtonUp(1))
        {
            selectedNode = null;
            activelyDraggingNode = false;
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            selectedNode = SelectManeuverNode(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            activelyDraggingNode = true;
            SetManeuverNodeExecution(false);
        }

        if (Input.GetMouseButton(1))
        {
            float trueAnomaly = CalculateTrueAnomalyOfWorldPosition(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            
            // Update node values, including position and rotation
            if (selectedNode != null)
                selectedNode.UpdateParameters(trueAnomaly);
        }
    }

    void HandleNodeVectorAdjustment()
    {
        if (Input.GetMouseButtonUp(0))
        {
            DeselectManeuverNodeVector();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectManeuverNodeVector();
        }

        if (selectedManeuverVector == null)
            return;

        if (Input.GetMouseButton(0))
        {
            selectedManeuverVector.DragVector(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void DeselectManeuverNodeVector()
    {
        if (selectedManeuverVector == null)
            return;

        selectedManeuverVector.EndVectorSelect();
        selectedManeuverVector = null;
    }

    void SelectManeuverNodeVector()
    {
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(mainCamera.ScreenPointToRay(Input.mousePosition), 100f, vectorLayerMask);
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit2D hit = rayHits[i];
            selectedManeuverVector = hit.collider.GetComponent<ManeuverVectorHandler>();
            if (selectedManeuverVector == null)
                continue;
            
            selectedManeuverVector.InitializeVectorSelect(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    private ManeuverNode SelectManeuverNode(Vector2 mousePosition)
    {
        //nodeLayerMask
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(mainCamera.ScreenPointToRay(Input.mousePosition), 100f, nodeLayerMask);
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit2D hit = rayHits[i];
            ManeuverNode maneuverNode = hit.collider.GetComponent<ManeuverNode>();
            if (maneuverNode != null)
                return maneuverNode;
        }
        return CreateManeuverNode(mousePosition);
    }

    private ManeuverNode CreateManeuverNode(Vector2 position)
    {
        float trueAnomaly = CalculateTrueAnomalyOfWorldPosition(position);

        ManeuverNode newNode;
        if (plannedManeuvers.Count >= maneuverNodeMaxCount)
        {
            // Effectively pop first node - DELETING all child nodes
            newNode = plannedManeuvers[0];
            newNode.ClearNodes();
            plannedManeuvers.RemoveAt(0);
        }
        else
        {
            GameObject newNodeObject = Instantiate(nodePrefab);
            newNode = newNodeObject.GetComponent<ManeuverNode>();
        }
        lastTrueAnomalyCalculated = ship.TrueAnomaly;
        newNode.Initialize(trueAnomaly, ship, nodeOutlineGradient);
        newNode.SetManeuverExecution(executeManeuversBool);
        plannedManeuvers.Add(newNode);
        return newNode;
    }

    private float CalculateTrueAnomalyOfWorldPosition(Vector2 worldPosition)
    {
        // Angle b/w from eccentricity vector and vector pointing from CurrentGravitySource to position
        Vector2 perifocalPosition = ship.WorldPositionToPerifocalPosition(worldPosition);
        Vector2 periapse = new Vector2(1f, 0f);
        float angle = Vector2.SignedAngle(periapse, perifocalPosition) * Mathf.Deg2Rad;

        if (ship.ClockWiseOrbit)
            angle *= -1f;

        if (ship.TrajectoryType == Mechanics.Globals.TrajectoryType.Hyperbola) // For hyperbolas, range +/- trueAnomOfAsymptote
            return angle;

        float twoPi = 2f * Mathf.PI;
        return (angle + twoPi) % twoPi;
    }

    private void ShipGravitySourceChanged()
    {
        HideNodes();
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

    private void EmptyManeuvers()
    {
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            thisNode.ClearNodes();
            thisNode.HideNode();
        }
        plannedManeuvers.Clear();
    }

    private void SetManeuverNodeExecution(bool val)
    {
        executeManeuversBool = val;
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            thisNode.SetManeuverExecution(executeManeuversBool);
        }
    }

    private void ExecuteManeuver(ManeuverNode node)
    {
        Vector2 deltaVel = node.DeltaOrbitalVelocity.RotateVector(ship.ArgumentOfPeriapsis);
        if (deltaVel.sqrMagnitude > 0f)
            ship.ExecuteInstantBurn(deltaVel);
        EmptyManeuvers();
        Destroy(node);
    }

    #endregion GENERAL
}
