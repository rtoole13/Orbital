using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Solver
{
    private Vector2 _calculatedPosition;
    private float _calculatedRadius;
    private float _calculatedSpeed;
    private Vector2 _calculatedVelocity;

    #region GETSET
    public Vector2 CalculatedPosition
    {
        get { return _calculatedPosition; }
        protected set { _calculatedPosition = value; }
    }

    public float CalculatedRadius
    {
        get { return _calculatedRadius; }
        protected set { _calculatedRadius = value; }
    }

    public float CalculatedSpeed
    {
        get { return _calculatedSpeed; }
        protected set { _calculatedSpeed = value; }
    }

    public Vector2 CalculatedVelocity
    {
        get { return _calculatedVelocity; }
        protected set { _calculatedVelocity = value; }
    }

    #endregion GETSET
}
