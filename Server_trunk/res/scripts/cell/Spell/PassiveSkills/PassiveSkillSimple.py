# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

import random

from ..SpellDef import SpellStatus
from ..Spell import PassiveSkill
from ..SpellLoader import g_spellLoader
from ..CastConditions import createCondition

class Object(object): pass

class PassiveSkillSimple(PassiveSkill):
	"""
	简单的通用被动技能
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		PassiveSkill.init( self, dataSection )
		
		generalFunction = dataSection["generalFunction"]
		self.fightEvent = generalFunction.readInt( "fightEvent" )                 # 触发事件（战斗事件）
		self.isOwnEffect = generalFunction.readBool( "isOwnEffect" )
		self.conditions = []
		for item in generalFunction["conditions"].values():
			condition = createCondition( item.asInt )
			if condition is not None:  # 允许某些条件在服务器端不进行判断
				condition.init( item )
				self.conditions.append( condition )
		self.cooldownID = generalFunction.readInt( "cooldownID" )
		self.cooldownTime = generalFunction.readFloat( "cooldownTime" )           # cooldown 时间

		combatFunction = dataSection["combatFunction"]
		self.relation = combatFunction.readIntArray( "relation", ',' )			  # 目标对象类型
		self.targetConditions = []
		if combatFunction.has_key( "targetConditions" ):
 			for item in combatFunction["targetConditions"].values():
 				targetCondition = createCondition( item.asInt )
 				if targetCondition is not None:
 					targetCondition.init( item )
 					self.targetConditions.append( targetCondition )

		# 效果
		self.effects = []
		if combatFunction.has_key( "effects" ):
			for effectID in combatFunction["effects"].readInts( "item" ):
				self.effects.append( g_spellLoader.getEffect( effectID ) )

	def trigger(self, eventType, responderEntity, thirdEntity, spell):
		"""
		virtual method.
		事件触发接口，表示有谁触发了事件
		@param eventType: 来自eFightEvent的事件定义类型
		@param responderEntity: Entity; 响应事件的人（有可能是攻击者，也有可能是受击者）
		@param thirdEntity: Entity; 与事件有关的第三方人士（有可能是攻击者，也有可能是受击者）
		@param spell: SpellEx; 产生这个事件的技能实例
		"""

		# 忽略不属于自己的事件
		if self.fightEvent != eventType:
			return

		for cond in self.conditions:
			if cond.verify( responderEntity, thirdEntity ) != SpellStatus.OK:
				return

		self.onTrigger(responderEntity, thirdEntity, spell)

	def onTrigger(self, responderEntity, thirdEntity, spell):
		"""
		template method.
		@return: None
		"""
		# 检查CD
		if not responderEntity.cooldownIsOut( self.cooldownID ):
			return	

		# 执行CD
		responderEntity.setCooldown( self.cooldownID, self.cooldownTime )
		
		# 广播动作、光效、声音
		responderEntity.allClients.triggerPassiveSkillFS( self.id )
					
		# 对符合条件的目标施放效果
		for effect in self.effects:
			if self.isOwnEffect == False:
				for cond in self.targetConditions:
					result = cond.verify( thirdEntity, responderEntity )
					if result != SpellStatus.OK:
						return
				effect.cast( responderEntity, thirdEntity, None )
			elif self.isOwnEffect == True:
				for cond in self.targetConditions:
					result = cond.verify( responderEntity, None )
					if result != SpellStatus.OK:
						return
				effect.cast( responderEntity, responderEntity, None )

 
