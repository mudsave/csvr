# -*- coding: utf-8 -*-
#

"""
产生组的触发器
"""

import KBEngine
from KBEDebug import *

import KST
import Extra.SpaceEventID as SpaceEventID
from Extra.Common import *
from SceneTrigger.CreateEntityTrigger import CreateEntityTrigger
import GloballyConst as GC

class CreateEntityWithTime:
	"""
	每隔一段时间，批量刷新各个点的怪物(单个怪物使用CreateEntityTrigger刷新)
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
		if self.num < len(self.config['birthPoint']):
			npcConfig = self.config['birthPoint'][self.num]
			config = {'interval':self.config['interval'],
					  'times':self.config['times'], 
					  'birthPoint':npcConfig}	
			self.space.AddTrigger('CreateEntityTrigger', config)
			
			self.num = self.num + 1 
			
		if self.num < len(self.config['birthPoint']):
			return
		
		self.space.delTimerTrigger(id) #关闭定时器			