# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *

from SpaceScripts.SpaceData import SpaceData
import GloballyDefine as GD
import GloballyStatus as GS
from Extra import ECBExtend, ECBExtendNextLayer
from SrvDef import eSpaceStatus

class QianNengSpaceData( SpaceData ) :
	"""
	队伍副本
	"""	
	def initSpace( self, space ):
		"""
		"""
		SpaceData.initSpace( self, space )
		space.status = eSpaceStatus.Unfinished

	def packDataForTeleport( self, player ):
		"""
		virtual method.
		打包传送所需要的参数；
		
		@param player: 想要传送到这个地图的玩家
		@return: dict，返回被进入的space所需要的entity数据。如，有些space可能会需要记录玩家的名字，这里就需要返回玩家的playerName属性
		"""
		return { "id" : 1 }

	def onPlayerEnterSpace( self, player ):
		"""
		virtual method.

		玩家进入space
		"""	
		pass

	def onPlayerLeaveSpace( self, player ):
		"""
		virtual method.

		玩家离开space
		"""	
		pass

	def onSpaceMsgEnd( self, space, result ):
		"""
		virtual method.
		消息通知Space结束（副本）
		"""
		if result > 0: #副本胜利,通知客户端副本胜利
			for player in space._player.values():
				if player is not None:
					player.sendStatusMessage( GS.SPACE_MSG_WIN)			
		else:
			for player in space._player.values():
				if player is not None:
					player.sendStatusMessage( GS.SPACE_MSG_FAIL)
		
		space.addTimer( 10, 0, ECBExtend.SPACE_DUPLICATION_LEAVE )
		
	def leaveSpaceMsgOnTimer( self, last, timerID, cbID ):
		"""
		离开副本回调
		"""		
		for player in last._player.values():
			if player is not None:
				player.gotoPrevSpace()		
	
	def getGateList( self , entity):
		"""
		获取space可以跳转的场景
		"""
		entity.client.onGetGateList([entity.eLastSpaceIdent])
		
	def gotoMapIndex(self, entity, index):
		"""
		根据序列处理
		"""
		#<todo:yelei>临时写
		entity.gotoMap( 3000001 )
	
ECBExtendNextLayer.register_callback( ECBExtend.SPACE_DUPLICATION_LEAVE, QianNengSpaceData.leaveSpaceMsgOnTimer )
	
		


		
