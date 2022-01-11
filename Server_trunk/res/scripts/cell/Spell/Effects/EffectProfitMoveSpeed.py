# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem
import GloballyDefine as GD

class EffectProfitMoveSpeed(SpellEffect):
	"""
	移动速度增益
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		
		# 增益百分比(int)
		self.ProfitPercent = dataSection.readFloat( "param1" ) * GD.COEF_PERCENT
		
		# 增益固定值(int)
		self.ProfitValue = dataSection.readInt( "param2" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		dst.moveSpeed_percent = int(dst.moveSpeed_percent + self.ProfitPercent)
		dst.moveSpeed_appended = dst.moveSpeed_appended + self.ProfitValue
		
		dst.calcMoveSpeed()