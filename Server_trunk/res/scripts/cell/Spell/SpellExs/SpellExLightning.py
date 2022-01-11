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

class SpellExLightning( SpellExCurveFightRelySer ):
	"""
	闪电链技能
	"""
	def __init__( self ):
		"""
		"""
		SpellExCurveFightRelySer.__init__( self )

	def init( self, dataSection ):
		"""
		"""
		SpellExCurveFightRelySer.init( self, dataSection )

		self.triggerIndex = dataSection["combatFunction"].readInt("triggerIndex")
		self.lightBuff = g_spellLoader.getEffect( 10101112 )  #闪电链buff

	def doTriggerEffect( self, caster, index, targetID ):
		"""
		根据指定的index选择触发的技能效果
		"""
		if index == self.triggerIndex:
			self.lightBuff.cast(caster, caster, self, targetID)

		SpellExCurveFightRelySer.doTriggerEffect(self, caster, index, targetID)

	def notifyClientCastSpell(self, caster, targetData):
		"""
		通知客户端施展技能
		"""
		caster.allClients.startSpellFS( self.id )


