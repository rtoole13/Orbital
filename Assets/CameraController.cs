using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;


    [SerializeField]
    private float cameraSizeMin = 5f;

    [SerializeField]
    private float cameraSizeMax = 100f;

    private float currentCameraSizeTarget;

    [SerializeField]
    private GameObject currentTarget; // Make this an interface, ICameraTrackable

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        currentCameraSizeTarget = mainCamera.orthographicSize;
        Debug.Log(currentCameraSizeTarget);
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 target = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, target, 0.075f);
    }
}
