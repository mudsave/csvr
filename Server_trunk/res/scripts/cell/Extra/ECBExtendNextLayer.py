# -*- coding: utf-8 -*-

"""
This module implements Entity Callback Extend.
"""


import KBEngine
from KBEDebug import *


#
# 全局Callback字典，用来从指定的ID查询回调函数
#
gcbs = {}

def register_callback( callbackID, func ):
	"""
	注册timer回调函数
	
	@param targetClass: entity类
	@param     mapDict: userData与回调函数的映射表，类似于 { TIMER_ON_BUFFTICK : Avatar.onTimer_buffTick, ... }
	"""
	global gcbs
	gcbs[callbackID] = func

#
# ECBExtend类的二级定时器
#
#
class ECBExtendNextLayer:
	def onTimer( self, last, timerID, cbID ):
		"""
		功能: 实现KBEngine.Entity.onTimer，当某个Timer Tick到达时被调用
		      转向该cbID对应的回调函数
		"""
		global gcbs
		
		cb = gcbs[cbID]
		cb( self, last, timerID, cbID )
	
