# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem

class EffectHealHP(SpellEffect):
	"""
	治疗效果
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		
		# 治疗百分比(float)
		self.healPercent = dataSection.readFloat( "param1" )
		
		# 治疗固定值(int)
		self.healValue = dataSection.readInt( "param2" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		FightSystem.healHP( src, dst, self.healPercent, self.healValue )

