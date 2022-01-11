# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *

import random
import time
from SrvDef import eAIStatus, eCampRelationship, eEntityStatus, eActionRestrict, eEntityEvent
import AIRefDataType

from .AIEvent import AIEventDef
from .AIStatus import AIStatus
from Extra import ECBExtend
from .NPCAI_AD import AD_ChaseEntity
from Spell.SpellDef import SpellStatus

class NPCAI_Patrol( AIRefDataType.AIRefDataType ):
	"""
	NPC的巡逻AI
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
		self.hoverTime = dataSection["hoverTime"]				# 游荡时间上限（游荡总时间 + 游荡结束后思考的间隔时间）
		self.hoverSpeed = dataSection["hoverSpeed"]           	# 战斗徘徊奔跑速度
		self.activity = dataSection["activity"]					# 进攻积极性
		self.stopTime = dataSection["stopTime"]                 # 攻击停留时间
		
		self.idleSpeed = dataSection["idleSpeed"]				# 空闲移动速度
		self.hoverWalkSpeed = dataSection["hoverWalkSpeed"]		# 战斗徘徊移动速度
		self.idleStopTime = dataSection["idleStopTime"]			# 空闲移动停顿时间
		
		self.statusD = {
			eAIStatus.Idle           : Patrol_Idle( self ),
			eAIStatus.FightThink     : Patrol_FightThink( self ),
			eAIStatus.ChaseEntity    : AD_ChaseEntity( self ),
			eAIStatus.CastSpell      : Patrol_CastSpell( self ),
			eAIStatus.Reset          : Patrol_Reset( self ),
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
		AIEventDef.register( executor, eEntityEvent.OnEnterFight )
		
		if executor.patrolPath:
			executor.aiDatas.patrolPathID = executor.patrolPath["begin"]
		
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
		AIEventDef.deregister( executor, eEntityEvent.OnEnterFight )
		
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
			event( executor, killer )

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
		
	def onTimer_Update( self, executor ):
		"""
		AI定时器update
		"""
		event = getattr( self.statusD[executor.aiStatus], "event_onUpdate", None )
		if event:
			event( executor )
			
	def event_onEnterFight( self, executor, enemy ):
		"""
		事件：通知进入战斗状态
		"""
		event = getattr( self.statusD[executor.aiStatus], "event_onEnterFight", None )
		if event:
			event( executor, enemy )
			
			
class Patrol_Idle( AIStatus ):
	"""
	空闲状态
	"""
	TYPE = eAIStatus.Idle
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.status != eEntityStatus.Death and executor.status != eEntityStatus.Idle:
			executor.changeEntityStatus( eEntityStatus.Idle )
			
		executor.aiDatas.alertID = 0
		executor.aiDatas.moveID = 0
		
		if self.parent.alertRadius > 0.0:
			executor.aiDatas.alertID = executor.addProximity( self.parent.alertRadius, self.parent.alertRadius, ECBExtend.TRAP_ON_ALERT_RADIUS )
			
	def leave( self, executor ):
		"""
		virtual method.
		"""
		if executor.aiDatas.alertID:
			executor.cancelController( executor.aiDatas.alertID )
			executor.aiDatas.alertID = 0
		
		if executor.aiDatas.moveID:
			executor.cancelController( executor.aiDatas.moveID )
			executor.aiDatas.moveID = 0

	def think( self, executor ):
		"""
		virtual method.
		思考下一步行为
		
		@param executor: AI的执行人
		@return: 无
		"""
		if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
			executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
		else:
			if executor.aiDatas.moveID <= 0:
				if executor.patrolPath and str(executor.aiDatas.patrolPathID) in executor.patrolPath["pathGroup"].keys():
					position = executor.patrolPath["pathGroup"][str(executor.aiDatas.patrolPathID)]["position"]
					executor.aiDatas.moveID = executor.navigate_( position, self.parent.idleSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0, self._onArrive )
					if executor.aiDatas.moveID <= 0:
						executor.think( 0.5 )
				
	def _onArrive( self, executor, controllerId, state ):
		"""
		到达指定点
		"""
		if state != 0:
			executor.aiDatas.moveID = 0
			executor.aiDatas.patrolPathID = executor.patrolPath["pathGroup"][str(executor.aiDatas.patrolPathID)]["next"]
			executor.think( self.parent.idleStopTime )

	def event_onReceiveDamage( self, executor, attacker, damage ):
		"""
		virtual method.
		事件：受到来自某人的伤害
		"""
		executor.changeAIStatus( eAIStatus.FightThink, 0 )
		self._callFriends( executor, attacker )
	
	def event_onEnterAlertRadius( self, isEnter, executor, entity, controllerID ):
		"""
		"""
		if isEnter and executor.checkRelation( entity ) == eCampRelationship.Hostile and not entity.status == eEntityStatus.Death and not entity.status == eEntityStatus.Pending:
			self._callFriends( executor, entity )
			# 把敌对关系的人放到自己的战斗列表中，并切换到战斗思考状态
			executor.enemyList.add( entity )
			executor.cancelController( controllerID )
			executor.aiDatas.alertID = 0
			executor.changeAIStatus( eAIStatus.FightThink, 0 )
			
	def _callFriends( self, executor, enemy ):
		"""
		查找AI附近的友军
		"""
		objs = executor.entitiesInRange( 10.0, "Monster" )
		friends = []
		for obj in objs:
			if executor.checkRelation(obj) == eCampRelationship.Friendly and obj.status != eEntityStatus.Death:
				friends.append(obj)
		
		if len(friends) <= 0:
			return
		
		for friend in friends:
			friend.fireEvent( eEntityEvent.OnEnterFight, enemy )
		
	def event_onEnterFight( self, executor, enemy ):
		"""
		事件：通知进入战斗状态
		"""
		if not executor.enemyList.isEmpty():
			return
		
		if executor.checkRelation( enemy ) == eCampRelationship.Hostile and not enemy.status == eEntityStatus.Death and not enemy.status == eEntityStatus.Pending:
			executor.enemyList.add( enemy )
			executor.aiDatas.alertID = 0
			executor.changeAIStatus( eAIStatus.FightThink, 0 )
			self._callFriends( executor, enemy )
		
class Patrol_FightThink( AIStatus ):
	"""
	战斗思考
	"""
	TYPE = eAIStatus.FightThink
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.status != eEntityStatus.Death and executor.status != eEntityStatus.Fight:
			executor.changeEntityStatus( eEntityStatus.Fight )
			
		executor.aiDatas.updateTimerID = executor.addTimer( 0.5, 0.5, ECBExtend.TIMER_ON_AI_SIDESWAY_UPDATE )
		
	def leave( self, executor ):
		"""
		virtual method.
		"""
		if executor.aiDatas.updateTimerID:
			executor.delTimer( executor.aiDatas.updateTimerID )
			executor.aiDatas.updateTimerID = 0
			
	def event_onDead( self, executor, attacker ):
		"""
		"""
		if executor.aiDatas.updateTimerID:
			executor.delTimer( executor.aiDatas.updateTimerID )
			executor.aiDatas.updateTimerID = 0

	def think( self, executor ):
		"""
		virtual method.
		思考下一步行为
		
		@param executor: AI的执行人
		@return: 无
		"""
		if self.parent.chaseRadius > 0 and executor.target is not None and self.parent.chaseRadius < executor.position.flatDistTo( executor.target.position ):
			executor.changeAIStatus( eAIStatus.Reset, 1.0 )
			return

		# 已经有一个目标
		if executor.adjustTarget():
			self._thinkForTarget( executor )
			return

		# 没有目标，从战斗列表中挑一个
		if not executor.enemyList.isEmpty():
			# 有敌人了，将攻击名单中累积伤害最高的目标置为第一位
			executor.target = executor.enemyList.calcTarget( executor )	
			if executor.target is None:  # 我擦！全跑了？
				executor.changeAIStatus( eAIStatus.Reset, 0.1 )
				return
			self._thinkForTarget( executor )
		else:
			# 没有敌人了，复位
			executor.changeAIStatus( eAIStatus.Reset, 1.0 )

	def _thinkForTarget( self, executor ):
		"""
		针对当前目标选择一个行为
		"""
		# 根据积极性选择下一次是攻击还是游荡
		if random.random() <= self.parent.activity:
			# 看看能不能找个法术放一放
			spell = self.parent.selectSpell( executor, executor.target )
			if spell is not None:   # 找到了一个可放的技能
				executor.aiCurrentSpell = spell
				executor.changeAIStatus( eAIStatus.CastSpell, 0.1 )
				return

		executor.changeAIStatus( eAIStatus.ChaseEntity, 0.1 )
		
	def event_onUpdate( self, executor ):
		"""
		更新AI状态
		"""
		if executor.target and executor.hasActionRestrict( eActionRestrict.ForbidMove ) is False:
			executor.lookAt( executor.target.position )
			
class Patrol_CastSpell( AIStatus ):
	"""
	追击施法
	"""
	TYPE = eAIStatus.CastSpell
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.aiCurrentSpell.distance < executor.position.flatDistTo( executor.target.position ):
			if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
				executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
			else:
				if self.parent.chaseRadius <= 0:
					executor.changeAIStatus( eAIStatus.Reset, 0.5 )
				else:
					executor.aiDatas.moveID = executor.navigate_( executor.target.position, self.parent.hoverSpeed, executor.aiCurrentSpell.distance - 0.5, 0xFFFF, 0xFFFF, True, 0, self._onSpellChaseOver )
				
					# 走不动？那就重新想想要做啥吧
					if executor.aiDatas.moveID <= 0:
						executor.aiCurrentSpell = None
						executor.changeAIStatus( eAIStatus.FightThink, 1.0 )
		else:
			executor.lookAt( executor.target.position )
			result = executor.castSpell( executor.aiCurrentSpell, executor.target )

			if result == SpellStatus.OK:
				executor.aiCurrentSpell = None
			else:
				# 进来就有问题？那就重新想想要做啥吧
				executor.aiCurrentSpell = None
				executor.changeAIStatus( eAIStatus.FightThink, 1.0 )
				return
		
	def leave( self, executor ):
		"""
		virtual method.
		"""
		if executor.aiDatas.moveID:
			executor.cancelController( executor.aiDatas.moveID )
			executor.aiDatas.moveID = 0

	def think( self, executor ):
		"""
		virtual method.
		思考下一步行为
		
		@param executor: AI的执行人
		@return: 无
		"""
		if self.parent.chaseRadius > 0 and executor.target is not None and self.parent.chaseRadius < executor.position.flatDistTo( executor.target.position ):
			executor.changeAIStatus( eAIStatus.Reset, 1.0 )
			return
		
		# 没有技能在施展了，切回思考状态
		if executor.aiCurrentSpell is None and executor.currentSpell is None:
			executor.changeAIStatus( eAIStatus.FightThink, self.parent.stopTime )
			return
			
		executor.think( 0.1 )

	def _onSpellChaseOver( self, executor, controllerId, state ):
		"""
		选定技能追击目标结束
		"""
		if state != 0:
			executor.lookAt( executor.target.position )
			executor.castSpell( executor.aiCurrentSpell, executor.target )
			executor.aiCurrentSpell = None
			executor.think( 0.1 )

	def event_onActionRestrictChanged( self, executor, flag ):
		"""
		事件：当某个行为限制状态改变时被调用
		"""
		if ( flag == eActionRestrict.ForbidMove ):
			if executor.hasActionRestrict( flag ):
				if executor.aiDatas.moveID:
					executor.cancelController( executor.aiDatas.moveID )
					executor.aiDatas.moveID = 0

		# 这里这么做的目的是为了保证被禁止思考结束后能立即切换到战斗状态，不受攻击停留时间影响
		if ( flag == eActionRestrict.ForbidThink ):
			if not executor.hasActionRestrict( flag ):
				executor.changeAIStatus( eAIStatus.FightThink, 0 )

class Patrol_Reset( AIStatus ):
	"""
	重置状态：脱离战斗，回到路径点
	"""
	TYPE = eAIStatus.Reset
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
			executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
		else:
			self.reset( executor )
			executor.changeAIStatus( eAIStatus.Idle, 0.5 )

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

	def reset( self, executor ):
		"""
		"""
		# @TODO(xh): 这里reset不清除aiDatas的原因是由于AI不同状态之间需要传递数据，甚至是不同AI之间也需要传递aiDatas里的数据，不保存的属性不清理也没关系
		#executor.aiDatas.__dict__.clear()
		executor.target = None                        # Entity; 攻击目标
		executor.aiCurrentSpell = None                # 当前正准备用的技能
