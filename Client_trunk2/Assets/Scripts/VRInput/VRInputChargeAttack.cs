using System.Collections;
using UnityEngine;

public class VRInputChargeAttack : MonoBehaviour
{
    public GameObject ball = null;

    private GameObject _ball = null;

    protected Hand controllerHand = Hand.RIGHT;
    protected bool isPressed = false;

    protected Transform tip_nib;
    protected VRInputAttackTarget attackTarget = null;

    private void Start()
    {
        if (controllerHand == Hand.LEFT)
            tip_nib = VRInputManager.Instance.tip_nib_left;
        else
            tip_nib = VRInputManager.Instance.tip_nib_right;
        attackTarget = VRInputAttackTarget.right;
        //attackTarget.SetTestBall(ball);
    }

    private void OnEnable()
    {
        GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.register("OnTriggerReleased", this, "OnReleased");
    }

    private void OnDisable()
    {
        GlobalEvent.deregister(this);
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = true;
            StartCollectPower(0.5f);
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = false;
            if (collectFull)
            {
                StartLeakPower(0.1f);
                OnFire();
            }
            else
            {
                StartLeakPower(0.2f);
            }
        }
    }

    private void OnReady()
    {
        if (attackTarget != null)
            attackTarget.StartUpdate();
    }

    private void OnFire()
    {
        if (attackTarget != null)
            attackTarget.StopUpdate();

        FireArgs args = new FireArgs();
        args.gestureName = "fire";
        args.originPoint = attackTarget.nib.position;
        args.direction = attackTarget.targetDirection;
        args.targetPoint = attackTarget.targetPoint;
        GlobalEvent.fire("OnFire", args);
    }

    #region 普通攻击蓄力

    private bool collecting = false;
    private bool collectFull = false;
    private float power = 0;

    private Transform GetBall()
    {
        if (_ball == null)
        {
            _ball = Instantiate(ball, VRInputManager.Instance.tip_nib_right) as GameObject;
            _ball.transform.localPosition = Vector3.zero;
            _ball.transform.localRotation = Quaternion.identity;
            _ball.transform.localScale = Vector3.zero;
        }
        return _ball.transform;
    }

    //蓄力
    private void StartCollectPower(float time = 1)
    {
        collectFull = false;
        collecting = true;
        StartCoroutine(CollectPower(time));
    }

    private IEnumerator CollectPower(float time = 1)
    {
        float timer = Time.time;
        power = 0;
        while (collecting && !collectFull)
        {
            power = (Time.time - timer) / time;
            if (power >= 1)
            {
                power = 1;
                collectFull = true;
                collecting = false;
                VRInputManager.Instance.Shake(controllerHand, 1500, 0.1f, 0.01f);
                OnReady();
                //Debug.Log("CollectPower:Full");
            }
            //Debug.Log("CollectPower:" + power);
            GetBall().localScale = Vector3.one * power / 2;
            yield return new WaitForEndOfFrame();
        }
    }

    //泄力
    private void StartLeakPower(float time = 1)
    {
        collectFull = false;
        collecting = false;
        StartCoroutine(LeakPower(time));
    }

    private IEnumerator LeakPower(float time = 1)
    {
        float timer = Time.time;
        time *= power;
        float oldPower = power;
        while (!collecting && power > 0)
        {
            power = (1 - (Time.time - timer) / time) * oldPower;
            if (power <= 0)
            {
                power = 0;
            }
            //Debug.Log("LeakPower:" + power);
            GetBall().localScale = Vector3.one * power / 2;
            yield return new WaitForEndOfFrame();
        }
    }

    #endregion 普通攻击蓄力

    #region 键盘模拟

    private void Update()
    {
        if (VRInputDefined.openSimKey)
            UpdateSimKey();
    }

    private void UpdateSimKey()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnPressed(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
        else if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            OnReleased(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
    }

    #endregion 键盘模拟
}