# -*- coding: utf-8 -*-

"""
"""
from KBEDebug import *
from .ConditionBase import ConditionBase
from ..SpellDef import SpellStatus

class StatusCondition( ConditionBase ):
	"""
	依据状态条件
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		self.status = dataSection.readInt( "status" )
		self.type = dataSection.readInt( "type" )

	def verify( self, target, otherTarget ):
		"""
		判断施法者或受术者自身是否满足条件
		@param target: Entity; 施法者或受术者
		@param otherTarget: Entity; 与target判断关系的目标对象（若只判断自身是否满足条件，该参数可以为None）
		"""
		if target.status == self.status:

			if self.type == 0:
				return SpellStatus.INVALID_TARGET_TYPE
			else:
				return SpellStatus.OK
		else:
			if self.type == 0:
				return SpellStatus.OK
			else:
				return SpellStatus.INVALID_TARGET_TYPE


			


