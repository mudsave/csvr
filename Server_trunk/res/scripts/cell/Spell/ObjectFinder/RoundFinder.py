# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .ObjectFinder import ObjectFinder

class RoundFinder(ObjectFinder):
	"""
	圆形目标搜索器
	"""
	def find(self, finder, position = None):
		"""
		"""
		return finder.entitiesInRange( self.radius, None, position)

	def init(self, dataSection):
		"""
		virtual method.
		从配置中初始化
		"""
		# 搜索半径
		self.radius = dataSection.readFloat( "radius" )
