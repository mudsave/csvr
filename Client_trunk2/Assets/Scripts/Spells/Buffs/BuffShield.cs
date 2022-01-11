using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 玩家法力盾Buff
    /// </summary>
    public class BuffShield : BuffSimple
    {
        public string shieldEffect = "";             // 护盾光效名称
        public string shieldBindingObjectPath = "";  // 护盾出生绑定点

        public int needEnergyValue = 25;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            // 战斗表现模块
            DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            shieldEffect = combatPerformance.readString("shieldEffect");
            shieldBindingObjectPath = combatPerformance.readString("shieldBindingObjectPath");
        }

        public override SpellStatus OnCast(AvatarComponent src, AvatarComponent dst, SpellEx spell)
        {
            if (dst.energyMgr && dst.energyMgr.CurrentEnergyValue < needEnergyValue)
                return SpellStatus.FORBID_ACTION_LIMIT;

            foreach (var buff in dst.buffs.Values)
            {
                if (buff.buffID == id)
                {
                    return SpellStatus.BUFF_EFFECT_SUPERPOSITION;
                }

            }
            return SpellStatus.OK;
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            if (shieldEffect == "")
            {
                return;
            }

            Transform transf = owner.transform;
            if (shieldBindingObjectPath != "")
            {
                transf = owner.myTransform.FindChild(shieldBindingObjectPath);
                if (!transf)
                    transf = owner.myTransform;
            }

            EffectComponent eComponent = owner.effectManager.AddEffect(shieldEffect, transf);
            if (eComponent.gameObject != null)
            {
                ShieldCollider sc = eComponent.gameObject.AddComponent<ShieldCollider>();
                sc.Init(owner, buffData.index);
            }

            // 记录光效实例
            buffData.localBuffData["shieldEffect"] = eComponent;

            if (owner.energyMgr)
                owner.energyMgr.ChangeRecoveryValuePercent(-0.7f);

            AudioManager.Instance.SoundPlay("盾-展开",0.2f);
            buffData.localBuffData["audio"] = AudioManager.Instance.SoundPlay("盾-持续", 0.1f, 1.1f, true);

        }

        protected override bool OnTick(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            if (owner.energyMgr && (owner.energyMgr.CurrentEnergyValue <= 0))
            {
                if (buffData.localBuffData.ContainsKey("shieldEffect"))
                {
                    owner.effectManager.RemoveEffect((EffectComponent)buffData.localBuffData["shieldEffect"]);

                }
                Transform transf = owner.transform;
                if (shieldBindingObjectPath != "")
                {
                    transf = owner.myTransform.FindChild(shieldBindingObjectPath);
                    if (!transf)
                        transf = owner.myTransform;
                }

                EffectComponent e = owner.effectManager.AddEffect("shield_suilie", transf);
                e.gameObject.transform.parent = null;
                Detach(owner, buffData);
                return false;
            }
            return true;
        }

        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            if (owner.energyMgr)
                owner.energyMgr.ChangeRecoveryValuePercent(0.7f);

            //光效销毁
            if (buffData.localBuffData.ContainsKey("shieldEffect"))
            {
                owner.effectManager.RemoveEffect((EffectComponent)buffData.localBuffData["shieldEffect"]);

            }

            if (buffData.localBuffData.ContainsKey("audio"))
            {
                UnityEngine.Object.Destroy(((AudioSource)buffData.localBuffData["audio"]).gameObject);
            }

            buffData.localBuffData.Clear();

            Transform transf = owner.transform;
            if (shieldBindingObjectPath != "")
            {
                transf = owner.myTransform.FindChild(shieldBindingObjectPath);
                if (!transf)
                    transf = owner.myTransform;
            }
            //EffectComponent eComponent = owner.effectManager.AddEffect("shieldxiaoshi", transf);
            AudioManager.Instance.SoundPlay("盾-收起",0.2f);
        }

        protected override void OnInterrupt(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            OnDetach(owner, buffData);
        }
    }
}