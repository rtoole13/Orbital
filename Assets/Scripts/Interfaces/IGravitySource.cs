using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGravitySource
{
    void AddAffectedBody(GravityAffected body);
    void RemoveAffectedBody(GravityAffected body);
}