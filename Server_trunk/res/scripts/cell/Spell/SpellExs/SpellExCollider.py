# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..SpellDef import SpellStatus
from ..ObjectFinder import createObjectFinder
from ..ObjectSizer import createObjectSizer
from ..SpellLoader import g_spellLoader
from Extra.FightSystem import FightSystem
from Extra.FightSystem import FightResult
from SrvDef import eFightEvent, eCampRelationship
from .SpellExCurveFightRelySer import SpellExCurveFightRelySer
from ..CastConditions import createCondition
from ..SpellDataTypes.SpellExCurveDataType import SpellExCurveDataType
from ..CastConditions.RelationCondition import RelationCondition

class Object(object): pass

class SpellExCollider( SpellExCurveFightRelySer ):
	"""
	客户端触发碰撞效果的技能
	"""
	def __init__( self ):
		"""
		"""
		SpellExCurveFightRelySer.__init__( self )

	def init( self, dataSection ):
		"""
		"""
		SpellExCurveFightRelySer.init( self, dataSection )

		# 与curves触发点一一对应，表示触发点触发时的效果
		section = dataSection["combatFunction"]["colliderEffects"]
		self.colliderEffects = []
		for effectID in section.readInts( "item" ):
			self.colliderEffects.append(g_spellLoader.getEffect( effectID ))

	def requestColliderTriggerEffect(self, casterID, dstID):
		"""
		"""
		caster = KBEngine.entities.get(casterID,None)
		dst = KBEngine.entities.get(dstID,None)

		if dst is not None:
			for effect in self.colliderEffects:
				effect.cast( caster, dst, self )
			


