using SPELL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 
 * 文件名称：PlayerSkillShield
 * 功能说明：玩家技能左手盾牌
 * 涉及配置： 
 * 文件作者：JACK
 * 创建时间：2017/8/21/11:50
 * 更改记录: 
 */
public class PlayerSkillShield : PlayerSkillBase
{
    [Tooltip("护盾使用需要的最小精力值")]
    public int NeedEnergyValue = 25;
    [Tooltip("护盾使用被攻击时能量消耗")]
    public int DefendExpendValue = 25;
    [Tooltip("护盾光效的名称")]
    public string ShieldEffecName = "shield2";
    [Tooltip("护盾破碎特效名")]
    public string ShieldEffecBrokenName = "shield_suilie";
    [Tooltip("护盾出生绑定点")]
    public string ShieldBindObjectPath = "LeftController/Weapon/";
    [Tooltip("护盾不同破损状态的材质名（0号为默认材质），在Effects/VR_Shield/Materials/文件夹下")]
    public List<string> shieldMaterialNameList = new List<string>() { "T_VR_dun_D", "T_VR_dun_D_1", "T_VR_dun_D_2", "T_VR_dun_D_3" };

    /// <summary>玩家当前精力</summary>
    private int curEnergyValue = -1;
    /// <summary>更新前的精力</summary>
    private int OldEnergyValue = -1;
    /// <summary>护盾绑定位置</summary>
    private Transform shieldTransform;
    /// <summary>是否开启护盾</summary>
    private bool isOpenShield = false;
    /// <summary>护盾特效</summary>
    private EffectComponent shieldEffectComponent;
    /// <summary>护盾网格渲染器</summary>
    private MeshRenderer shieldMeshRenderer = null;
    /// <summary>材质基本路径</summary>
    private string materialsBaseFile = "Effects/VR_Shield/Materials/";
    /// <summary>护盾材质列表</summary>
    private Material[] shieldMaterialList = new Material[4];

    private float waveSpeed = 0.0f;
    /// <summary>目标击退效果</summary>
    public SPELL.SpellEffect beatBackEffect1;
    public SPELL.SpellEffect beatBackEffect2;
    public SPELL.SpellEffect beatBackEffect3;
    private List<AvatarComponent> TriggerList = new List<AvatarComponent>();

    // Update is called once per frame
    void Update ()
    {    
        curEnergyValue = player.energyMgr ? player.energyMgr.CurrentEnergyValue : -1;
        if (OldEnergyValue == -1 || OldEnergyValue != curEnergyValue)
        {
            OldEnergyValue = curEnergyValue;

            if (curEnergyValue != -1)
            {
                if (isOpenShield)
                {
                    if (curEnergyValue < NeedEnergyValue)
                    {
                        CloseShield(true);
                    }
                    else
                    {
                        UpdateShieldMaterial();
                    }
                }
                else
                {
                    if (curEnergyValue >= NeedEnergyValue)
                    {
                        OpenShield();
                        UpdateShieldMaterial();
                    }
                }
            }
        }
	}

    public override void Init()
    {
        base.Init();
        InitShield();
        OpenShield();
    }

    private void InitShield()
    {
        beatBackEffect1 = SpellLoader.instance.GetEffect(3001001); //击退2米
        beatBackEffect2 = SpellLoader.instance.GetEffect(3001002); //击退5米
        beatBackEffect3 = SpellLoader.instance.GetEffect(3003001);
        if (ShieldBindObjectPath != "")
        {
            shieldTransform = castHandTransform.FindChild(ShieldBindObjectPath);
            if (!shieldTransform)
            { //左手盾牌绑定点
                shieldTransform = player.myTransform;
            }
        }

        for (int i = 0; i < shieldMaterialNameList.Count; i++)
        {//0号位置添加默认材质
            Object materialObj = Resources.Load(materialsBaseFile + shieldMaterialNameList[i]);
            shieldMaterialList[i] = materialObj ? (Material)Instantiate(materialObj) : null;
        }
    }

