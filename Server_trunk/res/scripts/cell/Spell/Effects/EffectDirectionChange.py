# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
import Vector
import math

class EffectDirectionChange(SpellEffect):
	"""
	受击效果：改变朝向
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		self.type = dataSection.readInt("param1")

	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		if src is None:
			return

		if self.type == 1:
			dst.lookAt(src.position)
		elif self.type == 2:
			forward = dst.position - src.position
			forward.y = 0.0
			try:
				ang = Vector.angle( Vector.forward, forward )
			except ZeroDivisionError:
				return
			
			v = Vector.cross( Vector.forward, forward )
			if v[1] < 0:
				ang *= -1
			dst.direction.z = ang;
		elif self.type == 3:
			dst.direction.z = src.direction.z
		elif self.type == 4:
			dst.direction.z = src.direction.z + math.pi
		elif self.type == 5:
			dst.direction = dst.direction
		elif self.type == 6:
			dst.direction.z = dst.direction.z + math.pi