# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *


class Trap(object):
	"""
	陷阱基础类
	"""
	def __init__( self ):
		"""
		"""
		self.id = 0  # 表示陷阱的唯一标识，必须是int32以内的整数
	
	def init( self, dataSection ):
		"""
		"""
		pass
	
	def enter( self, owner, entity ):
		"""
		进入陷阱
		@param owner: Entity；表示此陷阱依附的对象
		@param entity: Entity；表示谁进入了这个陷阱
		"""
		pass
	
	def leave( self, owner, entity ):
		"""
		离开陷阱
		@param owner: Entity；表示此陷阱依附的对象
		@param entity: Entity；表示谁进入了这个陷阱
		"""
		pass

