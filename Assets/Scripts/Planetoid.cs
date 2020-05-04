using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planetoid : GravitySource, ISelectable, ICameraTrackable
{
    #region UNITY
    protected override void Awake()
    {
        base.Awake();
    }

    public void OnValidate()
    {
        bool valid = false;
        int targetMask = LayerMask.GetMask("ObjectSelection");
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject hitBoxObject = colliders[i].gameObject;
            if (1 << hitBoxObject.layer != targetMask)
            {
                continue;
            }
            SpriteRenderer hitBoxSprite = hitBoxObject.GetComponent<SpriteRenderer>();
            if (hitBoxSprite == null)
                hitBoxSprite = hitBoxObject.GetComponentInChildren<SpriteRenderer>();


            if (hitBoxSprite == null)
            {
                // SpriteRenderer not on object or child
                continue;
            }
            valid = true;
        }
        if (!valid)
        {
            throw new UnityException(string.Format("For {0} to implement ISelectable, there must be a child GameObject on layer 'ObjectSelection' with some type of collider, and a SpriteRenderer on it or on a child!", name));
        }
    }
    
    #endregion UNITY

    public void ToggleSelectionSprite()
    {
        throw new System.NotImplementedException();
    }
}
