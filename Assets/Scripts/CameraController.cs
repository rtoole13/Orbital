using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Range(1, 4)]
    private float zoomMultiplier = 4f;

    [SerializeField]
    [Range(0.01f, 0.2f)]
    private float trackingDeltaDistance = 0.075f;

    [SerializeField]
    [Range(0, 4)]
    private float zoomDeltaDistance = 2f;

    [SerializeField]
    private float cameraSizeMin = 5f;

    [SerializeField]
    private float cameraSizeMax = 100f;

    [SerializeField]
    private GameObject currentTarget; // Make this an interface, ICameraTrackable
    private Camera mainCamera;
    private float targetOrthographicSize;
    private float defaultOrthographicSize;

    

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetOrthographicSize = defaultOrthographicSize = mainCamera.orthographicSize;
    }
    
    void Update()
    {
        HandleMouseInputs();

        Vector3 target = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, target, trackingDeltaDistance);
        mainCamera.orthographicSize = Mathf.MoveTowards(mainCamera.orthographicSize, targetOrthographicSize, zoomDeltaDistance);
    }

    private void HandleMouseInputs()
    {
        if (Input.GetMouseButton(0))
        {
            ChangeFocusTarget();
        }

        if (Input.GetMouseButtonDown(3))
        {
            ResetOrthographicSize();
            return;
        }

        if (Input.mouseScrollDelta.y == 0)
            return;
        targetOrthographicSize = Mathf.Clamp(targetOrthographicSize - (float)Input.mouseScrollDelta.y * zoomMultiplier, cameraSizeMin, cameraSizeMax);
    }

    private void ChangeFocusTarget()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Debug.Log("wee");
        }
        
    }

    private void ResetOrthographicSize()
    {
        targetOrthographicSize = defaultOrthographicSize;
    }
}
