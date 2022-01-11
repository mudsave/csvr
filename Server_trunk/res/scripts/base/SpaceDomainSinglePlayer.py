# -*- coding: gb18030 -*-
#

"""
Space domain class
"""

import KBEngine
from KBEDebug import *

from Functor import Functor
from Extra.SpaceItem import SpaceItem
from interfaces.SpaceDomainBase import SpaceDomainBase
from SrvDef import eSpaceLoginAction
from MapConfigMgr import g_mapConfigMgr

class SpaceDomainSinglePlayer(SpaceDomainBase):
	"""
	单人单线地图的空间领域
	"""
	def __init__( self ):
		SpaceDomainBase.__init__(self)
		
		# 以玩家的dbid来映射SpaceItem实例，以提高副本同一条件的进入判断速度，
		# 玩家的dbid也标识着与之相对应的SpaceItem实例的拥有者，
		# 使用玩家的dbid而不使用entityID的原因是为了防止玩家下（断）线后重上时找不到原来的所属space，
		# 也是为了防止玩家以下（断）线的方式绕过副本短时间内可进入的次数
		# 此表与self.spaceItems_对应，如果在self.spaceItems_删除一项，也应该在这里删除，创建亦然
		
		# key = player's dbid, value = instance of SpaceItem
		self._dbid2spaceItem = {}
		
		# key = space entity id, value = player's dbid
		self._spaceID2dbid = {}

	def onSpaceLoseCell( self, spaceEntityID ):
		"""
		define method.
		space entity 失去了cell部份后的通告；
		主要用于未来有可能存在的可存储副本，当副本数量太大时可能会考虑在没有玩家的时候只保留base部份，这时就需要这种通告；
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d lose cell."%( self.eMetaClass, spaceEntityID ))
		playerDBID = self._spaceID2dbid[spaceEntityID]
		spaceItem = self._dbid2spaceItem[playerDBID]
		spaceItem.onLoseCell()
		
	def onSpaceGetCell( self, spaceEntityID ):
		"""
		define method.
		某个space的cell部份创建完成回调，此回调来自于被创建的space在onGetCell()被触发时调用。
		我们可在此回调中执行一些事情，如把等待进入此space的玩家传送进此space等等。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d create cell Complete"%( self.eMetaClass, spaceEntityID ))
		playerDBID = self._spaceID2dbid[spaceEntityID]
		spaceItem = self._dbid2spaceItem[playerDBID]
		spaceItem.onGetCell()
		
	def onSpaceCloseNotify( self, spaceEntityID ):
		"""
		define method.
		空间关闭，space entity销毁通知。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space close : %d"%( self.eMetaClass, spaceEntityID ))
		dbid = self._spaceID2dbid.pop( spaceEntityID )
		self._dbid2spaceItem.pop( dbid )

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
		spaceItem = self.findSpaceItem( params["dbID"], True )
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
		pass  # 单人副本没有多人传送功能
		
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
		DEBUG_MSG( "entity id: %s, prev space: '%s', prev pos: '%s', params: '%s'" % (baseMailbox.id, prevSpace, prevPos, params) )
		loginAct = self.config_.loginAction
		if loginAct == eSpaceLoginAction.Locale:                 # 原地上线，不存在则创建
			spaceItem = self.findSpaceItem( params.get("dbID", 0), True )
			spaceItem.logon( baseMailbox, defaultPos, defaultDir )
		elif loginAct == eSpaceLoginAction.LocaleIfExisted:      # 存在则原地上线，不存在则返回到上一地图（进入地图时的地图）
			spaceItem = self.findSpaceItem( params.get("dbID", 0), False )
			if spaceItem:
				spaceItem.logon( baseMailbox, defaultPos, defaultDir )
			else:
				self.gotoPrevSpace_(baseMailbox, prevSpace, prevPos, defaultDir, params)
		elif loginAct == eSpaceLoginAction.GotoPrev:             # 强制返回到上一地图（进入地图时的地图）
			self.gotoPrevSpace_(baseMailbox, prevSpace, prevPos, defaultDir, params)
		elif loginAct == eSpaceLoginAction.LocalBirthPos:		 # 重新创建一个新的space，在出生点上线
			self.teleportEntityOnLogin_(baseMailbox, params)			
		else:
			assert False, "unknow action. loginAct = '%s'" % loginAct
	
	def teleportEntityOnLogin_(self, baseMailbox, params):
		"""
		重登陆进入新副本地图的出生点
		"""
		birthPoint = self.config_.birthPoint
		spaceData = g_mapConfigMgr.get(birthPoint)
		if spaceData is None:
			ERROR_MSG("Map config error, can not found the space, birthPoint = %d" % (birthPoint))
			return
		
		spaceItem = self.findSpaceItem( params.get("dbID", 0), True )
		if spaceItem is None:
			ERROR_MSG( "can't found or create space, params = %s, entity id = %s" % ( params, baseMailbox.id ) )
			return

		spaceItem.logon( baseMailbox, tuple(spaceData["position"]), tuple(spaceData["rotation"]) )	
	
	
	def _onSpaceBaseCreated( self, dbID, spaceBase, spaceItem ):
		"""
		副本base创建完毕的回调
		"""
		self._spaceID2dbid[spaceBase.id] = dbID
		spaceItem.createCell()

	def createNewSpace( self, dbID ):
		"""
		创建一个新的space
		"""
		# 创建新的SpaceItem实例
		spaceItem = SpaceItem( self.spaceType, self, self.eMetaClass, {} )
		self._dbid2spaceItem[dbID] = spaceItem
		spaceItem.createBase( Functor( self._onSpaceBaseCreated, dbID ) )
		return spaceItem

	def findSpaceItem( self, dbID, createIfNotExisted ):
		"""
		查找space。

		@return: instance of SpaceItem or None
		"""
		if dbID in self._dbid2spaceItem:
			return self._dbid2spaceItem[dbID]
			
		# 创建新的SpaceItem实例
		if createIfNotExisted:
			return self.createNewSpace( dbID )
		return None

