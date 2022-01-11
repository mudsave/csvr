# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .NullFinder import NullFinder
from .RoundFinder import RoundFinder
from .SectorFinder import SectorFinder
from .OffsetRoundFinder import OffsetRoundFinder
from .OneSelfFinder import OneSelfFinder

# 类型映射表
# key = int32
# value = class which inherit from ObjectFinder
g_TYPE_MAPPING = {
	1 : NullFinder,
	2 : RoundFinder,
	3 : SectorFinder,
	4 : OffsetRoundFinder,
	5 : OneSelfFinder,
}

def createObjectFinder( typeID ):
	"""
	通过类型标识符创建相应的施法条件实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( typeID )
	if c is not None:
		result = c()
	return result
