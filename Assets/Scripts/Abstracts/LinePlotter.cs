using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public abstract class LinePlotter : MonoBehaviour
{
    [SerializeField]
    protected float zepth = 1f;
    protected LineRenderer lineRenderer;

    [SerializeField]
    private float minimumWidth = 0.25f;

    [SerializeField]
    private float maximumWidth = 1f;
    private float currentLineWidth;

    [Range(3, 64)]
    public int segments;

    #region UNITY
    protected virtual void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        CameraController.OrthographicSizeChangeEvent += AdjustLineThickness;
    }

    protected virtual void OnDisable()
    {
        CameraController.OrthographicSizeChangeEvent -= AdjustLineThickness;
    }
    #endregion UNITY

    protected Vector3 RotateVertex(Vector3 vertex, float angle)
    {
        return new Vector3(vertex.x * Mathf.Cos(angle) - vertex.y * Mathf.Sin(angle),
                       vertex.x * Mathf.Sin(angle) + vertex.y * Mathf.Cos(angle), zepth);
    }

    protected Vector3 TranslateVector(Vector3 vertex, Vector3 distance)
    {
        return new Vector3(vertex.x + distance.x, vertex.y + distance.y, zepth);
    }

    private void AdjustLineThickness(float minOrthoSize, float maxOrthoSize, float targetOrthoSize)
    {
        float newLineWidth = MathUtilities.RescaleFloat(targetOrthoSize, minOrthoSize, maxOrthoSize, minimumWidth, maximumWidth);
        lineRenderer.startWidth = lineRenderer.endWidth = newLineWidth;
    }


    public void SetDisplay(bool display)
    {
        lineRenderer.enabled = display;
    }

}
