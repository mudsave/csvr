# -*- coding: gb18030 -*-
#

"""
Space Manager class
"""

import KBEngine
from KBEDebug import *

import GloballyDefine as GD
import MapDataConfig

# 场景管理器
class SpaceManager( KBEngine.Base ):
	"""
	场景管理器
	控制普通空间和副本
	"""
	def __init__( self ):
		KBEngine.Base.__init__(self)
		
		# 向所有服务器(baseapp、cellapp)广播自己
		#KBEngine.globalData["SpaceManager"] = self
		self.isInit = False
		self.initProgress = 0

	def init( self ):
		"""
		初始化所有的地图领域
		@type	spaceDatas : dict
		@param	spaceDatas : from MapDataConfig import MapDataConfig
		"""
		if self.isInit:
			return
			
		datas = MapDataConfig.getAllDatas()
		relatedDatas = MapDataConfig.getAllRelatedDatas()

		for key in datas.keys():
			# 创建地图领域
			INFO_MSG( "creating space domain:", key )
			
			if datas[key].relatedID == 0:
				KBEngine.createBaseAnywhere(datas[key].domainType, {"eMetaClass" : key, "spaceType" : datas[key].spaceType})
			
		for key in relatedDatas.keys():	
			if len(relatedDatas[key]) == 0:
				continue
			
			id = relatedDatas[key][0]
			data = datas[id]
			KBEngine.createBaseAnywhere(data.domainType, {"eMetaClass" : key, "spaceType" : data.spaceType})
		
		self.initProgress = 1
		self.isInit = True
		

