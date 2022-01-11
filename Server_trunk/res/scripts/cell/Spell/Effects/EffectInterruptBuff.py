# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem

class EffectInterruptBuff(SpellEffect):
	"""
	通过中断码中断目标身上携带的某个buff
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)

		# 所要中斷的buff中斷碼
		self.interruptCodes = dataSection.readIntArray( "param1", ';' )

	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""	
		for interruptCode in self.interruptCodes:
			dst.buffMgr.interruptBuff( dst, interruptCode )