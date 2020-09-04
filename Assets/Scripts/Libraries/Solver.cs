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
    private float _trueAnomaly;

    // General Orbital Parameters
    protected float sourceMass;

    #region GETSET
    public Trajectory Trajectory { get; private set; }
    public Vector2 CalculatedPosition
    {
        get { return _calculatedPosition; }
        protected set 
        {
            _calculatedPosition = value;
        }
    }

    public float CalculatedRadius
    {
        get { return _calculatedRadius; }
        protected set 
        {
            _calculatedRadius = value; 
        }
    }

    public float CalculatedSpeed
    {
        get { return _calculatedSpeed; }
        protected set 
        {
            _calculatedSpeed = value; 
        }
    }

    public Vector2 CalculatedVelocity
    {
        get { return _calculatedVelocity; }
        protected set 
        {
            _calculatedVelocity = value; 
        }
    }


    public float FlightPathAngle
    {
        get { return _flightPathAngle; }
        protected set { _flightPathAngle = value; }
    }

    public float TrueAnomaly
    {
        get { return _trueAnomaly; }
        protected set { _trueAnomaly = value; }
    }
    #endregion GETSET


    public virtual void InitializeSolver(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float _sourceMass, Trajectory trajectory)
    {
        sourceMass = _sourceMass;
        Trajectory = trajectory;
    }
}
