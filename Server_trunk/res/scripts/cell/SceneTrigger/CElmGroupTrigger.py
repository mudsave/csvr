# -*- coding: utf-8 -*-
#

"""
产生组的触发器
"""

import KBEngine
from KBEDebug import *

import KST
import Extra.SpaceEventID as SpaceEventID
import Extra.Common as Common
from Extra.CellConfig import g_entityConfig
import GloballyConst as GC
import random

class CElmGroupTrigger:
	"""
	场景触发器系统
	"""
	def __init__(self, space, config, triggerID):
		"""
		构造函数。
		"""
		self.triggerID = triggerID
		self.space = space
		self.groupID = config['groupID']
		self.delay = config.get('delay',0)
		self.config = config
		self.npcIdList = []
		
		self.num = 0
		"""
		增加监听组队的结束事件
		"""
		self.space.registerEvent(SpaceEventID.StartElmGroup, self)		
		self.space.registerEvent(SpaceEventID.EntityDead, self)
		
	def onEvent(self, name, *args):
		if name == SpaceEventID.EntityDead:
			self.onEntityDead(*args)
		elif name == SpaceEventID.StartElmGroup:
			self.onStartElmGroup(*args)
			
	"""
	组队事件通过事件
	"""
	def onStartElmGroup(self, groupID):
		
		if groupID != self.groupID:
			return
		
		if self.delay == 0:
			self.CreateEntity()
		else:
			self.space.addTimerTrigger(self.delay, 0, self.onTime)
	
	def onTime(self, timeid, userArg ):
		self.CreateEntity()
		self.space.delTimerTrigger(timeid)

	def CreateEntity(self):
		"""
		执行事件
		"""
		if self.config.get('event'):		
			for event in self.config['event']:
				func = getattr(self, 'event_' + event['eventName'])
				func(event['value'])
		
		self.space.addTimerTrigger(GC.SPACE_CREATE_ENTITY_TIME, GC.SPACE_CREATE_ENTITY_TIME, self.createEntityTime)
	
	def createEntityTime(self,  id , userArg ):
		"""
		开始创建NPC
		"""			
		if self.num < len(self.config['birthPoint']):		
			npcConfig = self.config['birthPoint'][self.num]
			
			type = npcConfig.get('type', None)
			if type != None and type == "CEntityGroupRandom":
				self.CEntityGroupRandomFun(npcConfig)			
			else:
				config = g_entityConfig[npcConfig['entityID']]
				e = Common.createEntity(self.space.spaceID, npcConfig['position'], npcConfig['rotation'], config, npcConfig)
				if e is not None:
					self.npcIdList.append(e.id)	
			
			self.num = self.num + 1
		
		if self.num < len(self.config['birthPoint']):
			return
		
		self.space.delTimerTrigger(id) #关闭定时器

	def CEntityGroupRandomFun(self, config):
		"""
		节点随机创建
		"""			
		configlist = config['birthPoint']
		entityIDList = []
		for _config in configlist:
			entityIDList.append(_config["entityID"])
		
		length = len(entityIDList)
		randomlist = random.sample(range(length), length)
		#产生entity
		for index in range(length):
			npcConfig = configlist[index]
			entityID = entityIDList[randomlist[index]]
			config = g_entityConfig[entityID]
			e = Common.createEntity(self.space.spaceID, npcConfig['position'], npcConfig['rotation'], config, npcConfig)
			if e is not None:
				self.npcIdList.append(e.id)				
	
	"""
	NPC死亡事件
	"""
	def onEntityDead(self, entity):
		if entity.id in self.npcIdList:
			self.npcIdList.remove( entity.id )
			if len(self.npcIdList) == 0 and self.num >= len(self.config['birthPoint']): #创建的entity已经全部死亡
				self.space.fireEvent(SpaceEventID.ElmGroupPass, self.groupID) #发出组通过事件
				self.space.fireEvent(SpaceEventID.StartElmGroup, self.groupID + 1) #触发开始下一组事件
				
	"""
	扩展事件
	"""
	def event_startSequence(self, sequenceID):
		targets = self.space.entitiesInRange(GC.SPACE_CONST_DISTANCE, 'Player')
		if targets is not None:
			for target in targets:
				target.addSequence(sequenceID)
	
	def event_startEGroup(self, id):
		"""
		刷一组怪
		"""
		self.space.fireEvent(SpaceEventID.StartElmGroup, id) #触发第一个刷怪事件
	
	def event_startSceneCrash(self, relevanceID):
		"""
		刷一组怪
		"""
		self.space.fireEvent(SpaceEventID.GearSyncMsg, relevanceID, "SceneCrash", "")
	
	def event_duplicateEnd(self, result):
		self.space.fireEvent( SpaceEventID.SpaceMsgEnd, result )