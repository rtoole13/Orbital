using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeplerSolver
{
    private float eccentricAnomaly;
    private float meanAnomaly;
    private float meanAnomalyAtEpoch;
    private float meanMotion;
    
    public KeplerSolver(float _eccentricAnomaly = 0f)
    {
        eccentricAnomaly = _eccentricAnomaly;
        meanAnomaly = 0f;
        meanAnomalyAtEpoch = 0f;
        meanMotion = 0f;
    }

    public void UpdateStateVariables()
    {
        throw new System.NotImplementedException();
    }
}
