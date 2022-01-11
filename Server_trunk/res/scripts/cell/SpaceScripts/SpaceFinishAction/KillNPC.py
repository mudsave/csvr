# -*- coding: utf-8 -*-

"""
"""
import KBEngine
from KBEDebug import *
from .FinishConditionBase import FinishConditionBase
import Extra.SpaceEventID as SpaceEventID
from SpaceFinishData import SpaceFinishData

class KillNPC(FinishConditionBase):
	"""
	杀死指定数量npc，胜利
	"""
	def init( self, configData, space ):
		"""
		"""
		data = SpaceFinishData()
		data.index = KBEngine.genUUID64()
		data.type = "killnpc"
		data.misc["killNPC"] = {}
		for key, value in configData.items():
			data.misc["killNPC"][int(key)] = value
		
		space.finishData.datas[data.index] = data
		space.registerEventID( SpaceEventID.EntityDead, data.index, "onFinishEvent" )
		
	def verify( self, space, data, entity ):
		"""
		"""
		items = data.misc["killNPC"]
		if entity.eMetaClass in items:
			items[entity.eMetaClass] -= 1
			if items[entity.eMetaClass] <= 0:
				del items[entity.eMetaClass]
				if len( items ) <= 0:
					space.unregisterEventID( SpaceEventID.EntityDead, data.index )
					self.onEnd( space, data.parent, True )