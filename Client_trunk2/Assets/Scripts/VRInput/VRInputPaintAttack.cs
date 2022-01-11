using System.Collections;
using UnityEngine;

public class VRInputPaintAttack : MonoBehaviour
{
    protected Hand controllerHand = Hand.LEFT;
    protected bool isPressed = false;

    protected Transform tip_nib;
    protected VRInputAttackTarget attackTarget = null;

    public GameObject standbyObj = null;
    public GameObject completedObj = null;

    private GameObject _effectObj = null;

    private PaintPath paintPath = null;
    private bool isCompleted = false;

    private AudioSource audioSource = null;

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
        GlobalEvent.register("OnGripPressed", this, "OnPressed");
        GlobalEvent.register("OnGripReleased", this, "OnReleased");
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
            //OnReady();
            if (_effectObj != null)
                DestroyImmediate(_effectObj);
            isCompleted = false;
            _effectObj = Instantiate(standbyObj) as GameObject;
            _effectObj.transform.SetParent(tip_nib, false);
            paintPath = _effectObj.GetComponentInChildren<PaintPath>();
            paintPath.onPaintCompleted += () => { OnReady(); };

            //audioSource = AudioManager.Instance.SoundPlay("能量球-蓄力", 1, 0, true);
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = false;
            //OnFire();
            if (_effectObj != null)
                DestroyImmediate(_effectObj);
            if (isCompleted)
            {
                isCompleted = false;
                OnFire();
            }

            if (audioSource != null)
                DestroyImmediate(audioSource.gameObject);
        }
    }

    private void OnReady()
    {
        isCompleted = true;
        if (_effectObj != null)
            Destroy(_effectObj);
        //_effectObj = Instantiate(completedObj) as GameObject;
        //_effectObj.transform.SetParent(tip_nib, false);
        //if (attackTarget != null)
        //    attackTarget.StartUpdate();

        SPELL.SpellTargetData data = new SPELL.SpellTargetData();
        data.pos = paintPath.mirrorTransform.position;
        VRInputManager.Instance.playerComponent.CastSpell(1000003, data);
    }

    private void OnFire()
    {
        if (_effectObj != null)
            DestroyImmediate(_effectObj);

        FireArgs args = new FireArgs();
        args.gestureName = "blackhole";
        args.originPoint = attackTarget.nib.position;
        args.direction = attackTarget.targetDirection;
        args.targetPoint = attackTarget.targetPoint;
        args.entityID = attackTarget.entityID;
        //Debug.Log("entityID:" + attackTarget.entityID);
        //GlobalEvent.fire("OnFire", args);
        if (attackTarget != null)
            attackTarget.StopUpdate();
    }

    private void OnFire_()
    {
        if (_effectObj != null)
            DestroyImmediate(_effectObj);

        FireArgs args = new FireArgs();
        args.gestureName = "dian";
        args.originPoint = attackTarget.nib.position;
        args.direction = attackTarget.targetDirection;
        args.targetPoint = attackTarget.targetPoint;
        args.entityID = attackTarget.entityID;
        //Debug.Log("entityID:" + attackTarget.entityID);
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
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnReady();
            //OnPressed(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            OnFire();
            //OnReleased(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnReady();
            //OnPressed(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
        else if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            OnFire_();
            //OnReleased(VRInputDefined.MakeEventArgs(controllerHand, Vector2.zero));
        }
    }

    #endregion 键盘模拟
}