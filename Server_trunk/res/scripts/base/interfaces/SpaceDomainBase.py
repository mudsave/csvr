# -*- coding: gb18030 -*-
#

"""
Space domain class
"""
import json
import KBEngine
from KBEDebug import *

from Functor import Functor
import GloballyConst as GC
import GloballyDefine as GD
import MapDataConfig

# 领域类
class SpaceDomainBase(KBEngine.Base):
	"""
	只允许一个space对象存在的空间领域
	"""
	def __init__( self ):
		KBEngine.Base.__init__(self)
		
		# 声明过的属性，记录此space domain维护的是哪个space，例如：fengming、yanhuang等等
		# 此参数由SpaceDomain被创建时传递，因此在此不进行初始化
		#self.eMetaClass = ""
		
		# 记录对应的配置脚本
		self.config_ = MapDataConfig.get( self.eMetaClass )
		
		# 注册自己的MailBox到全局数据中，以方便其它的baseapp、cellapp传送时使用
		self.registerToGlobalData()
		
	def registerToGlobalData(self):
		"""
		注册自己的MailBox到全局数据中，以方便其它的baseapp、cellapp传送时使用
		"""
		key = GD.GLOBALDATAPREFIX_SPACE_DOMAIN + self.eMetaClass
		if key in KBEngine.globalData:
			assert "There has one '%s' alreadly!!!" % self.eMetaClass
		
		KBEngine.globalData[key] = self

	@staticmethod
	def findSpaceDomain( domainName ):
		"""
		查找指定的地图领域管理器
		"""
		try:
			key = GD.GLOBALDATAPREFIX_SPACE_DOMAIN + domainName
			spaceDM = KBEngine.globalData[key]
			return spaceDM
		except KeyError:
			return None

	def gotoPrevSpace_( self, baseMailbox, prevSpace, prevPos, prevDir, params ):
		"""
		返回上一地图
		"""
		if not prevSpace or prevSpace == self.eMetaClass: # 如果上一个地图与当前地图一样，那肯定是出问题了，只能尝试返回固定地图
			prevSpace = GC.SPAWN_SPACE
			prevPos = GC.SPAWN_POSITION
		prevDomain = self.findSpaceDomain( prevSpace )
		prevDomain.teleportEntityOnLogin( baseMailbox, prevPos, prevDir, "", (0.0, 0.0, 0.0), params )

	def onSpaceLoseCell( self, spaceEntityID ):
		"""
		define method.
		space entity 失去了cell部份后的通告；
		主要用于未来有可能存在的可存储副本，当副本数量太大时可能会考虑在没有玩家的时候只保留base部份，这时就需要这种通告；
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d lose cell."%( self.eMetaClass, spaceEntityID ))
		
	def onSpaceGetCell( self, spaceEntityID ):
		"""
		define method.
		某个space的cell部份创建完成回调，此回调来自于被创建的space在onGetCell()被触发时调用。
		我们可在此回调中执行一些事情，如把等待进入此space的玩家传送进此space等等。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space %d create cell Complete"%( self.eMetaClass, spaceEntityID ))
		
	def onSpaceCloseNotify( self, spaceEntityID ):
		"""
		define method.
		空间关闭，space entity销毁通知。
		@param 	key: int; 代表space的entity的entity id
		"""
		INFO_MSG("%s space close : %d"%( self.eMetaClass, spaceEntityID ))

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
		pass

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
		pass

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
		pass

	def getMultiLines( self, baseMailbox ):
		"""
		define method
		玩家获取各线地图及其人数
		@param baseMailbox: entity 的base mailbox
		"""
		baseMailbox.client.getMultiLines( json.dumps([]) )
		
	def setMultiLine( self, baseMailbox, position, direction, lineNumber ):
		"""
		define method
		玩家换线
		@param baseMailbox: entity 的base mailbox
		@param position : VECTOR3, 
		@param direction : VECTOR3, 
		@param lineNumber:线号
		"""
		pass