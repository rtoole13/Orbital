using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(SpriteRenderer))]
public class ManeuverVectorHandler : MonoBehaviour
{
    private Collider2D clickCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 selectionInitialPosition;

    public delegate void SelectedVector();
    public event SelectedVector selectedVectorEvent;

    #region UNITY
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
    }
    #endregion UNITY
    #region GENERAL
    public void HideSprite()
    {
        spriteRenderer.enabled = false;
    }

    public void ShowSprite()
    {
        spriteRenderer.enabled = true;
    }
    
    public void InitializeVectorSelect(Vector2 initialPosition)
    {
        animator.SetBool("extended", true);
        selectionInitialPosition = initialPosition;
    }

    public void EndVectorSelect()
    {
        animator.SetBool("extended", false);
    }

    public void DragVector(Vector2 mousePosition)
    {
        Debug.Log("Dragging");
    }
    
    #endregion GENERAL

}
