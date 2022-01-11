using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    /// <summary>
    /// 基于位置触发效果的玩家技能类型
    /// </summary>
    [AddComponentMenu("Spell/PlayerSpellExToPos")]
    public class PlayerSpellExToPos : SpellExFightWithTime
    {
        public string posEffect = "";
        public string sound = "";

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            posEffect = dataSection["combatPerformance"].readString("positionEffect");
            sound = dataSection["combatPerformance"].readString("sound");

        }

        public override bool FireStart(AvatarComponent caster, SpellTargetData targetData)
        {
            //在指定位置释放光效
            if (posEffect != "")
            {
                EffectComponent eComponent = caster.effectManager.AddEffect(posEffect, targetData.pos);
                eComponent.gameObject.transform.rotation = caster.myTransform.rotation;

                if (sound != "" && AudioManager.Instance != null)
                    AudioManager.Instance.SoundPlay(sound, 1, 0, true, eComponent.gameObject);

                BlackHoleSkill bs = eComponent.gameObject.AddComponent<BlackHoleSkill>();
                bs.Init(caster);
            }

            _CurveBaseDatas cDatas = new _CurveBaseDatas();
            cDatas.currTrigger = CurveIndex2Value(0);
            cDatas.totalTime = intervalTimeTotal();
            caster.SetMapping("CurveBaseDatas", cDatas);
            return true;
        }

        public override void OnHit(AvatarComponent caster, SpellTargetData targetData, int curveIndex)
        {
        }
    }
}

