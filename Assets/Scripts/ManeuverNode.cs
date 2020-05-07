using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverNode : MonoBehaviour
{
    public Vector2 velocity;
    public float trueAnomaly;
    public int rank; //intended to specify whether maneuver is on current trajectory, rank 0, or a future trajectory 1+
}
