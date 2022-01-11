using UnityEngine;
using System.Collections;

namespace SPELL
{

    public enum SpellStatus
    {
        OK = 0,                       // 正常（成功）状态
        INVALID_TARGET_TYPE,          // 无效的目标类型
        NO_TARGET,                    // 没有攻击目标
        CASTING,                      // 当前正在施法中
        COOLDOWNING,                  // cd中
        TOOFAR,                       // 自己离施法目标太远
        OUT_OF_INDEX,                 // 超出技能管理器中的技能索引
        FORBID_ACTION_LIMIT,          // 某些禁止行为状态限制
        NO_SUCH_SPELL,                // 该技能不存在
        BUFF_EFFECT_SUPERPOSITION,    // BUFF效果叠加
        LACK_OF_MP,                   //MP不足
    };

    public enum TargetType
    {
        None = 0,   // 无目标无对象
        Entity,     // 指定目标
        Position,   // 指定位置
        Direction,  // 指定方向
    };
}
