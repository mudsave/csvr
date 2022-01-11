# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .RandomSizer import RandomSizer

# 类型映射表
# key = int32
# value = class which inherit from ObjectFinder
g_TYPE_MAPPING = {
	1 : RandomSizer,
}

def createObjectSizer( typeID ):
	"""
	通过类型标识符创建相应的筛选器实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( typeID )
	if c is not None:
		result = c()
	return result
