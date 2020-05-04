using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ObjectSelector : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private int selectionLayerMask; //FIXME: make an enum for editor.

    private GameObject _selectedObject;
    private Ship selectedShip;

    [SerializeField]
    private GameObject stabilityAssistUI;
    private TMP_Dropdown stabilityAssistDropdown;


    public delegate void OnObjectSelection(GameObject selectedObject);
    public static event OnObjectSelection OnObjectSelectionEvent;

    // Singleton style
    private static ObjectSelector _instance;

    #region GETSET
    public static ObjectSelector Instance
    {
        get
        {
            if (!_instance)
            {
                ObjectSelector[] handlers = FindObjectsOfType<ObjectSelector>();
                if (handlers.Length > 1)
                {
                    string handlerParentObjects = handlers[0].name;
                    for (int i = 1; i < handlers.Length; i++)
                    {
                        handlerParentObjects = string.Format("{0}, {1}", handlerParentObjects, handlers[i].name);
                    }
                    throw new UnityException(string.Format("{0} all have ObjectSelectors in the scene! Must have just one.", handlerParentObjects));
                }
                else if (handlers.Length == 0)
                {
                    Debug.Log("No ObjectSelector found in the scene! Add an ObjectSelector to a game object to control ships.");
                    return null;
                }
                _instance = handlers[0];
            }
            return _instance;
        }
    }

    public GameObject SelectedObject
    {
        get { return _selectedObject; }
        private set { _selectedObject = value; }
    }

    #endregion GETSET

    #region UNITY
    private void Awake()
    {
        if (mainCamera == null)
        {
            throw new UnityException(string.Format("For ObjectSelector on {0} to work, reference a camera in the scene!", name));
        }
        // Listen to UI
        stabilityAssistDropdown = stabilityAssistUI.GetComponentInChildren<TMP_Dropdown>();
        stabilityAssistDropdown.onValueChanged.AddListener(ChangeStabilityAssistMode);

        // Listen to InputHandler
        InputHandler.ToggleStabilityAssistUIEvent += ToggleStabilityAssistUI;

        selectionLayerMask = LayerMask.GetMask("ObjectSelection");
        //Debug.Log(selectionLayerMask);
    }

    private void OnDisable()
    {
        // Stop listening to UI
        stabilityAssistDropdown.onValueChanged.RemoveListener(ChangeStabilityAssistMode);

        // Stop listening to InputHandler
        InputHandler.ToggleStabilityAssistUIEvent -= ToggleStabilityAssistUI;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SearchForNewTarget();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTarget();
        }
    }
    #endregion UNITY

    private void SearchForNewTarget()
    {
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(mainCamera.ScreenPointToRay(Input.mousePosition), 100f, selectionLayerMask);
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit2D hit = rayHits[i];
            
            ISelectable selectable = hit.collider.GetComponentInParent<ISelectable>();
            if (hit.collider.isTrigger || selectable == null)
            {
                continue;
            }
            SelectTarget(hit.collider.transform.parent.gameObject); // Well this is gross
            
            return;
        }
    }

    private void SelectTarget(GameObject newTarget)
    {
        Debug.LogFormat("Selected {0}", newTarget.name);
        SelectedObject = newTarget;
        OnObjectSelectionEvent(SelectedObject);
        selectedShip = SelectedObject.GetComponent<Ship>(); // Null if not ship
        if (selectedShip != null)
        {
            stabilityAssistDropdown.value = (int)selectedShip.StabilityAssistMode;
            stabilityAssistUI.SetActive(selectedShip.StabilityAssistEnabled);
        }
    }

    private void DeselectTarget()
    {
        // Cleanup
        selectedShip = null;
        stabilityAssistUI.SetActive(false);
        
        // Deselect if anything selected
        SelectedObject = null;
        OnObjectSelectionEvent(SelectedObject);   
    }

    #region FROMUI
    private void ChangeStabilityAssistMode(int newValue)
    {
        if (selectedShip != null)
            selectedShip.ChangeStabilityAssistMode(newValue);

    }
    #endregion FROMUI

    #region FROMINPUTMODEL

    private void ToggleStabilityAssistUI()
    {
        stabilityAssistDropdown.value = (int)ShipSystems.StabilityAssistMode.Hold;
        stabilityAssistUI.SetActive(selectedShip.StabilityAssistEnabled);
    }
    #endregion FROMINPUTMODEL
}
