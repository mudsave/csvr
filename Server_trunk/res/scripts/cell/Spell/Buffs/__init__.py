# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .BuffSimple import BuffSimple
from .BuffBullet import BuffBullet
from .BuffPush import BuffPush
from .BuffSummon import BuffSummon
from .BuffFrozen import BuffFrozen
from .BuffShield import BuffShield
from .BuffLightning import BuffLightning
from .BuffAbsorb import BuffAbsorb

# 类型映射表
# key = string
# value = class which inherit from Spell
g_TYPE_MAPPING = {
	"BuffSimple"              : BuffSimple,
	"BuffBullet"              : BuffBullet,
	"BuffPush"                : BuffPush,
	"BuffSummon"              : BuffSummon,
	"BuffFrozen"              : BuffFrozen,
	"BuffShield"              : BuffShield,
	"BuffLightning"           : BuffLightning,
	"BuffAbsorb"              : BuffAbsorb,
}

def createBuff( classType ):
	"""
	通过类型标识符创建相应的施法条件实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( classType )
	if c is not None:
		result = c()
	return result


