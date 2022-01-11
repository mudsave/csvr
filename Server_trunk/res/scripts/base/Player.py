# -*- coding: utf-8 -*-
import time
import json
import sys
from KBEDebug import *
import KST
import GloballyConst as GC
import GloballyDefine as GD
from Extra import ECBExtend
from interfaces.GM import GM
import MapDataConfig

TEAM_DESTROY_TIME_OUT = 2

class Player(
	KBEngine.Proxy,
	ECBExtend.ECBExtend,
	GM,
	):
	
	def __init__(self):
		# 基本模块
		KBEngine.Proxy.__init__( self )
		GM.__init__(self)
		
		# 临时数据
		self._isLogout = False                         # 表示自己是否为被注销,如果是则会被送回角色选择界面
		self.name = self.cellData["name"]              # undefine variable; 角色名称
		self.eMetaClass = self.cellData["eMetaClass"]  # 初始化自己的eMetaClass
		self.spaceMailbox = None                       # 
		self.cellData["dbID"] = self.databaseID        # 把自己的dbid同步到cell上的一个变量里面，以让cell上需要dbid的功能使用
		self._destroyTimerID = 0					   # 销毁玩家TimerID
		self.loginTime = 0
		self.creatingCell = False                      # 记录当前是否正在创建cell的过程中
		
	def getName(self):
		"""
		virtual method.
		@return: the name of entity
		@rtype:  STRING
		"""
		return self.name
		
	def onEntitiesEnabled( self ):
		"""
		引擎机制，详情请看引擎相关文档
		我们在这个时候建玩家cell实体，并把玩家送到指定地图。
		"""
		if hasattr( self, "cellData" ) and self.cellData != None:
