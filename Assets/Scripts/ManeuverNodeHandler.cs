using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ManeuverNodeHandler : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject nodePrefab;

    private bool isActive = false;
    private Ship selectedShip;
    private Vector2 orbitalPosition;
    private GameObject instantiatedNode;

    #region UNITY
    private void Awake()
    {
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
            float trueAnomaly = CalculateTrueAnomalyOfPosition(selectedShip.WorldPositionToPerifocalPosition(clickPosition));
            float orbitalRadius = OrbitalMechanics.OrbitalRadius(selectedShip.Eccentricity, selectedShip.SemimajorAxis, trueAnomaly);
            orbitalPosition = OrbitalMechanics.OrbitalPosition(orbitalRadius, trueAnomaly, selectedShip.ClockWiseOrbit);
            instantiatedNode.transform.position = OrbitalPositionToWorld();
        }
    }
    #endregion UNITY

    #region GENERAL
    private float CalculateTrueAnomalyOfPosition(Vector2 position)
    {
        Vector2 periapse = new Vector2(1f, 0f);
        float angle = Vector2.SignedAngle(periapse, position) * Mathf.Deg2Rad;
        float twoPi = 2f * Mathf.PI;
        return (angle + twoPi) % twoPi;
    }

    private Vector2 OrbitalPositionToWorld()
    {
        Vector2 translation = selectedShip.CurrentGravitySource != null
            ? selectedShip.CurrentGravitySource.Position
            : Vector2.zero;
        return orbitalPosition.RotateVector(selectedShip.ArgumentOfPeriapsis) + translation;
    }

    private void ObjectSelectionChanged(GameObject newlySelectedObject)
    {
        if (newlySelectedObject == null)
        {
            isActive = false;
            selectedShip = null;
            return;
        }
        else
        {
            selectedShip = newlySelectedObject.GetComponent<Ship>();
            isActive = (selectedShip != null);
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
