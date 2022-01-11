# -*- coding: utf-8 -*-

from KBEDebug import *

from BuffDataType import BuffDataType
from ..SpellDef import SpellStatus
from .BuffSimple import BuffSimple
from ..SpellLoader import g_spellLoader
from Extra.CellConfig import g_entityConfig
from SrvDef import eObjectType, eEntityStatus, eEffectStatus
import Extra.Common as Common


class BuffFrozen(BuffSimple):
	"""
	冰冻buff
	"""
	def init(self, dataSection):
		BuffSimple.init( self, dataSection )

	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情

		"""
		dst.effectStatusCounterIncr(eEffectStatus.Frozen)

		for effect in self.attachEffect:
			effect.cast( src, dst, None )
	
	def onDetach(self, owner, buffData):
		"""
		template method.
		当buff从owner身上取下来时，此接口被调用（仅调用一次）
		可以在此处做一些buff结束时的事情
		例如：给owner减去10点基础伤害
		"""
		owner.effectStatusCounterDecr(eEffectStatus.Frozen)
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.detachEffect:
			effect.cast( src, owner, None )
	
	def onInterrupt(self, owner, buffData):
		"""
		template method.
		buff中断回调，继承于此类的buff可以根据实际情况做自己想做的事情
		"""
		owner.effectStatusCounterDecr(eEffectStatus.Frozen)
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.interruptEffect:
			effect.cast( src, owner, None )

