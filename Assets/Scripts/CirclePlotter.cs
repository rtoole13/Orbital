using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//[ExecuteAlways]
public class CirclePlotter : LinePlotter
{
    #region CUSTOMEDITOR
    [HideInInspector]
    public bool useCustomRadius = false;

    [HideInInspector]
    public float customRadius = 0.5f;
    #endregion CUSTOMEDITOR

    private float radius;
    #region UNITY
    protected override void Awake()
    {
        base.Awake();
        if (useCustomRadius)
        {
            radius = customRadius;
        }
        else
        {
            radius = 0.5f; // FIXME only checking x scale. Should make a more general ellipse plotter perhaps.
        }
    }

    //protected void Update()
    //{
    //    if (useCustomRadius)
    //    {
    //        radius = customRadius;
    //    }
    //    else
    //    {
    //        radius = 0.5f;
    //    }
    //    DrawCircle();
    //}

    #endregion UNITY
    
    private void DrawCircle()
    {
        lineRenderer.useWorldSpace = false; // Important for having SOI follow object
        if (radius == Mathf.Infinity || radius == 0)
            return;

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 2 * Mathf.PI;
            points[i] = new Vector3(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius, zepth);
        }
        points[segments] = points[0];
        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(points);
    }

    public void SetGradient(Gradient gradient)
    {
        lineRenderer.colorGradient = gradient;
    }

}

[CustomEditor(typeof(CirclePlotter))]
public class CirclePlotterEditor : Editor
{
    SerializedProperty useCustomRadius;
    SerializedProperty customRadius;

    private void OnEnable()
    {
        useCustomRadius = serializedObject.FindProperty("useCustomRadius");
        customRadius = serializedObject.FindProperty("customRadius");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(useCustomRadius, new GUIContent("Use Custom Radius"));
        
        if (useCustomRadius.boolValue)
            EditorGUILayout.PropertyField(customRadius, new GUIContent("Custom Radius"));

        serializedObject.ApplyModifiedProperties();
    }
}
