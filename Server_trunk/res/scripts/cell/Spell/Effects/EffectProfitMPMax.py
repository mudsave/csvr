# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem
import GloballyDefine as GD

class EffectProfitMPMax(SpellEffect):
	"""
	法力值上限增益
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		
		# 法力上限增益百分比(int)
		self.ProfitPercent = dataSection.readFloat( "param1" ) * GD.COEF_PERCENT
		
		# 法力上限增益固定值(int)
		self.ProfitValue = dataSection.readInt( "param2" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		oldMPMax = dst.MPMax
		dst.MPMax_percent = int(dst.MPMax_percent + self.ProfitPercent)
		dst.MPMax_appended = dst.MPMax_appended + self.ProfitValue
		#改变最大法力值上限，根据比例改变当前法力值
		dst.calcMPMax()

