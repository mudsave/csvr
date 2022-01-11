# -*- coding: utf-8 -*-
#

import KBEngine
from KBEDebug import *
from Functor import Functor

# SpaceItem : 负责对 space 的封装，处理space的创建、加载和玩家进入
class SpaceItem:
	def __init__( self, entityType, parent, eMetaClass, params ):
		self.entityType = entityType    # Space Entity的类型，如“Space”、“SpaceNormal”等
		self.parent = parent			# 记录包含自己的SpaceDomain
		self.params = params			# dict, 记录了与此space相关的额外参数，如进入此space的同一条件(由SpaceItem创建时传递进来决定)
		self.baseMailbox = None			# 空间的BASE mailbox
		self.baseCreateing = False		# 当前space base是否在创建中
		self.hasCell = False			# 是否存在cell标志
		self.cellCreateing = False		# 当前space是否在创建中
		self._enters = []				# 要进入空间的玩家数据: [(),...]
		self._logons = []				# 要上线进入空间的玩家数据: [Base,...]
		self.eMetaClass = eMetaClass    # 要创建的spaceConfigID
		
	def onLoadDomainBase_( self, baseRef, databaseID, wasActive ):
		"""
		load domain call back
		"""
		if baseRef == None and databaseID == 0 and not wasActive:
			ERROR_MSG( "load domain entity error! number:", self.eMetaClass )
			return

		self.baseMailbox = baseRef

	def createBase( self, callBack = None ):
		"""
		创建domain实体
		"""
		if self.baseCreateing or self.baseMailbox:
			# 如果正在创建base或已经存在base，不再创建
			WARNING_MSG( "I have base or creating base.", self.eMetaClass )
			return
		self.baseCreateing = True
		dict = { "domainMB" : self.parent, "params" : self.params, "eMetaClass" : self.eMetaClass }
		KBEngine.createBaseAnywhere( self.entityType, dict, Functor( self.onCreateBaseCallback_, callBack ) )

	def onCreateBaseCallback_( self, callBack, base ):
		"""
		create domain call back
		@param 	base	:		domain entity base
		@type 	base	:		mailbox
		"""
		if not base:
			ERROR_MSG( "space entity created error!", self.eMetaClass )
			return
		self.baseMailbox = base
		self.baseCreateing = False
		if callBack:
			callBack( base, self )

	def onLoseCell( self ):
		"""
		cell关闭
		"""
		self.hasCell = False

	def onGetCell( self ):
		"""
		space获得了cell部份，执行状态改变，并把需要进入space的玩家传到space中
		"""
		self.hasCell = True
		self.cellCreateing = False
		
		for playerBase, position, direction in self._enters:
			self.baseMailbox.teleportEntity( position, direction, playerBase )
		
		for playerBase, position, direction in self._logons:
			self.baseMailbox.entityCreateCell( position, direction, playerBase )

		self._enters = []
		self._logons = []

	def createCell( self ):
		"""
		创建cell
		"""
		if self.cellCreateing or self.hasCell:
			# 如果正在创建cell或已经存在cell，不再创建
			WARNING_MSG( "SpaceItem::createCell(), I have cell or creating cell.", self.eMetaClass )
			return
		self.cellCreateing = True
		self.baseMailbox.createCell()

	def addToEnterList( self, playerBase, position, direction ):
		"""
		添加进入空间的玩家记录
		"""
		self._enters.append( ( playerBase, position, direction ) )

	def addToLogonList( self, playerBase, position, direction ):
		"""
		添加在空间上线的玩家记录
		@param 	playerBase	:		玩家的base
		@type 	playerBase	:		mailbox
		"""
		self._logons.append( ( playerBase, position, direction ) )

	def enter( self, playerBase, position, direction ):
		"""
		玩家进入空间
		@param 	playerBase	:		玩家的base
		@type 	playerBase	:		mailbox
		@param 	position	:		玩家的位置
		@type 	position	:		vector3
		@param 	direction	:		玩家的位置
		@type 	direction	:		vector3
		@return: None
		"""
		if self.baseMailbox is None:
			def onCreateBaseCallback( spaceBase, spaceItemInst ):
				# 创建了space的base后则创建cell部份
				self.createCell()
				
			# 自身实例还没创建出来，先创建自身实例
			self.createBase( onCreateBaseCallback )
			self.addToEnterList( playerBase, position, direction )		# 加入等待列表中
			return
		
		if self.hasCell:
			self.baseMailbox.teleportEntity( position, direction, playerBase )
		else:
			self.addToEnterList( playerBase, position, direction )
			self.createCell()

	def logon( self, playerBase, position, direction ):
		"""
		玩家上线
		@param 	playerBase	:		玩家的base
		"""
		if self.baseMailbox is None:
			def onCreateBaseCallback( spaceBase, spaceItemInst ):
				# 创建了space的base后则创建cell部份
				self.createCell()
				
			# 自身实例还没创建出来，先创建自身实例
			self.createBase( onCreateBaseCallback )
			self.addToLogonList( playerBase, position, direction )		# 加入等待列表中
			return
		
		if self.hasCell:
			self.baseMailbox.entityCreateCell( position, direction, playerBase )
		else:
			self.addToLogonList( playerBase, position, direction )
			self.createCell()
			

