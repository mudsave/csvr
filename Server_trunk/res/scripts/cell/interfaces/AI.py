# -*- coding: utf-8 -*-
import KBEngine
from KBEDebug import *

from Extra import ECBExtend
from Spell.Spell import SpellTargetData
from Spell.SpellDef import TargetType
from SrvDef import eActionRestrict, eEntityStatus

class AI:
	"""
	"""
	def castSpell( self, spell, target ):
		"""
		施展一个技能
		"""
		targetData = SpellTargetData()
		targetType = spell.targetType
		if targetType == TargetType.Entity:
			targetData.gameObject = target
		elif targetType == TargetType.Position:
			targetData.posOrDir = target.position
		elif targetType == TargetType.Direction:
			targetData.posOrDir = target.direction
		return spell.cast( self, targetData )

	def adjustTarget( self ):
		"""
		校正目标：如果目标有效则返回目标对象，否则重置并返回None
		"""
		target = self.target
		if target is None or target.isDestroyed or self.spaceID != target.spaceID or target.status == eEntityStatus.Death or target.status == eEntityStatus.Pending:
			self.target = None
		return self.target

	def changeAIStatus( self, newStatus, delayThink = 0.0 ):
		"""
		@param  newStatus: eAIStatus.*; 切換到新的状态
		@param delayThink: 切换状态后延迟多长时间才重新开始think
		"""
		self.ai.leaveStatus( self )
		self.aiStatus = newStatus
		self.ai.enterStatus( self )
		self.think( delayThink )

	def onThink( self ):
		"""
		virtual method.
		AI思考
		"""
		if self.isDead():
			return

		# 判断NPC是否处于禁止思考
		if self.hasActionRestrict( eActionRestrict.ForbidThink ):
			self.think( 0.1 )
			return

		self.ai.event_onThink( self )


	def onTimer_Update( self, timerID, cbID ):
		"""
		AI定时器update
		"""
		self.ai.onTimer_Update( self )

	def aiTrap_onAlertRadius( self, isEnter, entity, rangeXZ, rangeY, controllerID, cbID ):
		"""
		事件：有entity进入、离开怪物的警戒范围
		"""
		self.ai.event_onEnterAlertRadius( isEnter, self, entity, controllerID )

	def aiEvent_onReceiveDamage( self, attacker, damage ):
		"""
		事件：受到来自某人的伤害
		"""
		self.ai.event_onReceiveDamage( self, attacker, damage )
	
	def aiEvent_onHPChanged( self, old ):
		"""
		事件：hp改变
		"""
		self.ai.event_onHPChanged( self, old )

	def aiEvent_onKilled( self, victim ):
		"""
		事件：我杀死了敌人
		"""
		self.ai.event_onKilled( self, victim )

	def aiEvent_onEnterFight( self, enemy ):
		"""
		事件：进入战斗
		"""
		self.ai.event_onEnterFight( self, enemy )

	def aiEvent_onEnterRiding( self ):
		"""
		事件：进入坐骑状态
		"""
		self.ai.event_onEnterRiding( self )

	def aiEvent_onLeaveRiding( self ):
		"""
		事件：离开坐骑状态
		"""
		self.ai.event_onLeaveRiding( self )

	def aiEvent_onDead( self, killer ):
		"""
		事件：死亡
		"""
		self.ai.event_onDead( self, killer )		

# 注册timer等回调处理接口
ECBExtend.register_entity_callback( ECBExtend.TRAP_ON_ALERT_RADIUS, AI.aiTrap_onAlertRadius )
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_AI_SIDESWAY_UPDATE, AI.onTimer_Update )

