using System.Collections;
using UnityEngine;
using UnityEngine.VR;

public static class VRInputDefined
{
    public static bool openSimKey { get { return true; } }

    public static bool isHMDConnected
    {
        get
        {
            if (VRSettings.enabled == true)
            {
                if (VRDevice.isPresent)
                {
                    return true;
                }
            }
            else
            {
                if (SteamVR.connected[0] == true)
                {
                    return true;
                }
                else if (OVRManager.isHmdPresent)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static VRControllerEventArgs ChangeArgsType(Hand hand, VRTK.ControllerInteractionEventArgs e)
    {
        VRControllerEventArgs ee;
        ee.hand = hand;
        ee.controllerIndex = e.controllerIndex;
        ee.buttonPressure = e.buttonPressure;
        ee.touchpadAngle = e.touchpadAngle;
        ee.touchpadAxis = e.touchpadAxis;
        return ee;
    }

    public static VRControllerEventArgs MakeEventArgs(Hand hand, Vector2 touchpadAxis, float buttonPressure = 0, float touchpadAngle = 0, uint controllerIndex = 0)
    {
        VRControllerEventArgs e;
        e.hand = hand;
        e.buttonPressure = buttonPressure;
        e.controllerIndex = controllerIndex;
        e.touchpadAngle = touchpadAngle;
        e.touchpadAxis = touchpadAxis;
        return e;
    }
}

public enum Hand { NULL, LEFT, RIGHT }

/// <summary>
/// Event Payload
/// </summary>
/// <param name="controllerIndex">The index of the controller that was used.</param>
/// <param name="buttonPressure">The amount of pressure being applied to the button pressed. `0f` to `1f`.</param>
/// <param name="touchpadAxis">The position the touchpad is touched at. `(0,0)` to `(1,1)`.</param>
/// <param name="touchpadAngle">The rotational position the touchpad is being touched at, 0 being top, 180 being bottom and all other angles accordingly. `0f` to `360f`.</param>
public struct VRControllerEventArgs
{
    public Hand hand;
    public uint controllerIndex;
    public float buttonPressure;
    public Vector2 touchpadAxis;
    public float touchpadAngle;
}

public struct FireArgs
{
    public string gestureName;
    public Vector3 originPoint;
    public Vector3 targetPoint;
    public Vector3 direction;
    public int entityID;
}