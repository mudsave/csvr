using System.Collections;
using UnityEngine;

public class VRInputShield : MonoBehaviour
{
    protected Hand controllerHand = Hand.LEFT;
    protected bool isPressed = false;

    protected Transform tip_nib;
    protected VRInputAttackTarget attackTarget = null;

    private bool canController = false;

    private void Start()
    {
        if (controllerHand == Hand.LEFT)
            tip_nib = VRInputManager.Instance.tip_nib_left;
        else
            tip_nib = VRInputManager.Instance.tip_nib_right;
        attackTarget = VRInputAttackTarget.left;
    }

    private void OnEnable()
    {
        GlobalEvent.register("Event_RegisterControllerEvents", this, "RegisterControllerEvents");
        GlobalEvent.register("Event_DeregisterControllerEvents", this, "DeregisterControllerEvents");

        RegisterControllerEvents();
    }

    private void OnDisable()
    {
        GlobalEvent.deregister(this);
    }

    public void RegisterControllerEvents()
    {
        if (canController)
            return;

        GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.register("OnTriggerReleased", this, "OnReleased");

        canController = true;
    }

    public void DeregisterControllerEvents()
    {
        if (!canController)
            return;

        GlobalEvent.deregister("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.deregister("OnTriggerReleased", this, "OnReleased");

        canController = false;
        OnFire();
        VRInputManager.Instance.handLeftAnimator.SetBool("take", false);
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = true;
            OnReady();
            VRInputManager.Instance.handLeftAnimator.SetBool("take", true);
            GlobalEvent.fire("GuideEvent", GuideEvent.StartShield);
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = false;
            OnFire();
            VRInputManager.Instance.handLeftAnimator.SetBool("take", false);
            GlobalEvent.fire("GuideEvent", GuideEvent.EndShield);
        }
    }

    private void OnReady()
    {
        if (attackTarget != null)
            attackTarget.StartUpdate();

        FireArgs args = new FireArgs();
        args.gestureName = "dun";
        args.originPoint = attackTarget.nib.position;
        args.direction = attackTarget.targetDirection;
        args.targetPoint = attackTarget.targetPoint;
        GlobalEvent.fire("OnFire", args);

        if (attackTarget != null)
            attackTarget.StopUpdate();
    }

    private void OnFire()
    {
        if (attackTarget != null)
            attackTarget.StartUpdate();

        FireArgs args = new FireArgs();
        args.gestureName = "dunCancel";
        args.originPoint = attackTarget.nib.position;
        args.direction = attackTarget.targetDirection;
        args.targetPoint = attackTarget.targetPoint;
        GlobalEvent.fire("OnFire", args);

        if (attackTarget != null)
            attackTarget.StopUpdate();
    }

    #region 键盘模拟

    private void Update()
    {
        if (VRInputDefined.openSimKey)
            UpdateSimKey();
    }

    private void UpdateSimKey()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnPressed(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            OnReleased(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
    }

    #endregion 键盘模拟
}