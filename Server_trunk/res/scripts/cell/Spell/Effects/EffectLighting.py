# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from ..Spell import SpellEffect

class EffectLighting(SpellEffect):
	"""
	光效播放
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
	
	def cast(self,src, dst , spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		# 允许没有src
		srcID = -1
		if src is not None:
			srcID = src.id

		dst.allClients.seeSpellEffectFS( self.id, srcID )
