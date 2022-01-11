# -*- coding: utf-8 -*-

"""
This module implements Entity Callback Extend.
"""

########################################################################
#
# 模块: ECBExtend
# 功能: 实现KBEngine.Entity的Callback扩展类ECBExtend
#       包括下列回调
#         onMove(controllerID, userData)
#         onMoveFailure(controllerID, userData)
#         onNavigate(controllerID, userData)
#         onNavigateFailed(controllerID, userData)
#         onTurn(controllerID, userData)
#         onTimer(controllerID, userData)
#       相关的函数
#         moveToEntity
#         moveToPoint
#         navigate
#         navigateFollow
#         navigateStep
#         addYawRotator
#         addTimer
#
########################################################################
import KBEngine
from KBEDebug import *

#from Traps.TrapLoader import g_trapLoader

# 在此文件中定义的回调ID号的最大值，
# 超过这个值的ID号另有其它系统使用。
# 谨记！！！
MAX_CBID = 99999


#
# Timer Callback ID section from 2001-3000
#
TIMER_ON_THINKING            = 2001    # think机制的回调
TIMER_ON_SPELL_CASTING       = 2002    # 技能施展定时回调
TIMER_ON_BUFFTICK            = 2003    # BUFF心跳定时回调
TIMER_ON_PK_KILL_WAITING     = 2004    # 杀戮等待时间结束回调
TIMER_ON_PK_PASSIVE_KILL     = 2005    # 被动杀戮（还击）模式结束回调
TIMER_ON_NPC_DEAD            = 2006    # NPC死亡销毁定时回调
TIMER_ON_DELAY_INIT          = 2007    # 延迟初始化数据
TIMER_ON_DESTROY_SPACE       = 2008    # 异步销毁space（为了避开一个引起cellapp崩溃的bug）
TIMER_ON_RAGE_CONSUME		 = 2009	   # 怒气值消耗
TIMER_ON_STATUS_FIGHTING     = 2010    # 战斗状态结束回调
TIMER_ON_GEAR_DEAD           = 2011    # 机关销毁定时回调
TIMER_ON_GEAR_DELAY_INIT     = 2012    # 机关延迟初始化数据
TIMER_ON_AI_SIDESWAY_UPDATE  = 2013	   # AI的Update回调

#
# Timer Callback ID section from 3001-4000 (space)
#
SPACE_TRIGGER_ID = 3001 #触发器定时器ID
SPACE_TRIGGER_CREATE_TMP = 3002 #触发器管理
SPACE_DUPLICATION_LEAVE = 3003	#离开副本定时回调

#
# addProximity() Callback ID section from 5001-6000 (onEnterTrap()、onLeaveTrap())
#
TRAP_ON_ALERT_RADIUS     = 5001    # 有entity进入、离开警戒半径


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
		if cbID not in gcbs:
			self.onRawTimer( timerID, cbID )
			return
		
		cb = gcbs[cbID]
		cb( self, timerID, cbID )

	def onMove( self, controllerID, callbackFunc ):
		"""
		功能: 实现KBEngine.Entity.onMove，直接调用userData所指向的函数
		
		@param controllerID: moveToPoint或者moveToEntity返回的controllerID
		@param callbackFunc: Callback function
		"""
		if callable( callbackFunc ):
			callbackFunc( self, controllerID, 0 )

	def onMoveFailure( self, controllerID, callbackFunc ):
		"""
		功能: 实现KBEngine.Entity.onMoveFailure，直接调用userData所指向的函数
		
		@param controllerID: moveToPoint或者moveToEntity返回的controllerID
		@param callbackFunc: Callback function
		"""
		if callable( callbackFunc ):
			callbackFunc( self, controllerID, -1 )

	def onMoveOver( self, controllerID, callbackFunc ):
		"""
		功能: 实现KBEngine.Entity.onMoveOver，直接调用userData所指向的函数
		
		@param controllerID: controllerID
		@param callbackFunc: Callback function
		"""
		if callable( callbackFunc ):
			callbackFunc( self, controllerID, 1 )

	def onEnterTrap( self, entityEntering, rangeXZ, rangeY, controllerID, cbID ):
		"""
		功能: 实现KBEngine.Entity.addProximity()的回调分发
		"""
		if cbID <= MAX_CBID:
			global gcbs
			cb = gcbs[cbID]
			cb( self, True, entityEntering, rangeXZ, rangeY, controllerID, cbID )
		else:
			trap = g_trapLoader.get( cbID )
			trap and trap.enter( self, entityEntering )

	def onLeaveTrap( self, entityEntering, rangeXZ, rangeY, controllerID, cbID ):
		"""
		功能: 实现KBEngine.Entity.addProximity()的回调分发
		"""
		if cbID <= MAX_CBID:
			global gcbs
			cb = gcbs[cbID]
			cb( self, False, entityEntering, rangeXZ, rangeY, controllerID, cbID )
		else:
			trap = g_trapLoader.get( cbID )
			trap and trap.leave( self, entityEntering )




# End of ECBExtend.py
