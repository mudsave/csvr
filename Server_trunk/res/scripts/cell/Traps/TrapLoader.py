# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from Singleton import Singleton

from PyDataSection import PyXMLSection
from PyDataSection import PyTabTableSection




# 类型映射表
# key = string
# value = class type which inherit from Trap
g_TYPE_MAPPING = {
	
}

def createTrap( classType ):
	"""
	通过类型标识符创建相应的施法条件实例
	"""
	result = None
	c = g_TYPE_MAPPING.get( classType )
	if c is not None:
		result = c()
	return result






class TrapLoader(Singleton):
	"""
	陷阱配置加载器
	"""
	def __init__(self):
		"""
		"""
		# key = trapID, value = instance of Trap
		self.datas = {}

	def init( self, configPath ):
		"""
		初始化所有陷阱相关配置
		"""
		for cf in KBEngine.listPathRes( configPath, "xml" ):
			root = PyXMLSection.parse( cf )
			className = root.readString( "className" )
			trap = createTrap( className )
			if trap is None:
				ERROR_MSG( "Create trap '%s' from '%s' fault! No such trap type!" % (className, cf) )
				continue
			trap.init( root )
			self.datas[trap.id] = trap

	def get( self, id ):
		"""
		"""
		return self.datas.get( id, None )


g_trapLoader = TrapLoader()
