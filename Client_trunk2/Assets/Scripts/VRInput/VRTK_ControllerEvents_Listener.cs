using UnityEngine;
using VRTK;

public class VRTK_ControllerEvents_Listener : MonoBehaviour
{
    public Hand hand = Hand.NULL;

    private void Start()
    {
        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("VRTK_ControllerEvents_ListenerExample is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }

        //Setup controller event listeners
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += OnTriggerPressed;
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += OnTriggerReleased;

        GetComponent<VRTK_ControllerEvents>().TriggerTouchStart += OnTriggerTouchStart;
        GetComponent<VRTK_ControllerEvents>().TriggerTouchEnd += OnTriggerTouchEnd;

        GetComponent<VRTK_ControllerEvents>().TriggerHairlineStart += OnTriggerHairlineStart;
        GetComponent<VRTK_ControllerEvents>().TriggerHairlineEnd += OnTriggerHairlineEnd;

        GetComponent<VRTK_ControllerEvents>().TriggerClicked += OnTriggerClicked;
        GetComponent<VRTK_ControllerEvents>().TriggerUnclicked += OnTriggerUnclicked;

        GetComponent<VRTK_ControllerEvents>().TriggerAxisChanged += OnTriggerAxisChanged;

        GetComponent<VRTK_ControllerEvents>().GripPressed += OnGripPressed;
        GetComponent<VRTK_ControllerEvents>().GripReleased += OnGripReleased;

        GetComponent<VRTK_ControllerEvents>().GripTouchStart += OnGripTouchStart;
        GetComponent<VRTK_ControllerEvents>().GripTouchEnd += OnGripTouchEnd;

        GetComponent<VRTK_ControllerEvents>().GripHairlineStart += OnGripHairlineStart;
        GetComponent<VRTK_ControllerEvents>().GripHairlineEnd += OnGripHairlineEnd;

        GetComponent<VRTK_ControllerEvents>().GripClicked += OnGripClicked;
        GetComponent<VRTK_ControllerEvents>().GripUnclicked += OnGripUnclicked;

        GetComponent<VRTK_ControllerEvents>().GripAxisChanged += OnGripAxisChanged;

        GetComponent<VRTK_ControllerEvents>().TouchpadPressed += OnTouchpadPressed;
        GetComponent<VRTK_ControllerEvents>().TouchpadReleased += OnTouchpadReleased;

        GetComponent<VRTK_ControllerEvents>().TouchpadTouchStart += OnTouchpadTouchStart;
        GetComponent<VRTK_ControllerEvents>().TouchpadTouchEnd += OnTouchpadTouchEnd;

        GetComponent<VRTK_ControllerEvents>().TouchpadAxisChanged += OnTouchpadAxisChanged;

        GetComponent<VRTK_ControllerEvents>().ButtonOnePressed += OnButtonOnePressed;
        GetComponent<VRTK_ControllerEvents>().ButtonOneReleased += OnButtonOneReleased;

        GetComponent<VRTK_ControllerEvents>().ButtonOneTouchStart += OnButtonOneTouchStart;
        GetComponent<VRTK_ControllerEvents>().ButtonOneTouchEnd += OnButtonOneTouchEnd;

        GetComponent<VRTK_ControllerEvents>().ButtonTwoPressed += OnButtonTwoPressed;
        GetComponent<VRTK_ControllerEvents>().ButtonTwoReleased += OnButtonTwoReleased;

        GetComponent<VRTK_ControllerEvents>().ButtonTwoTouchStart += OnButtonTwoTouchStart;
        GetComponent<VRTK_ControllerEvents>().ButtonTwoTouchEnd += OnButtonTwoTouchEnd;

        GetComponent<VRTK_ControllerEvents>().ControllerEnabled += OnControllerEnabled;
        GetComponent<VRTK_ControllerEvents>().ControllerDisabled += OnControllerDisabled;

        GetComponent<VRTK_ControllerEvents>().ControllerIndexChanged += OnControllerIndexChanged;
    }

    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        return;
        Debug.Log(hand.ToString() + " Controller on index '" + index + "' " + button + " has been " + action
                + " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
    }

    private void OnTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "pressed", e);
        GlobalEvent.fire("OnTriggerPressed", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "released", e);
        GlobalEvent.fire("OnTriggerReleased", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "touched", e);
        GlobalEvent.fire("OnTriggerTouchStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "untouched", e);
        GlobalEvent.fire("OnTriggerTouchEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerHairlineStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "hairline start", e);
        GlobalEvent.fire("OnTriggerHairlineStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerHairlineEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "hairline end", e);
        GlobalEvent.fire("OnTriggerHairlineEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "clicked", e);
        GlobalEvent.fire("OnTriggerClicked", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "unclicked", e);
        GlobalEvent.fire("OnTriggerUnclicked", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTriggerAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "axis changed", e);
        GlobalEvent.fire("OnTriggerAxisChanged", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "pressed", e);
        GlobalEvent.fire("OnGripPressed", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "released", e);
        GlobalEvent.fire("OnGripReleased", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "touched", e);
        GlobalEvent.fire("OnGripTouchStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "untouched", e);
        GlobalEvent.fire("OnGripTouchEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripHairlineStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "hairline start", e);
        GlobalEvent.fire("OnGripHairlineStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripHairlineEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "hairline end", e);
        GlobalEvent.fire("OnGripHairlineEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripClicked(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "clicked", e);
        GlobalEvent.fire("OnGripClicked", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "unclicked", e);
        GlobalEvent.fire("OnGripUnclicked", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnGripAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "axis changed", e);
        GlobalEvent.fire("OnGripAxisChanged", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "pressed Onwn", e);
        GlobalEvent.fire("OnTouchpadPressed", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "released", e);
        GlobalEvent.fire("OnTouchpadReleased", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "touched", e);
        GlobalEvent.fire("OnTouchpadTouchStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "untouched", e);
        GlobalEvent.fire("OnTouchpadTouchEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "axis changed", e);
        GlobalEvent.fire("OnTouchpadAxisChanged", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonOnePressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON ONE", "pressed Onwn", e);
        GlobalEvent.fire("OnButtonOnePressed", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonOneReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON ONE", "released", e);
        GlobalEvent.fire("OnButtonOneReleased", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonOneTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON ONE", "touched", e);
        GlobalEvent.fire("OnButtonOneTouchStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonOneTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON ONE", "untouched", e);
        GlobalEvent.fire("OnButtonOneTouchEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON TWO", "pressed Onwn", e);
        GlobalEvent.fire("OnButtonTwoPressed", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonTwoReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON TWO", "released", e);
        GlobalEvent.fire("OnButtonTwoReleased", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonTwoTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON TWO", "touched", e);
        GlobalEvent.fire("OnButtonTwoTouchStart", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnButtonTwoTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "BUTTON TWO", "untouched", e);
        GlobalEvent.fire("OnButtonTwoTouchEnd", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnControllerEnabled(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "CONTROLLER STATE", "ENABLED", e);
        GlobalEvent.fire("OnControllerEnabled", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnControllerDisabled(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "CONTROLLER STATE", "DISABLED", e);
        GlobalEvent.fire("OnControllerDisabled", VRInputDefined.ChangeArgsType(hand, e));
    }

    private void OnControllerIndexChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "CONTROLLER STATE", "INDEX CHANGED", e);
        GlobalEvent.fire("OnControllerIndexChanged", VRInputDefined.ChangeArgsType(hand, e));
    }
}