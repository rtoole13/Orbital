using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithCamera : MonoBehaviour
{
    [SerializeField]
    private Vector2 scaleRange = new Vector2(0.5f, 5f);
    private float currentScale;
    #region UNITY
    private void Awake()
    {
        CameraController.OrthographicSizeChangeEvent += AdjustScale;
    }

    private void Start()
    {
        AdjustScale();
    }

    private void OnDisable()
    {
        CameraController.OrthographicSizeChangeEvent -= AdjustScale;
    }
    #endregion UNITY

    private void AdjustScale()
    {
        float newScale = MathUtilities.RescaleFloat(CameraController.cameraSizeTarget, CameraController.cameraSizeMin, CameraController.cameraSizeMax, scaleRange[0], scaleRange[1]);
        transform.localScale = new Vector3(newScale, newScale, 1f);
    }
}
