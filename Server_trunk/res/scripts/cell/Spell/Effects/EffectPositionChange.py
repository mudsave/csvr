# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from ..Spell import SpellEffect
import random
import Math 
import math

class EffectPositionChange(SpellEffect):
	"""
	根据目标位置改变施法者位置
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)

		self.minRadius = dataSection.readFloat("param1")
		self.extraRadius = dataSection.readFloat("param2")
	
	def cast(self,src, dst , spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		if src is None:
			return

		dstPos = Math.Vector3(dst.position)
		dstDir = Math.Vector3(dst.direction)
		randomAngle = random.random() * 360 * math.pi / 180.0
		randomRadius = math.sqrt(random.random()) * self.extraRadius + self.minRadius
		randomDir = Math.Vector3(math.sin(dstDir.z - randomAngle), 0, math.cos(dstDir.z - randomAngle)) # 这里是随机角度，所以选取任何方向作为初始方向都是可以的
		randomPos = dstPos + randomDir * randomRadius  # 随机出生点

		listPos = KBEngine.raycast( dst.spaceID, 0, dst.position, randomPos )
		if listPos is not None:
			randomPos = listPos[len(listPos)-1]

		src.position = randomPos
		src.lookAt(dst.position)