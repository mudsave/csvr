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

class CreateEntityTrigger:
	"""
	每隔一段时间，刷新单个点的怪物
	"""
	
	def __init__(self, space, config, triggerID):
		"""
		构造函数。
		"""
		self.triggerID = triggerID
		self.space = space
		self.config = config
		self._entityID = None
		self.times = self.config['times']
		
		if (self.times == 0): #次数为0
			return
		
		self.space.registerEvent(SpaceEventID.EntityDead, self)
		self.space.addTimerTrigger(config['interval'], 0, self.createEntityTime)		
		
	def onEvent(self, name, *args):
		if name == SpaceEventID.EntityDead:
			self.onEntityDead(*args)
	
	def createEntityTime(self, id, userArg ):
		config = g_entityConfig[self.config['birthPoint']['entityID']]
		e = Common.createEntity(self.space.spaceID, self.config['birthPoint']['position'], self.config['birthPoint']['rotation'], config, self.config['birthPoint'])
		self.space.delTimerTrigger(id)
		if e == None:
			return
		
		self._entityID  = e.id		
		
		if (self.times < 0): #次数为无数次
			return
		self.times = self.times-1
		
		if self.times == 0:
			self.space.unregisterEvent(SpaceEventID.EntityDead, self)
			
	"""
	NPC死亡事件
	"""
	def onEntityDead(self, entity):
		if entity.id == self._entityID:
			self.space.addTimerTrigger(self.config['interval'], 0, self.createEntityTime)