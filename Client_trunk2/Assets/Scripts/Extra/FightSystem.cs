using UnityEngine;
using System.Collections;

//public class FightResult
//{
//    public bool hit = false;                                   //是否命中
//    public bool crit = false;                                  //是否暴击
//    public bool parry = false;                                 //是否招架
//    public bool shielded = false;                              //是否被护盾抵挡
//    public eDamageType damageType = eDamageType.physicsATK;    //伤害类型
//    public int damage = 0;                                     //伤害值
//    public int realDamage = 0;                                 //真实造成伤害值

//    public FightResult()
//    {
//    }

//    public FightResult(bool hit, bool crit, bool parry, bool shielded, eDamageType damageType, int damage, int realDamage)
//    {
//        this.hit = hit;
//        this.crit = crit;
//        this.parry = parry;
//        this.shielded = shielded;
//        this.damageType = damageType;
//        this.damage = damage;
//        this.realDamage = realDamage;
//    }
//}

public class FightSystem
{
    //public static int DamageType2ATK(eDamageType damageType, AvatarComponent component)
    //{
    //    if (damageType == eDamageType.physicsATK)
    //        return component.entity.physicsATK;
    //    else
    //        return component.entity.magicATK;
    //}

    //public static int DamageType2DEF(eDamageType damageType, AvatarComponent component)
    //{
    //    if (damageType == eDamageType.physicsATK)
    //        return component.entity.physicsDEF;
    //    else
    //        return component.entity.magicATK;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src">攻击者</param>
    /// <param name="dst">受击者</param>
    /// <param name="spell">技能</param>
    /// <param name="damageValue">伤害值</param>
    /// <param name="damageVol">波动率</param>
    public static void Fight(AvatarComponent src, AvatarComponent dst,SPELL.Spell spell, int damageValue, float damageVol, CDeadType deadType)
    {
        int value = (int)(damageValue * Random.Range(1 - damageVol, 1 + damageVol));
        dst.receiveDamage(src, value, deadType);
    }
}
