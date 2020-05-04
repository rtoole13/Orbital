using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputHandlerBasic : InputHandler
{
    #region GETSET

    #endregion GETSET

    #region EVENTS
    #endregion
    
    protected override void HandleInputs()
    {
        //------Rotation------//
        if (Input.GetKey(KeyCode.A))
        {
            // Rotate CCW
            ship.IncrementRotationRate();
            
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Rotate CW
            ship.DecrementRotationRate();
        }

        //------Stability Assist------//
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Toggle stability assist on ship
            ship.ToggleStabilityAssist();

            // Toggle stability assist UI
            InvokeToggleStabilityAssistEvent();
        }

        //------Thrust Controls------//
        if (Input.GetKey(KeyCode.X))
        {
            ship.ResetThrust();
            return;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            // Throttle to max thrust
            ship.ThrottleMax();
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            // Throttle up
            ship.ThrottleUp();
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            // Throttle down
            ship.ThrottleDown();
        }
    }
}
