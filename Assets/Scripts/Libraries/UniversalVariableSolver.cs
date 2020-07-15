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
    
    public UniversalVariableSolver(float _x = 0f)
    {
        x = _x;
        z = 0f;
        stumpffC = 0.5f;
        stumpffS = 1f / 6f;
        f = 1f;
        g = 0f;
        fPrime = 0f;
        g = 1f;
    }
    
    public void UpdateStateVariables(float timeOfFlight, float mainMass, float semimajorAxis, Vector2 orbitalPosition, Vector2 orbitalVelocity, Vector3 eccentricityVector, Vector3 specificRelativeAngularMomentum)
    {
        float orbitalRadius = orbitalPosition.magnitude;
        // Update the universal variable x
        OrbitalMechanics.UniversalVariableMethod.UniversalVariable(ref x, timeOfFlight, mainMass, semimajorAxis, orbitalRadius, orbitalPosition, orbitalVelocity);

        // Update z, c, and s based off of latest x
        z = Mathf.Pow(x, 2) / semimajorAxis;
        stumpffS = OrbitalMechanics.UniversalVariableMethod.StumpffS(x, semimajorAxis);
        stumpffC = OrbitalMechanics.UniversalVariableMethod.StumpffC(z);

        // Get f, g, and update calculated position
        f = OrbitalMechanics.UniversalVariableMethod.VariableF(x, orbitalRadius, stumpffC);
        g = OrbitalMechanics.UniversalVariableMethod.VariableG(timeOfFlight, x, mainMass, stumpffS);
        CalculatedPosition = OrbitalMechanics.UniversalVariableMethod.OrbitalPosition(f, g, orbitalPosition, orbitalVelocity);
        CalculatedRadius = CalculatedPosition.magnitude;

        // Get fPrime, gPrime, and update calculated velocity
        fPrime = OrbitalMechanics.UniversalVariableMethod.VariableFprime(mainMass, orbitalRadius, CalculatedRadius, x, z, stumpffS);
        gPrime = OrbitalMechanics.UniversalVariableMethod.VariableGprime(x, CalculatedRadius, stumpffC);
        CalculatedVelocity = OrbitalMechanics.UniversalVariableMethod.OrbitalVelocity(fPrime, gPrime, orbitalPosition, orbitalVelocity);
        CalculatedSpeed = CalculatedVelocity.magnitude;

        // Update true anomaly and flight path angle
        TrueAnomaly = OrbitalMechanics.Trajectory.TrueAnomaly(CalculatedPosition, CalculatedVelocity, eccentricityVector);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(specificRelativeAngularMomentum.magnitude, CalculatedPosition, CalculatedVelocity);
        
        // NOTE: many unchanging or known variables for a stable orbit being passed
        // semimajorAxis (unchanging)
        // specificRelativeAngularMomentum (unchanging)
        // orbitalRadius (known)
        // orbitalPosition (LastPosition)
        // orbitalVelocity (known)
    }
}
