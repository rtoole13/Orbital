using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSpriteController : MonoBehaviour
{
    [SerializeField]
    private Vector2 scaleRange = new Vector2(0.5f, 5f);
    private float currentScale;
    #region UNITY
    private void Awake()
    {
        CameraController.OrthographicSizeChangeEvent += AdjustScale;
    }

    private void OnDisable()
    {
        CameraController.OrthographicSizeChangeEvent -= AdjustScale;
    }
    #endregion UNITY

    private void AdjustScale(float minOrthoSize, float maxOrthoSize, float targetOrthoSize)
    {
        float newScale = MathUtilities.RescaleFloat(targetOrthoSize, minOrthoSize, maxOrthoSize, scaleRange[0], scaleRange[1]);
        transform.localScale = new Vector3(newScale, newScale, 1f);
    }
}
