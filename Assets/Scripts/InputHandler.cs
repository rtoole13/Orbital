using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class InputHandler : MonoBehaviour
{
    // Singleton style
    private static InputHandler instance;
    public static InputHandler Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<InputHandler>();
                if (!instance)
                    throw new UnityException(string.Format("No input handler found in the scene! Add an InputHandler to a game object."));
            }
            return instance;
        }
    }

    public delegate void OnLeftClick();
    public static event OnLeftClick OnLeftClickEvent;

    private void Awake()
    {
        instance = new InputHandler();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
            

        //if (Input.GetMouseButtonDown(3))
        //{
        //    ResetOrthographicSize();
        //    return;
        //}

        //if (Input.mouseScrollDelta.y == 0)
        //    return;
    }
}
