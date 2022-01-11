# -*- coding: gb18030 -*-

"""
目的：希望每个baseapp都能知道其它的baseapp，以便广播一些包，
因此我们需要在每个baseapp上产生一个BaseEntity并把它注册成为globalBase，
以此来标识每一个baseapp，那么，我们就可以通过这个baseEntity广播一些数据到所有的baseapp上，
如：全局聊天

"""

import KBEngine
from KBEDebug import *
import Account
import Player
import json
import GloballyDefine as GD

class BaseappEntity( KBEngine.Base ):
	"""
	"""
	C_PREFIX_GBAE = "GBAE"
	def __init__( self ):
		KBEngine.Base.__init__( self )

		# 记录所有和自己同一类型的其它baseApp mailbox
		# 这样每次广播的时候就可以直接使用，而不需要再次到KBEngine.globalBases里去查询比较
		self.globalData = {}

		# entity创建队列，值为任意的本地spaceEntity，
		# 第一个为当前正在创建的space，最后一个为最后加入的space，以此类推
		self.spawnQueue = []

		self.register2baseAppData()

		# 通过在此baseapp上线的玩家的名称与entity实例的对应表
		# { "玩家名称" : instance of entity which live in KBEngine.entities, ... }
		self._localPlayers = {}

		# 临时列表，用于实现lookupPlayerBaseByName()的回调机制
		# { 临时唯一ID : [ 预期回复数量, 超时时间, callback ], ...}
		# “预期回复数量”：指的是当前向几个baseapp发送了请求，每收到一个回复，该值将减一
		# “超时时间”：单位“秒”，float；每秒检测一次，如果超时则中断并回调通知目标未找到。会产生此问题的原因很可能是运行过程中某台baseapp崩溃了
		# callback: function；由调用者提供的回调函数
		self._tmpSearchCache = {}
		self._tmpSearchCurrentID = 0	# 用于记录最后一次分配的ID值，值 0 用于表示没有或失败，因此不作为可用ID分配
		self._searchTimerID = 0			# 用于检查某个lookupPlayerBaseByName()请求是否过期的timer

	def register2baseAppData( self ):
		"""
		注册到baseAppData中
		"""
		# 使用自己的entityID加上前缀形成唯一的名字
		# GBAE is global baseApp Entity, don't use "GBAE*" on other globalBases Key
		self.globalName = "%s%i" % ( self.C_PREFIX_GBAE, self.id )
		KBEngine.baseAppData[self.globalName] = self

		KBEngine.globalData[ self.globalName ] = self		# 同时注册到KBEngine.globalData中
		# todo(phw)：这里会引发循环引用，服务器关闭时有可能会产生问题，将来可能需要进行处理
		self.globalData[self.globalName] = self					# 自身内部引用；
		#将其他的也检索进来
		for key,value in KBEngine.baseAppData.items():
			index = key.find(self.C_PREFIX_GBAE)
			if index != -1:
				self.addRef(key, value)
				value.addRef(self.globalName, self)
				
	def addRef( self, globalName, baseMailbox ):
		"""
		defined method.
		通知加入引用

		@param globalName: 全局base标识名
		@type  globalName: STRING
		@param baseMailbox: 被引用者的mailbox
		@type  baseMailbox: MAILBOX
		@return: 一个声明了的方法，没有返回值
		"""
		self.globalData[globalName] = baseMailbox

	def removeRef( self, globalName ):
		"""
		defined method.
		通知删除引用

		@param baseMailbox: 被引用者的mailbox
		@type  baseMailbox: MAILBOX
		@return: 一个声明了的方法，没有返回值
		"""
		try:
			del self.globalData[globalName]
		except KeyError:
			WARNING_MSG( "no global base entity %s." % globalName )
			pass

	# -----------------------------------------------------------------
	# 在线玩家登录相关
	# -----------------------------------------------------------------
	def registerPlayer( self, entity ):
		"""
		登记一个玩家的entity，所有被登记的玩家都被认为是在线的
		"""
		self._localPlayers[entity.getName()] = entity

	def deregisterPlayer( self, entity ):
		"""
		取消对一个玩家entity的登记
		"""
		# 使用pop代替del，以避免因为玩家因某些原因在登录时未正确注册而使得反注册失败
		# 注：当失注册失败产生异常时，会导致Player的destroy流程被打断，而使得玩家无法再次登录
		self._localPlayers.pop(entity.getName(), None)

	def iterOnlinePlayers( self ):
		"""
		获取一个在线玩家的iterator

		@return: iterator
		"""
		return self._localPlayers.values()

	# -----------------------------------------------------------------
	# 聊天消息广播相关
	# -----------------------------------------------------------------
	def broadcastChat( self, channel, speakerName, speakerDBID, msg, params ):
		"""
		Define method.
		广播玩家的发言内容到当前BaseApp的所有client

		@param     channel: 广播频道
		@type      channel: INT8
		@param speakerName: 源说话者名字
		@type  speakerName: STRING
		@param speakerDBID: 源说话者DBID
		@param         msg: 消息内容
		@type          msg: STRING
		@param      params: 特殊文字
		@type       params: ARRAY <of> UNICODE </of>
		@return: 一个声明了的方法，没有返回值
		"""
		# 广播给每个client
		for e in self._localPlayers.values():
			# 只广播给玩家
			if not isinstance( e, Player.Player ): continue
			e.client.receiveChatMessage( channel, speakerName, speakerDBID, msg, params )

	def globalChat( self, channel, speakerName, speakerDBID, msg, params ):
		"""
		广播玩家的发言内容到所有的BaseApp

		@param     channel: 广播频道
		@type      channel: INT8
		@param speakerName: 源说话者名字
		@type  speakerName: STRING
		@param         msg: 消息内容
		@type          msg: STRING
		@param      params: 特殊文字
		@type       params: ARRAY <of> UNICODE </of>
		@return: 无
		"""
		# 通知每个baseApp, 包括自己
		for e in self.globalData.values():
			e.broadcastChat( channel, speakerName, speakerDBID, msg, params )

	# -----------------------------------------------------------------
	# timer
	# -----------------------------------------------------------------
	def onTimer( self, timerID, userData ):
		"""
		"""
		# 详看lookupPlayerBaseByName()里调用的addTimer()
		# 处理超时的请求
		if timerID == 12345:
			time = KBEngine.time()
			for k, v in list(self._tmpSearchCache.items()):	# 使用items()，直接复制列表，这样在循环里可以直接删除字典数据，此方法只适用于少量数据的地方
				if time >= v[1]:
					del self._tmpSearchCache[k]
					v[2]( None )

			# 如果没有其它请求了，必须停止timer
			if len( self._tmpSearchCache ) == 0:
				self.delTimer( self._searchTimerID )
				self._searchTimerID = 0


	# -----------------------------------------------------------------
	# about lookupPlayerBaseByName() mechanism
	# -----------------------------------------------------------------
	def lookupPlayerBaseByName( self, name, callback ):
		"""
		根据名字查找在线主角的base mailbox
		@param name: string; 要查找的在线主角的名字。
		@param callback: function; 该回调函数必须有一个参数，用于给回调者提供查找到的在线主角的base mailbox，如果未找到则参数值为None。
		@return: None
		"""
		resultID = self._getLookupResultID()
		self._tmpSearchCache[resultID] = [ len( self.globalData ), KBEngine.time() + 2, callback ]	# [ 预期回复数量, 超时时间写死2秒, callback ]
		for v in self.globalData.values():
			v._broadcastLookupPlayerBaseByName( self, resultID, name )

		if self._searchTimerID == 0:
			self.addTimer( 1, 1, 12345 )

	def _broadcastLookupPlayerBaseByName( self, resultBase, resultID, name ):
		"""
		defined method.
		用于BaseappEntity内部调用，用于实现lookupPlayerBaseByName()的回调功能

		@param resultBase: BASE MAILBOX
		@param resultID: int32
		@param name: string
		"""
		resultBase._broadcastLookupPlayerBaseByNameCB( resultID, self._localPlayers.get( name ) )

	def _broadcastLookupPlayerBaseByNameCB( self, resultID, baseMailbox ):
		"""
		defined method.
		用于BaseappEntity内部调用，用于实现lookupPlayerBaseByName()的回调功能
		如果baseMailbox不为None，则表示找到，从_tmpSearchCache中清除，并回调；
		如果baseMailbox为None，且所有的baseapp已回复，表示没有找到，从_tmpSearchCache中清除，并回调；
		如果baseMailbox为None，且回复的baseapp还没有全部回复，baseapp的回复数减一，直接返回

		@param baseMailbox: 被找到的目标entity base mailbox
		"""
		if resultID not in self._tmpSearchCache: return		# 未找到表示已经被回复了，不再作处理
		r, time, callback = self._tmpSearchCache[resultID]
		r -= 1
		if baseMailbox is None and r > 0:	# 还有其它的未回复，改变计数器后直接返回
			self._tmpSearchCache[resultID][0] = r
			return

		del self._tmpSearchCache[resultID]
		# 如果没有其它请求了，必须停止timer
		if len( self._tmpSearchCache ) == 0:
			self.delTimer( self._searchTimerID )
			self._searchTimerID = 0

		# 回调
		callback( baseMailbox )

	def _getLookupResultID( self ):
		"""
		获得一个用于广播lookupPlayerBaseByName()的唯一的id值
		@return: INT32
		"""
		self._tmpSearchCurrentID += 1
		if self._tmpSearchCurrentID >= 0x7FFFFFFF:
			self._tmpSearchCurrentID = 1

		# 循环判断并获取一个不在_tmpSearchCache中存在的ID值
		while self._tmpSearchCurrentID in self._tmpSearchCache:
			self._tmpSearchCurrentID += 1
			if self._tmpSearchCurrentID >= 0x7FFFFFFF:
				self._tmpSearchCurrentID = 1
		return self._tmpSearchCurrentID
	
	# ------------------------------------------------------------------------
	# space spawn的处理队列
	# 设计思想：当一个space创建完成了以后就向该space的baseappEntity(当前类)
	#           注册(调用pushSpawn())，baseappEntity把它排到队列中，当轮到该
	#           space创建entity时则调用该spaceEntity的createSpawnPoint()方法。
	#           spaceEntity创建entity完成后则向baseappEntity请求从队列中删除
	#           （调用popSpawn()）。
	# 问：为什么不直接在每个spaceEntity创建时自己决定创建entity？
	# 答：由于一个baseapp可能会同时创建多个space，如果每个space同时创建entity，
	#     那么只能允许每个space每秒同时创建少量的（如10个）entity，否则该baseapp
	#     的entityID分配跟不上速度则会出错；
	# 问：为什么需要space每秒创建大量的entity，每个只创建10个会有什么问题：
	# 答：如果每秒只创建10个entity，那么，当玩家进入副本的时候，该副本可能需要很
	#     长时间才能把entity创建完，这样当玩家刚进入副本时可能会什么都看不到。
	#     因此，我们需要同时创建更多的entity来使创建时间尽量减少。至于怎么处理
	#     多个副本同时创建的问题，我想这个需要多方面调控，如副本的entity数量少些，
	#     以期让创建速度加快，或在不同的baseapp里创建副本。
	# 问：为什么不放在spaceManager中，而是放在每个baseapp中？
	# 答：这样做的好处是每个baseapp都可以同时创建自己服务器上的space的entity，
	#     以达到分流的目的。
	# ------------------------------------------------------------------------
	def pushSpawn( self, spaceEntity ):
		"""
		入栈一个spaceEntity到spawn创建队列中
		"""
		self.spawnQueue.append( spaceEntity )
		if len( self.spawnQueue ) == 1:
			self.spawnQueue[0].createSpawnPoint()

	def popSpawn( self ):
		"""
		出栈当前的正在创建spawn的spaceEntity
		"""
		self.spawnQueue.pop( 0 )
		if len( self.spawnQueue ) > 0:
			self.spawnQueue[0].createSpawnPoint()

			