using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(SpriteRenderer))]
public class ManeuverVectorHandler : MonoBehaviour
{
    private Collider2D clickCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 worldDirection = Vector2.up;
    private Vector2 selectionInitialPosition;
    private float dragDeltaVelFactor = 0.01f; // Used to dampen the user's drag input
    private Color defaultColor;
    public delegate void deltaVelocityAdjusted(float deltaVelocityMag);
    public event deltaVelocityAdjusted DeltaVelocityAdjustedEvent;

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

        defaultColor = spriteRenderer.color;
    }
    #endregion UNITY
    #region GENERAL
    public void HideVector()
    {
        spriteRenderer.enabled = false;
        clickCollider.enabled = false;
    }

    public void ShowVector()
    {
        spriteRenderer.enabled = true;
        clickCollider.enabled = true;
    }
    
    public void UpdateDirection(Vector2 newWorldDirection)
    {
        worldDirection = newWorldDirection;
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

    public void SetExecuting(bool executing, Color executionColor)
    {
        animator.SetBool("executing", executing);
        spriteRenderer.color = executing
            ? executionColor
            : defaultColor;
    }

    public void DragVector(Vector2 mouseWorldPosition)
    {
        Vector2 dragVector = mouseWorldPosition - selectionInitialPosition;
        if (Vector2.Dot(dragVector, worldDirection) <= 0) // If dragging in wrong direction, ignore
            return;

        dragVector = dragVector.Projection(worldDirection);

        // Invoke deltaV adjustment event
        DeltaVelocityAdjustedEvent(Mathf.Max(0f, dragVector.magnitude * dragDeltaVelFactor));
    }

    #endregion GENERAL
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawLine(transform.position, transform.position + (Vector3)worldDirection * 2f);
    //}
}
