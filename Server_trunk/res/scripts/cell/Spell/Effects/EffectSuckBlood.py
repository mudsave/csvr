# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem

class EffectSuckBlood(SpellEffect):
	"""
	吸血效果（根据造成伤害治疗）
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		
		# 治疗百分比(float)
		self.ProfitPercent = dataSection.readFloat( "param1" )
		
		# 治疗固定值(int)
		self.ProfitValue = dataSection.readInt( "param2" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""

		value = src.result.damage * self.ProfitPercent + self.ProfitValue
		dst.changeHP( value )