# -*- coding: gb18030 -*-
#

"""
Space domain class
"""

import KBEngine
from KBEDebug import *

from Functor import Functor
import GloballyDefine as GD
from Extra.SpaceItem import SpaceItem
from interfaces.SpaceDomainBase import SpaceDomainBase

# 领域类
class SpaceDomain(SpaceDomainBase):
	"""
	只允许一个space对象存在的空间领域
	"""
	def __init__( self ):
		SpaceDomainBase.__init__(self)
		
		self.__spaceItem = None
		
		# 如果该地图需要在服务器起来时就加载，那么就创建一个地图出来
		if self.config_.loadOnStart:
			self.createSpace()
	
	def onSpaceLoseCell( self, spaceEntityID ):
		"""
		define method.
		space entity 失去了cell部份后的通告；
		主要用于未来有可能存在的可存储副本，当副本数量太大时可能会考虑在没有玩家的时候只保留base部份，这时就需要这种通告；
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d lose cell."%( self.eMetaClass, spaceEntityID ))
		self.__spaceItem.onLoseCell()
		
	def onSpaceGetCell( self, spaceEntityID ):
		"""
		define method.
		某个space的cell部份创建完成回调，此回调来自于被创建的space在onGetCell()被触发时调用。
		我们可在此回调中执行一些事情，如把等待进入此space的玩家传送进此space等等。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d create cell Complete"%( self.eMetaClass, spaceEntityID ))
		self.__spaceItem.onGetCell()
		
	def onSpaceCloseNotify( self, spaceEntityID ):
		"""
		define method.
		空间关闭，space entity销毁通知。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space close : %d"%( self.eMetaClass, spaceEntityID ))
		self.__spaceItem = None

	def onEntityEnterSpace( self, spaceBaseMailbox, entityBaseMailbox ):
		"""
		玩家进入了某个space的通知，以让space domain根据自身特性做一些事情
		"""
		pass

	def onEntityLeaveSpace( self, spaceBaseMailbox, entityBaseMailbox ):
		"""
		玩家离开了某个space的通知，以让space domain根据自身特性做一些事情
		"""
		pass

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
		self.__spaceItem.enter( baseMailbox, position, direction )
			
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
			self.__spaceItem.enter( baseMailbox, position, direction )
		
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
		self.__spaceItem.logon( baseMailbox, defaultPos, defaultDir )

	def createSpace( self ):
		"""
		virtual method.
		创建一个指定的space
		"""
		if self.__spaceItem is not None:
			ERROR_MSG("space item instance is max count. %s" % ( self.eMetaClass ))
			return

		def _onCreateBase( base, spaceItem ):
			"""
			创建base完成回调函数
			"""
			INFO_MSG("space base created, create cell now.", self.eMetaClass)
			self.__spaceItem.createCell()

		self.__spaceItem = SpaceItem( "Space", self, self.eMetaClass, {} )
		self.__spaceItem.createBase( _onCreateBase )