    public void OpenShield()
    {
        isOpenShield = true;
        shieldEffectComponent = player.effectManager.AddEffect(ShieldEffecName, shieldTransform);
        ColliderDelegate colliderDelegate = shieldEffectComponent.gameObject.AddComponent<ColliderDelegate>();
        colliderDelegate.TriggerStayEvent += OnSkillStay;
        colliderDelegate.TriggerEnterEvent += OnSkillEnter;
        colliderDelegate.TriggerExitEvent += OnSkillExit;
        if (shieldMeshRenderer)
        {
            UpdateShieldMaterial();
        }
        else
        {
            shieldMeshRenderer = shieldEffectComponent.GetComponentInChildren<MeshRenderer>();
        }
        AudioManager.Instance.SoundPlay("盾-展开", 0.2f);
    }

    public void CloseShield(bool IsAddEffectSound = false)
    {
        if(isOpenShield)
        {
            isOpenShield = false;
            if (shieldEffectComponent)
            {
                shieldEffectComponent.DestroyEffect();
                shieldEffectComponent = null;             
            }
            if (IsAddEffectSound)
            {
                AddShieldCloaseEffect();
            }
        }      
    }

    private void AddShieldCloaseEffect()
    {
        // player.effectManager.RemoveModelEffect(ShieldEffecName);
        // Destroy(shieldEffectComponent.GetComponent<ColliderDelegate>());
        EffectComponent effectComponent = player.effectManager.AddEffect(ShieldEffecBrokenName, shieldTransform);
        effectComponent.gameObject.transform.parent = null;
        AudioManager.Instance.SoundPlay("盾-收起", 0.2f);
    }

    private void UpdateShieldMaterial()
    {
        if (curEnergyValue <= 75 && curEnergyValue >= 50)
        {
            shieldMeshRenderer.material = shieldMaterialList[1];
        }
        else if (curEnergyValue <= 50 && curEnergyValue >= 25)
        {
            shieldMeshRenderer.material = shieldMaterialList[2];
        }
        else if (curEnergyValue <= 25)
        {
            shieldMeshRenderer.material = shieldMaterialList[3];
        }
        else
        {
            shieldMeshRenderer.material = shieldMaterialList[0];
        }
    }

    private void OnSkillEnter(Collider other)
    {
        ColliderComponent enemyColliderComponent = other.gameObject.GetComponent<ColliderComponent>();
        if (enemyColliderComponent)
        {
            if (player.energyMgr)
            {
                player.energyMgr.ChangeEnergy(-DefendExpendValue);
                player.energyMgr.StopRecovery(1);
            }
            beatBackEffect3.Cast(player, enemyColliderComponent.caster, null, null);
            VRInputManager.Instance.Shake(Hand.LEFT, 1500, 0.1f, 0.01f);
            return;
        }
        AvatarComponent enemyAvatarComponent = other.gameObject.GetComponent<AvatarComponent>();
        if (enemyAvatarComponent)
        {
            if (player.CheckRelationship(enemyAvatarComponent) == eTargetRelationship.HostileMonster && enemyAvatarComponent.status != eEntityStatus.Death)
            {
                TriggerList.Add(enemyAvatarComponent);
                VRInputManager.Instance.Shake(Hand.LEFT, 1500, 0.1f, 0.01f);
            }
        }
    }

    private void OnSkillStay(Collider other)
    {
        waveSpeed = VRInputManager.Instance.GetControllerEvents(Hand.LEFT).GetVelocity().magnitude;
        if (waveSpeed > 2.0f)
        {
            foreach (AvatarComponent enemyAvatarComponent in TriggerList)
            {
                beatBackEffect2.Cast(player, enemyAvatarComponent, null, null);
            }
            TriggerList.Clear();
        }
    }

    private void OnSkillExit(Collider other)
    {
        ColliderComponent enemyColliderComponent = other.gameObject.GetComponent<ColliderComponent>();
        if (enemyColliderComponent)
        {
            TriggerList.Remove(enemyColliderComponent.caster);
            return;
        }
        AvatarComponent enemyAvatarComponent = other.gameObject.GetComponent<AvatarComponent>();
        if (enemyAvatarComponent)
        {
            TriggerList.Remove(enemyAvatarComponent);
        }
    }
}
