using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerComponent : AvatarComponent
{
    private CPlayerViewFade viewFade = null;
    private CPlayerViewFade recoveryViewFade = null;

    private NavigaterMovementController_vr m_movementController = null;
    private Transform eyeTransform = null;
    private Transform roomTransform = null;
    private NavMeshAgent m_navMeshAgent = null;
    private CPlayerHurtDisplay playerHurtDisplay;   //玩家伤害信息
    private VRModelComponent m_vrModelComponent;
    private EffectComponent weaponEffect = null;

    private int maxMoveSpeed = 3;    //最大移动速度

    public MovementController movementController
    {
        get { return m_movementController; }
    }

    public NavMeshAgent navMeshAgent
    {
        get { return m_navMeshAgent; }
    }

    public VRModelComponent vrModelComponent
    {
        get { return m_vrModelComponent; }
    }

    private void Start()
    {
        //临时写法
        MaxHP = 10000;
        HP = 10000;
    }

    //public void OnFireReady(string gestureName)
    //{
    //    SkillConfig skill = ClientConst.GetSkillConfigByGestureName(gestureName);
    //    if (skill == null) return;
    //    if (skill.skillType == SkillType.Point)
    //    {
    //        VRInputManager.Instance.FireReady(true);
    //    }
    //    //else
    //    //{
    //    //VRInputManager.Instance.FireReady(false);
    //    Transform rightWeapon = m_vrModelComponent.bindingGameObject["rightWeapon"].compent.transform.FindChild("Model");
    //    SkillConfig skillConfig = ClientConst.GetSkillConfigByGestureName(gestureName);
    //    if (skillConfig != null && !string.IsNullOrEmpty(skillConfig.weaponEffectName))
    //    {
    //        //weaponEffect = EffectManager.Instance.PlayEffect(skillConfig.weaponEffectName, rightWeapon, true);
    //        //bingweapon\dianweapon
    //        //TODO: huxiubo 临时调整武器特效
    //        weaponEffect = effectManager.AddEffect(skillConfig.weaponEffectName, rightWeapon);
    //        //weaponEffect.transform.localPosition = Vector3.zero;
    //        //weaponEffect.transform.localScale = Vector3.one;
    //        //weaponEffect.transform.localRotation = Quaternion.identity;
    //    }
    //    //}
    //}

    #region 移动方法

    public void OnStartMove(Vector2 axis)
    {
        m_movementController.MoveForward(maxMoveSpeed, new Vector3(axis.x, 0, axis.y));
        GlobalEvent.fire("GuideEvent", GuideEvent.Move);
    }

    public void OnStopMove()
    {
        m_movementController.StopMove();
    }

    public void OnChangeMove(Vector2 axis)
    {
        if (m_movementController.moving)
        {
            m_movementController.MoveForward(maxMoveSpeed, new Vector3(axis.x, 0, axis.y));
        }
    }

    public void OnDoubleClickMove(Vector2 axis)
    {
        Debug.Log("OnDoubleClickMove");
        FireArgs args = new FireArgs();
        args.gestureName = "shanxian";
        Vector3 v = eyeTransform.forward * axis.y + eyeTransform.right * axis.x;
        args.direction = v.normalized;
        args.originPoint = transform.position;
    }

    #endregion 移动方法

    public void OnFire(FireArgs args)
    {
        //Debug.Log("OnFire");
        if (weaponEffect != null) weaponEffect.DestroyEffect();
        SkillConfig skill = ClientConst.GetSkillConfigByGestureName(args.gestureName);
        if (skill == null) return;
        if (skill.skillType == SkillType.Point)
        {
            SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
            targetData.pos = args.originPoint;
            targetData.dir = args.direction;
            CastSpell(skill.skillID, targetData);
        }
        else if (skill.skillType == SkillType.Shoot)
        {
            Debug.Log("OnFire");
            SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
            targetData.pos = args.originPoint;
            targetData.dir = args.direction;
            CastSpell(skill.skillID, targetData);
        }
        else if (skill.skillType == SkillType.Direction)
        {
            SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
            targetData.pos = args.originPoint;
            targetData.dir = args.direction;
            CastSpell(skill.skillID, targetData);
        }
        else if (skill.skillType == SkillType.Target)
        {
            SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
            targetData.pos = args.originPoint;
            targetData.dir = args.direction;
            CastSpell(skill.skillID, targetData);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.K)) 
        {
            //EffectComponent component = effectManager.AddEffect("penhuo", VRInputManager.Instance.handLeft.transform);
            //SPELL.FireJetSkill fj = component.gameObject.AddComponent<SPELL.FireJetSkill>();
            //fj.Init(this);
            //Resurrection();
        }
    }

    public void __init__()
    {
        objectType = CEntityType.Player;

        // 是玩家自己操控的对象
        m_vrModelComponent = gameObject.AddComponent<VRModelComponent>();
        eyeTransform = m_vrModelComponent.eyeTransform;
        roomTransform = m_vrModelComponent.roomTransform;
        m_movementController = gameObject.AddComponent<NavigaterMovementController_vr>();

        updateFun["SynNavPosition"] = SetPos;

        m_navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        m_navMeshAgent.autoBraking = false;
        m_navMeshAgent.acceleration = 1000;
        m_navMeshAgent.angularSpeed = 360;
        m_navMeshAgent.updateRotation = false;
        m_navMeshAgent.radius = 0;
        m_navMeshAgent.height = 0;

        //GlobalEvent.register("OnFireReady", this, "OnFireReady");

        //移动事件
        GlobalEvent.register("OnStartMove", this, "OnStartMove");
        GlobalEvent.register("OnStopMove", this, "OnStopMove");
        GlobalEvent.register("OnChangeMove", this, "OnChangeMove");
        GlobalEvent.register("OnDoubleClickMove", this, "OnDoubleClickMove");
        GlobalEvent.register("OnFire", this, "OnFire");

        playerHurtDisplay = new CPlayerHurtDisplay(this);

        //增加碰撞盒
        CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = true;
        capsuleCollider.center = new Vector3(0, 1, 0);
        capsuleCollider.radius = 0.3f;
        capsuleCollider.height = 1.8f;

        VRInputManager.Instance.playerComponent = this;

        //m_vrModelComponent.LoadWeapon(new CModelParameter(CConfigClass.modelConfig["fs_z18_01_lod0"]));

        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        energyMgr = gameObject.AddComponent<EnergyMgr>();
        energyMgr.Init(this);
        GlobalEvent.fire("EVENT_playerInited", this);

        //动态创建技能管理器
        GameObject skillManager = Instantiate(Resources.Load("PlayerSkillManager")) as GameObject;
        skillManager.name = "PlayerSkillManager";
        skillManager.transform.SetParent(this.transform);
    }

    public override void Destroy()
    {
        base.Destroy();

        GlobalEvent.deregister(this);
    }

    public override void EnterWorld()
    {
        SwitchCollider(true);
    }

    public override void LeaveWorld()
    {
        m_movementController.StopMove();
        SwitchCollider(false);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode model)
    {
        GlobalEvent.fire("Event_RegisterControllerEvents");
        VRInputManager.Instance.SelectBeam.SetActive(false);
        Resurrection();
    }

    public override Transform myTransform
    {
        get
        {
            //if (entity.isPlayer())
            //    return m_eyeTransform;

            return base.myTransform;
        }
    }

    public void SetPos()
    {
        // 1
        //保存眼睛与房间原始坐标与旋转角度
        Vector3 eyeP = eyeTransform.position;
        Quaternion eyeR = eyeTransform.rotation;
        Vector3 rigP = roomTransform.position;
        Quaternion rigR = roomTransform.rotation;

        // 2
        //设置整体坐标与眼睛坐标同步（Y轴不同步）
        Vector3 tmpP = eyeP;
        tmpP.y = transform.position.y;
        transform.position = tmpP;
        //设置整体y轴旋转角度与眼睛y轴旋转角度同步
        Quaternion tmpR = transform.rotation;
        tmpR.y = eyeR.y;
        tmpR.w = eyeR.w;
        transform.rotation = tmpR;

        // 3
        //还原房间坐标与旋转角度
        roomTransform.position = rigP;
        roomTransform.rotation = rigR;
        //还原眼睛坐标与旋转角度
        eyeTransform.position = eyeP;
        eyeTransform.rotation = eyeR;
    }

    public override void OnReceiveDamage(int damage, AvatarComponent attacker)
    {
        base.OnReceiveDamage(damage, attacker);
        playerHurtDisplay.CreatFightResult(damage.ToString());
    }

    public override void OnRecoveryHP(int value)
    {
        base.OnRecoveryHP(value);
        CreatRecoveryViewFade(0.2f, 0.5f,0.2f);
    }

    public override void OnKilled(AvatarComponent victim)
    {
        //获得目标上的魔法晶石
        MagicSparMgr.CreateMagicSpar(victim, victim.MagicSpar);
    }

    protected override void OnHPChange()
    {
        GlobalEvent.fire("Event_playerHPChange", HP, MaxHP);
    }

    protected override void OnMagicSparCountChange(int oldValue, int newValue)
    {
        GlobalEvent.fire("Event_ChangeSparCount", (newValue - oldValue));
    }

    public override void OnEnergyChanged(int newValue, int oldValue, int maxValue)
    {
        GlobalEvent.fire("Event_energyChange", newValue, maxValue);
    }

    public override void OnStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
        GlobalEvent.fire("Event_PlayerStatusChanged", newStatus, oldStatus);

        if (oldStatus != eEntityStatus.Death && newStatus == eEntityStatus.Death)
        {
            m_movementController.StopMove();
            GlobalEvent.fire("Event_OnPlayerDeath");
            GlobalEvent.deregister("OnStartMove", this, "OnStartMove");
            GlobalEvent.deregister("OnStopMove", this, "OnStopMove");
            GlobalEvent.deregister("OnChangeMove", this, "OnChangeMove");
        }
        else if (oldStatus == eEntityStatus.Death && newStatus == eEntityStatus.Idle)
        {
            GlobalEvent.fire("Event_OnPlayerResurrection");
            GlobalEvent.register("OnStartMove", this, "OnStartMove");
            GlobalEvent.register("OnStopMove", this, "OnStopMove");
            GlobalEvent.register("OnChangeMove", this, "OnChangeMove");
        }
    }

    /// <summary>
    /// 玩家视野效果淡入淡出
    /// </summary>
    /// <param name="fadeInTime">淡入时间</param>
    /// <param name="fadeOutTime">淡出时间</param>
    /// <param name="stagnationTime">停滞时间</param>
    public void CreatViewFade(float fadeInTime, float fadeOutTime, float stagnationTime)
    {
        if (viewFade == null)
        {
            GameObject viewFadeObject = Instantiate(Resources.Load("UI/ViewFade/PlayerViewFade")) as GameObject;
            viewFadeObject.name = "PlayerViewFade";
            Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
            viewFadeObject.transform.parent = cameraTransform;
            viewFadeObject.transform.localPosition = Vector3.zero;
            viewFadeObject.transform.rotation = cameraTransform.rotation;
            viewFade = viewFadeObject.GetComponent<CPlayerViewFade>();
        }
        viewFade.StartFade(fadeInTime, fadeOutTime, stagnationTime);
    }

    public void CreatRecoveryViewFade(float fadeInTime, float fadeOutTime, float stagnationTime)
    {
        if (recoveryViewFade == null)
        {
            GameObject viewFadeObject = Instantiate(Resources.Load("UI/ViewFade/PlayerRecovery")) as GameObject;
            viewFadeObject.name = "PlayerRecovery";
            Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
            viewFadeObject.transform.parent = cameraTransform;
            viewFadeObject.transform.localPosition = Vector3.zero;
            viewFadeObject.transform.rotation = cameraTransform.rotation;
            recoveryViewFade = viewFadeObject.GetComponent<CPlayerViewFade>();
        }
        recoveryViewFade.StartFade(fadeInTime, fadeOutTime, stagnationTime);
    }

    /// <summary>
    /// 复活
    /// </summary>
    public void Resurrection()
    {
        if (status != eEntityStatus.Death)
            return;

        changeEntityStatus(eEntityStatus.Idle);
        HP = MaxHP;
    }
}