# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *
from ConfigLoadFun import g_mapTriggerConfig
#from SrvDef import eEntityFlag
from MapConfigMgr import g_mapConfigMgr
from Extra import ECBExtendNextLayer, SpaceEvent
import GloballyDefine as GD

class SpaceData(ECBExtendNextLayer.ECBExtendNextLayer, object) :
	"""
	地图的基础实例化类，用于实例化地图数据
	"""
	
	def __init__( self ):
		"""
		"""
		pass

	def init( self, config ):
		"""
		"""
		self.eMetaClass = config["eMetaClass"]                          # 地图数据唯一标识
		self.clientPath = config["clientPath"]                          # 客户端地图预制体名称
		self.mappingPath = config["mappingPath"]                        # 服务器端地图路径
		self.name = config["name"]                                      # 地图名称
		self.canModifyPKMode = config["canModifyPKMode"]                # 如果为False则表示无法在当前场景修改自己的PK模式
		self.isNormalPKRule = config["isNormalPKRule"]					# 如果为False则表示不采用野外PK规则
		self.sceneTrigger = g_mapTriggerConfig.get(self.eMetaClass, None)  		# 场景触发器
		self.maxPlayer = config["maxPlayer"]                            # 最多能进入多少个玩家，0表示该地图玩家无法进入
		self.dataType = config["dataType"]
		self.campIDForPlayerEnter = config["campIDForPlayerEnter"]      # 玩家进入时的立场
		self.relatedID = config.get("relatedID", 0)						# 副本ID
		self.navFlags = config.get("navFlags", 0)						# 场景导航标志位 
		self.recordOnLeave = config.get("recordOnLeave", 0)             # 是否在离开时记下当前地图及离开的位置信息。0 不记录，1 记录		
		self.autoClose = config.get("autoClose", 0)                     # 地图没人时是否自动关闭
		self.autoCloseDelay = config.get("autoCloseDelay", 0)           # 自动关闭的延时时间
		self.aoi = config.get("aoi", 0)									# aoi半径
		
		self.disableReviveType = 0                                      # 禁止玩家的某种死亡复活类型；eReviveType.*
		for e in config.get("disableReviveType", []):
			self.disableReviveType |= 1 << e

		# 立场关系，记录友好的立场关系，
		# 该关系严格按照《01_战斗系统相关名词解释.docx》的“1.1.4	立场”文档中所描述的形式进行记录
		# 规则：
		# 1.无匹配的立场时默认为友好
		# 2.值0 == 友好，1 == 敌对
		self.campMap = config["campMap"] 
		
		self.initMapTriggerConfig(config)
		
	def initMapTriggerConfig(self, config):
		"""
		virtual method.
		初始化地图配置
		"""
		if self.sceneTrigger is None:
			return
		
		g_mapConfigMgr.init(self.sceneTrigger)
		pointData = g_mapConfigMgr.get(config["enterPos"])
		if pointData == None:
			ERROR_MSG( "mapConifg have not enterPos. enterPos = '%s'" % (config["enterPos"]) )
			return
		self.enterPoint = tuple( pointData["position"] )                # 地图的入口，也是玩家复活点
		self.enterDirection = tuple( pointData["rotation"] )            # 传送到该地图的入口
		
		
	def onPlayerEnterSpace( self, player ):
		"""
		virtual method.
		当有玩家进入该地图时被调用
		
		@param player: Entity; 进入该地图的玩家
		"""
		# 为新进入的玩家设置新的立场
		player.setSceneCamp( self.campIDForPlayerEnter )

		"""
		if self.isNormalPKRule:
			if not player.hasFlag( eEntityFlag.PLAYER_PKRULE ):
				player.addFlag( eEntityFlag.PLAYER_PKRULE )      # 采用野外PK规则
		else:
			if player.hasFlag( eEntityFlag.PLAYER_PKRULE ):
				player.removeFlag( eEntityFlag.PLAYER_PKRULE )   # 不采用野外PK规则
		"""
	
	def onPlayerLeaveSpace( self, player ):
		"""
		virtual method.
		当有玩家离开该地图时被调用
		
		@param player: Entity; 进入该地图的玩家
		"""		
		pass
	
	def packDataForTeleport( self, player ):
		"""
		virtual method.
		打包传送所需要的参数；
		
		@param player: 想要传送到这个地图的玩家
		@return: dict，返回被进入的space所需要的entity数据。如，有些space可能会需要记录玩家的名字，这里就需要返回玩家的playerName属性
		"""
		return {}

	def initSpace( self, space ):
		"""
		virtual method.
		初始化
		"""
		space.navFlags = self.navFlags
		
		KBEngine.setSpaceData( space.spaceID, GD.SPACEDATA_SPACE_IDENT, self.eMetaClass )
		KBEngine.setSpaceData( space.spaceID, GD.SPACEDATA_NAV_FLAGS, str(self.navFlags))
		KBEngine.setSpaceData( space.spaceID, GD.SPACEDATA_REVIVE_TYPE_DISABLE, str( self.disableReviveType ) )
		
		if len(self.mappingPath):
			KBEngine.addSpaceGeometryMapping( space.spaceID, None, "spaces/" + self.mappingPath )
		else:
			WARNING_MSG( "space %s has no geometry mapping." % (self.eMetaClass) )
		
		if self.sceneTrigger is not None:
			space.initTrigger(self.sceneTrigger)

	def	onCreateEntity( self, space, entity ):
		"""
		virtual method.
		space内Entity创建事件
		"""
		pass

	def	onInitEntity( self, space, entity ):
		"""
		virtual method.
		space内Entity创建完毕初始化事件
		"""
		pass
		
	def onSpaceMsgEnd( self, space, params ):
		"""
		virtual method.
		消息通知Space结束（副本）
		"""
		pass
		
	def getGateList( self , entity):
		"""
		获取space可以跳转的场景
		"""
		#<todo:yelei>临时写
		entity.client.onGetGateList(['30200002'])
	
	def gotoMapIndex(self, entity, index):
		"""
		根据序列处理
		"""
		#<todo:yelei>临时写
		entity.gotoMap( 3000003 )