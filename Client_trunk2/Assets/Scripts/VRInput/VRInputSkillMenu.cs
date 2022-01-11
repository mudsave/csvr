using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRInputSkillMenu : MonoBehaviour
{
    /// <summary>
    /// 技能菜单状态
    /// </summary>
    public enum SkillMenuStatus
    {
        NULL,               //空
        CLOSED,             //关闭
        OPENED,             //开启
        ATTACKING,          //攻击中
    }

    //攻击手柄
    public Hand controllerHand = Hand.RIGHT;

    //菜单
    private Transform skillMenu;
    public GameObject skillMenuPrefab;

    //public Transform skillMenuOrigin;

    public GameObject standbyObj = null;
    //public GameObject whipObj = null;
    public float menuSlideSpeed = 4f;

    private GameObject _effectObj = null;
    private PaintPath paintPath = null;
    private AudioSource audioSource = null;

    private SkillMenuStatus currentStatus = SkillMenuStatus.CLOSED;
    private SkillMenuStatus nextStatus = SkillMenuStatus.NULL;

    //手
    private Transform hand = null;

    //头
    private Transform head = null;

    private List<SkillMenuItem> items = null;
    private bool hidden = true;
    private bool isMove = false;

    private EnergyMgr playerEnergyMgr = null;

    private BoxCollider skillMenuOriginCollider = null;

    protected VRInputAttackTarget attackTarget = null;

    private void Start()
    {
        if (controllerHand == Hand.RIGHT)
        {
            hand = VRInputManager.Instance.handRight.transform;
        }
        else
        {
            hand = VRInputManager.Instance.handLeft.transform;
        }
        head = VRInputManager.Instance.head.transform;

        //SkillMenuOrigin so = skillMenuOrigin.gameObject.AddComponent<SkillMenuOrigin>();
        //so.Init(this);

        CreateSkillMenu();

        if (controllerHand == Hand.LEFT)
            tip_nib = VRInputManager.Instance.tip_nib_left;
        else
            tip_nib = VRInputManager.Instance.tip_nib_right;

        playerEnergyMgr = VRInputManager.Instance.playerComponent.energyMgr;

        //skillMenuOriginCollider = skillMenuOrigin.GetComponent<BoxCollider>();
        //if (skillMenuOriginCollider)
        //    skillMenuOriginCollider.enabled = false;

        //注册菜单触发开关事件
        GlobalEvent.register("Event_SkillMenuOriginColliderSwitch", this, "Event_SkillMenuOriginColliderSwitch");
        SceneManager.sceneLoaded += onSceneLoaded;

        attackTarget = VRInputAttackTarget.left;
    }

    //private void Update()
    //{
        //Vector3 forward = head.forward;
        //forward.y = 0;
        //skillMenuOrigin.position = head.position - new Vector3(0, 0.5f, 0) + head.right * 0.2f - forward.normalized * 0.2f;
    //}

    private void ChangeStatus(SkillMenuStatus status = SkillMenuStatus.NULL)
    {
        if (status != SkillMenuStatus.NULL)
            nextStatus = status;
        if (nextStatus != SkillMenuStatus.NULL)
            currentStatus = nextStatus;
        //Debug.Log(currentStatus);
        nextStatus = SkillMenuStatus.NULL;
    }

    private float time = 0;
    private float maxTime = 0.5f;
    private bool isStay = false;

    public void SkillMenuOriginStay()
    {
        //if (!isStay && !isPressed)
        //{
        //    if (time < maxTime)
        //        time += Time.deltaTime;
        //    else
        //    {
        //        time = 0f;
        //        //启动
        //        if (currentStatus == SkillMenuStatus.CLOSED)
        //        {
        //            Debug.Log("启动菜单");
        //            VRInputManager.Instance.Shake(controllerHand, 1500, 0.1f, 0.01f);
        //            ChangeStatus(SkillMenuStatus.OPENED);
        //            GlobalEvent.fire("GuideEvent_StartSkillMenu");
        //        }
        //        //关闭
        //        else if (currentStatus == SkillMenuStatus.OPENED)
        //        {
        //            Debug.Log("关闭菜单");
        //            VRInputManager.Instance.Shake(controllerHand, 1500, 0.1f, 0.01f);
        //            HideSkillMenu();
        //            ChangeStatus(SkillMenuStatus.CLOSED);
        //            GlobalEvent.fire("GuideEvent_EndSkillMenu");
        //        }
        //        isStay = true;
        //    }
        //}
    }

    public void SkillMenuOriginExit()
    {
        //time = 0;
        //isStay = false;
        //if (currentStatus == SkillMenuStatus.OPENED && hidden)
        //    ShowSkillMenu();
    }

    private void ShowSkillMenu()
    {
        hidden = false;
        //isMove = true;
        //VRInputManager.Instance.GetComponent<VRInputShield>().enabled = false;
        //skillMenu.position = skillMenuOrigin.position;
        //skillMenu.forward = head.forward;
        //skillMenu.localScale = Vector3.zero;

        Vector3 targetForward = head.forward;
        targetForward.y = 0;

        Vector3 targetPosition = head.position;
        targetPosition += targetForward * 0.6f;
        targetPosition -= Vector3.up * 0.1f;

        //while (true)
        //{
        //    skillMenu.position = Vector3.MoveTowards(skillMenu.position, targetPosition, Time.deltaTime * menuSlideSpeed * 0.5f);
        //    skillMenu.forward = Vector3.Lerp(skillMenu.forward, targetForward, Time.deltaTime * menuSlideSpeed);
        //    skillMenu.localScale = Vector3.Lerp(skillMenu.localScale, Vector3.one, Time.deltaTime * menuSlideSpeed);
        //    if (Vector3.Distance(skillMenu.position, targetPosition) < 0.005f) { break; }
        //    yield return null;
        //}
        skillMenu.position = targetPosition;
        skillMenu.forward = targetForward;
        skillMenu.localScale = Vector3.one;
        skillMenu.gameObject.SetActive(true);

        for (int i = 0; i < items.Count; i++)
        {
            items[i].colliderEnable = true;
        }

        VRInputManager.Instance.Shake(controllerHand, 1500, 0.05f, 0.01f);
        //isMove = false;
    }

    private void HideSkillMenu()
    {
        hidden = true;
        //isMove = true;
        for (int i = 0; i < items.Count; i++)
        {
            items[i].colliderEnable = false;
        }
        //Vector3 targetPosition = skillMenuOrigin.position;

        //while (true)
        //{
        //    skillMenu.position = Vector3.MoveTowards(skillMenu.position, targetPosition, Time.deltaTime * menuSlideSpeed);
        //    skillMenu.localScale = Vector3.Lerp(skillMenu.localScale, Vector3.zero, Time.deltaTime * menuSlideSpeed);
        //    if (Vector3.Distance(skillMenu.position, targetPosition) < 0.005f) { break; }
        //    yield return null;
        //}
        //skillMenu.position = targetPosition;

        skillMenu.gameObject.SetActive(false);

        //if (currentSkilItem == null)
        //    VRInputManager.Instance.GetComponent<VRInputShield>().enabled = true;

        VRInputManager.Instance.Shake(controllerHand, 1500, 0.05f, 0.01f);
        //isMove = false;
    }

    private GameObject skillHand = null;
    private SkillMenuItem currentSkilItem = null;

    public void SelectSkillItem(SkillMenuItem item)
    {
        currentSkilItem = item;
        GlobalEvent.fire("GuideEvent", GuideEvent.TouchSkill);
        //skillHand = Instantiate(item.transform.parent.gameObject);
        //Destroy(skillHand.GetComponentInChildren<SkillMenuItem>().gameObject);
        //skillHand.transform.parent = VRInputManager.Instance.handLeftModel.transform;
        //skillHand.transform.localPosition = Vector3.zero;
        //skillHand.transform.localScale = Vector3.one;
        //skillHand.transform.localRotation = Quaternion.identity;
        HighlightingSystem.Highlighter lighter = VRInputManager.Instance.handLeftModel.GetComponentInChildren<HighlightingSystem.Highlighter>();
        lighter.ConstantOn(item.highlightColor);
        ChangeStatus(SkillMenuStatus.ATTACKING);
        HideSkillMenu();
    }

    protected bool isPressed = false;

    protected Transform tip_nib;

    private EffectComponent eComponent;

    private void OnEnable()
    {
        GlobalEvent.register("Event_PlayerStatusChanged", this, "OnPlayerStatusChanged");

        GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.register("OnTriggerReleased", this, "OnReleased");
        GlobalEvent.register("OnGripPressed", this, "OnGripPressed");
        GlobalEvent.register("OnGripReleased", this, "OnGripReleased");
    }

    private void OnDisable()
    {
        GlobalEvent.deregister(this);
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            if (currentSkilItem != null)
            {
                if (currentSkilItem.skillID == 1 && VRInputManager.Instance.playerComponent.CheckMagicSparCount(10))
                {
                    if (_effectObj == null)
                    {
                        VRInputManager.Instance.handLeftAnimator.SetBool("at", true);
                        _effectObj = Instantiate(standbyObj) as GameObject;
                        //_effectObj.transform.SetParent(tip_nib, false);
                        _effectObj.transform.position = tip_nib.position;
                        Vector3 forward = VRInputManager.Instance.handLeft.transform.forward;
                        forward.y = 0;
                        _effectObj.transform.forward = forward;
                        paintPath = _effectObj.GetComponentInChildren<PaintPath>();
                        paintPath.onPaintCompleted += () => OnPaintCompleted();

                        audioSource = AudioManager.Instance.SoundPlay("能量球-蓄力", 1, 0, true, _effectObj);
                        StartCoroutine(SkillTimer(5));

                        VRInputManager.Instance.playerComponent.MagicSparCountChange(-10);
                    }
                }
                else if (currentSkilItem.skillID == 2 && VRInputManager.Instance.playerComponent.CheckMagicSparCount(3))
                {
                    if (eComponent == null)
                    {
                        VRInputManager.Instance.handLeftAnimator.SetBool("launch", true);
                        eComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect("penhuo", VRInputManager.Instance.handLeft.transform);
                        SPELL.FireJetSkill fj = eComponent.gameObject.AddComponent<SPELL.FireJetSkill>();
                        fj.Init(VRInputManager.Instance.playerComponent);
                        StartCoroutine(SkillTimer(5));

                        //playerEnergyMgr.ChangeEnergy(-40);
                        //playerEnergyMgr.StopRecovery(6);
                        VRInputManager.Instance.playerComponent.MagicSparCountChange(-3);
                    }
                }
                else if (currentSkilItem.skillID == 3 && VRInputManager.Instance.playerComponent.CheckMagicSparCount(3))
                {
                    if (eComponent == null)
                    {
                        VRInputManager.Instance.handLeftAnimator.SetBool("launch", true);
                        eComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect("frostJet", VRInputManager.Instance.handLeft.transform);
                        SPELL.FrostJetSkill fj = eComponent.gameObject.AddComponent<SPELL.FrostJetSkill>();
                        fj.Init(VRInputManager.Instance.playerComponent);
                        StartCoroutine(SkillTimer(5));

                        VRInputManager.Instance.playerComponent.MagicSparCountChange(-3);
                    }
                }
                else if (currentSkilItem.skillID == 4)
                {
                    if (eComponent == null)
                    {
                        VRInputManager.Instance.handLeftAnimator.SetBool("launch", true);
                        eComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect("lightningball", VRInputManager.Instance.handLeft.transform);
                        eComponent.gameObject.AddComponent<SPELL.LightningballSkill>();
                        StartCoroutine(SkillTimer(0));
                    }
                }
                //else if (currentSkilItem.skillID == 5 && VRInputManager.Instance.playerComponent.CheckMagicSparCount(1))
                //{
                //    if (eComponent == null)
                //    {
                //        int value = VRInputManager.Instance.playerComponent.MagicSpar;
                //        VRInputManager.Instance.playerComponent.RecoveryHp(value * 1000);
                //        VRInputManager.Instance.playerComponent.effectManager.AddEffect("skill_helth", VRInputManager.Instance.handLeft.transform);
                //        VRInputManager.Instance.playerComponent.MagicSparCountChange(-value);
                //        StartCoroutine(SkillTimer(0));
                //    }
                //}
                else
                {
                    Attacked();
                }
            }
            else if(currentStatus == SkillMenuStatus.CLOSED && !isPressed)
            {
                ChangeStatus(SkillMenuStatus.OPENED);
            }
            else if (currentStatus == SkillMenuStatus.OPENED && !isPressed)
            {
                HideSkillMenu();
                ChangeStatus(SkillMenuStatus.CLOSED);

                GlobalEvent.fire("GuideEvent",GuideEvent.EndSkillMenu);
            }
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        //if (e.hand == controllerHand)
        //{
            //isPressed = false;
            //if (currentSkilItem.skillID != 1)
            //    Attacked();
        //}

        if (currentStatus == SkillMenuStatus.OPENED && hidden && !isPressed)
        {
            ShowSkillMenu();

            GlobalEvent.fire("GuideEvent",GuideEvent.StartSkillMenu);
        }
    }

    private void Attacked()
    {
        if (currentSkilItem == null)
            return;

        if (currentSkilItem.skillID == 1)
        {
            VRInputManager.Instance.handLeftAnimator.SetBool("at", false);
            if (eComponent != null)
                VRInputManager.Instance.playerComponent.effectManager.RemoveEffect(eComponent);
        }
            
        else if (currentSkilItem.skillID == 4)
        {
            //whipObj.SetActive(false);
            //VRInputManager.Instance.handLeftAnimator.SetBool("launch", false);
        }
        else
        {
            VRInputManager.Instance.handLeftAnimator.SetBool("launch", false);
            StartCoroutine(SkillComponentDelayRemove(0.5f));
        }

        if (_effectObj != null)
            Destroy(_effectObj);

        if (currentStatus == SkillMenuStatus.ATTACKING)
        {
            ChangeStatus(SkillMenuStatus.CLOSED);

            // Destroy(skillHand);
            HighlightingSystem.Highlighter lighter = VRInputManager.Instance.handLeftModel.GetComponentInChildren<HighlightingSystem.Highlighter>();
            lighter.ConstantOff();
            currentSkilItem = null;

            //VRInputManager.Instance.GetComponent<VRInputShield>().enabled = true;
        }
    }

    private void OnPaintCompleted()
    {
        if (_effectObj != null)
            Destroy(_effectObj);
        if (audioSource != null)
            Destroy(audioSource.gameObject);
        SPELL.SpellTargetData data = new SPELL.SpellTargetData();
        data.pos = paintPath.mirrorTransform.position;
        VRInputManager.Instance.playerComponent.CastSpell(1000003, data);
        Attacked();
    }

    private IEnumerator SkillTimer(float stayTime = 0)
    {
        if (stayTime > 0)
            yield return new WaitForSeconds(stayTime);
        if (currentSkilItem != null)
            Attacked();
    }

    private IEnumerator SkillComponentDelayRemove(float delay)
    {
        if (eComponent != null)
        {
            foreach (Transform child in eComponent.gameObject.transform)
            {
                ParticleSystem ps = child.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = 0;
                }

            }
        }
        yield return new WaitForSeconds(delay);
        VRInputManager.Instance.playerComponent.effectManager.RemoveEffect(eComponent);
    }

    public void Event_SkillMenuOriginColliderSwitch(bool value)
    {
        if (skillMenuOriginCollider)
            skillMenuOriginCollider.enabled = value;
    }

    public void onSceneLoaded(Scene scene, LoadSceneMode model)
    {
        CreateSkillMenu();

        if (skillMenuOriginCollider)
            skillMenuOriginCollider.enabled = true;

        currentStatus = SkillMenuStatus.CLOSED;
    }

    private void CreateSkillMenu()
    {
        skillMenu = Instantiate(skillMenuPrefab).transform;
        skillMenu.gameObject.SetActive(false);
        items = new List<SkillMenuItem>(skillMenu.GetComponentsInChildren<SkillMenuItem>());
        for (int i = 0; i < items.Count; i++)
        {
            items[i].onTriggerEnter += SelectSkillItem;
        }
    }

    public void OnGripPressed(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand && currentStatus == SkillMenuStatus.CLOSED)
        {
            isPressed = true;
            OnReady();
            VRInputManager.Instance.handLeftAnimator.SetBool("take", true);
            GlobalEvent.fire("GuideEvent",GuideEvent.StartShield);
        }
    }

    public void OnGripReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand && currentStatus == SkillMenuStatus.CLOSED)
        {
            isPressed = false;
            OnFire();
            VRInputManager.Instance.handLeftAnimator.SetBool("take", false);
            GlobalEvent.fire("GuideEvent",GuideEvent.EndShield);
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

    public void OnPlayerStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
        if (newStatus == eEntityStatus.Death)
        {
            if (currentStatus == SkillMenuStatus.OPENED)
            {
                HideSkillMenu();
                ChangeStatus(SkillMenuStatus.CLOSED);
            }
            GlobalEvent.deregister("OnTriggerPressed", this, "OnPressed");
            GlobalEvent.deregister("OnTriggerReleased", this, "OnReleased");
        }
        else if (newStatus == eEntityStatus.Idle && oldStatus == eEntityStatus.Death)
        {
            GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
            GlobalEvent.register("OnTriggerReleased", this, "OnReleased");
        }
    }
}