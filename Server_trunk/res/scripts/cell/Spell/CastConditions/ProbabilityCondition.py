# -*- coding: utf-8 -*-

"""
"""
import random

from KBEDebug import *
from .ConditionBase import ConditionBase
from ..SpellDef import SpellStatus

class ProbabilityCondition( ConditionBase ):
	"""
	概率条件
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		self.probability = dataSection.readFloat( "probability" )

	def verify( self, target, otherTarget ):
		"""
		判断施法者或受术者自身是否满足条件
		@param target: Entity; 施法者或受术者
		@param otherTarget: Entity; 与target判断关系的目标对象（若只判断自身是否满足条件，该参数可以为None）
		"""
		if random.random() <= self.probability:
			return SpellStatus.OK
			
		return SpellStatus.INVALID_TARGET_TYPE


