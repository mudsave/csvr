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

class CDynamicBirthPointTrigger:
	"""
	场景触发器系统
	"""
	def __init__(self, space, config, triggerID):
		"""
		构造函数。
		"""
		self.triggerID = triggerID
		self.space = space
		self.config = config
		self.npcIdList = []
		
		self.num = 0
		self.space.addTimerTrigger(GC.SPACE_CREATE_ENTITY_TIME, GC.SPACE_CREATE_ENTITY_TIME, self.createTrigger)		
	
	def createTrigger(self, id, userArg):
		"""
		开始创建trigger
		"""	
		if self.num < len(self.config['dynamic']):
			npcConfig = self.config['dynamic'][self.num]
			self.num = self.num + 1
			zhenfaNpcConfig = space.cellParams.get( "zhenfaNpcConfig", ZhenfaNpcList() )
			if zhenfaNpcConfig == None:
				return
			if not npcConfig["entityID"] in zhenfaNpcConfig:
				return
			npc = zhenfaNpcConfig[npcConfig["entityID"]]
			config = g_entityConfig[npc.configID]
			params = {
				'fightID' : npc.fightID
			}
			e = Common.createEntity(self.space.spaceID, npcConfig['position'], npcConfig['rotation'], config, params)
			if e is not None:
				self.npcIdList.append(e.id)
		
		if self.num < len(self.config['birthPoint']):
			return
		
		self.space.delTimerTrigger(id) #关闭定时器