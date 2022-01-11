# -*- coding: utf-8 -*-
from KBEDebug import *

from ..Spell import SpellEffect
import Vector
import math

class EffectFlash(SpellEffect):
	"""
	闪现效果：改变玩家位置
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)

		#闪现距离
		self.distance = dataSection.readFloat("param1")

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

		# offSetDir = src.castDirection
		endPos = src.position + src.castDirection * self.distance
		
		listPos = KBEngine.raycast( src.spaceID, 0, src.position, endPos )

		if listPos is not None:
			endPos = listPos[len(listPos)-1]

		src.position = endPos