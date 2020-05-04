using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Ship))]
public abstract class InputHandler : MonoBehaviour
{
    protected Ship ship;
    private bool isActive = false;

    #region GETSET

    #endregion GETSET

    #region EVENTS
    public delegate void ToggleStabilityAssistUI();
    public static event ToggleStabilityAssistUI ToggleStabilityAssistUIEvent;

    #endregion
    private void Awake()
    {
        ship = GetComponent<Ship>();
        ObjectSelector.OnObjectSelectionEvent += ObjectSelectionChanged;
    }

    private void OnDisable()
    {
        ObjectSelector.OnObjectSelectionEvent -= ObjectSelectionChanged;
    }
    
    void Update()
    {
        if (!isActive)
            return; // this object is not actively selected

        HandleInputs();
    }

    protected virtual void HandleInputs()
    {
        // Override this method to inplement input listening
    }

    protected void InvokeToggleStabilityAssistEvent()
    {
        // Awkwardly have to nest this in this protected function to invoke from subclasses
        ToggleStabilityAssistUIEvent();
    }

    private void ObjectSelectionChanged(GameObject selectedObject)
    {
        Ship shipComponent = selectedObject.GetComponent<Ship>(); // if null, should be fine too
        isActive = (shipComponent == ship);
    }
}
