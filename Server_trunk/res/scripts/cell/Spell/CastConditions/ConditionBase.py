# -*- coding: utf-8 -*-

from KBEDebug import *

from ..SpellDef import SpellStatus

class ConditionBase(object):
	"""
	1.判断施法者或受术者自身是否满足条件
	2.判断两者之间的关系是否满足条件
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		pass

	def verify( self, target, otherTarget ):
		"""
		判断施法者或受术者自身是否满足条件
		@param target: Entity; 施法者或受术者
		@param otherTarget: Entity; 与target判断关系的目标对象（若只判断自身是否满足条件，该参数可以为None）
		"""
		return SpellStatus.OK;
