using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    [AddComponentMenu("Spell/SpellEx")]
    public abstract class SpellEx : Spell
    {
        public string name = "";
        public int level = 0;
        public string icon = ""; //图标，格式： 图集目录；图标名称  例：GUI/chuangshi;skill_blue
        public string memo = "";
        public string description = "";
        public TargetType targetType = TargetType.None;  // 标识技能施法时所需要的目标
        public int cooldownID = 0;
        public float cooldownTime = 0.0f;
        public int mpCost = 0;
        public bool isTriggerDo = false; // false：普通攻击 true：技能
        public ConditionBase[] conditions = null;  // 施法者条件限制
        public SpellAnimation casterAnimation;  // 施法者动作、光效、声音配置
        public ChantBehaviour chantBehaviour = null;  // 施法者吟唱表现（暂未支持）

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            // 常规表现模块
            DataSection.DataSection generalPerformance = dataSection["generalPerformance"];
            name = generalPerformance.readString("name");
            level = generalPerformance.readInt("level");
            icon = generalPerformance.readString("icon");
            memo = generalPerformance.readString("memo");
            description = generalPerformance.readString("description");
            // 常规功能模块
            DataSection.DataSection generalFunction = dataSection["generalFunction"];
            targetType = (TargetType)generalFunction.readInt("targetType");
            cooldownTime = generalFunction.readFloat("cooldownTime");
            cooldownID = generalFunction.readInt("cooldownID");
            mpCost = generalFunction.readInt("mpCost");
            isTriggerDo = generalFunction.readBool("isTriggerDo");
            if (generalFunction["conditions"] != null)
            {
                List<ConditionBase> conds = new List<ConditionBase>();

                foreach (var section in generalFunction["conditions"].values())
                {
                    var typeID = section.readInt("type");
                    var cond = ConditionBase.CreateCondition(typeID);
                    if (cond != null)
                    {
                        cond.Init(section);
                        conds.Add(cond);
                    }
                }
                conditions = conds.ToArray();
            }
            // 战斗表现模块
            DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            casterAnimation = new SpellAnimation();
            DataSection.DataSection casterAnimationData = combatPerformance["casterAnimation"];
            if (casterAnimationData != null)
            {
                casterAnimation.Init(casterAnimationData);
            }

            //if (CConfigClass.skillDescriptionConfig.ContainsKey(id))
            //{
            //    description = CConfigClass.skillDescriptionConfig[id].description;
            //}
        }

        /* 施法
		 * @param caster: 施法者
		 * @return: 返回施法成功或失败的状态码
		 */
        public override SpellStatus Cast(AvatarComponent caster, SpellTargetData targetData)
        {
            SpellStatus status = SpellStatus.OK;

            // 判断施法条件
            status = CanStart(caster, targetData);
            if (status != SpellStatus.OK)
                return status;

            // 设置施法者当前的施放的技能
            Lock(caster.gameObject, this);

            // 设置CD时间
            caster.cooldowns.Set(cooldownID, cooldownTime);

            // 开始施法，主要是播放动画
            var coroutine = PreFire(caster, targetData);
            caster.SetMapping("coroutine", coroutine);  // 保存临时协程数据，以备后续使用
            caster.StartCoroutine(coroutine);
            return status;
        }

        /// <summary>
        /// 从服务器过来的施法应该调用这个接口
        /// </summary>
        //public override void CastFromServer(AvatarComponent caster, SpellTargetData targetData)
        //{
        //    //if (caster.entity.objectType == CEntityType.Player && !caster.entity.isPlayer())
        //    //{
        //    //    caster.filter.enabled = false;
        //    //}
        //    //Debug.Log(string.Format("SpellEx::CastFromServer(), caster id = {0}, spell id = {1}", caster.entity.id, id));

        //    // 对于来自服务器的技能施法，我们总是优先响应
        //    Spell spell = CurrentLocked(caster.gameObject);
        //    if (spell != null)
        //        spell.Stop(caster, 0);

        //    // 设置施法者当前的施放的技能
        //    Lock(caster.gameObject, this);

        //    var coroutine = PreFire(caster, targetData);
        //    caster.SetMapping("coroutine", coroutine);  // 保存临时协程数据，以备后续使用
        //    caster.StartCoroutine(coroutine);
        //}

        /* 检查施法者是否允许施法，此接口应该在施法前进行判断
		 * 1.检查技能cd；
		 * 2.检查施展目标是否正确；
		 * @param caster: 施法者
		 * @return: 如果允许施法，则返回SpellStatus.OK状态，否则返回代表具体错误的状态码
		 */
        public override SpellStatus CanStart(AvatarComponent caster, SpellTargetData targetData)
        {
            // 是否禁止施法
            if (caster.HasActionRestrict(eActionRestrict.ForbidSpell))
                return SpellStatus.FORBID_ACTION_LIMIT;

            // 是否正在施展技能（这个是针对服务器主控）
            //if (caster.entity.hasEffectStatus(eEffectStatus.SpellCasting))
            //    return SpellStatus.CASTING;

            // 是否正在施展技能（这个是针对客户端主控）
            if (Locked(caster.gameObject))
                return SpellStatus.CASTING;

            // 检查CD
            if (!caster.cooldowns.IsTimeout(cooldownID))
            {
                return SpellStatus.COOLDOWNING;
            }

            // 检查MP是否足够
            //if (caster.entity.mp < mpCost)
            //{
            //    return SpellStatus.LACK_OF_MP;
            //}

            // 施法者条件检查
            if (conditions != null)
            {
                foreach (ConditionBase cond in conditions)
                {
                    SpellStatus result = cond.Verify(caster,null);
                    if (result != SpellStatus.OK)
                    {
                        return result;
                    }
                }
            }

            return SpellStatus.OK;
        }

        /* 开始执行技能行为。技能行为包括吟唱、处理消耗处理，执行效果，后摇等等
		 * 这个函数会在一个新的协程上执行，因此返回值必须是IEnumerator
		 */
        public IEnumerator PreFire(AvatarComponent caster, SpellTargetData targetData)
        {
            bool result = false;
            // 开始处理吟唱
            if (chantBehaviour != null)
            {
                try
                {
                    result = chantBehaviour.BehaviourStart(this, caster);
                }
                catch (System.Exception e)
                {
                    Debug.Log(this + "::PreFire(), 1.0 Exception: \n" + e);
                    goto fireOut;
                }

                while (result)
                {
                    yield return new WaitForFixedUpdate();
                    try
                    {
                        result = chantBehaviour.BehaviourUpdate(this, caster);
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(this + "::PreFire(), 1.1 Exception: \n" + e);
                        goto fireOut;
                    }
                }
            }

            caster.SendMessage("OnSpellFireMSG", this, SendMessageOptions.DontRequireReceiver);
            if (isTriggerDo)
            {
                //caster.entity.eventObj.fire("Event_OnSpellFireMSG", new object[] { name });
            }

            //if (caster.entity.objectType == CEntityType.Player || (caster.entity.objectType == CEntityType.NPC && caster.entity.controlledFlag))
            //{
            //    // 设置施法动作的碰撞类型
            //    caster.animatPosSyncFlag = casterAnimation.collisionType;
            //}

            // 开始播放施法动作、光效、声音,并将效果实例存储起来
            casterAnimation.Do(caster);

            // begin fire spell, do spell effect actual.
            try
            {
                result = FireStart(caster, targetData);
            }
            catch (System.Exception e)
            {
                Debug.Log(this + "::PreFire(), 3.0 Exception: \n" + e);
                goto fireOut;
            }

            if (result)
            {
                do
                {
                    yield return new WaitForFixedUpdate();
                    try
                    {
                        result = FireUpdate(caster, targetData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(this + "::PreFire(), 3.1 Exception: \n" + e);
                        goto fireOut;
                    }
                } while (result);
            }

        fireOut:
            //Debug.Log(this + "::PreFire(), spell end.");
            // 技能结束后0.3秒销毁光效和音效，该数值由策划根据配置提供
            Stop(caster, 0.3f);
            yield break;
        }

        /// <summary>
        /// 停止/中断此技能的施法
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="dalayTime"></param>
        public override void Stop(AvatarComponent caster, float dalayTime)
        {
            //caster.animatPosSyncFlag = eAnimatPosSyncFlag.NotSync;
            //if (caster.entity.objectType == CEntityType.Player && !caster.entity.isPlayer())
            //{
            //    caster.filter.enabled = true;
            //}

            //Debug.Log(string.Format("SpellEx::Stop(), caster id = {0}, spell id = {1}", caster.entity.id, id));
            Spell spell = CurrentLocked(caster.gameObject);
            if (spell == null)
                return;

            if (spell != this)
            {
                //throw new System.Exception(string.Format("Spell not match!!! this = {0}, locked = {1}, entity id = {2}", id, spell.id, caster.entity.id));
            }

            if(caster.effectManager != null)
                caster.effectManager.EffectListCastClear();

            // 施法完成则重置当前的技能施放状态
            var coroutine = (IEnumerator)caster.PopMapping("coroutine");
            caster.StopCoroutine(coroutine);
            Unlock(caster.gameObject);


            try
            {
                // 结束回调，以让技能清理自身状态
                OnOver(caster);
            }
            catch (System.Exception e)
            {
                Debug.Log(this + "::PreFire(), 5.0 Exception: \n" + e);
            }
            finally
            {
                // 产生技能施展结束的消息
                caster.SendMessage("OnSpellCastOverMSG", this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// 技能效果实际执行的地方，这是开始，仅执行一次。
        /// 注：此机制与SpellBehaviour的机制概念一样，仅是为技能的实现提供另一种方案而已
        /// </summary>
        /// <returns><c>true</c>, if want to continue call FireUpdate(), <c>false</c> otherwise.</returns>
        public virtual bool FireStart(AvatarComponent caster, SpellTargetData targetData)
        {
            return false;
        }

        /// <summary>
        /// 技能效果实际执行的地方，这是每tick更新的地方。
        /// 注：此机制与SpellBehaviour的机制概念一样，仅是为技能的实现提供另一种方案而已
        /// </summary>
        /// <returns><c>true</c>, iif want to continue call FireUpdate(), <c>false</c> otherwise.</returns>
        public virtual bool FireUpdate(AvatarComponent caster, SpellTargetData targetData)
        {
            return false;
        }

        /* 施法结束，清理技能自身特有的数据
		 */
        protected virtual void OnOver(AvatarComponent caster)
        {

        }

    }

}
