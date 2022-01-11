using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// VR玩家技能基类
    /// </summary>
    public abstract class PlayerSkillBase : MonoBehaviour
    {

        [Header("技能基础模块")]
        [Tooltip("技能名称")]
        public string skillName;

        [Tooltip("技能描述"), TextArea(1, 3)]
        public string description;

        [Tooltip("技能ID")]
        public int id;

        [Tooltip("技能消耗 [ 晶石数量 ]"), Range(0, 10)]
        public int magicSparCount;

        [Tooltip("技能施展 [ 时间 ]")]
        public float castTime;

        [Tooltip("指定施法的 [ 手 ]")]
        public Hand castHand;

        [Tooltip("技能施展的手部 [ 动作名称 ]")]
        public string handAnimation;

        protected float passTime;
        protected AvatarComponent player = null;
        protected Transform castHandTransform = null;
        protected Animator castHandAnimator = null;

        void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            player = VRInputManager.Instance.playerComponent;

            if (castHand == Hand.LEFT)
            {
                castHandTransform = VRInputManager.Instance.handLeft.transform;
                castHandAnimator = VRInputManager.Instance.handLeftAnimator;
            }
            else
            {
                castHandTransform = VRInputManager.Instance.handRight.transform;
                castHandAnimator = VRInputManager.Instance.handRightAnimator;
            }
        }

        /// <summary>
        /// 施法
        /// </summary>
        /// <returns></returns>
        public SpellStatus Cast()
        {
            SpellStatus status = CanCast();

            if(status == SpellStatus.OK)
                player.StartCoroutine(Fire());

            return status;
        }

        /// <summary>
        /// 判断是否满足条件施法
        /// </summary>
        /// <returns></returns>
        public virtual SpellStatus CanCast()
        {
            //判断是否在施法中
            if (player.HasEffectStatus(eEffectStatus.SpellCasting))
                return SpellStatus.CASTING;

            //判断是否被禁止施法
            if (player.HasActionRestrict(eActionRestrict.ForbidSpell))
                return SpellStatus.FORBID_ACTION_LIMIT;

            //判断魔法晶石是否足够
            if (!player.CheckMagicSparCount(magicSparCount))
                return SpellStatus.LACK_OF_MP;

            return SpellStatus.OK;
        }

        protected IEnumerator Fire()
        {
            FireStart();
            GlobalEvent.fire("GuideEvent", GuideEvent.CastSkill);

            bool result = true;
            while (result)
            {
                yield return new WaitForEndOfFrame();
                result = (CastTimer() & FireUpdate());
            }

            FireEnd();
        }

        protected virtual void FireStart()
        {
            //设置正在施法状态
            player.EffectStatusCounterIncr((int)eEffectStatus.SpellCasting);

            //魔法晶石的扣除
            player.MagicSparCountChange(-magicSparCount);

            //播放手部动作
            if (castHandAnimator)
            {
                castHandAnimator.SetBool(handAnimation, true);
            }
        }

        protected virtual bool FireUpdate()
        {
            return true;
        }

        protected virtual void FireEnd()
        {
            //解除正在施法状态
            player.EffectStatusCounterDecr((int)eEffectStatus.SpellCasting);

            if (castHandAnimator)
            {
                castHandAnimator.SetBool(handAnimation, false);
            }
        }

        protected virtual bool CastTimer()
        {
            passTime += Time.unscaledDeltaTime;

            if (passTime >= castTime)
            {
                passTime = 0.0f;
                return false;
            }
            return true;
        }
    }
}


