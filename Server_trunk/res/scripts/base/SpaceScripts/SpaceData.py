# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *
#from ConfigLoadFun import g_mapTriggerConfig
from MapConfigMgr import g_mapConfigMgr

class SpaceData( object ) :
	"""
	地图的基础实例化类，用于实例化地图数据
	"""
	
	def __init__( self ):
		"""
		"""
		self.spaceType = ""
		self.domainType = ""

	def init( self, config ):
		"""
		"""
		self.eMetaClass = config["eMetaClass"]                          # 地图数据唯一标识
		self.clientPath = config["clientPath"]                          # 客户端地图预制体名称
		self.mappingPath = config["mappingPath"]                        # 服务器端地图路径
		self.name = config["name"]                                      # 地图名称
		self.canModifyPKMode = config["canModifyPKMode"]                # 如果为False则表示无法在当前场景修改自己的PK模式
		#self.sceneTrigger = g_mapTriggerConfig[self.eMetaClass]  # 场景触发器
		self.maxPlayer = config["maxPlayer"]                            # 最多能进入多少个玩家，0表示该地图玩家无法进入
		self.dataType = config["dataType"]
		self.campIDForPlayerEnter = config["campIDForPlayerEnter"]      # 玩家进入时的立场
		self.relatedID = config.get("relatedID", 0)						# 副本ID
		self.recordOnLeave = config.get("recordOnLeave", 0)             # 是否在离开时记下当前地图及离开的位置信息。0 不记录，1 记录
		self.autoClose = config.get("autoClose", 0)                     # 地图没人时是否自动关闭
		self.autoCloseDelay = config.get("autoCloseDelay", 0)           # 自动关闭的延时时间
		self.aoi = config.get("aoi", 0)									# aoi半径
		self.monopolize = config.get("monopolize", 0)					# 在创建cell时是否独占一个cellapp
		
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
		"""
		pass
		#g_mapConfigMgr.init( self.sceneTrigger )
		#spaceData = g_mapConfigMgr.get(config["enterPos"])
		#self.enterPoint = tuple( spaceData["position"] )                # 地图的入口，也是玩家复活点
		#self.enterDirection = tuple( spaceData["rotation"] )            # 传送到该地图的入口

	def packDataForTeleport( self, player ):
		"""
		virtual method.
		打包传送所需要的参数；
		
		@param player: 想要传送到这个地图的玩家
		@return: dict，返回被进入的space所需要的entity数据。如，有些space可能会需要记录玩家的名字，这里就需要返回玩家的playerName属性
		"""
		return {}

	def playerEnterDuplication( self, player ):
		"""
		玩家进入副本
		"""
		pass
	
	def playerExitDuplication( self, player ):
		"""
		玩家离开副本
		"""
		pass

	def playerLeaveDuplication( self, player ):
		"""
		玩家退出副本
		"""
		pass
		
	def onEnd( self, space, result ):
		"""
		副本结束
		"""
		pass