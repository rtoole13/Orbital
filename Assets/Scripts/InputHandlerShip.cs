using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(ObjectSelector))]
public sealed class InputHandlerShip : MonoBehaviour
{

    private ObjectSelector objectSelector;
    private Ship selectedShip;

    [SerializeField]
    private GameObject stabilityAssistUI;
    private TMP_Dropdown stabilityAssistDropdown;

    // Singleton style
    private static InputHandlerShip _instance;

    #region GETSET
    public static InputHandlerShip Instance
    {
        get
        {
            if (!_instance)
            {
                InputHandlerShip[] handlers = FindObjectsOfType<InputHandlerShip>();
                if (handlers.Length > 1)
                {
                    string handlerParentObjects = handlers[0].name;
                    for (int i = 1; i < handlers.Length; i++)
                    {
                        handlerParentObjects = string.Format("{0}, {1}", handlerParentObjects, handlers[i].name);
                    }
                    throw new UnityException(string.Format("{0} all have InputHandlerShips in the scene! Must have just one.", handlerParentObjects));
                }
                else if (handlers.Length == 0)
                {
                    Debug.Log("No InputHandlerShip found in the scene! Add a InputHandlerShip to a game object to control ships.");
                    return null;
                }
                _instance = handlers[0];
            }
            return _instance;
        }
    }
    #endregion GETSET

    private void Awake()
    {
        objectSelector.OnObjectSelectionEvent += ObjectSelectionChanged;
        stabilityAssistDropdown = stabilityAssistUI.GetComponentInChildren<TMP_Dropdown>();
        stabilityAssistDropdown.onValueChanged.AddListener(ChangeStabilityAssistMode);
    }

    private void OnDisable()
    {
        objectSelector.OnObjectSelectionEvent -= ObjectSelectionChanged;
        stabilityAssistDropdown.onValueChanged.RemoveListener(ChangeStabilityAssistMode);
    }
    private void Start()
    {
        
    }

    void Update()
    {
        if (selectedShip == null) // Selected object isn't a controllable ship
            return;

        // Rotation
        if (Input.GetKey(KeyCode.A))
        {
            // Rotate CCW
            selectedShip.DecrementRotationRate();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Rotate CW
            selectedShip.IncrementRotationRate();
        }

        // Stability Assist
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Toggle stability assist on ship
            selectedShip.ToggleStabilityAssist();

            // Toggle stability assist UI
            ToggleStabilityAssistUI();
        }

        // Thrust Controls
        if (Input.GetKey(KeyCode.X))
        {
            // Cut thrust
        }

        if (Input.GetKey(KeyCode.Z))
        {
            // Throttle to max thrust
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            // Throttle up
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            // Throttle down
        }
    }

    private void ToggleStabilityAssistUI()
    {
        stabilityAssistDropdown.value = (int)ShipSystems.StabilityAssistMode.Hold;
        stabilityAssistUI.SetActive(selectedShip.StabilityAssistEnabled);
    }

    private void ChangeStabilityAssistMode(int newValue)
    {
        selectedShip.ChangeStabilityAssistMode(newValue);
    }


    private void ObjectSelectionChanged(GameObject selectedObject)
    {
        Ship selected = selectedObject.GetComponent<Ship>();
        selectedShip = selected;
    }
}
