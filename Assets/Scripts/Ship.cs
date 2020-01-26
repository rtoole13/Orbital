using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityAffected
{
    public float clickForce = 1f;
    public void OnMouseDown()
    {
        

    }

    public void Update()
    {
        if (Input.GetMouseButton(0))
            AddClickForce();
        Debug.Log("Semimajor: " + calculateSemimajorAxis());
        //Debug.Log("Semiminor: " + calculateSemimajorAxis());
    }

    private void AddClickForce()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 distance = mousePosition - (Vector2)rigidbody.transform.position;
        rigidbody.AddForce(clickForce * distance.normalized);
    }
}


