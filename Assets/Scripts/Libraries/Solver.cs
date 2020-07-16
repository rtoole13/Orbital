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

    // Orbital Parameters
    protected float sourceMass;
    protected Vector3 specificRelativeAngularMomentum;
    protected bool clockWiseOrbit;
    protected Vector3 _eccentricityVector;
    protected float eccentricity;
    protected float semimajorAxis;
    protected float semimajorAxisReciprocal;
    protected OrbitalMechanics.Globals.TrajectoryType trajectoryType;


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

    protected Vector3 EccentricityVector
    {
        get { return _eccentricityVector; }
        set
        {
            _eccentricityVector = value;
            eccentricity = _eccentricityVector.magnitude;
            if (eccentricity == 1f)
            {
                trajectoryType = OrbitalMechanics.Globals.TrajectoryType.Parabola;
            }
            else if (eccentricity < 1f)
            {
                trajectoryType = OrbitalMechanics.Globals.TrajectoryType.Ellipse;
            }
            else
            {
                trajectoryType = OrbitalMechanics.Globals.TrajectoryType.Hyperbola;
            }
        }
    }
    #endregion GETSET

    public virtual void InitializeSolver(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float _sourceMass)
    {
        sourceMass = _sourceMass;
        specificRelativeAngularMomentum = OrbitalMechanics.Trajectory.SpecificRelativeAngularMomentum(sourceRelativePosition, sourceRelativeVelocity);
        clockWiseOrbit = specificRelativeAngularMomentum.z < 0;
        EccentricityVector = OrbitalMechanics.Trajectory.EccentricityVector(sourceRelativePosition, sourceRelativeVelocity, specificRelativeAngularMomentum, sourceMass);
        semimajorAxis = OrbitalMechanics.Trajectory.SemimajorAxis(sourceRelativePosition.magnitude, sourceRelativeVelocity.sqrMagnitude, sourceMass);
        semimajorAxisReciprocal = 1f / semimajorAxis;
    }

}