#			self.onRestoreCooldown( self.cellData["eCooldowns"] )	# 无论entityMailbox是否存在，我们都必须恢复cooldown
#			self.onRestoreBuff( self.cellData["eBuffs"] )	# 无论entityMailbox是否存在，我们都必须恢复buff
			if self.cell is not None:
				return

			"""
			if self.cellData["status"] == eEntityStatus.Death:	 # 如果玩家死亡，改变其当前状态，恢复生命，这里还缺少对回到主城的处理
				self.cellData["HP"] = self.cellData["HPMax"]
				eMetaClass = GC.SPAWN_SPACE	 					 # 地图关键字,用于确定地图的唯一性。
				data = MapDataConfig.get( eMetaClass ) 
				self.cellData["eSpaceIdent"] = eMetaClass        # 将指定的地图改为主城
				self.cellData["position"] = data.enterPoint
				self.cellData["direction"] = data.enterDirection
			"""
			# 除非玩家死亡，否则我们都需要把状态改为未决状态，
			# 以解决玩家在怪堆里上线就被杀死的问题。
			# 但是，由于前面对上线时死亡状态作了处理，因此这里只要直接设置为未决状态即可。
			#if self.cellData["status"] != eEntityStatus.Death:				
			#	self.cellData["status"] = eEntityStatus.Pending
			ERROR_MSG( "ceate player")
			self.logonSpace()
		#KST.g_baseApp.registerPlayer(self)

		
	def onGetCell(self):
		"""
		cell 实体创建完成。
		"""
		self.creatingCell = False

	def onLoseCell(self):
		"""
		CELL死亡
		"""
		assert self.creatingCell == False
		self.destroySelf()

	def onClientDeath( self ):
		"""
		客户端断开连接通报，销毁自身即下线操作。
		"""
		DEBUG_MSG( "The base just told us (id %d) we're dead!" % self.id )
		KST.g_baseApp.deregisterPlayer(self)
		self._destroyTimerID = self.addTimer( TEAM_DESTROY_TIME_OUT, 0.1, ECBExtend.TIMER_ON_DESTROY_PLAYER )
		
	def onTimer_destroy(self, timerID, userData):
		"""
		延时销毁自身
		"""
		DEBUG_MSG( "onTimer_destroy: (id %d)" % self.id )
		if self.creatingCell:
			# 正在创建cell，那么就必须要等cell创建完毕后才能销毁
			# 否则self.destroy()就会报异常
			return
		self.delTimer( self._destroyTimerID )
		self._destroyTimerID = 0
		self.destroySelf()
		
	def onClientGetCell( self ):
		"""
		If present, this method is called when the message to the client that it now has a cell is called.
		"""
		DEBUG_MSG( "onClientGetCell: ", time.time() )

	def onWriteToDB( self, cellData ):
		"""
		see also api_python/python_base.chm
		"""
		pass
		"""
		buffDataList = cellData["buffMgr"]["items"]
		for index in range(len(buffDataList)-1, -1, -1):
			# 不保存类型的buff，将buff数据删除
			if buffDataList[index]["saveType"] == eBuffSaveType.NotSave or buffDataList[index]["saveType"] == eBuffSaveType.NotInterrupt:
				buffDataList.pop(index)

			# 对挂起类型buff数据做一个处理，将buff剩余的时间记录到endTime中
			elif buffDataList[index]["saveType"] == eBuffSaveType.SaveByHang:
				if buffDataList[index]["endTime"] >= time.time():
					buffDataList[index]["endTime"] -= time.time()

		self.saveMail()
		"""
		
	def logout(self):
		"""
		注销到人物选择界面
		Exposed for client
		"""
		INFO_MSG( "%s(%i): logout." % (self.getName(), self.id) )
		self._isLogout = True
		self.destroySelf()

	def logoff(self):
		"""
		注销到登录界面
		Exposed for client
		"""
		INFO_MSG( "%s(%i): logout." % (self.getName(), self.id) )
		self._isLogout = False
		self.destroySelf()		
		
	def destroySelf( self ):
		"""
		销毁自己
		"""
		
		if self.cell is not None:
			self.destroyCellEntity()
		else:
			self.onlineTime = time.time() - self.loginTime
			KST.g_baseApp.deregisterPlayer(self)
			if self.spaceMailbox is not None:
				self.spaceMailbox.onLeave( self, {} )
			
			# 如果有account entity则一定要通知account entity
			if self.accountEntity is not None:
				if self._isLogout and self.hasClient and not self.accountEntity.hasClient:
					# 如果是注销，且自己有一个client，并且自己的accountEntity没有client，则可以把client交到account中
					# 需要判断accountEntity.hasClient是为了避免玩家断线后重新登录时注销旧的角色而设
					self.giveClientTo( self.accountEntity )		# 把控制权交到account中
				self.accountEntity.onAvatarDeath()				# 通知account，必须在调用giveClientTo()之后调用，详看Account::onAvatarDeath()
			else:
				INFO_MSG( "%s(%i): is not account entity." % (self.getName(), self.id) )


			self.destroy()	# destroy必须在调用giveClientTo()之后调用
	
	def logonSpace( self ):
		"""
		玩家上线时触发，请求登录到指定的地图
		玩家登录向SpaceDomain请求登录 -> SpaceDomain通知Space玩家要登录 -> 玩家在空间创建cell
                      |
                  在这个位置
		"""
		spaceIdent = self.cellData["eSpaceIdent"]

		# 提取传送所需要的数据
		spaceConfig = MapDataConfig.get( spaceIdent )
		data = spaceConfig.packDataForTeleport( self )

		key = GD.GLOBALDATAPREFIX_SPACE_DOMAIN + spaceIdent
		spaceDomain = KBEngine.globalData[key]
		cellData = self.cellData
		spaceDomain.teleportEntityOnLogin( self, cellData["position"], cellData["direction"], cellData["eLastSpaceIdent"], cellData["eLastSpacePosition"], data )
		self.loginTime = time.time()

	def createCellFromSpace( self, position, direction, spaceCell ):
		"""
		define method.
		在spaceCell上创建Player.cell
		玩家登录向SpaceDomain请求登录 -> SpaceDomain通知Space玩家要登录 -> 玩家在空间创建cell
                                                                                  |
                                                                              在这个位置
		"""
		self.cellData["position"] = position
		self.cellData["direction"] = direction
		self.createCellEntity( spaceCell )
		self.creatingCell = True
	
	def onEnterSpace(self, spaceMailbox):
		"""
		由cell通知过来的进入了某个地图的消息
		"""
		if spaceMailbox is not None:
			self.spaceMailbox = spaceMailbox

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

	def onDestroy( self ):
		"""
		"""
		pass
		
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_DESTROY_PLAYER, Player.onTimer_destroy )