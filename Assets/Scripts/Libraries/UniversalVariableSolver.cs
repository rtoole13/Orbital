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
    private Vector2 epochPosition;
    private float epochRadius;
    private Vector2 epochVelocity;
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

        OrbitalPeriod = (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
            ? OrbitalMechanics.Trajectory.OrbitalPeriod(semimajorAxis, sourceMass)
            : Mathf.Infinity;

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
        epochRadius = CalculatedRadius;
        epochPosition = CalculatedPosition;

        // Calculate velocity
        Vector2 calculatedVelocity = (Vector2)sourceRelativeVelocity;
        CalculatedSpeed = sourceRelativeVelocity.magnitude;
        CalculatedVelocity = calculatedVelocity.RotateVector(-argumentOfPeriapsis);
        epochVelocity = CalculatedVelocity;

        // Initialize true anomaly and FPA
        TrueAnomaly = OrbitalMechanics.Trajectory.TrueAnomaly(sourceRelativePosition, sourceRelativeVelocity, EccentricityVector);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(specificRelativeAngularMomentum.magnitude, CalculatedPosition, CalculatedVelocity);

        
        if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
        {
            InitializeEllipticalParameters();
        }
        else
        {
            //Hyperbolic or parabolic(?)
            InitializeHyperbolicParameters();
        }
    }

    public void InitializeEllipticalParameters()
    {
        // Initial guess at x
        x = OrbitalMechanics.UniversalVariableMethod.EllipticalGuessX(sourceMass, semimajorAxis, 0f);
        HyperbolicExcessSpeed = Mathf.Infinity;
        TrueAnomalyOfAsymptote = Mathf.Infinity;
        HyperbolicAsymptotes= new Vector2[2];

    }

    public void InitializeHyperbolicParameters()
    {
        // Initial guess at x
        x = OrbitalMechanics.UniversalVariableMethod.HyperbolicGuessX(sourceMass, semimajorAxis, 0, CalculatedPosition, CalculatedVelocity);
        HyperbolicExcessSpeed = OrbitalMechanics.HyperbolicTrajectory.ExcessVelocity(sourceMass, semimajorAxis);
        TrueAnomalyOfAsymptote = OrbitalMechanics.HyperbolicTrajectory.TrueAnomalyOfAsymptote(eccentricity, clockWiseOrbit);
        HyperbolicAsymptotes = OrbitalMechanics.HyperbolicTrajectory.Asymptotes(TrueAnomalyOfAsymptote, clockWiseOrbit);
    }

    public void UpdateStateVariables(float timeOfFlight)
    {
        // Track last calculated position, velocity
        //SetLastStateVariables(CalculatedRadius, CalculatedSpeed, CalculatedPosition, CalculatedVelocity);

        // Wrap time of flight by period if possible
        if (trajectoryType == OrbitalMechanics.Globals.TrajectoryType.Ellipse)
            timeOfFlight %= OrbitalPeriod;

        // Update the universal variable x
        OrbitalMechanics.UniversalVariableMethod.UniversalVariable(ref x, timeOfFlight, sourceMass, semimajorAxis, epochRadius, epochPosition, epochVelocity);
        
        // Update z, c, and s based off of latest x
        z = Mathf.Pow(x, 2) / semimajorAxis;
        stumpffS = OrbitalMechanics.UniversalVariableMethod.StumpffS(z);
        stumpffC = OrbitalMechanics.UniversalVariableMethod.StumpffC(z);

        // Get f, g, and update calculated position
        f = OrbitalMechanics.UniversalVariableMethod.VariableF(x, epochRadius, stumpffC);
        g = OrbitalMechanics.UniversalVariableMethod.VariableG(timeOfFlight, x, sourceMass, stumpffS);
        CalculatedPosition = OrbitalMechanics.UniversalVariableMethod.OrbitalPosition(f, g, epochPosition, epochVelocity);
        CalculatedRadius = CalculatedPosition.magnitude;
        //Debug.LogFormat("f: {0}, g: {1}", f, g);

        // Get fPrime, gPrime, and update calculated velocity
        fPrime = OrbitalMechanics.UniversalVariableMethod.VariableFprime(sourceMass, epochRadius, CalculatedRadius, x, z, stumpffS);
        gPrime = OrbitalMechanics.UniversalVariableMethod.VariableGprime(x, CalculatedRadius, stumpffC);
        //Debug.LogFormat("test: {0}", f*gPrime - g*fPrime);
        CalculatedVelocity = OrbitalMechanics.UniversalVariableMethod.OrbitalVelocity(fPrime, gPrime, epochPosition, epochVelocity);

        CalculatedSpeed = CalculatedVelocity.magnitude;

        // Update true anomaly and flight path angle
        TrueAnomaly = OrbitalMechanics.Trajectory.TrueAnomaly(CalculatedPosition);
        FlightPathAngle = OrbitalMechanics.Trajectory.FlightPathAngle(eccentricity, TrueAnomaly);
    }
}
