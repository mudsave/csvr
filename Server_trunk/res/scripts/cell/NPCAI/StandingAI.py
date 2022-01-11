# -*- coding: utf-8 -*-

"""
"""

import KBEngine
import random
import time
from KBEDebug import *

from SrvDef import eAIStatus, eCampRelationship, eEntityStatus, eObjectType, eEntityEvent, eActionRestrict
import AIRefDataType

from Extra import ECBExtend
from .AIEvent import AIEventDef
from .AIStatus import AIStatus
from .AIStatus import STA_Idle, STA_FightThink, STA_ChaseEntity, STA_CastSpell, STA_Reset, STA_Birth

class StandingAI( AIRefDataType.AIRefDataType ):
	"""
	站桩不动的测试AI
	"""
	def init( self, dataSection ):
		"""
		"""
		AIRefDataType.AIRefDataType.init( self, dataSection )

		self.statusD = {
			eAIStatus.Idle           : StandingAI_Idle( self ),
		}

	def currentStatus( self, executor ):
		"""
		"""
		return self.statusD[executor.aiStatus]

	def attach( self, executor ):
		"""
		virtual method.
		把ai附到指定的对象上
		"""
		AIEventDef.register( executor, eEntityEvent.OnReceiveDamage )
		AIEventDef.register( executor, eEntityEvent.OnDead )
		
		executor.aiStatus = eAIStatus.Idle
		self.statusD[executor.aiStatus].enter( executor )
		executor.think( 0 )

	def detach( self, executor ):
		"""
		virtual method.
		把ai从对象身上卸下来
		"""
		AIEventDef.deregister( executor, eEntityEvent.OnReceiveDamage )
		AIEventDef.deregister( executor, eEntityEvent.OnDead )
		
		self.statusD[executor.aiStatus].leave( executor )

	def leaveStatus( self, executor ):
		"""
		离开当前状态
		"""
		self.statusD[executor.aiStatus].leave( executor )
	
	def enterStatus( self, executor ):
		"""
		进入当前状态
		"""
		self.statusD[executor.aiStatus].enter( executor )

	def event_onThink( self, executor ):
		"""
		virtual method.
		思考（类似于update）
		"""
		self.statusD[executor.aiStatus].think( executor )

	def event_onReceiveDamage( self, executor, attacker, damage ):
		"""
		virtual method.
		事件：受到来自某人的伤害
		"""
		attackerTarget = attacker
			
		# 谁搞我，我就把谁加入到敌人列表中
		executor.enemyList.event_onReceiveDamage( executor, attackerTarget, damage )
		
		event = getattr( self.statusD[executor.aiStatus], "event_onReceiveDamage", None )
		if event:
			event( executor, attackerTarget, damage )

	def event_onDead( self, executor, killer ):
		"""
		被搞死了

		@param  executor: AI拥有者
		@param killer: 凶手
		"""
		# 我死了，需要处理敌人列表
		executor.enemyList.clearAndNotify( executor )

		event = getattr( self.statusD[executor.aiStatus], "event_onDead", None )
		if event:
			event( executor, attacker, damage )

	def event_onActionRestrictChanged( self, executor, flag ):
		"""
		事件：当某个行为限制状态改变时被调用
		"""
		func = getattr( self.statusD[executor.aiStatus], "event_onActionRestrictChanged", None )
		func and func( executor, flag )

		
class StandingAI_Idle( AIStatus ):
	"""
	空闲状态
	"""
	TYPE = eAIStatus.Idle
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		pass

	def leave( self, executor ):
		"""
		virtual method.
		"""
		pass

	def think( self, executor ):
		"""
		virtual method.
		思考下一步行为
		@param executor: AI的执行人
		@return: 无
		"""
		pass

	def event_onReceiveDamage( self, executor, attacker, damage ):
		"""
		virtual method.
		事件：受到来自某人的伤害
		"""
		pass
		#executor.changeAIStatus( eAIStatus.ChaseEntity, 0 )

