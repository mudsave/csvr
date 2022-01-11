# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem
import GloballyDefine as GD

class EffectProfitHPMax(SpellEffect):
	"""
	生命值上限增益
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		
		# 生命上限增益百分比(int)
		self.ProfitPercent = dataSection.readFloat( "param1" ) * GD.COEF_PERCENT
		
		# 生命上限增益固定值(int)
		self.ProfitValue = dataSection.readInt( "param2" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		oldHPMax = dst.HPMax
		dst.HPMax_percent = int( dst.HPMax_percent + self.ProfitPercent )
		dst.HPMax_appended = dst.HPMax_appended + self.ProfitValue
		#改变最大生命值上限，根据比例改变当前生命值
		dst.calcHPMax()

