# -*- coding: utf-8 -*-

"""
This module implements Entity Callback Extend.
"""

########################################################################
#
# 模块: ECBExtend
# 功能: 实现KBEngine.Entity的Callback扩展类ECBExtend
#       包括下列回调
#         onTimer(controllerID, userData)
#       相关的函数
#         addTimer
#
########################################################################
import KBEngine
from KBEDebug import *

#
# Timer Callback ID section from 0x2001-0x3000
#
TIMER_ON_TEAM_CLEAR_REFUSE_PLAYER      = 2001    # 清理被拒绝的加入队伍申请的玩家
TIMER_ON_RELATION_CLEAR_ADDFRIEND_PLAYER = 2002  # 清理超时没处理的加好友请求
TIMER_ON_INIT_RELATION_DATA = 2003				 # 定时初始化好友关系数据

TIMER_ON_DESTROY_PLAYER = 2004					 # 延时销毁玩家的base


#
# 全局Callback字典，用来从指定的ID查询回调函数
#
gcbs = {}

def register_entity_callback( callbackID, func ):
	"""
	注册timer回调函数
	
	@param targetClass: entity类
	@param     mapDict: userData与回调函数的映射表，类似于 { TIMER_ON_BUFFTICK : Avatar.onTimer_buffTick, ... }
	"""
	global gcbs
	gcbs[callbackID] = func

#
# KBEngine.Entity的Callback扩展类ECBExtend
#
# 注意: 本Class不能被单独实例化，只能与KBEngine.Entity同时被ConcreteEntity类继承才有意义
#
class ECBExtend:
	def onTimer( self, timerID, cbID ):
		"""
		功能: 实现KBEngine.Entity.onTimer，当某个Timer Tick到达时被调用
		      转向该cbID对应的回调函数
		"""
		global gcbs
		cb = gcbs[cbID]
		cb( self, timerID, cbID )

# End of ECBExtend.py
