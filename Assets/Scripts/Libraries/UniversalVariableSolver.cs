using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalVariableSolver : Solver
{
    private float x;
    private float z;
    private float stumpffC;
    private float stumpffS;
    private float f;
    private float g;
    private float fPrime;
    private float gPrime;
    
    public UniversalVariableSolver()
    {
        x = 0f;
        z = 0f;
        stumpffC = 0.5f;
        stumpffS = 1f / 6f;
        f = 1f;
        g = 0f;
        fPrime = 0f;
        g = 1f;
    }

    public override void InitializeSolver(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity, float _sourceMass, Vector3 _specificRelativeAngularMomentum, Vector3 eccentricityVector, float _semimajorAxis)
    {
        // Initialize orbital parameters
        base.InitializeSolver(sourceRelativePosition, sourceRelativeVelocity, _sourceMass, _specificRelativeAngularMomentum, eccentricityVector, _semimajorAxis);

        // Initialize calculated variables
        InitializeVariables(sourceRelativePosition, sourceRelativeVelocity);
    }

    private void InitializeVariables(Vector3 sourceRelativePosition, Vector3 sourceRelativeVelocity)
    {

        float argumentOfPeriapsis = OrbitalMechanics.Trajectory.ArgumentOfPeriapse(EccentricityVector, sourceRelativePosition);
        
        // Calculate Position
        Vector2 calculatedPosition = (Vector2)sourceRelativePosition;
        CalculatedRadius = sourceRelativePosition.magnitude;
        CalculatedPosition = calculatedPosition.RotateVector(-argumentOfPeriapsis);

        // Calculate velocity
        Vector2 calculatedVelocity = (Vector2)sourceRelativeVelocity;
        CalculatedSpeed = sourceRelativeVelocity.magnitude;
        CalculatedVelocity = calculatedVelocity.RotateVector(-argumentOfPeriapsis);

        // UVM makes use of last iterations position and velocity
        LastRadius = CalculatedRadius;
        LastPosition = CalculatedPosition;
        LastSpeed = CalculatedSpeed;
        LastVelocity = CalculatedVelocity;

        // Initialize true anomaly and FPA
        TrueAnomaly = OrbitalMechanics.Trajectory.TrueAnomaly(CalculatedPosition, CalculatedVelocity, EccentricityVector);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(specificRelativeAngularMomentum.magnitude, CalculatedPosition, CalculatedVelocity);
    }

    public void UpdateStateVariables(float timeOfFlight)
    {
        // UVM makes use of last iterations position and velocity
        SetLastStateVariables(CalculatedRadius, CalculatedSpeed, CalculatedPosition, CalculatedVelocity);

        // Update the universal variable x
        OrbitalMechanics.UniversalVariableMethod.UniversalVariable(ref x, timeOfFlight, sourceMass, semimajorAxis, LastRadius, LastPosition, LastVelocity);
        
        // Update z, c, and s based off of latest x
        z = Mathf.Pow(x, 2) / semimajorAxis;
        stumpffS = OrbitalMechanics.UniversalVariableMethod.StumpffS(x, semimajorAxis);
        stumpffC = OrbitalMechanics.UniversalVariableMethod.StumpffC(z);

        // Get f, g, and update calculated position
        f = OrbitalMechanics.UniversalVariableMethod.VariableF(x, LastRadius, stumpffC);
        g = OrbitalMechanics.UniversalVariableMethod.VariableG(timeOfFlight, x, sourceMass, stumpffS);
        CalculatedPosition = OrbitalMechanics.UniversalVariableMethod.OrbitalPosition(f, g, LastPosition, LastVelocity);
        CalculatedRadius = CalculatedPosition.magnitude;
        
        // Get fPrime, gPrime, and update calculated velocity
        fPrime = OrbitalMechanics.UniversalVariableMethod.VariableFprime(sourceMass, LastRadius, CalculatedRadius, x, z, stumpffS);
        gPrime = OrbitalMechanics.UniversalVariableMethod.VariableGprime(x, CalculatedRadius, stumpffC);
        CalculatedVelocity = OrbitalMechanics.UniversalVariableMethod.OrbitalVelocity(fPrime, gPrime, LastPosition, LastVelocity);

        CalculatedSpeed = CalculatedVelocity.magnitude;

        // Update true anomaly and flight path angle
        TrueAnomaly = OrbitalMechanics.Trajectory.TrueAnomaly(CalculatedPosition, CalculatedVelocity, EccentricityVector);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(specificRelativeAngularMomentum.magnitude, CalculatedPosition, CalculatedVelocity);
    }

    private void SetLastStateVariables(float radius, float speed, Vector2 position, Vector2 velocity)
    {
        LastRadius = radius;
        LastSpeed = speed;
        LastPosition = position;
        LastVelocity = velocity;
    }
}
