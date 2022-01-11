# -*- coding: utf-8 -*-
import time
import json

import math
import Math
import KBEngine
import sys
from KBEDebug import *

import GloballyConst as GC
import GloballyDefine as GD
import GloballyStatus as GS
from SrvDef import eObjectType, eEntityStatus, eReviveType
from MapConfigMgr import g_mapConfigMgr

from Extra import ECBExtend
from interfaces.GameObject import GameObject
from interfaces.Avatar import Avatar
from interfaces.GM import GM
from interfaces.PlayerQuest import PlayerQuest
from Extra.CellConfig import g_entityConfig, g_entityFightConfig

import MapDataConfig

class Player(
	GameObject,
	Avatar,
	GM,
	PlayerQuest,
	):
	
	def __init__(self):
		"""
		"""
		GameObject.__init__(self)
		Avatar.__init__(self)
		PlayerQuest.__init__(self)
		GM.__init__(self)
		
		
		self.objectType = eObjectType.Player
		self.addAttribute(self.getFightConfig())
		self.onEnterSpace_()
		self.attributeOperation()
		#self.setAoiRadius( 20.0, 2.0 ) #设置AOI半径
		if self.status != eEntityStatus.Death and self.status != eEntityStatus.Pending:
			# 把自己设置为未决状态，以避免传送时客户端还未准备好就受到攻击
			self.changeEntityStatus( eEntityStatus.Pending )
		
	def onSpaceGone( self ):
		"""
		space关闭
		"""
		pass
		
	def onDestroy( self ):
		"""
		"""
		#self.fireEvent( eEntityEvent.OnDestroy )
		pass
		
	def getName( self ):
		"""
		virtual method.
		@return: the name of entity
		@rtype:  STRING
		"""
		return self.name
	
	def getFightConfig(self):
		fightID = g_entityConfig[self.eMetaClass]['fightID']
		return g_entityFightConfig[fightID]		
		
	def gotoMap( self, teleportID, isShowLoading = True):
		"""
		define method.
		进入空间
		作用：
			根据spaceName，查找到要进入的空间信息，根据空间需要的条件，从cell里收集条件(领域条件和同一条件)，
			然后调用base的enterSpace入口，将条件传过去
		@param 			teleportID : int; 每个传送门唯一的ID
		"""
		INFO_MSG( teleportID )
		
		spaceData = g_mapConfigMgr.get(teleportID)
		if spaceData is None:
			ERROR_MSG("Map config error, can not found the space, teleportID = %d" % (teleportID))
			return
		
		self.isShowLoading = isShowLoading
		
		spaceName = spaceData["scene"]
		self.gotoSpace(spaceName, tuple(spaceData["position"]), tuple(spaceData["rotation"]))

	def gotoSpace( self, spaceName, position, direction ):
		"""
		define method.
		进入空间
		作用：
			根据spaceName，查找到要进入的空间信息，根据空间需要的条件，从cell里收集条件(领域条件和同一条件)，
			然后调用base的enterSpace入口，将条件传过去
		@param 			spaceName : string; 空间唯一标识，空间的关键字
		@param 			position  : vector3; 目标位置
		@param 			direction : vector3; 方向
		"""
		INFO_MSG( spaceName, position, direction )
		
		#记录要传送的地图ID（每次传送都会重置，所以没有清理的必要）
		self.tempMappingSet('nextSpaceName', spaceName)
		self.tempMappingSet('lastSpaceName', self.getSpaceConfig_().eMetaClass)
		
		# 提取传送所需要的数据
		spaceConfig = MapDataConfig.get( spaceName )
		data = spaceConfig.packDataForTeleport( self )
		
		key = GD.GLOBALDATAPREFIX_SPACE_DOMAIN + spaceName
		spaceDM = KBEngine.globalData[key]

		spaceDM.teleportEntity( position, direction, self.base, data )
		
	def teleportToSpace( self, position, direction, cellMailBox ):
		"""
		defined method.
		作用：传送到指定space，调用玩家自身的功能，让其进入指定空间。
		注：由于底层的teleport()没有完成回调，因此我们需要自己模拟。
			由于传送目标有“相同地图”传送和“不同地图”传送两种，
			因此，在模拟的时候有两个问题需要解决：
				- 传送目标是相同地图
				- 传送目标不是相同地图
			
		@type     position: vector3
		@param    position: 目标位置
		@type    direction: vector3
		@param   direction: 方向
		@type  cellMailBox: MAILBOX
		@param cellMailBox: 用于定位需要跳转的目标space，此mailbox可以是任意的有效的cell mailbox
		"""
		entity = KBEngine.entities.get( cellMailBox.id, None )
		
		# 能在当前 cellapp 找到指定的 entity 且两个 entity 在同一个 space 里，是相同地图传送
		isSameSpace = ( entity is not None and entity.spaceID == self.spaceID )
		
		if isSameSpace:
			self.position = position
			self.direction = direction
		else:
			self.onLeaveSpace_()
			self.teleport( cellMailBox, position, direction )

	def onTeleportFailure( self ): 
		"""
		This method is called on a real entity when a teleport() call for that entity fails. 
		This can occur if the nearby entity mailbox passed into teleport() is stale, 
		meaning that the entity that it points to no longer exists on the destination CellApp pointed to by the mailbox.
		"""		
		ERROR_MSG( "id %i(%s) teleport failure. current space id %i, space name %s, position" % ( self.id, self.getName(), self.spaceID, self.getCurrentSpaceData( GD.SPACEDATA_SPACE_IDENT ) ), self.position )

	def onTeleportSuccess( self, nearbyEntity ):
		"""
		This method is called on a real entity when a teleport() call for that entity has succeeded. 
		The nearby entity is passed as the only argument. 
		When the teleport has been caused by a call from the base entity, 
		the position of the entity will be BigWorld.INVALID_POSITION. 
		The desired position should be set in this callback. 
		This will typically be the position of the nearby entity or a close offset. 
		The nearby entity argument is guaranteed to be a real entity. 
		Two-way methods that modify state can be called on this argument. 
		This can be useful if the nearby entity contains state for where the entity should be positioned. 
		@param nearbyEntity: The entity associated with the mailbox passed to Entity.teleport or Base.. This is guaranteed to be a real entity. 

		@see Entity.teleport  
		"""
		DEBUG_MSG( "id %i(%s) teleport success. current space id %i, space name %s, position" % ( self.id, self.getName(), self.spaceID, self.getCurrentSpaceData( GD.SPACEDATA_SPACE_IDENT ) ), self.position )
		
		self.onEnterSpace_()

	def onEnterSpace_( self ):
		"""
		当玩家进入某空间，该方法被调用
		"""
		# 记下当前进入的地图
		self.eSpaceIdent = KBEngine.getSpaceData( self.spaceID, GD.SPACEDATA_SPACE_IDENT )

		# 回调space配置，以执行一些进入space时的特殊行为
		spaceConfig = self.getSpaceConfig_()
		spaceConfig.onPlayerEnterSpace( self )
		
		if spaceConfig.aoi != 0:
			self.setAoiRadius( spaceConfig.aoi, GC.SPACE_HYST ) #设置AOI半径
			
		spaceBase = self.getCurrentSpaceBase()
		
		try:
			cellMailbox = KBEngine.entities[spaceBase.id]
		except KeyError:
			cellMailbox = spaceBase.cell
		# 通知当前所在的space，某个玩家进来了
		cellMailbox.onEnter( self.base, {} )
		
		#self.fireEvent( eEntityEvent.OnEnterSpace, cellMailbox )
		
	def onLeaveSpace_( self ):
		"""
		当玩家离开某空间，该方法被调用
		"""
		spaceConfig = self.getSpaceConfig_()
		#调用数据结构的函数，做一些处理
		spaceConfig.onPlayerLeaveSpace( self )
		
		# 离开在即，记下最后一次所在的地图标识和位置
		if spaceConfig.recordOnLeave:
			self.eLastSpaceIdent = self.eSpaceIdent
			self.eLastSpacePosition = Math.Vector3(self.position)

		# 取得entity当前所在的space的space entity base
		# 如果找到了则返回相应的base，找不到则返回None.
		# 找不到的原因通常是因为space处于destoryed中，而自己还没有收到转移通知或destroy.
		spaceBase = self.getCurrentSpaceBase()

		try:
			cellMailbox = KBEngine.entities[spaceBase.id]
		except KeyError:
			cellMailbox = spaceBase.cell
		# 通知当前所在的space，某个玩家要出去了
		cellMailbox.onLeave( self.base, {} )
		
		if self.status != eEntityStatus.Death:
			# 把自己设置为未决状态，以避免传送时客户端还未准备好就受到攻击
			self.changeEntityStatus( eEntityStatus.Pending )
		
		#self.fireEvent( eEntityEvent.OnLeaveSpace )
		
	def sendStatusMessage( self, statusID, *args ) :
		"""
		send status message to client
		@type			statusID : INT32
		@param			statusID : defined in common/scdefine.py
		@type			args	 : int/float/str/double
		@param			args	 : it must match the message defined in csstatus_msgs.py
		@return					 : None
		"""
		args = args == () and "" or json.dumps( args )
		self.client.onStatusMessage( statusID, args )

	def onDead( self, killer ):
		"""
		virtual method.
		我被杀死了
		"""
		Avatar.onDead( self, killer )
		# 通知自己的客户端，我被谁杀死了
		if isinstance( killer, Player ):
			self.sendStatusMessage( GS.FIGHT_KILLED_BY, killer.getName() )
		else:
			self.sendStatusMessage( GS.FIGHT_KILLED_BY, "玩家[%s]" % killer.getName() )
			
		# 通知队伍中的队友
		if self.isInTeam():
			# 通知以凶手为中心的一定半径内的队友
			for e in self.getAllMemberInRange( GC.TEAM_DISTAN_FOR_REWARD, killer.position ):
				if e.id == self.id:
					continue
				e.sendStatusMessage( GS.FIGHT_SOMEBODY_KILLED_SOMEBODY, self.getName(), killer.getName() )
				
		self.fireEvent( eEntityEvent.OnDead )

	def onKilled( self, victim ):
		"""
		virtual method.
		我杀死了谁
		
		@param victim: 受害者
		"""
		#Avatar.onKilled( self, victim )
		pass
		

	def isEntityInRange(self, entity, range):
		"""
		判断entity是否在自身一定范围之内
		
		@param entity:  entity
		@param range:   范围半径
		"""				
		if not entity or entity.spaceID != self.spaceID:
			return False
	
		if self.position.flatDistTo(entity.position) <= range:
			return True	
		
		return False
		
	def gotoPrevSpace(self):
		"""
		返回上一地图
		"""
		# 提取传送所需要的数据
		spaceConfig = MapDataConfig.get( self.eLastSpaceIdent )
		data = spaceConfig.packDataForTeleport( self )

		key = GD.GLOBALDATAPREFIX_SPACE_DOMAIN + self.eLastSpaceIdent
		spaceDomain = KBEngine.globalData[key]
		spaceDomain.teleportEntity(self.eLastSpacePosition, self.direction, self.base, data)
				
	def clientLoadedOverNotify( self, srcEntityID ):
		"""
		客户端加载完成通知，用于让服务器把玩家从未决状态改为idle状态
		"""
		if self.id != srcEntityID:
			return
		
		if self.status == eEntityStatus.Pending:
			self.changeEntityStatus( eEntityStatus.Idle )



	def pingCall( self, srcEntityID, time):
		"""
		网络ping
		"""
		if self.id != srcEntityID:
			return
		self.client.pingBack( time )
		
	def setSceneCamp( self, camp ):
		"""
		设置玩家阵营
		"""
		self.sceneCampID = camp

	def notifyClientDisplayDamage( self, attacker, result ):
		"""
		virtual method.
		通知客户端掉血显示
		"""
		displayResullt = result.hit* GD.FightResultType.Hit | result.crit* GD.FightResultType.Crit

		self.client.triggerFightResultFS( displayResullt, -result.damage )

		if attacker.id != self.id and attacker.objectType == eObjectType.Player:
			try:
				attacker.clientEntity(self.id).triggerFightResultFS( displayResullt, -result.damage )
			except:
				pass

	def onReceiveDamage( self, attacker, result ):
		"""
		virtual method.
		受到伤害的触发：在减血以后，死亡之前触发
		"""
		self.notifyClientDisplayDamage( attacker, result )	
			
	def getGateList( self, srcEntityID ):
		if self.id != srcEntityID:
			return
	
		spaceConfig = self.getSpaceConfig_()
		#调用数据结构的函数，做一些处理
		spaceConfig.getGateList( self )
	
	def gotoMapIndex( self, srcEntityID, index ):
		if self.id != srcEntityID:
			return
		
		spaceConfig = self.getSpaceConfig_()
		#调用数据结构的函数，做一些处理
		spaceConfig.gotoMapIndex( self, index )		
	
	def requestResurrection( self, entityID ):
		"""
		由客户端请求，原地复活
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		if self.status != eEntityStatus.Death :
			ERROR_MSG( "The caller '%s' status is '%s'" % (entityID, self.status) )
			return
		
		reviveType = int(KBEngine.getSpaceData( self.spaceID, GD.SPACEDATA_REVIVE_TYPE_DISABLE))
		if (reviveType << int(eReviveType.Locale)) & 1:
			return

		#缺少复活条件的判断，例如需要特定的复活药或者一定数量的金钱
		self.changeEntityStatus( eEntityStatus.Idle )
		self.HP = self.HPMax
		self.MP = self.MPMax

	def requestResurrectionOnEntrance( self, entityID ):
		"""
		由客户端请求，地图入口复活
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return		

		if self.status != eEntityStatus.Death :
			ERROR_MSG( "The caller '%s' status is '%s'" % (entityID, self.status) )
			return
			
		reviveType = int(KBEngine.getSpaceData( self.spaceID, GD.SPACEDATA_REVIVE_TYPE_DISABLE))
		if (reviveType << int(eReviveType.Portal)) & 1:
			return

		#暂时无具体要求，暂不添加条件判断
		self.changeEntityStatus( eEntityStatus.Idle )
		self.HP = self.HPMax
		self.MP = self.MPMax
		spaceConfig = self.getSpaceConfig_()
		self.teleportToSpace(tuple(spaceConfig.enterPoint), tuple(spaceConfig.enterDirection), self)

	def requestReturnMainCity( self, entityID ):
		"""
		由客户端请求，回主城复活
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		if self.status != eEntityStatus.Death :
			ERROR_MSG( "The caller '%s' status is '%s'" % (entityID, self.status) )
			return
	
		reviveType = int(KBEngine.getSpaceData( self.spaceID, GD.SPACEDATA_REVIVE_TYPE_DISABLE))
		if (reviveType << int(eReviveType.MainCity)) & 1:
			return

		self.changeEntityStatus( eEntityStatus.Idle )
		self.HP = self.HPMax
		self.MP = self.MPMax

		eMetaClass = GC.SPAWN_SPACE	 # 地图关键字,用于确定地图的唯一性。
		data = MapDataConfig.get( eMetaClass ) 		
		self.gotoSpace( eMetaClass, tuple(data.enterPoint), tuple(data.enterDirection))

	def addExp(self, count):
		# level = self.level
		exp = self.exp + count
		# config = g_playerLevelConfig[level]		
		# while config['exp'] <= exp:		
			# if not level + 1 in g_playerLevelConfig:
			# 	exp = config['exp']
			# 	break
			# exp = exp - config['exp']
			# level = level + 1
			# config = g_playerLevelConfig[level]
		
		# if level > self.level:
		# 	self.subAttribute(self.getFightConfig()) #删除原来的基础值
		# 	self.level = level
		# 	self.base.onLevelUpgrade(self.level)			
		# 	self.addAttribute(self.getFightConfig()) #增加现在的基础值
		# 	self.attributeOperation()
		# 	self.HP = self.HPMax
		# 	self.calcMaxPower()
		# 	if self.power < self.powerMax:
		# 		self.setPower( self.powerMax )
		
		if self.exp != exp:
			self.exp = exp


# 注册timer等回调处理接口

# 放在最后，以避免循环引用
#from Spell.SpellLoader import g_spellLoader

# 注册到事件表中
Player.registerClass( Player )
