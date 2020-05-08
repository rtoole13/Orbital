using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(SpriteRenderer))]
public class ManeuverVectorHandler : MonoBehaviour
{
    private Collider2D clickCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private int selectionLayerMask; //FIXME: make an enum for editor.
    
    private void Awake()
    {
        clickCollider = GetComponent<Collider2D>();
        if (clickCollider == null || !clickCollider.isTrigger)
            throw new UnityException(string.Format("Expecting a Collider2D (isTrigger) of some sort on {0}'s ManeuverVectorHandler", gameObject.name));
        
        animator = GetComponent<Animator>();
        if (animator == null)
            throw new UnityException(string.Format("Expecting {0} to have an Animator!", gameObject.name));

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            throw new UnityException(string.Format("Expecting {0} to have a SpriteRenderer!", gameObject.name));

        mainCamera = Camera.main;
        selectionLayerMask = LayerMask.GetMask("ManeuverVectorSelection");
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectVector();
        }
    }
    void SelectVector()
    {
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(mainCamera.ScreenPointToRay(Input.mousePosition), 100f, selectionLayerMask);
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit2D hit = rayHits[i];
            Debug.LogFormat("{0}", hit.transform.name);
            
        }
    }
}
