using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Range(1, 4)]
    private float zoomMultiplier = 4f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float moveSmoothTime = 0.5f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float zoomSmoothTime = 0.15f;

    [SerializeField]
    private float cameraSizeMin = 5f;

    [SerializeField]
    private float cameraSizeMax = 100f;

    [SerializeField]
    private GameObject currentTarget; // Make this an interface, ICameraTrackable

    [SerializeField]
    private float doubleClickSelectInterval = 1f;

    private Vector3 moveVelocity = Vector3.zero;
    private float zoomVelocity = 0f;
    private float firstClickTime;
    private bool inDoubleClickRange = false;
    private Camera mainCamera;
    private float targetOrthographicSize;
    private float defaultOrthographicSize;

    public delegate void OnOrthographicSizeChangeDelegate(float minOrthoSize, float maxOrthoSize, float currentOrthoSize);
    public static event OnOrthographicSizeChangeDelegate OrthographicSizeChangeEvent;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetOrthographicSize = defaultOrthographicSize = mainCamera.orthographicSize;
    }
    
    void Update()
    {
        HandleMouseInputs();
        Vector3 target = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z);
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, target, ref moveVelocity, moveSmoothTime);
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, targetOrthographicSize, ref zoomVelocity, zoomSmoothTime);
    }

    private void HandleMouseInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        if (Input.GetMouseButtonDown(3))
        {
            ResetOrthographicSize();
            return;
        }

        if (Input.mouseScrollDelta.y == 0)
            return;

        UpdateOrthographicSize(Mathf.Clamp(targetOrthographicSize - (float)Input.mouseScrollDelta.y * zoomMultiplier, cameraSizeMin, cameraSizeMax));
    }

    private void HandleLeftClick()
    {
        if (!inDoubleClickRange)
        {
            inDoubleClickRange = true;
            IEnumerator doubleClickCoroutine = DoubleClickRange();
            StartCoroutine(doubleClickCoroutine);
        }
        else
        {
            SelectFocusTarget();
            inDoubleClickRange = false;
        }
    }

    private void UpdateOrthographicSize(float newOrthoSize)
    {
        if (newOrthoSize == targetOrthographicSize)
            return;
        targetOrthographicSize = newOrthoSize;
        OrthographicSizeChangeEvent(cameraSizeMin, cameraSizeMax, targetOrthographicSize); // Invoke delegate
    }

    private IEnumerator DoubleClickRange()
    {
        // Waits until interval expires, then sets bool back to false   
        yield return new WaitForSeconds(doubleClickSelectInterval);
        inDoubleClickRange = false;
    }

    private void SelectFocusTarget()
    {
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(mainCamera.ScreenPointToRay(Input.mousePosition));
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit2D hit = rayHits[i];
            ICameraTrackable cameraTrackable = hit.collider.GetComponent<ICameraTrackable>();
            if (hit.collider.isTrigger || cameraTrackable == null)
            {
                continue;
            }
            currentTarget = hit.collider.gameObject;
        }
    }

    private void ResetOrthographicSize()
    {
        UpdateOrthographicSize(defaultOrthographicSize);
    }
}
