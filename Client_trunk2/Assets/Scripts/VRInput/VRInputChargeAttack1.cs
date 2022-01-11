using System.Collections;
using UnityEngine;

public class VRInputChargeAttack1 : MonoBehaviour
{
    //public GameObject ball = null;

    private EffectComponent _ball = null;
    private EffectComponent _collectEffect = null;
    private EffectComponent _collectFinishEffect = null;

    private ParticleSystem _ballParticleSystem = null;

    protected Hand controllerHand = Hand.RIGHT;
    protected bool isPressed = false;

    protected Transform tip_nib;
    protected VRInputAttackTarget attackTarget = null;

    //private EnergySystem energySystem = null;
    private AudioSource audioSource = null;

    private void Start()
    {
        if (controllerHand == Hand.LEFT)
            tip_nib = VRInputManager.Instance.tip_nib_left;
        else
            tip_nib = VRInputManager.Instance.tip_nib_right;
        attackTarget = VRInputAttackTarget.right;
        //attackTarget.SetTestBall(ball);
        //energySystem = VRInputManager.Instance.playerComponent.energySystem;
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
                StartLeakPower(0.3f);
                OnFire();
            }
            else
            {
                StartLeakPower(0.5f);
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
        FireArgs args = new FireArgs();
        args.gestureName = "fire";
        args.originPoint = attackTarget.nib.position;
        args.direction = attackTarget.targetDirection;
        args.targetPoint = attackTarget.targetPoint;
        args.entityID = attackTarget.entityID;
        //Debug.Log("entityID:" + attackTarget.entityID);
        // GlobalEvent.fire("OnFire", args);

        _ball.transform.SetParent(null);
        _ball.GetComponent<SPELL.FireBallSkill>().StartFly(args.direction);
        _ball = null;

        if (attackTarget != null)
            attackTarget.StopUpdate();

        DestroyAllCollectEffect();
    }

    #region 普通攻击蓄力

    private bool collecting = false;
    private bool collectFull = false;
    private float power = 0;

    //蓄力
    private void StartCollectPower(float time = 1)
    {
        collectFull = false;
        collecting = true;
        StartCoroutine(CollectPower(time));
    }

    private IEnumerator CollectPower(float time = 1)
    {
        //if (energySystem)
        //    energySystem.StartConsumeStatus(3.0f);
        float timer = Time.time;
        power = 0;
        CollectEffect();
        while (collecting && !collectFull)
        {
            //if (energySystem.energy < 3.0f)
            //{
            //    collecting = false;
            //    StartLeakPower(0.5f);
            //}
            power = (Time.time - timer) / time;
            if (power >= 1)
            {
                power = 1;
                collectFull = true;
                collecting = false;
                VRInputManager.Instance.Shake(controllerHand, 1500, 0.05f, 0.01f);
                CollectFinishEffect();
                OnReady();
                //Debug.Log("CollectPower:Full");
            }
            //Debug.Log("CollectPower:" + power);
            //GetBall().localScale = Vector3.one * power / 2;
            if (_ballParticleSystem)
            {
                _ballParticleSystem.startSize = power * 0.22f;
            }
            yield return new WaitForEndOfFrame();
        }
        //if (energySystem)
        //    energySystem.EndConsumeStatus(3.0f);
    }

    //泄力
    private void StartLeakPower(float time = 1)
    {
        //collectFull = false;
        collecting = false;
        StartCoroutine(LeakPower(time));
    }

    private IEnumerator LeakPower(float time = 1)
    {
        float timer = Time.time;
        time *= power;
        float oldPower = power;
        CollectFail();
        while (!collecting && power > 0)
        {
            power = (1 - (Time.time - timer) / time) * oldPower;
            if (power <= 0)
            {
                power = 0;
            }
            //Debug.Log("LeakPower:" + power);
            //GetBall().localScale = Vector3.one * power / 2;
            if (_ballParticleSystem)
            {
                _ballParticleSystem.startSize = power * 0.22f;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void CollectEffect()
    {
        if (_collectEffect != null)
            _collectEffect.DestroyEffect();
        _collectEffect = VRInputManager.Instance.playerComponent.effectManager.AddEffect("xuli01", VRInputManager.Instance.tip_nib_right);
        audioSource = AudioManager.Instance.SoundPlay("火球-燃烧", 1, 0, true);
    }

    private void CollectFail()
    {
        if (_collectEffect != null)
            _collectEffect.DestroyEffect();
        if (audioSource != null && !collectFull)
            DestroyImmediate(audioSource.gameObject);
    }

    private void CollectFinishEffect()
    {
        if (_collectFinishEffect != null)
            _collectFinishEffect.DestroyEffect();
        _collectFinishEffect = VRInputManager.Instance.playerComponent.effectManager.AddEffect("xuli02", VRInputManager.Instance.tip_nib_right);

        if (_collectEffect != null)
            _collectEffect.DestroyEffect();

        if (_ball != null)
            _ball.DestroyEffect();
        _ball = VRInputManager.Instance.playerComponent.effectManager.AddEffect("fireballTailing", VRInputManager.Instance.tip_nib_right);

        //11111
        SPELL.FireBallSkill skill = _ball.gameObject.AddComponent<SPELL.FireBallSkill>();
        skill.Init(VRInputManager.Instance.playerComponent);
        skill.SetEnable(true);

        //StartCoroutine(GetBall(0.2f));
    }

    //private IEnumerator GetBall(float delay)
    //{
    //    if (_ball != null)
    //        _ball.DestroyEffect();
    //    yield return new WaitForSeconds(delay);
    //    _ball = VRInputManager.Instance.playerComponent.effectManager.AddEffect("fire01", VRInputManager.Instance.tip_nib_right);
    //}

    private void DestroyAllCollectEffect()
    {
        if (_collectEffect != null)
            _collectEffect.DestroyEffect();
        if (_collectFinishEffect != null)
            _collectFinishEffect.DestroyEffect();
        //if (_ball != null)
        //    _ball.DestroyEffect();

        StartCoroutine(DestroyAudioSource(0.5f));
    }

    private IEnumerator DestroyAudioSource(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null)
            DestroyImmediate(audioSource.gameObject);
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