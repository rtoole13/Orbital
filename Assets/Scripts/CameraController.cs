﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Range(1, 4)]
    private float zoomMultiplier = 4f;

    [SerializeField]
    [Range(0.0f, 1f)]
    private float moveSmoothTime = 0.5f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float zoomSmoothTime = 0.15f;

    public static float cameraSizeMin = 5f;
    public static float cameraSizeMax = 100f;

    [HideInInspector]
    public static float cameraSizeTarget;

    private float defaultOrthographicSize;

    [SerializeField]
    private GameObject currentTarget; // Make this an interface, ICameraTrackable

    [SerializeField]
    private float doubleClickSelectInterval = .5f;

    private Vector3 moveVelocity = Vector3.zero;
    private float zoomVelocity = 0f;
    private float firstClickTime;
    private bool inDoubleClickRange = false;
    private Camera mainCamera;

    public delegate void OnOrthographicSizeChangeDelegate();
    public static event OnOrthographicSizeChangeDelegate OrthographicSizeChangeEvent;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        cameraSizeTarget = defaultOrthographicSize = mainCamera.orthographicSize;
    }

    private void Start()
    {
        OrthographicSizeChangeEvent(); // Invoke delegate
    }
    void Update()
    {
        HandleMouseInputs();
        Vector3 target = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z);
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, target, ref moveVelocity, moveSmoothTime);
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, cameraSizeTarget, ref zoomVelocity, zoomSmoothTime);
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

        UpdateOrthographicSize(Mathf.Clamp(cameraSizeTarget - (float)Input.mouseScrollDelta.y * zoomMultiplier, cameraSizeMin, cameraSizeMax));
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
        if (newOrthoSize == cameraSizeTarget)
            return;
        cameraSizeTarget = newOrthoSize;
        OrthographicSizeChangeEvent(); // Invoke delegate
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
