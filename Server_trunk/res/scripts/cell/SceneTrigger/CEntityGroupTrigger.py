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

class CEntityGroupTrigger:
	"""
	组管理系统
	"""
	def __init__(self, space, config, triggerID):
		"""
		构造函数。
		"""
		self.triggerID = triggerID
		self.space = space
		self.id = config['id']
		self.config = config
		self.isAutoStart = config['isAutoStart']
		
		for _config in self.config['list']:
			self.space.AddTrigger(_config['type'], _config)
		
		if self.isAutoStart:
			self.space.fireEvent(SpaceEventID.StartElmGroup, self.id) #触发第一个刷怪事件