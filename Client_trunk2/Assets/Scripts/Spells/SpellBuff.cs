using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    [System.Serializable]
    public class SpellBuff : SpellEffect
    {
        public int level = 0;
        public string icon = "";
        public string memo = "";
        public SpellAnimation buffAnimation;
        public float stayTime = 0.0f;  //stayTime < 0 代表buff无限时长
        public float tickTime = 0.0f;  //tickTime < 0 代表buff没有心跳
        public int tickCounts = 0;
        public int[] interruptCodes;
        public ConditionBase[] conditions = null;

        private static Int32 m_buffIndex = 0;

        public static Int32 GetBuffIndex()
        {
            return ++m_buffIndex;
        }

        public override void Init(DataSection.DataSection dataSection)
        {
            id = dataSection.readInt("id");
            // 常规表现模块
            DataSection.DataSection generalPerformance = dataSection["generalPerformance"];
            name = generalPerformance.readString("name");
            level = generalPerformance.readInt("level");
            memo = generalPerformance.readString("memo");
            description = generalPerformance.readString("description");

            buffAnimation = new SpellAnimation();
            DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            if (combatPerformance != null)
            {
                buffAnimation.Init(combatPerformance);
            }

            DataSection.DataSection generalFunction = dataSection["generalFunction"];
            stayTime = generalFunction.readFloat("stayTime");
            tickTime = generalFunction.readFloat("tickTime");
            interruptCodes = generalFunction.readIntArray("interruptCode",',');

            if (tickTime <= 0.0f)
            {
                tickCounts = 0;
            }
            else
            {
                if (stayTime > 0)
                    tickCounts = (int)(stayTime / tickTime);
                else
                    tickCounts = 1;
            }

            if (generalFunction["conditions"] != null)
            {
                List<ConditionBase> conds = new List<ConditionBase>();

                foreach (var section in generalFunction["conditions"].values())
                {
                    var cond = ConditionBase.CreateCondition(section.asInt);
                    if (cond != null)
                    {
                        cond.Init(section);
                        conds.Add(cond);
                    }
                }
                conditions = conds.ToArray();
            }

        }

   //     private static Dictionary<string, System.Type> s_classTypeMap = new Dictionary<string, System.Type>()
   //     {
   //         //目前buff客户端无表现，直接使用SpellBuff类型
			//{"__default__", typeof(BuffSimple)},
   //         {"BuffSimple", typeof(BuffSimple)},
   //         {"BuffBullet", typeof(BuffBullet)},
   //         {"BuffPush", typeof(BuffPush)},
   //         {"BuffSummon", typeof(BuffSimple)},
   //         {"BuffFrozen", typeof(BuffSimple)},
   //         {"BuffShield", typeof(BuffShield)},
   //         {"BuffLightning", typeof(BuffLightning)},
   //         {"BuffAbsorb", typeof(BuffAbsorb)},
   //     };

   //     public new static System.Type GetClassType(string type)
   //     {
   //         if (!s_classTypeMap.ContainsKey(type))
   //             return null;

   //         return s_classTypeMap[type];
   //     }

   //     public static SpellBuff CreateSpellBuff(string type)
   //     {
   //         if (!s_classTypeMap.ContainsKey(type))
   //         {
   //             // 没有相同类型存在的时候，就使用默认的类型
   //             type = "__default__";
   //         }

   //         var obj = System.Activator.CreateInstance(s_classTypeMap[type]) as SpellBuff;
   //         return obj;
   //     }

        #region Buff Process Function
        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            if (OnCast(src, dst, spell) != SpellStatus.OK)
                return;

            //来到这里说明buff已经满足添加条件了
            Attach(src, dst, spell, targetData);
        }

        /// <summary>
        /// 这里用来检测buff是否有足够条件添加到目标上，例如buff的覆盖和叠加
        /// </summary>
        /// <param name="src">施法者</param>
        /// <param name="dst">目标</param>
        /// <param name="spell">技能</param>
        /// <returns></returns>
        public virtual SpellStatus OnCast(AvatarComponent src, AvatarComponent dst, SpellEx spell)
        {
            //目标条件检查
            if (conditions != null)
            {
                foreach (ConditionBase cond in conditions)
                {
                    SpellStatus result = cond.Verify(src, dst);
                    if (result != SpellStatus.OK)
                    {
                        return result;
                    }
                }
            }

            return SpellStatus.OK;
        }

        /// <summary>
        /// 把buff附到owner身上
        /// </summary>
        /// <param name="owner">拥有这个buff的人</param>
        /// <param name="buffData">存储的buff数据</param>
        public virtual void Attach(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            Alias.BuffDataType buffData = new Alias.BuffDataType();
            buffData.index = GetBuffIndex();
            buffData.buffID = id;
            buffData.casterID = src.id;
            buffData.counter = tickCounts;
            buffData.endTime = Time.time + stayTime;
            if(targetData != null)
                buffData.temp["SpellTargetData"] = targetData;
            OnAttach(dst, buffData);

            //开启buff的生命周期
            if (stayTime > 0.0f)
            {
                var stayProcess = StayProcess(dst, buffData);
                buffData.temp["stayProcess"] = stayProcess;
                dst.StartCoroutine(stayProcess);
            }
            //开启buff的心跳周期
            if (tickTime > 0.0f)
            {
                var tickProcess = TickProcess(dst, buffData);
                buffData.temp["tickProcess"] = tickProcess;
                dst.StartCoroutine(tickProcess);
            }
            dst.AddBuff(buffData);
        }

        /// <summary>
        /// 把buff从owner身上取下来
        /// </summary>
        /// <param name="owner">拥有这个buff的人</param>
        /// <param name="buffData">存储的buff数据</param>
        public virtual void Detach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            if (buffData.temp.ContainsKey("stayProcess"))
            {
                var stayProcess = (IEnumerator)buffData.temp["stayProcess"];
                buffData.temp.Remove("stayProcess");
                owner.StopCoroutine(stayProcess);
            }

            if (buffData.temp.ContainsKey("tickProcess"))
            {
                var stayProcess = (IEnumerator)buffData.temp["tickProcess"];
                buffData.temp.Remove("tickProcess");
                owner.StopCoroutine(stayProcess);
            }

            owner.RemoveBuff(buffData.index);
            OnDetach(owner, buffData);
        }

        /// <summary>
        /// buff被打断
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="buffData"></param>
        /// <param name="interruptCode"></param>
        public virtual void Interrupt(AvatarComponent owner, Alias.BuffDataType buffData, Int32 interruptCode)
        {
            bool flag = false;
            foreach (Int32 Code in interruptCodes)
            {
                if (Code == interruptCode)
                {
                    flag = true;
                    break;
                }     
            }

            if (!flag)
                return;

            if (buffData.temp.ContainsKey("stayProcess"))
            {
                var stayProcess = (IEnumerator)buffData.temp["stayProcess"];
                buffData.temp.Remove("stayProcess");
                owner.StopCoroutine(stayProcess);
            }

            if (buffData.temp.ContainsKey("tickProcess"))
            {
                var stayProcess = (IEnumerator)buffData.temp["tickProcess"];
                buffData.temp.Remove("tickProcess");
                owner.StopCoroutine(stayProcess);
            }

            OnInterrupt(owner, buffData);
            owner.RemoveBuff(buffData.index); 
        }

        /// <summary>
        /// 处理buff的生命周期
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="buffData"></param>
        /// <returns></returns>
        public virtual IEnumerator StayProcess(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            //yield return new WaitForSeconds(stayTime);
            while (Time.time < buffData.endTime)
            {
                yield return new WaitForEndOfFrame();
            }
            Detach(owner, buffData);
        }

        /// <summary>
        /// 处理buff的心跳周期
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="buffData"></param>
        /// <returns></returns>
        public virtual IEnumerator TickProcess(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            WaitForSeconds delay = new WaitForSeconds(tickTime);
            yield return delay;

            while (buffData.counter > 0 && OnTick(owner, buffData))
            {
                if(stayTime > 0.0f)
                    --buffData.counter;
                yield return delay;
            }
        }


        /// <summary>
        /// 当buff附到owner身上时，此接口被调用（仅调用一次）
        /// 可以在此处做一些前期初始化的事情
        /// 例如：给owner加上10点基础伤害
        /// </summary>
        protected virtual void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
        }

        /// <summary>
        /// 当buff从owner身上取下来时，此接口被调用（仅调用一次）
        /// 可以在此处做一些buff结束时的事情
        /// 例如：给owner减去10点基础伤害
        /// </summary>
        protected virtual void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
        }

        protected virtual void OnInterrupt(AvatarComponent owner, Alias.BuffDataType buffData)
        {
        }

        /// <summary>
        /// buff的心跳，每一跳都有可能会做一些事情
        /// </summary>
        /// <returns>true: buff需要继续心跳；false: buff不需要继续心跳了，亦即应该从玩家身上卸去了</returns>
        protected virtual bool OnTick(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            return true;
        }
        #endregion Buff Process Function

    }
}
