using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class StabilityAssistDropdownHandler : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    public delegate void OnStabilityAssistDropdownChange(int newValue);
    public static event OnStabilityAssistDropdownChange ValueChangedEvent;
    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(ValueChanged);
    }

    private void OnDisable()
    {
        dropdown.onValueChanged.RemoveListener(ValueChanged);
    }

    private void ValueChanged(int newVal)
    {
        //FIXME: No reason to have an event kick off an event.. this is a recipe for disaster
        // Need to look into how Unity EventSystems work..
        ValueChangedEvent(newVal);
    }

}
