using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planetoid : GravitySource, ISelectable, ICameraTrackable
{
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
            if (1 << colliders[i].gameObject.layer == targetMask)
            {
                valid = true;
                break;
            }
        }
        if (!valid)
        {
            throw new UnityException(string.Format("For {0} to implement ISelectable, there must be a child GameObject on layer 'ObjectSelection' with some type of collider!", name));
        }
    }
}
