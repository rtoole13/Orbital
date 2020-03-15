using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityAffected, ICameraTrackable
{
    public float clickForce = 1f;

    #region UNITY
    /*
    private void Start()
    {
        if (CurrentGravitySource != null)
        {
            body.velocity = startVelocity + CurrentGravitySource.startVelocity;
            return;
        }
        body.velocity = startVelocity;
    }
    */
    protected override void Update()
    {
        if (Input.GetMouseButton(0))
            AddClickForce();
        base.Update();

        
    }
    #endregion UNITY

    private void AddClickForce()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 distance = mousePosition - (Vector2)rigidbody.transform.position;
        AddExternalForce(clickForce * distance.normalized);
    }
}


