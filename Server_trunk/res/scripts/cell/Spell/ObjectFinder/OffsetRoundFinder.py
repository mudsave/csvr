# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .ObjectFinder import ObjectFinder
import Math 
import math


class OffsetRoundFinder(ObjectFinder):
	
	def find(self, finder, position = None):
		"""
		先算出偏移点，作为该圆形偏移后的圆心
		搜索出“偏移距离+ 圆形半径”做为半径的大圆内的所有目标
		"""
		srcPos = Math.Vector3(finder.position)
		dir = Math.Vector3(finder.direction)
		offSetDir = Math.Vector3(math.sin(dir.z - self.offsetAngle), 0, math.cos(dir.z - self.offsetAngle) ) # 偏移后的朝向
		offSetDir.normalise()

		offsetPos = srcPos + offSetDir * self.offsetDistance # 偏移点坐标
		result = finder.entitiesInRange( self.radius, None, offsetPos )
		
		return result

	def init(self, dataSection):
		"""
		从配置中初始化
		@param radius: 半径
		@param offsetAngle: 偏移角度
		@param offsetDistance: 偏移距离
		"""
		# 搜索半径
		self.radius = dataSection.readFloat("radius")
		self.offsetAngle = dataSection.readFloat("offsetAngle") * math.pi / 180.0 # 先换成弧度
		self.offsetDistance = dataSection.readFloat("offsetDistance")
		
	