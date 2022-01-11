# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

import time
from Extra import ECBExtend
from BuffDataType import BuffDataType
from ..SpellDef import SpellStatus
from .BuffSimple import BuffSimple
from ..SpellLoader import g_spellLoader
from SrvDef import eObjectType
from SrvDef import eFightEvent, eCampRelationship

class Object(object): pass

class BuffShield(BuffSimple):
	"""
	玩家法力盾buff
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		BuffSimple.init( self, dataSection )

		combatFunction = dataSection["combatFunction"]
		self.shieldEffectIDs = combatFunction["shieldEffect"].readInts( "item" )

		self.shieldEffect = []
		for id in self.shieldEffectIDs:
			self.shieldEffect.append( g_spellLoader.getEffect( id ) )

	def requestShieldTriggerEffect(self, casterID, dstID, buffData):
		"""
		触发护盾碰撞效果
		"""
		caster = KBEngine.entities.get(casterID,None)
		dst = KBEngine.entities.get(dstID,None)

		if dst is not None:
			for effect in self.shieldEffect:
				effect.cast( caster, dst, self )
		
	