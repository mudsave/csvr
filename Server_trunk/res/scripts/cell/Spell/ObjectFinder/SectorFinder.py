# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .ObjectFinder import ObjectFinder
import Math 
import math


class SectorFinder(ObjectFinder):

	def init(self, dataSection):
		"""
		virtual method.
		从配置中初始化
		@param radius: 半径
		@param angle: 搜索角度
		"""
		# 搜索半径
		self.radius = dataSection.readFloat("radius")
		self.angle = dataSection.readFloat("angle")
	
	def find(self, finder, position = None):
		"""
		"""
		forwardDir = finder.forward()
		forwardDir.normalise()

		if position == None:
			position = Math.Vector3(finder.position)

		return self.entitiesInSector( finder, position, forwardDir, self.radius, self.angle )

	def entitiesInSector( self, finder, position, direction, radius, angle ):
		"""
		搜索对象半径x米内朝向为中心y度扇形内的游戏对象。
		思路：
		1.让每一个找到的对象坐标减去搜索者坐标，得到相对于搜索者的矢量A
		2.使用搜索者的朝向矢量B与矢量A做夹角计算
		3.得出的夹角如果大于参数给于的角度的一半，则表示不在扇形内
		Cos(θ) = A . B /(|A||B|)
		"""
		result = []

		objs = finder.entitiesInRange(radius, None, position)
		
		for obj in objs :			
			desPos = Math.Vector3(obj.position)
			desDir = desPos - position 
			desDir.y = 0
			desDir.normalise()
			
			an = direction.dot(desDir)
			if an < -1:
				an = -1				
			if an > 1:
				an = 1
				
			angleTemp = int(math.acos(an)/ math.pi * 180)
			if angleTemp <= angle / 2.0:	# 小于或等于夹角
				result.append(obj)				
				
		return result
		
	