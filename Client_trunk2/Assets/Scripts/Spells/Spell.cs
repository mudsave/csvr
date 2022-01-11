using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SPELL
{
    public abstract class SpellBase
    {
        public int id = 0;         // spell uid
        public float distance = 0.0f;  // 标识技能施法时施法者与目标的距离。

        public virtual void Init(DataSection.DataSection dataSection)
        {
            id = dataSection.readInt("id");
            distance = dataSection["generalFunction"].readFloat("distance");
        }
    }

    /// <summary>
    /// 保存技能施法目标与类型
    /// </summary>
    public class SpellTargetData
    {
        public GameObject gameObject = null;
        public Vector3 pos = Vector3.zero;
        public Vector3 dir = Vector3.zero;
    }

    public abstract class Spell : SpellBase
    {
        /// <summary>
        /// 技能类型定义
        /// </summary>
        private static Dictionary<string, System.Type> s_classTypeMap = new Dictionary<string, System.Type>()
        {
            {"SpellExFightWithCurve", typeof(SpellExFightWithCurve)},
            {"SpellExFightWithTime", typeof(SpellExFightWithTime)},
            {"SpellExCollider", typeof(SpellExCollider)},
            {"PlayerSpellExToPos", typeof(PlayerSpellExToPos)},
        };

        public static System.Type GetClassType(string type)
        {
            if (!s_classTypeMap.ContainsKey(type))
                return null;

            return s_classTypeMap[type];
        }

        public static Spell CreateSpell(string type)
        {
            if (!s_classTypeMap.ContainsKey(type))
                return null;

            var obj = System.Activator.CreateInstance(s_classTypeMap[type]) as Spell;
            return obj;
        }



        static Dictionary<int, Spell> s_castMutex = new Dictionary<int, Spell>();

        public static bool Locked(GameObject caster)
        {
            Spell outValue;
            if (s_castMutex.TryGetValue(caster.GetInstanceID(), out outValue))
            {
                if (outValue != null)
                    return true;
            }
            return false;
        }

        public static bool Lock(GameObject caster, Spell spell)
        {
            if (Locked(caster))
                return false;

            s_castMutex[caster.GetInstanceID()] = spell;
            return true;
        }

        public static void Unlock(GameObject caster)
        {
            s_castMutex.Remove(caster.GetInstanceID());
        }

        public static Spell CurrentLocked(GameObject caster)
        {
            Spell outValue;
            s_castMutex.TryGetValue(caster.GetInstanceID(), out outValue);
            return outValue;
        }

        /// <summary>
        /// src want to cast this spell.
        /// </summary>
        /// <param name="src">spell caster.</param>
        public virtual SpellStatus Cast(AvatarComponent caster, SpellTargetData targetData)
        {
            return SpellStatus.OK;
        }

        /// <summary>
        /// 检测技能能否施展
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="targetData">目标</param>
        /// <returns>spell caster.</returns>
        public virtual SpellStatus CanStart(AvatarComponent caster, SpellTargetData targetData)
        {
            return SpellStatus.OK;
        }

        /// <summary>
        /// 停止/中断此技能的施法
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="dalayTime">效果销毁的延时时间（光效和音效）</param>
        public virtual void Stop(AvatarComponent caster, float dalayTime)
        {
        }

        /// <summary>
        /// 从服务器过来的施法应该调用这个接口
        /// </summary>
        public virtual void CastFromServer(AvatarComponent caster, SpellTargetData targetData)
        {

        }
    }
}
