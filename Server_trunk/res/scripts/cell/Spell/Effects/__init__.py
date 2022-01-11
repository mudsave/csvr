# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .EffectDynamicDamage import EffectDynamicDamage
from .EffectHitPose import EffectHitPose
from .EffectDirectionChange import EffectDirectionChange
from .EffectLighting import EffectLighting
from .EffectDizziness import EffectDizziness
from .EffectPositionChange import EffectPositionChange
from .EffectInvincible import EffectInvincible
from .EffectProfitHPMax import EffectProfitHPMax
from .EffectHealHP import EffectHealHP
from .EffectSuckBlood import EffectSuckBlood
from .EffectProfitMPMax import EffectProfitMPMax
from .EffectHealMP import EffectHealMP
from .EffectSuperBody import EffectSuperBody
from .EffectProfitMoveSpeed import EffectProfitMoveSpeed
from .EffectProfitDEF import EffectProfitDEF
from .EffectFlash import EffectFlash
from .EffectInterruptBuff import EffectInterruptBuff

# 类型映射表
# key = string
# value = class which inherit from Spell
g_TYPE_MAPPING = {
	"EffectDynamicDamage"      : EffectDynamicDamage,
	"EffectHitPose"            : EffectHitPose,
	"EffectDirectionChange"    : EffectDirectionChange,
	"EffectLighting"           : EffectLighting,
	"EffectDizziness"          : EffectDizziness,
	"EffectPositionChange"     : EffectPositionChange,
	"EffectInvincible"         : EffectInvincible,
	"EffectProfitHPMax"        : EffectProfitHPMax,
	"EffectHealHP"             : EffectHealHP,
	"EffectSuckBlood"          : EffectSuckBlood,
	"EffectProfitMPMax"        : EffectProfitMPMax,
	"EffectHealMP"             : EffectHealMP,
	"EffectSuperBody"          : EffectSuperBody,
	"EffectProfitMoveSpeed"    : EffectProfitMoveSpeed,
	"EffectProfitDEF"          : EffectProfitDEF,
	"EffectFlash"              : EffectFlash,
	"EffectInterruptBuff"      : EffectInterruptBuff,
}


def createEffect( classType ):
	"""
	通过类型标识符创建相应的施法条件实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( classType )
	if c is not None:
		result = c()
	return result


from .. import Buffs
g_TYPE_MAPPING.update( Buffs.g_TYPE_MAPPING )

from .. import PassiveSkills
g_TYPE_MAPPING.update( PassiveSkills.g_TYPE_MAPPING )
