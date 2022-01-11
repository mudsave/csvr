# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
import GloballyDefine as GD

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem

class EffectDynamicDamage(SpellEffect):
	"""
	伤害效果
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)

		# 伤害类型（物理、法术）
		self.damageType = dataSection.readInt( "param1" )
		
		# 伤害百分比(float)
		self.damagePercent = dataSection.readFloat( "param2" )
		
		# 伤害修正值(int)
		self.damageValue = dataSection.readInt( "param3" )
		
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		FightSystem.fight( src, dst, spell, self.damageType, self.damagePercent, self.damageValue )

