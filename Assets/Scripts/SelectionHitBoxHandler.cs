using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SelectionHitBoxHandler : MonoBehaviour, ISelectable
{
    [Range(-5f,5f)]
    public float zepth = 1f;

    private SpriteRenderer spriteRenderer;

    #region UNITY
    private void Awake()
    {
        spriteRenderer = GetSpriteRenderer();
        transform.position = new Vector3(transform.position.x, transform.position.y, zepth);
    }

    public void OnValidate()
    {
        int targetMask = LayerMask.GetMask("ObjectSelection");
        if (1 << gameObject.layer != targetMask)
        {
            throw new UnityException(string.Format("For {0} to implement ISelectable, it must be on layer 'ObjectSelection' with some type of Collider2D!", name));
        }

        SpriteRenderer hitBoxSprite = GetComponent<SpriteRenderer>();
        if (hitBoxSprite == null)
            hitBoxSprite = GetComponentInChildren<SpriteRenderer>();

        if (hitBoxSprite == null)
        {
            throw new UnityException(string.Format("For {0} to implement ISelectable, it must have a SpriteRenderer or have a child object with a SpriteRenderer!", name));
        }
    }

    #endregion UNITY

    private SpriteRenderer GetSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        return spriteRenderer;
    }

    public void ToggleSelectionSprite(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }

    public GameObject GetRootObject()
    {
        return transform.root.gameObject;
    }

    public SelectionHitBoxHandler GetHitBoxHandler()
    {
        return this;
    }
}
