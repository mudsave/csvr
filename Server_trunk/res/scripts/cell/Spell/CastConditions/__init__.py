# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .ConditionBase import ConditionBase
from .RelationCondition import RelationCondition
from .StatusCondition import StatusCondition
from .EffectStatusCondition import EffectStatusCondition
from .BuffCondition import BuffCondition
from .ProbabilityCondition import ProbabilityCondition
from .ActionRestrictCondition import ActionRestrictCondition

# 类型映射表
# 注意：此表需要与客户端的保持一致
# key = int32
# value = class which inherit from ConditionBase
g_TYPE_MAPPING = {
	1 : ConditionBase,
	2 : RelationCondition,
	3 : StatusCondition,
	4 : EffectStatusCondition,
	5 : BuffCondition,
	6 : ProbabilityCondition,
	7 : ActionRestrictCondition,
}

def createCondition( typeID ):
	"""
	通过类型标识符创建相应的施法条件实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( typeID )
	if c is not None:
		result = c()
	return result
