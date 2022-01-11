# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import SpellEffect
from Extra.FightSystem import FightSystem
import GloballyDefine as GD

class EffectProfitDEF(SpellEffect):
	"""
	防御力增益（包括物理防御力和法术防御力）
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)

		# 防御力类型（0=物理  1=法术）
		self.damageType = dataSection.readInt( "param1" )

		# 增益百分比(int)
		self.ProfitPercent = dataSection.readFloat( "param2" ) * GD.COEF_PERCENT
		
		# 增益固定值(int)
		self.ProfitValue = dataSection.readInt( "param3" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		if self.damageType == 0:
			dst.physicsDEF_percent = int(dst.physicsDEF_percent + self.ProfitPercent)
			dst.physicsDEF_appended = dst.physicsDEF_appended  + self.ProfitValue

			dst.calcPhysicsDEF()
		else:
			dst.magicDEF_percent = int(dst.magicDEF_percent + self.ProfitPercent)
			dst.magicDEF_appended = dst.magicDEF_appended + self.ProfitValue

			dst.calcMagicDEF()
