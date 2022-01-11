# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from BuffDataType import BuffDataType
from ..SpellDef import SpellStatus
from .BuffSimple import BuffSimple
from ..SpellLoader import g_spellLoader
from Extra.CellConfig import g_entityConfig
from SrvDef import eObjectType, eEntityStatus
import Extra.Common as Common


class BuffSummon(BuffSimple):
	"""
	召唤buff
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		BuffSimple.init( self, dataSection )

		combatFunction = dataSection["combatFunction"]
		self.entityID = combatFunction.readInt("entityID")

		self.entityEffectIDs = combatFunction["entityEffect"].readInts( "item" )

		self.entityEffect = []
		for id in self.entityEffectIDs:
			self.entityEffect.append( g_spellLoader.getEffect( id ) )

	
	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情
		例如：给owner加上10点基础伤害
		"""
		#召唤entity
		campID = src.sceneCampID
		config = g_entityConfig[self.entityID] # 获取对应entity的配置
		params = {
			'sceneCampID' : campID,

		}

		entityPos = src.position + src.forward() * 3
		if src.objectType == eObjectType.Player:
			entityPos = src.castPosition

		listPos = KBEngine.raycast( src.spaceID, 0, src.position, entityPos )
		if listPos is not None:
			entityPos = listPos[len(listPos)-1]

		obj = Common.createEntity(src.spaceID, entityPos, src.direction, config, params)
		buffData.misc["summonEntityID"] = obj.id

		for effect in self.entityEffect:
			effect.cast( src, obj, None )

		for effect in self.attachEffect:
			effect.cast( src, dst, None )
	
	def onDetach(self, owner, buffData):
		"""
		template method.
		当buff从owner身上取下来时，此接口被调用（仅调用一次）
		可以在此处做一些buff结束时的事情
		例如：给owner减去10点基础伤害
		"""
		self.onTimeOut(owner, buffData)

		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.detachEffect:
			effect.cast( src, owner, None )
	
	def onTick(self, owner, buffData):
		"""
		template method.
		buff的心跳，每一跳都有可能会做一些事情
		@return: bool; true: buff需要继续心跳；false: buff不需要继续心跳了，亦即应该从玩家身上卸去了
		"""
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.tickEffect:
			effect.cast( src, owner, None )
		return True

	def onInterrupt(self, owner, buffData):
		"""
		template method.
		buff中断回调，继承于此类的buff可以根据实际情况做自己想做的事情
		"""
		self.onTimeOut(owner, buffData)

		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.interruptEffect:
			effect.cast( src, owner, None )

	def onTimeOut(self, owner, buffData):
		"""
		BUFF时间结束或者被中断时处理一些事情
		"""
		obj = KBEngine.entities.get(buffData.misc["summonEntityID"],None)
		if obj:
			obj.HP = 0
			obj.changeEntityStatus( eEntityStatus.Death )
			obj.onDead( owner )

