# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *
from SpaceScripts.SpaceData import SpaceData
import GloballyDefine as GD

class MultilineSpaceData( SpaceData ) :
	"""
	多线空间
	"""
	def init(self, config):
		SpaceData.init(self, config)

		self.newLineByPlayerAmount = config["newLineByPlayerAmount"]	# 这个值不能比maxPlayer参数大，表示当某条线的人数达到多少以后就新开一条线
		self.maxLine = config["maxLine"]								# 最多有多少条线
		self.initLine = config["initLine"]								# 服务器开启时默认生成多少条线

	def initSpace( self, space ):
		"""
		初始化
		"""
		SpaceData.initSpace( self, space )
		KBEngine.setSpaceData( space.spaceID, GD.SPACEDATA_LINE_NUMBER, str( space.lineNumber ) )

	def packDataForTeleport( self, player ):
		"""
		virtual method.
		打包传送所需要的参数；
		
		@param player: 想要传送到这个地图的玩家
		@return: dict，返回被进入的space所需要的entity数据。如，有些space可能会需要记录玩家的名字，这里就需要返回玩家的playerName属性
		"""
		return { }
