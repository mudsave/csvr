using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRInputMovement : MonoBehaviour
{
    protected bool openDoubleClick = true;

    protected Hand controllerHand = Hand.LEFT;
    protected bool isPressed = false;
    protected bool moving = false;

    private bool canController = false;
    private bool touchpadGuideActive = false;

    private void Awake()
    {
        GlobalEvent.register("Event_RegisterControllerEvents", this, "RegisterControllerEvents");
        GlobalEvent.register("Event_DeregisterControllerEvents", this, "DeregisterControllerEvents");
        GlobalEvent.register("Event_GuideEvent", this, "OnGuideEvent");
        GlobalEvent.register("OnTouchpadTouchStart", this, "OnPressed");
        GlobalEvent.register("OnTouchpadTouchEnd", this, "OnReleased");
        GlobalEvent.register("OnTouchpadAxisChanged", this, "OnChanged");
        RegisterControllerEvents();

        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().name == "Demo" || SceneManager.GetActiveScene().name == "mi_jing")
        {
            touchpadGuideActive = false;
        }
        else
        {
            touchpadGuideActive = true;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode model)
    {
        if (scene.name == "Demo" || scene.name == "mi_jing")
        {
            touchpadGuideActive = false;
        }
        else
        {
            touchpadGuideActive = true;
        }
    }

    private void OnDisable()
    {
        GlobalEvent.deregister(this);
    }

    public void RegisterControllerEvents()
    {
        canController = true;
    }

    public void DeregisterControllerEvents()
    {
        canController = false;
        moving = false;
        GlobalEvent.fire("OnStopMove");
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if (!touchpadGuideActive)
            return;

        if (e.hand == controllerHand)
        {
            isPressed = true;
            if (openDoubleClick)
                DoPressed(e);
            else
                StartMove(e);
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if (!touchpadGuideActive)
            return;

        if (e.hand == controllerHand)
        {
            isPressed = false;
            StopMove(e);
        }
    }

    public void OnChanged(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if (!touchpadGuideActive)
            return;

        if (e.hand == controllerHand)
        {
            if (isPressed)
                ChangeMove(e);
        }
    }

    protected void StartMove(VRControllerEventArgs e)
    {
        moving = true;
        GlobalEvent.fire("OnStartMove", e.touchpadAxis);
    }

    protected void StopMove(VRControllerEventArgs e)
    {
        moving = false;
        GlobalEvent.fire("OnStopMove");
    }

    protected void ChangeMove(VRControllerEventArgs e)
    {
        if (moving)
            GlobalEvent.fire("OnChangeMove", e.touchpadAxis);
    }

    protected void DoubleClickMove(VRControllerEventArgs e)
    {
        GlobalEvent.fire("OnDoubleClickMove", e.touchpadAxis);
    }

    public void OnGuideEvent(GuideEvent guideEvent)
    {
        if (guideEvent == GuideEvent.Move)
        {
            touchpadGuideActive = true;
        }
    }

    #region 键盘模拟

    private void Update()
    {
        if (VRInputDefined.openSimKey)
            UpdateSimKey();
    }

    private Vector3 moveAxis = Vector3.zero;

    private void UpdateSimKey()
    {
        Vector3 axis = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            axis.z = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            axis.z = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            axis.x = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            axis.x = 1;
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)
        || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (axis == Vector3.zero)
            {
                //OnReleased
                OnReleased(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
            }
            else if (axis != moveAxis)
            {
                //OnChanged
                OnChanged(VRInputDefined.MakeEventArgs(controllerHand, new Vector2(axis.x, axis.z)));
            }
        }

        if (moveAxis == Vector3.zero && axis != Vector3.zero)
        {
            //OnPressed
            OnPressed(VRInputDefined.MakeEventArgs(controllerHand, new Vector2(axis.x, axis.z)));
        }

        moveAxis = axis;
    }

    #endregion 键盘模拟

    #region 双击功能

    private bool firstClick = false;
    private float firstClickTime;
    private float secondClickTimeSpan = 0.3f;

    private void DoPressed(VRControllerEventArgs e)
    {
        //第一击
        if (!firstClick)
        {
            firstClick = true;
            StartCoroutine(CheckDoubleClick(e));
        }
        //第二击
        else
        {
            firstClick = false;
            DoubleClickMove(e);
        }
    }

    private IEnumerator CheckDoubleClick(VRControllerEventArgs e)
    {
        firstClickTime = Time.time;
        while (firstClick)
        {
            //超时
            if (Time.time - firstClickTime > secondClickTimeSpan)
            {
                firstClick = false;
                if (isPressed)
                    StartMove(e);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    #endregion 双击功能
}