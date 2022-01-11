# -*- coding: utf-8 -*-
#

"""
标准场景，也可以作为场景基类
"""

import KBEngine
from KBEDebug import *

import KST
import SceneTrigger.CElmGroupTrigger as CElmGroupTrigger
import SceneTrigger.CreateEntityWithTime as CreateEntityWithTime
import SceneTrigger.CreateEntityTrigger as CreateEntityTrigger
import SceneTrigger.CEntityGroupTrigger as CEntityGroupTrigger
import SceneTrigger.CDynamicBirthPointTrigger as CDynamicBirthPointTrigger
import Extra.SpaceEventID as SpaceEventID
from Extra import ECBExtend

class SceneTrigger:
	"""
	场景触发器系统
	"""
	eventFun = {}
	
	def __init__(self):
		"""
		构造函数。
		"""
		self.timeFunc = {}
		self.triggerList = {}
		self.triggerID = 0 #触发器ID		
		self.triggerIndex = 0 #已经初始化触发器的数量
		self.config = None #配置
		
	def initTrigger(self, param):
		self.config = param
		self.addTimer(0.1, 0.1, ECBExtend.SPACE_TRIGGER_CREATE_TMP)
	
	def tmpOnTimer(self, id, userArg):
		if  self.triggerIndex >= len(self.config):
			self.delTimer(id)
			self.fireEvent(SpaceEventID.SpaceStart) #场景开始
			return
		
		_config = self.config[self.triggerIndex]
		self.AddTrigger(_config['type'], _config)
		self.triggerIndex = self.triggerIndex+1
		
	def addTimerTrigger(self, initialOffset, repeatOffset, func):
		"""
		增加触发器定时器
		"""
		id = self.addTimer(initialOffset, repeatOffset, ECBExtend.SPACE_TRIGGER_ID)
		self.timeFunc[id] = func
		return id
		
	def delTimerTrigger(self, id):
		"""
		移除触发器定时器
		"""
		del self.timeFunc[id]
		self.delTimer(id)
	
	def onTimerTrigger(self, id, userArg):
		"""
		触发器定时回调函数
		"""		
		if self.timeFunc.get(id) is None:
			return
		
		self.timeFunc[id](id, userArg)
		
	def RemoveTrigger( self, id ):
		"""
		移除触发器
		"""
		del self.triggerList[id]
	
	def AddTrigger( self, type, config ):
		"""
		增加触发器
		"""
		if type not in SceneTrigger.eventFun:
			return
		
		SceneTrigger.eventFun[type](self, config)
	
	def AddCElemGroupTrigger( self, config ):
		"""
		添加组队刷新怪物触发器
		"""
		trigger = CElmGroupTrigger.CElmGroupTrigger(self, config, self.triggerID)
		self.triggerList[self.triggerID] = trigger
		
		self.triggerID = self.triggerID + 1
	
	def AddCreateEntityWithTime( self, config ):
		"""
		添加定时批量刷怪的触发器
		"""
		trigger = CreateEntityWithTime.CreateEntityWithTime(self, config, self.triggerID)
		self.triggerList[self.triggerID] = trigger
		
		self.triggerID = self.triggerID + 1	
	
	def AddCreateEntityTrigger( self, config ):
		"""
		添加定时刷怪
		"""
		trigger = CreateEntityTrigger.CreateEntityTrigger(self, config, self.triggerID)
		self.triggerList[self.triggerID] = trigger
		
		self.triggerID = self.triggerID + 1
		
	def AddCEntityGroupTrigger( self, config ):
		"""
		添加组队列触发器
		"""
		trigger = CEntityGroupTrigger.CEntityGroupTrigger(self, config, self.triggerID)
		self.triggerList[self.triggerID] = trigger
		
		self.triggerID = self.triggerID + 1

	def AddCDynamicBirthPointTrigger( self, config ):
		"""
		添加阵法动态出生点
		"""		
		trigger = CDynamicBirthPointTrigger.CDynamicBirthPointTrigger(self, config, self.triggerID)
		self.triggerList[self.triggerID] = trigger
		
		self.triggerID = self.triggerID + 1
		
	def onDestroy( self ):
		"""
		"""
		self.triggerList.clear()
		
SceneTrigger.eventFun = {'CElemGroupTrigger' 	: SceneTrigger.AddCElemGroupTrigger,
				'CCreateEntityWithTime' 		: SceneTrigger.AddCreateEntityWithTime,
				'CreateEntityTrigger' 			: SceneTrigger.AddCreateEntityTrigger,
				'CEntityGroupTrigger' 			: SceneTrigger.AddCEntityGroupTrigger,
				'CDynamicBirthPointTrigger' 	: SceneTrigger.AddCDynamicBirthPointTrigger,
				}