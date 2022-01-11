# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from SrvDef import eEffectStatus, eActionRestrict

class TargetType:
	"""
	目标类型定义
	"""
	[
		none,       # 无目标无对象（None在python是一个关键字，没法只好用小写）
		Entity,     # 指定目标
		Position,   # 指定位置
		Direction,  # 指定方向
	] = range( 4 )  # 注意：増、减标志时这个数值需要做相应的修改

class SpellStatus(object):
	"""
	定义技能执行状态
	"""
	[
		OK,                           # 0 正常（成功）状态
		INVALID_TARGET_TYPE,          # 无效的目标类型
		NO_TARGET,                    # 没有攻击目标
		CASTING,                      # 当前正在施法中
		COOLDOWNING,                  # cd中
		TOOFAR,                       # 自己离施法目标太远
		OUT_OF_INDEX,                 # 超出技能管理器中的技能索引
		FORBID_ACTION_LIMIT,          # 某些禁止行为状态限制
		NO_SUCH_SPELL,                # 该技能不存在
		BUFF_EFFECT_SUPERPOSITION,    # BUFF效果叠加
		LACK_OF_MP,                   # MP不足
		PET_LIST_FULL,                # 幻兽栏已满
		TALISMAN_LIST_FULL,           # 法宝栏已满
	] = range( 13 )  # 注意：増、减状态时这个数值需要做相应的修改


# 效果状态改变时触发的中断码
EffectStatus2InterruptCode = [
	# 失去状态
	{
		eEffectStatus.Moving       : 2032,
		eEffectStatus.SpellCasting : 2033,
		eEffectStatus.HitBy        : 2034,
	},
	
	# 获得状态
	{
		eEffectStatus.Moving       : 1032,
		eEffectStatus.SpellCasting : 1033,
		eEffectStatus.HitBy        : 1034,
	},
]

# 行为限制改变时触发的中断码
ActionRestrict2InterruptCode = [
	# 失去状态
	{
		eActionRestrict.ForbidMove  : 2001,
		eActionRestrict.ForbidSpell : 2002,
		eActionRestrict.ForbidHitBy : 2003,
	},
	
	# 获得状态
	{
		eActionRestrict.ForbidMove  : 1001,
		eActionRestrict.ForbidSpell : 1002,
		eActionRestrict.ForbidHitBy : 1003,
	},
]


