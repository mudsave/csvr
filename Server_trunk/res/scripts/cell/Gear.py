# -*- coding: utf-8 -*-
#

"""
"""

import KBEngine
from KBEDebug import *

from interfaces.GameObject import GameObject
from SrvDef import eObjectType
from Extra import ECBExtend
import Extra.SpaceEventID as SpaceEventID

class Gear( GameObject ):
	"""
	游戏机关基础类
	"""
	def __init__(self):
		"""
		构造函数
		"""
		GameObject.__init__(self)
		self.objectType = eObjectType.Gear
		
		self.addTimer( 1.0, 0.0, ECBExtend.TIMER_ON_GEAR_DELAY_INIT )

	def getName( self ):
		"""
		virtual method.
		@return: the name of entity
		@rtype:  STRING
		"""
		return self.name

	def onDead( self ):
		"""
		销毁
		"""	
		self.addTimer( 5.0, 0, ECBExtend.TIMER_ON_GEAR_DEAD )

	def onTimer_destroy( self, timerID, userArg ):
		"""
		死亡销毁定时回调
		"""		
		#注销场景事件
		self.destroy()
	
	def onTimer_delayInit( self, timerID, userArg ):
		"""
		做一些延时初始化。目的地二：
		一是使一些无法在__init__()上初始化的功能得到正确的初始化，例如：npc创建以后需要放个陷阱；
		二是为了让AI晚些触发，以保证客户端上存在这个entity了才做进一步的事情，
		否则有可能导致有些消息发送到客户端后找不到对象接收，使得某些功能可能出现异常。
		"""
		"""
		增加场景事件
		"""
		spaceBase = self.getCurrentSpaceBase()
		space = KBEngine.entities[spaceBase.id]	
		space.registerEvent(SpaceEventID.GearSyncMsg, self)
	
	def onEvent(self, name, *args):
		if name == SpaceEventID.GearSyncMsg:
			self.OnSpaceEvent_GearSyncMsg(*args)
	
	def OnSpaceEvent_GearSyncMsg(self, relevanceID, msgName, msg):
		if self.relevanceID != relevanceID:
			return
		
		self.sendSyncMsg(self.id, msgName, msg)
	
	def doEvent(self, srcEntityID, id):
		"""
		执行事件ID
		"""
		#临时写法，等后面敏宗整理
		#获取space
		self.addNavFlags(0x0008)		
	
	def sendSyncMsg(self, srcEntityID, msgName, msg):
		"""
		执行事件ID
		"""
		self.allClients.onSyncMsg(msgName, msg)

	"""
	事件类型
	"""
	def addNavFlags(self, value):
		"""
		value = flag
		"""
		spaceBase = self.getCurrentSpaceBase()
		space = KBEngine.entities[spaceBase.id]		
		space.addNavFlags(value)		
	
# 注册timer等回调处理接口
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_GEAR_DEAD, Gear.onTimer_destroy )
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_GEAR_DELAY_INIT, Gear.onTimer_delayInit )

