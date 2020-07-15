using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Solver
{
    private Vector2 _calculatedPosition;
    private float _calculatedRadius;
    private float _calculatedSpeed;
    private Vector2 _calculatedVelocity;
    private float _flightPathAngle;
    private Vector2 _lastPosition;
    private float _lastRadius;
    private float _lastSpeed;
    private Vector2 _lastVelocity;
    private float _trueAnomaly;

    #region GETSET
    public Vector2 CalculatedPosition
    {
        get { return _calculatedPosition; }
        protected set 
        {
            _lastPosition = value;
            _calculatedPosition = value;
        }
    }

    public float CalculatedRadius
    {
        get { return _calculatedRadius; }
        protected set 
        {
            _lastRadius = _calculatedRadius;
            _calculatedRadius = value; 
        }
    }

    public float CalculatedSpeed
    {
        get { return _calculatedSpeed; }
        protected set 
        {
            _lastSpeed = _calculatedSpeed;
            _calculatedSpeed = value; 
        }
    }

    public Vector2 CalculatedVelocity
    {
        get { return _calculatedVelocity; }
        protected set 
        {
            _lastVelocity = _calculatedVelocity;
            _calculatedVelocity = value; 
        }
    }

    public float FlightPathAngle
    {
        get { return _flightPathAngle; }
        protected set { _flightPathAngle = value; }
    }

    public Vector2 LastPosition { get { return _lastPosition; } }
    public float LastRadius { get { return _lastRadius; } }
    public float LastSpeed { get { return _lastSpeed; } }
    public Vector2 LastVelocity { get { return _lastVelocity; } }

    public float TrueAnomaly
    {
        get { return _trueAnomaly; }
        protected set { _trueAnomaly = value; }
    }

    

    #endregion GETSET
}
