# -*- coding: utf-8 -*-
import KBEngine
from KBEDebug import *
import GloballyConst as GC
import GloballyStatus as GS
import KST
import time

import SrvDef
import Extra.Common as Common

# --------------------------------------------------------------------
# inner global methods
# --------------------------------------------------------------------
def queryPlayers( parentID, callback ):
	"""
	query all roles which parent account is  parentID
	"""
	query = "select id, sm_eMetaClass, sm_name, sm_level, sm_profession, sm_modelID, sm_weaponID \
			from tbl_Player where sm_accountDBID = %i" % parentID
	INFO_MSG( query )
	KBEngine.executeRawDatabaseCommand( query, callback )

class Account(KBEngine.Proxy):
	def __init__(self):
		KBEngine.Proxy.__init__(self)
		self.avatar = None
		self._login = False
		self.lastClientIpAddr = 0
		self._playerList = []			# 临时记录当前的角色数量
		self._destroySelf = True		# 如果是选择角色进入世界，则该变量被置为 False
		self._isInitPlayerList = False	# 服务器角色列表是否已初始化
		self.deleteDBID = None			# 用于避免同一时间处理多条删除角色的请求，从而引发不必要的问题		
		
	def onTimer(self, id, userArg):
		"""
		KBEngine method.
		使用addTimer后， 当时间到达则该接口被调用
		@param id		: addTimer 的返回值ID
		@param userArg	: addTimer 最后一个参数所给入的数据
		"""
		pass

	# ----------------------------------------------------------------
	# public
	# ----------------------------------------------------------------
	def statusMessage( self, statusID, *args ) :
		"""
		send status message
		@type			statusID : INT32
		@param			statusID : defined in common/scdefine.py
		@type			args	 : int/float/str/double
		@param			args	 : it must match the message defined in GS_msgs.py
		@return					 : None
		"""
		if args == ():
			args = ""
		else:
			args = str(args)
			
		if self.client:
			self.client.onStatusMessage( statusID, args )		
		
	def onEntitiesEnabled(self):
		"""
		KBEngine method.
		该entity被正式激活为可使用， 此时entity已经建立了client对应实体， 可以在此创建它的
		cell部分。
		"""
		#ERROR_MSG("Account[%i]::onEntitiesEnabled:entities enable. mailbox:%s, clientType(%i), accountName=%s" % \
		#	(self.id, self.client, self.getClientType(), self.__ACCOUNT_NAME__))

		self.lastClientIpAddr = self.clientAddr[0];			# 记录最后登进来的IP地址，用于帐号重登录判断
		
		self.requestInitPlayers()    #请求角色列表，如果存在角色，就直接登录
		
	def requestInitPlayers(self):
		"""
		请求角色列表
		"""
		queryPlayers( self.databaseID, self.onQueryPlayers )
		return		

	def onQueryPlayers( self, resultSet, rows, insertid, errstr ):
		"""
		The object to call back (e.g. a function) with the result of the command execution.
		The callback will be called with 3 parameters: result set, number of affected rows and error string.

		@param resultSet:	list of list of string like as [ [ field1, field2, ... ], ... ];
						The result set parameter is a list of rows.
						Each row is a list of strings containing field values.
						The XML database will always return a result set with 1 row and 1 column containing the return code of the command.
						The result set will be None for commands to do not return a result set e.g. DELETE,
						or if there was an error in executing the command.
		@param rows:	The number of a affected rows parameter is a number indicating the number of rows affected by the command.
						This parameter is only relevant for commands to do not return a result set e.g. DELETE.
						This parameter is None for commands that do return a result set or if there was and error in executing the command.
		@param errstr:	The error string parameter is a string describing the error that occurred if there was an error in executing the command.
						This parameter is None if there was no error in executing the command.
		"""
		if errstr is not None:
			ERROR_MSG( errstr )
			return
			
		if self.isDestroyed:
			return

		DEBUG_MSG( "resultSet--Begin>>>", resultSet )
		for k in resultSet:
			for index, value in enumerate( k ):
				if value == None:
					k[index] = 0
		DEBUG_MSG( "resultSet--end>>>", resultSet )

		Players = resultSet									# [ [ id, playerName, level, raceclass, lifetime, hairNumber ], ... ]
		self._playerList = [int( e[0] ) for e in Players]		# 汇总所有角色用于登录、删除操作时认证
		tmpPlayers = []
		for Player in Players:
			# id
			loginPlayer = {				
				"dbid"        : int( Player[0] ),
				"eMetaClass"  :	int( Player[1] ),
				"name"		  : Player[2].decode( "utf-8" ),
				"level"		  : int( Player[3] ),
				"profession"  : int( Player[4] ),
				"modelID"	  : str(Player[5].decode( "utf-8" )),
				"weaponID"	  : str(Player[6].decode( "utf-8" )),
			}
			DEBUG_MSG( "login info for %s" % ( loginPlayer ) )
			tmpPlayers.append( loginPlayer )
			
		self._isInitPlayerList = True			
		#self.client.requestInitPlayersCB( tmpPlayers )	
		
		#<todo:yelei> 暂时这么写，等我离一下思路，再修改这边的代码
		self.createAndLoginPlayer()		
		
	def createAndLoginPlayer(self):
		"""
		如果存在角色，则直接登录，没有角色，就需要创建角色
		"""
		if len( self._playerList ) > 0:
			self.Login(self._playerList[0])
		else:
			self.createPlayer(self.__ACCOUNT_NAME__, 5000002) #<todo:yelei>目前entitiy配置还未对接，所以暂时写成0
	
	def createPlayer(self, playerName, eMetaClass):
		"""
		创建角色
		"""
		INFO_MSG("Account[%i]::CreatePlayer: playerName=%s, eMetaClass=%s" %(self.id, playerName, eMetaClass))
		
		if not self._isInitPlayerList:
			self.statusMessage( GS.ACCOUNT_STATE_NOT_INIT_PLAYERLIST )
			return
		
		"""
		if len( self._playerList ) >= GC.LOGIN_ROLE_UPPER_LIMIT:
			self.statusMessage( GS.ACCOUNT_STATE_CREATE_FULL )
			return
		INFO_MSG( "Account[%i]: create new player: playername = '%s', eMetaClass = %s" % (self.id, playerName, eMetaClass ) )
		"""
		
		if playerName == "" :
			self.statusMessage( GS.ACCOUNT_STATE_NAME_NONE )
			return
		
		"""
		if len( playerName ) > 32:
			self.statusMessage( GS.ACCOUNT_STATE_NAME_TOO_LONG )
			return
		"""
		
		isNovice = 0
		if len( self._playerList ) <= 0:
			isNovice = 1

		paramDict = { 
			"name"			:	playerName, 
			"accountDBID"	:	self.databaseID,
			"position"		:	GC.SPAWN_POSITION,
			"direction"		:	GC.SPAWN_DIRECTION,
			"eSpaceIdent"	:	GC.SPAWN_SPACE,
			"isNovice"		:	isNovice,
		}
		
		avatar = Common.createNewPlayerLocally( eMetaClass, paramDict )

		avatar.writeToDB( self.onWriteRoleToDBCollback )	
	
	# ---------------------------------------
	def onWriteRoleToDBCollback( self, success, avatar ):
		if success:
			self._playerList.append( avatar.databaseID )
		else:
			ERROR_MSG( "%s: I can't create Player anymore" % self.name)
			self.statusMessage( GS.ACCOUNT_STATE_NAME_EXIST )
		# destroy entity
		
		avatar.destroy( writeToDB = False )
		
		#<todo:yelei> 暂时这么写，等我离一下思路，再修改这边的代码
		self.createAndLoginPlayer()
	
	
	def onLogOnAttempt(self, ip, port, password):
		"""
		KBEngine method.
		客户端登陆失败时会回调到这里
		"""
		INFO_MSG("Account[%i]::onLogOnAttempt: ip=%s, port=%i, selfclient=%s" % (self.id, ip, port, self.client))
		
		if ip == self.lastClientIpAddr:	# 如果登录的IP与最后一次登录的IP相同则允许登录
			return KBEngine.LOG_ON_ACCEPT
		return KBEngine.LOG_ON_REJECT
		
	def onClientDeath(self):
		"""
		KBEngine method.
		客户端对应实体已经销毁
		"""
		DEBUG_MSG("Account[%i].onClientDeath:" % self.id)
		self.Logoff()
		
	
	def Logoff(self):
		"""
		Exposed method.
		玩家下线
		"""
		INFO_MSG( "%s(%i): Logoff." % (self.name, self.id) )
		if self.cell is not None:
			self._destroyMeOnClientDeath = True
			self.destroyCellEntity()
		else:
			self.destroy()
			
	def onWriteDB( self, success, srcEntity ):
		"""
		存储自己成功
		"""
		if not success:
			ERROR_MSG( "[%s:%s]save data to db fault!!!" % ( srcEntity.id, srcEntity.databaseID ) )
	
	def onTimer( self, timerID, userData ): 
		"""
		定时器回调
		"""
		pass
	
	def Login(self, databaseID):
		"""
		player login by databaseID

		@param databaseID: indicate which role exist in Role table want to login
		@type  databaseID: INT64
		@return: none
		"""
		if self._login:
			self.statusMessage( GS.ACCOUNT_STATE_ID_ALREADY_LOGIN )
			return

		if (self.avatar is not None) and (not self.avatar.isDestroyed):
			self.statusMessage( GS.ACCOUNT_STATE_ROLE_ALREADY_LOGIN )
			return
		if databaseID not in self._playerList:
			ERROR_MSG( "%s: you have no that player(dbid = %i)." % ( self.name, databaseID ) )
			self.statusMessage( GS.ACCOUNT_STATE_ROLE_NOT_EXIST )
			return

		self._login = True

		INFO_MSG( "%s: create player by databaseID %i" % (self.name, databaseID) )
		KBEngine.createBaseFromDBID( "Player", databaseID, self.onLoadedAvatar )

	def onLoadedAvatar( self, baseRef, databaseID, wasActive ):
		"""
		This is an optional callable object that will be called when the function createBaseFromDBID operation completes.
		The callable object will be called with three arguments: baseRef, databaseID and wasActive.
		If the operation was successful then baseRef will be a reference to the newly created Base entity,
		databaseID will be the database ID of the entity and wasActive will indicate whether the entity was already active.
		If wasActive is true,
		then baseRef is referring to a pre-existing Base entity and may be a mailbox rather than a direct reference to a base entity.
		If the operation failed, then baseRef will be None,
		databaseID will be 0 and wasActive will be false.
		The most common reason for failure is the that entity does not exist in the database but intermittent failures like timeouts or unable to allocate IDs may also occur.
		"""
		#print( "Account::onLoadedAvatar(), wasActive = ", type(wasActive), " -> ", wasActive )
		# 理论上，下面这行永远不会触发
		assert not wasActive, "%s(%i): the target entity was active, I can't do it." % (self.name, self.id)

		if self.isDestroyed:
			# 经测试，在某些情况下确实会发生此问题。
			ERROR_MSG( "%s(%i): Failed to load Avatar for player, because account entity is destroyed." % (self.name, self.id), baseRef )
			if baseRef is not None:
				baseRef.destroySelf()
			return
			
		if baseRef != None:
			INFO_MSG( "Account::onLoadedAvatar(), '%s' - '%s'" % ( self.name, baseRef.cellData["name"] ) )
			self.__onAvatarReady( baseRef )
		else:
			INFO_MSG( "%s(%i): Failed to load Avatar for player." % (self.name, self.id) )
			self.__onAvatarReady( None )
		self._login = False

	def __onAvatarReady( self, avatar ):
		if avatar != None:
			self.lastLogonDBID = avatar.databaseID
			self.avatar = avatar
			avatar.accountEntity = self
			self.giveClientTo( avatar )
			self._destroySelf = False
		else:
			self.statusMessage( GS.ACCOUNT_STATE_CREATE_FAIL )
		
	def onAvatarDeath( self ):
		self.avatar = None
		if not self.hasClient:
			INFO_MSG( "%s: Avatar is destroyed, I will destroy self also." % self.name )
			self.destroy()

	def deletePlayer( self, databaseID ):
		"""
		remove Player by databaseID
		"""
		if self.deleteDBID is not None:
			ERROR_MSG( "%s: I am busy, try it later." % self.name )
			self.statusMessage( GS.ACCOUNT_STATE_SERVER_BUSY )
			return
			
		if databaseID not in self._playerList:
			ERROR_MSG( "%s: you have no that Player(dbid = %i)." % ( self.name, databaseID ) )
			self.statusMessage( GS.ACCOUNT_STATE_ROLE_NOT_EXIST )
			return
			
		KBEngine.deleteBaseByDBID( "Player", databaseID, self.onDeletePlayer )
		self.deleteDBID = databaseID

	def onDeletePlayer( self, state ):
		"""
		delete Player callback
		"""
		if isinstance( state, bool ):
			if state:
				self.client.deletePlayerCB( self.deleteDBID )
				self._playerList.remove( self.deleteDBID )
			else:
				ERROR_MSG( "%s: Failed to delete your avatar, no such Player." % self.name )
				self.statusMessage( GS.ACCOUNT_STATE_ROLE_NOT_EXIST )
		else:
			ERROR_MSG( "%s: Failed to delete your avatar, perhap it's logon already." % self.name )
			self.statusMessage( GS.ACCOUNT_STATE_ROLE_UNALLOWD_DEL)
		self.deleteDBID = None		