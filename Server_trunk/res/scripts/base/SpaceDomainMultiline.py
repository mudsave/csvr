# -*- coding: gb18030 -*-
#

"""
Space domain class
"""
import json
import time
import KBEngine
from KBEDebug import *

from Functor import Functor
from Extra.SpaceItem import SpaceItem
from interfaces.SpaceDomainBase import SpaceDomainBase

TIMER_ENTER_TIMEROUT = 5 #玩家登入space超时时间，超出这个时间，认为玩家登入space失败，则将对应线的人数减一

class SpaceDomainMultiline(SpaceDomainBase):
	"""
	多线地图的空间领域，平衡space策略
	"""
	def __init__( self ):
		SpaceDomainBase.__init__(self)
		
		self._lineNumber2spaceItem = {}
		self._spaceEntityID2lineNumber = {}
		self._playerAmount = {}					# 包含每个space的人数；key就是线号，value就是该线下的玩家数量
		self._enteringPlayer = {}				# 记录当前正在跳转进地图的玩家
		
		# 是否被配置了固定开启的副本个数
		if self.config_.initLine > 0:
			for line in range( 1, self.config_.initLine + 1 ):
				self.createNewSpace( line )
				
		self.addTimer(TIMER_ENTER_TIMEROUT, TIMER_ENTER_TIMEROUT, 0)

	def _onSpaceBaseCreated( self, lineNumber, spaceBase, spaceItem ):
		"""
		副本base创建完毕的回调
		"""
		self._spaceEntityID2lineNumber[spaceBase.id] = lineNumber
		spaceItem.createCell()

	def createNewSpace( self, lineNumber ):
		"""
		创建一个新的space
		"""
		# 创建新的SpaceItem实例
		spaceItem = SpaceItem( self.spaceType, self,  self.eMetaClass, { "lineNumber" : lineNumber } )
		self._lineNumber2spaceItem[lineNumber] = spaceItem
		self._playerAmount[lineNumber] = 0
		spaceItem.createBase( Functor( self._onSpaceBaseCreated, lineNumber ) )
		return spaceItem

	def onSpaceLoseCell( self, spaceEntityID ):
		"""
		define method.
		space entity 失去了cell部份后的通告；
		主要用于未来有可能存在的可存储副本，当副本数量太大时可能会考虑在没有玩家的时候只保留base部份，这时就需要这种通告；
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d lose cell."%( self.eMetaClass, spaceEntityID ))
		lineNumber = self._spaceEntityID2lineNumber[spaceEntityID]
		self._lineNumber2spaceItem[lineNumber].onLoseCell()
		
	def onSpaceGetCell( self, spaceEntityID ):
		"""
		define method.
		某个space的cell部份创建完成回调，此回调来自于被创建的space在onGetCell()被触发时调用。
		我们可在此回调中执行一些事情，如把等待进入此space的玩家传送进此space等等。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d create cell Complete"%( self.eMetaClass, spaceEntityID ))
		lineNumber = self._spaceEntityID2lineNumber[spaceEntityID]
		self._lineNumber2spaceItem[lineNumber].onGetCell()
		
	def onSpaceCloseNotify( self, spaceEntityID ):
		"""
		define method.
		空间关闭，space entity销毁通知。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space close : %d"%( self.eMetaClass, spaceEntityID ))
		lineNumber = self._spaceEntityID2lineNumber[spaceEntityID]
		self._spaceEntityID2lineNumber.pop(spaceEntityID, None)
		self._lineNumber2spaceItem.pop(lineNumber, None)
		
		for entityID in list(self._enteringPlayer.keys()):
			lineNum = self._enteringPlayer[entityID]["lineNumber"]
			if lineNum == lineNumber:
				self._enteringPlayer.pop(entityID) 

	def onEntityEnterSpace( self, spaceBaseMailbox, entityBaseMailbox ):
		"""
		玩家进入了某个space的通知，以让space domain根据自身特性做一些事情
		"""
		lineNumber = self._spaceEntityID2lineNumber[spaceBaseMailbox.id]
		#self._playerAmount[lineNumber] += 1
		self._enteringPlayer.pop(entityBaseMailbox.id, None)

	def onEntityLeaveSpace( self, spaceBaseMailbox, entityBaseMailbox ):
		"""
		玩家离开了某个space的通知，以让space domain根据自身特性做一些事情
		"""
		lineNumber = self._spaceEntityID2lineNumber[spaceBaseMailbox.id]
		self._playerAmount[lineNumber] -= 1

	def teleportEntity( self, position, direction, baseMailbox, params ):
		"""
		define method.
		传送一个entity到指定的space中
		@type position : VECTOR3, 
		@type direction : VECTOR3, 
		@param baseMailbox: entity 的base mailbox
		@type baseMailbox : MAILBOX, 
		@param params: 一些关于该entity进入space的额外参数； (domain条件)
		@type params : PY_DICT = None
		"""
		spaceItem = self.findSpaceItem( baseMailbox.id, params.get("lineNumber", 0) )
		spaceItem.enter( baseMailbox, position, direction )
			
	def teleportEntitys( self, position, direction, baseMailboxs, params ):
		"""
		define method.
		同时传送多个entity到指定的space中。提供此接口主要是用于方便一些特殊的任务在做完判断后一次性传送，从而简化代码。
		@type position : VECTOR3, 
		@type direction : VECTOR3, 
		@param baseMailboxs: entity 的base array of mailbox
		@type baseMailboxs : ARRAY OF MAILBOX, 
		@param params: 一些关于该entity进入space的额外参数； (domain条件)
		@type params : PY_DICT = None
		"""		
		for mb in baseMailboxs:
			spaceItem = self.findSpaceItem( mb.id, params.get("lineNumber", 0) )
			spaceItem.enter( mb, position, direction )
		
	def teleportEntityOnLogin( self, baseMailbox, defaultPos, defaultDir, prevSpace, prevPos, params ):
		"""
		define method.
		在玩家重新登录的时候被调用，用于让玩家在指定的space中出现（一般情况下为玩家最后下线的地图）；
		@param baseMailbox: entity 的base mailbox
		@param defaultPos: Vector3；默认的上线坐标
		@param defaultDir: Vector3；默认的上线朝向
		@param prevSpace: string；上一个地图的标识
		@param prevPos: Vector3；上一个地图的位置
		@param params: PY_DICT；一些关于该entity进入space的额外参数；(domain条件)
		"""
		spaceItem = self.findSpaceItem( baseMailbox.id, params.get("lineNumber", 0) )
		spaceItem.logon( baseMailbox, defaultPos, defaultDir )

	def findSpaceItem( self, entityID, lineNumber ):
		"""
		通过线号来查找space。

		@param entityID: int; 
		@param lineNumber: int; 获取指定线号
		@param createIfNotExisted: bool; 当找不到时是否创建
		@return: instance of SpaceItem or None
		"""		
		# 线号不在有效范围内，找一个空闲的线号
		if lineNumber <= 0 or lineNumber > self.config_.maxLine:
			lineNumber = self.findFreeSpace()
			
		spaceItem = self._lineNumber2spaceItem.get( lineNumber )
		if not spaceItem:
			spaceItem = self.createNewSpace( lineNumber )
		
		if entityID in self._enteringPlayer:
			if self._enteringPlayer[entityID]["lineNumber"] in self._playerAmount:
				self._playerAmount[self._enteringPlayer[entityID]["lineNumber"]] -= 1
			
		self._enteringPlayer[entityID] = { "lineNumber" : lineNumber, "time" : int(time.time()) }
		self._playerAmount[lineNumber] += 1	
		
		return spaceItem

	def findFreeSpace( self ):
		"""
		寻找一个相对空闲的副本 返回副本编号
		"""
		if self.config_.maxLine <= 0 or len( self._playerAmount ) <= 0:
			return 1;

		# 寻找未满承载量的副本
		for lineNumber, playerAmount in self._playerAmount.items():
			if playerAmount < self.config_.newLineByPlayerAmount:
				return lineNumber

		# 如果还有副本未开， 则开启它
		if len( self._lineNumber2spaceItem ) < self.config_.maxLine:
			for lineNumber in range( 1, self.config_.maxLine + 1 ):
				if not lineNumber in self._lineNumber2spaceItem:
					return lineNumber

		# 所有副本都开了，那么寻找人数最少的第一个副本
		sitems = list( self._playerAmount.items() )
		enterID, playerAmountMin = sitems.pop(0)
		for spaceEnterID, playerAmount in self._playerAmount.items():
			if playerAmount < playerAmountMin:
				enterID = spaceEnterID
				playerAmountMin = playerAmount
		return enterID
		
	def getMultiLines( self, baseMailbox ):
		"""
		define method
		玩家获取各线地图及其人数
		@param baseMailbox: entity 的base mailbox
		"""
		lst = []
		for key, value in self._playerAmount.items():
			dct = {"lineNumber" : key, "amount" : value}
			lst.append(dct)
			
		baseMailbox.client.getMultiLines( json.dumps(lst) )
		
	def setMultiLine( self, baseMailbox, position, direction, lineNumber ):
		"""
		define method
		玩家换线
		@param baseMailbox: entity 的base mailbox
		@param position : VECTOR3, 
		@param direction : VECTOR3, 
		@param lineNumber:线号
		"""
		spaceItem = self._lineNumber2spaceItem.get( lineNumber )
		if spaceItem:
			if baseMailbox.id in self._enteringPlayer:
				if self._enteringPlayer[baseMailbox.id]["lineNumber"] in self._playerAmount:
					self._playerAmount[self._enteringPlayer[baseMailbox.id]["lineNumber"]] -= 1
			
			self._enteringPlayer[baseMailbox.id] = { "lineNumber" : lineNumber, "time" : int(time.time()) }	
			self._playerAmount[lineNumber] += 1	
			spaceItem.enter( baseMailbox, position, direction )
			
	def onTimer( self, timerID, userArg ):
		"""
		到点移除未成功跳转到地图的玩家
		"""
		for entityID in list(self._enteringPlayer.keys()):
			t = time.time() - self._enteringPlayer[entityID]["time"]
			if t >= TIMER_ENTER_TIMEROUT:
				lineNumber = self._enteringPlayer[entityID]["lineNumber"]
				if lineNumber in self._playerAmount:
					self._playerAmount[lineNumber] -= 1
				
				self._enteringPlayer.pop(entityID)
			
		
