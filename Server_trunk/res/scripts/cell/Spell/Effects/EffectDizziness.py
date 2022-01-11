# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from SrvDef import eEffectStatus

from ..Spell import SpellEffect

class EffectDizziness(SpellEffect):
	"""
	眩晕效果
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)

		#眩晕状态的得失，0：失去，1：获得
		self.dizziness = dataSection.readInt("param1")

	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		if self.dizziness == 0:
			dst.effectStatusCounterDecr(eEffectStatus.Dizziness)
		if self.dizziness == 1:
			dst.effectStatusCounterIncr(eEffectStatus.Dizziness)
