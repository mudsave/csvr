# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *

from SrvDef import eAIStatus, eEntityEvent, eObjectType
import AIRefDataType

from .AIEvent import AIEventDef
from .AIStatus import STA_Idle, STA_FightThink, STA_ChaseEntity, STA_CastSpell, STA_Reset, STA_Birth

class NPCAI_A( AIRefDataType.AIRefDataType ):
	"""
	NPC的AI基础类
	"""
	def init( self, dataSection ):
		"""
		"""
		AIRefDataType.AIRefDataType.init( self, dataSection )
		
		self.skillList = dataSection["skillList"]               # 可用的攻击技能
		self.hoverRadiusMax = dataSection["hoverRadiusMax"]     # 徘徊半径（外圈）
		self.hoverRadiusMin = dataSection["hoverRadiusMin"]     # 徘徊半径（内圈）
		self.chaseRadius = dataSection["chaseRadius"]           # 追击半径（超过此半径，怪物应脱离战斗并回到出生点）
		self.alertRadius = dataSection["alertRadius"]           # 警戒半径（有玩家进入此范围则自动攻击），小于或等于0表示不警戒
		self.randomWalkRadius = dataSection["randomWalkRadius"] # 随机移动半径，小于或等于0表示不随机移动
		self.hoverTime = dataSection["hoverTime"]				# 游荡时间上限（游荡总时间 + 游荡结束后思考的间隔时间）
		self.hoverSpeed = dataSection["hoverSpeed"]           	# 游荡速度
		self.activity = dataSection["activity"]					# 进攻积极性
		self.stopTime = dataSection["stopTime"]                 # 攻击停留时间
		self.birthTime = dataSection["birthTime"]               # 出生状态时间

		self.statusD = {
			eAIStatus.Birth          : STA_Birth( self ),
			eAIStatus.Idle           : STA_Idle( self ),
			eAIStatus.FightThink     : STA_FightThink( self ),
			eAIStatus.ChaseEntity    : STA_ChaseEntity( self ),
			eAIStatus.CastSpell      : STA_CastSpell( self ),
			eAIStatus.Reset          : STA_Reset( self ),
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
		
		if self.birthTime > 0.0:
			executor.aiStatus = eAIStatus.Birth
		else:
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

	def event_onEnterAlertRadius( self, isEnter, executor, entity, controllerID ):
		"""
		事件：有entity进入怪物的警戒范围
		"""
		event = getattr( self.statusD[executor.aiStatus], "event_onEnterAlertRadius", None )
		if event:
			event( isEnter, executor, entity, controllerID )

	def event_onKilled( self, executor, victim ):
		"""
		事件：executor杀死了victim
		"""
		# 杀死了一个目标，强行切换状态，重新思考
		executor.changeAIStatus( eAIStatus.FightThink, 0.5 )

	def event_onActionRestrictChanged( self, executor, flag ):
		"""
		事件：当某个行为限制状态改变时被调用
		"""
		func = getattr( self.statusD[executor.aiStatus], "event_onActionRestrictChanged", None )
		func and func( executor, flag )

