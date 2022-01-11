# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *
from SpaceScripts.SpaceData import SpaceData

class QianNengSpaceData( SpaceData ) :
	"""
	队伍副本
	"""
	def init(self, config):
		"""
		"""
		SpaceData.init(self, config)
		self.spaceType = "SpaceMultiPlayer"
		self.domainType = "SpaceDomainMultiPlayer"	
		
		self.loginAction = config.get("loginAction", 0)                 # 玩家下线重上时的跳转处理；eSpaceLoginAction
		
	def packDataForTeleport( self, player ):
		"""
		virtual method.
		打包传送所需要的参数；
		
		@param player: 想要传送到这个地图的玩家
		@return: dict，返回被进入的space所需要的entity数据。如，有些space可能会需要记录玩家的名字，这里就需要返回玩家的playerName属性
		"""
		# see also: SpaceDomainSingle.py::teleportEntityOnLogin()
		return { "id" : 1}
