# -*- coding: utf-8 -*-
#

"""
标准场景，也可以作为场景基类
"""

import KBEngine
from KBEDebug import *

import KST
import MapDataConfig



class Space( KBEngine.Base ):
	"""
	标准场景。
	"""
	def __init__(self):
		"""
		构造函数。
		"""
		KBEngine.Base.__init__(self)
		self.eMetaClass = self.cellData["eMetaClass"]
		self.config_ = MapDataConfig.get( self.eMetaClass )	# 记下与自己相匹配的配置
		self._shouldDestroy = False							# 临时变量，用于决定当收到onLoseCell()消息时是否destroy space
		self._shouldDeleteFromDB = False					# 临时变量，用于决定当收到onLoseCell()消息时且_shouldDestroy为True时，是否要把space从数据库删除
		self._player = {}							# 记录当前在此space的玩家 {entityID : entityMailbox, ...}
		self.domainMB = self.cellData["domainMB"]
		self.cellData["cellParams"] = self.params
		self.cellappGroupOrderID = 0
		
	def createCell( self ):
		"""
		define method.
		创建space cell部分
		"""
		INFO_MSG("Space::createCell(), space %i create cell."%( self.id ) )
		if self.cell is not None:
			INFO_MSG("Space::createCell(), space %i already has cell, not need new one."%( self.id ) )
			return
		
		if self.config_.monopolize > 0:
			if len(KST.g_notBalancingCellappsGroupOrder) > 0:
				self.cellappGroupOrderID = KST.g_notBalancingCellappsGroupOrder.pop(0)
				self.createInNewSpace( self.cellappGroupOrderID )		# 创建空间 创建cell
				return
			else:
				ERROR_MSG("The server does not have enough cellapp not participate in load balancing")
				self.createInNewSpace(None)
		else:
			self.createInNewSpace( None )		# 创建空间 创建cell

	def onGetCell( self ):
		"""
		cell实体创建完成通知，回调callbackMailbox.onSpaceComplete，通知创建完成。
		"""
		INFO_MSG("Space::onGetCell(), space %i got cell." % ( self.id ) )
		self.domainMB.onSpaceGetCell( self.id )

	def onLoseCell( self ):
		"""
		CELL死亡
		"""
		self.domainMB.onSpaceLoseCell( self.id )
		
		#如果自己的cell是独占一个cellapp，在销毁的时候，应该要回收该cellapp的grouporderID
		if self.cellappGroupOrderID > 0:
			KST.g_notBalancingCellappsGroupOrder.append(self.cellappGroupOrderID)
			self.cellappGroupOrderID = 0
		
		if self._shouldDestroy:
			self.__onClose()
			self.destroy( deleteFromDB = self._shouldDeleteFromDB )

	def getCurrentPlayerCount( self ):
		"""
		获得当前space上玩家的数量
		"""
		return len( self._player )

	def spaceIsFull( self ):
		"""
		检测空间满员状态
		baseMailbox.client.spaceMessage( csstatus.SPACE_MISS_MEMBER_FULL )
		"""
		if len( self._player ) >= self.config_.maxPlayer:
			return False
		return True

	def banishPlayer( self ):
		"""
		驱赶空间里的所有玩家
		"""
		INFO_MSG( "SpaceNormal::banishPlayer(), start banish the all player in the space ..." )
		for baseMailBox in self._player.itervalues():
			baseMailBox.cell.gotoForetime()

	def onEnter( self, baseMailbox, params ):
		"""
		define method.
		玩家进入了空间
		@param baseMailbox: 玩家mailbox
		@type baseMailbox: mailbox
		@param params: 玩家onEnter时的一些额外参数
		@type params: py_dict
		"""
		# 记录进入该副本的玩家的mailbox
		self._player[baseMailbox.id] = baseMailbox
		self.domainMB.onEntityEnterSpace( self, baseMailbox )

	def onLeave( self, baseMailbox, params ):
		"""
		define method.
		玩家离开空间
		@param baseMailbox: 玩家mailbox
		@type baseMailbox: mailbox
		@param params: 玩家onLeave时的一些额外参数
		@type params: py_dict
		"""
		# 把离开的玩家从列表中移除
		self._player.pop( baseMailbox.id, None )
		self.domainMB.onEntityLeaveSpace( self, baseMailbox )
		
		if len( self._player ) <= 0 and self.config_.autoClose:
			if self.config_.autoCloseDelay > 0:
				self.addTimer( self.config_.autoCloseDelay, 0.0, 0 )
			else:
				self.closeSpace()

	def closeSpace( self, deleteFromDB = True ):
		"""
		define method.
		destroy space的唯一入口，所有的space删除都应该走此接口；
		space生命周期结束，删除space
		"""
		INFO_MSG("space close:", self.eMetaClass, self.id )

		if self.cell:
			# 如果cell部份还存在则必须在onLostCell()中执行销毁动作
			self._shouldDestroy = True
			self._shouldDeleteFromDB = deleteFromDB
			self.destroyCellEntity()
		else:
			self.__onClose()
			self.destroy( deleteFromDB = deleteFromDB )

	def __onClose( self ):
		"""
		space关闭（destroy）时执行的一些额外事情。
		如向自己的领域发出通知，自己已经关闭了
		"""
		if self.domainMB:
			self.domainMB.onSpaceCloseNotify( self.id )

	def entityCreateCell( self, position, direction, playerBase ):
		"""
		define method.
		玩家登录向SpaceDomain请求登录 -> SpaceDomain通知Space玩家要登录 -> 玩家在空间创建cell
		                                              |
		                                         在这个位置
		@param playerBase	:	玩家Base
		@type playerBase	:	mailboxhttp://172.16.0.251/bbs/index.php
		"""
		playerBase.createCellFromSpace( position, direction, self.cell )

	def teleportEntity( self, position, direction, baseMailbox ):
		"""
		define method.
		传送一个baseMailbox所指向的cell entity到当前space中
		"""
		baseMailbox.cell.teleportToSpace( position, direction, self.cell)
	
	def broadcastMessage(self, type,  sendname, senddbid, msg, params):
		"""
		广播sapce消息
		"""
		for (dbid,baseMailBox) in self._player.items():
			if (baseMailBox.client is not None):
				baseMailBox.client.receiveChatMessage(type, sendname, senddbid, msg, params)

	def onTimer( self, timerID, userData ):
		"""
		"""
		self.closeSpace()
