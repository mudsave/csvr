# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .PassiveSkillSimple import PassiveSkillSimple

# 类型映射表
# key = string
# value = class which inherit from Spell
g_TYPE_MAPPING = {
	"PassiveSkillSimple" : PassiveSkillSimple,
}

def createPassiveSkill( classType ):
	"""
	通过类型标识符创建相应的施法条件实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( classType )
	if c is not None:
		result = c()
	return result


