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
    private bool executeManeuvers = false;
    private float trueAnomalyExecutionThreshold = 0.01f;
    private Ship ship;
    private List<ManeuverNode> plannedManeuvers;
    private ManeuverNode selectedNode;
    private int vectorLayerMask; //FIXME: make an enum for editor.
    private ManeuverVectorHandler selectedManeuverVector;

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

        vectorLayerMask = LayerMask.GetMask("ManeuverVectorSelection");
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

        HandleNodeVectorAdjustment();
        HandleNodePosition();
        HandleManeuverExecution();
    }
    #endregion UNITY

    #region GENERAL
    void HandleManeuverExecution()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleManeuverExecution();
        }

        if (!executeManeuvers)
            return;
        
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            if (Mathf.Abs(thisNode.TrueAnomaly - ship.TrueAnomaly) <= trueAnomalyExecutionThreshold)
            {
                ExecuteManeuver(thisNode);
                return;
            }
        }

    }

    void HandleNodePosition()
    {
        if (Input.GetMouseButtonUp(1))
        {
            selectedNode = null;
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            selectedNode = SelectManeuverNode(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButton(1))
        {
            float trueAnomaly = CalculateTrueAnomalyOfWorldPosition(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            
            // Update node values, including position and rotation
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
        newNode.Initialize(trueAnomaly, ship);
        newNode.ToggleManeuverExecution(executeManeuvers);
        plannedManeuvers.Add(newNode);
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

    private void ToggleManeuverExecution()
    {
        executeManeuvers = !executeManeuvers;
        for (int i = 0; i < plannedManeuvers.Count; i++)
        {
            ManeuverNode thisNode = plannedManeuvers[i];
            thisNode.ToggleManeuverExecution(executeManeuvers);
        }
    }

    private void ExecuteManeuver(ManeuverNode node)
    {
        Vector2 deltaVel = node.DeltaOrbitalVelocity.RotateVector(ship.ArgumentOfPeriapsis);
        ship.ExecuteInstantBurn(deltaVel);
        EmptyManeuvers();
    }

    #endregion GENERAL
}
