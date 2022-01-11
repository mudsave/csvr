# -*- coding: utf-8 -*-

"""
"""
from KBEDebug import *
from .ConditionBase import ConditionBase
from ..SpellDef import SpellStatus

class BuffCondition( ConditionBase ):
	"""
	目标身上是否拥有某个ID的Buff
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		self.buffID = dataSection.readInt( "buffID" )
		self.type = dataSection.readInt( "type" )

	def verify( self, target, otherTarget ):
		"""
		判断施法者或受术者自身是否满足条件
		@param target: Entity; 施法者或受术者
		@param otherTarget: Entity; 与target判断关系的目标对象（若只判断自身是否满足条件，该参数可以为None）
		"""
		buffData = target.buffMgr.getByBuffID( self.buffID )

		if buffData is not None:
			if self.type == 0:
				return SpellStatus.INVALID_TARGET_TYPE # 无效目标类型
			else:
				return SpellStatus.OK
		else:
			if self.type == 0:
				return SpellStatus.OK
			else:
				return SpellStatus.INVALID_TARGET_TYPE # 无效目标类型


