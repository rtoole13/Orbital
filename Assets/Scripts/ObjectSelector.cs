using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelector : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private GameObject _selectedObject;

    public delegate void OnObjectSelection(GameObject selectedObject);
    public event OnObjectSelection OnObjectSelectionEvent;

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
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectTarget();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTarget();
        }
    }
    #endregion UNITY

    private void SelectTarget()
    {
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(mainCamera.ScreenPointToRay(Input.mousePosition));
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit2D hit = rayHits[i];
            ISelectable selectable = hit.collider.GetComponent<ISelectable>();
            if (hit.collider.isTrigger || selectable == null)
            {
                continue;
            }
            SelectedObject = hit.collider.gameObject;
            OnObjectSelectionEvent(SelectedObject);
            return;
        }
    }

    private void DeselectTarget()
    {
        // Deselect if anything selected
        SelectedObject = null;
        OnObjectSelectionEvent(SelectedObject);
        // Any other cleanup logic
    }
}
