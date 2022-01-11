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
from .AIStatus import AIStatus, STA_Birth
from Extra import ECBExtend
from Spell.SpellDef import SpellStatus

class NPCAI_AD( AIRefDataType.AIRefDataType ):
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
		self.hoverSpeed = dataSection["hoverSpeed"]           	# 战斗徘徊奔跑速度
		self.activity = dataSection["activity"]					# 进攻积极性
		self.stopTime = dataSection["stopTime"]                 # 攻击停留时间
		self.birthTime = dataSection["birthTime"]               # 出生状态时间
		
		self.moveActivity = dataSection["moveActivity"]			# 移动积极性
		self.idleSpeed = dataSection["idleSpeed"]				# 空闲移动速度
		self.hoverWalkSpeed = dataSection["hoverWalkSpeed"]		# 战斗徘徊移动速度
		self.idleStopTime = dataSection["idleStopTime"]			# 空闲移动停顿时间
		
		self.statusD = {
			eAIStatus.Birth          : STA_Birth( self ),
			eAIStatus.Idle           : AD_Idle( self ),
			eAIStatus.FightThink     : AD_FightThink( self ),
			eAIStatus.ChaseEntity    : AD_ChaseEntity( self ),
			eAIStatus.CastSpell      : AD_CastSpell( self ),
			eAIStatus.Reset          : AD_Reset( self ),
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
			
class AD_Idle( AIStatus ):
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
		
		if self.parent.idleStopTime <= 0.5:
			self.parent.idleStopTime = 0.5
		
		if self.parent.randomWalkRadius > 0.0:
			if random.random() <= self.parent.moveActivity:
				pos = self.parent.calcRandomWalkPosition( executor )
				executor.aiDatas.moveID = executor.navigate_( pos, self.parent.idleSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
			if executor.aiDatas.moveID <= 0:
				executor.think( self.parent.idleStopTime )
		else:
			executor.aiDatas.moveID = 0

		# 注意：陷阱的放置必须放在函数最后面，
		#       因为在陷阱放置的那一刻就有可能直接触发onEnterTrap()的回调，
		#       在这种情况下，如果此代码不在最后面，那回调完成后下面还会接着进行初始化，
		#       就会导致逻辑上的错乱——例如卡在STA_ChaseEntity状态上!!!
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
		if executor.aiDatas.moveID <= 0 and self.parent.randomWalkRadius > 0.0:
			if random.random() <= self.parent.moveActivity:
				pos = self.parent.calcRandomWalkPosition( executor )
				executor.aiDatas.moveID = executor.navigate_( pos, self.parent.idleSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
			if executor.aiDatas.moveID <= 0:
				executor.think( self.parent.idleStopTime )

	def _onChaseOver( self, executor, controllerId, state ):
		"""
		空闲移动距离结束
		"""
		if state != 0:
			executor.aiDatas.moveID = 0
			executor.think( self.parent.idleStopTime )

	def event_onReceiveDamage( self, executor, attacker, damage ):
		"""
		virtual method.
		事件：受到来自某人的伤害
		"""
		executor.changeAIStatus( eAIStatus.FightThink, 0 )
	
	def event_onEnterAlertRadius( self, isEnter, executor, entity, controllerID ):
		"""
		"""
		if isEnter and executor.checkRelation( entity ) == eCampRelationship.Hostile and not entity.status == eEntityStatus.Death and not entity.status == eEntityStatus.Pending:
			# 把敌对关系的人放到自己的战斗列表中，并切换到战斗思考状态
			executor.enemyList.add( entity )
			# 注意：在这个时刻，必须主动使用controllerID这个参数来取消陷阱，
			#       因为由于陷阱的机制问题，有可能在这个回调触发的时候，
			#       executor.aiDatas.alertID = executor.addProximity( ... )行为还未执行完，
			#       这样alertID还未保存有相应的controller id。
			#       这就使得在下面的切換状态时的leave()上不会取消当前的陷阱。
			executor.cancelController( controllerID )
			executor.aiDatas.alertID = 0
			executor.changeAIStatus( eAIStatus.FightThink, 0 )

class AD_FightThink( AIStatus ):
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
		被搞死了
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
		# 只要离出生点太远了，就要脱离战斗
		if self.parent.chaseRadius > 0 and self.parent.chaseRadius < executor.position.flatDistTo( executor.spawnPosition ):
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

class AD_ChaseEntity( AIStatus ):
	"""
	围绕目标徘徊
	"""
	TYPE = eAIStatus.ChaseEntity
	
	SIDESWAY_PR = 0.5

	def enter( self, executor ):
		"""
		virtual method.
		"""
		executor.aiDatas.updateTimerID = executor.addTimer( 0.5, 0.5, ECBExtend.TIMER_ON_AI_SIDESWAY_UPDATE )
		executor.aiDatas.startChaseTime = time.time() #记录开始游荡的时间
		if self.parent.hoverTime <= 0.5:
			self.parent.hoverTime = 0.5
		self._chaseEntity( executor )

	def leave( self, executor ):
		"""
		virtual method.
		"""
		if executor.aiDatas.moveID:
			executor.cancelController( executor.aiDatas.moveID )
			executor.aiDatas.moveID = 0
			
		if executor.aiDatas.updateTimerID:
			executor.delTimer( executor.aiDatas.updateTimerID )
			executor.aiDatas.updateTimerID = 0
			
	def event_onDead( self, executor, attacker ):
		"""
		"""
		if executor.aiDatas.updateTimerID:
			executor.delTimer( executor.aiDatas.updateTimerID )
			executor.aiDatas.updateTimerID = 0
			
	def event_onUpdate( self, executor ):
		"""
		更新AI状态
		"""
		if executor.target and executor.hasActionRestrict( eActionRestrict.ForbidMove ) is False:
			executor.lookAt( executor.target.position )
		
	def _chaseEntity( self, executor ):
		"""
		"""
		if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
			executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
		else:
			if self.parent.hoverRadiusMax <= 0:
				executor.changeAIStatus( eAIStatus.FightThink, self.parent.hoverTime )
			
			else:
				# 太远了，要跟上，加0.1的原因是navigate()移动的结果会有少少的误差
				if self.parent.hoverRadiusMax + 0.1 < executor.position.flatDistTo( executor.target.position ):
					executor.aiDatas.moveID = executor.navigate_( executor.target.position, self.parent.hoverSpeed, self.parent.hoverRadiusMax, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
				else:
					# 在目标附近晃悠（徘徊）
					distance = executor.position - executor.target.position
					if distance.length == 0:
						distance = executor.target.direction
					if random.random() > self.SIDESWAY_PR:
						pos = self.parent.retreatCalcHoverPosition( distance, executor.target.position )						
						speed = self.parent.hoverWalkSpeed
						if executor.target.position.flatDistTo( pos ) < executor.position.flatDistTo( executor.target.position ):
							speed = self.parent.hoverSpeed
							
						executor.aiDatas.moveID = executor.navigate_( pos, speed, 0.0, 0xFFFF, 0xFFFF, False, 0, self._onChaseOver )
					else:
						pos = self.parent.sideswayCalcHoverPosition( distance, executor.target.position )
						executor.aiDatas.moveID = executor.navigate_( pos, self.parent.hoverWalkSpeed, 0.0, 0xFFFF, 0xFFFF, False, 0, self._onChaseOver )
						
				# 徘徊失败，过一会后重新思考下一步行动
				if executor.aiDatas.moveID <= 0:
					executor.changeAIStatus( eAIStatus.FightThink, 1.0 )

	def think( self, executor ):
		"""
		virtual method.
		思考下一步行为
		
		@param executor: AI的执行人
		@return: 无
		"""
		pass

	def _onChaseOver( self, executor, controllerId, state ):
		"""
		选定技能追击目标结束
		"""
		if state != 0:
			remainTime = self.parent.hoverTime - ( time.time() - executor.aiDatas.startChaseTime )
			if remainTime < 0:
				remainTime = 0
				
			executor.changeAIStatus( eAIStatus.FightThink, remainTime )

	def event_onActionRestrictChanged( self, executor, flag ):
		"""
		事件：当某个行为限制状态改变时被调用
		"""
		if ( flag == eActionRestrict.ForbidMove ):
			if executor.hasActionRestrict( flag ):
				if executor.aiDatas.moveID:
					executor.cancelController( executor.aiDatas.moveID )
					executor.aiDatas.moveID = 0
					executor.changeAIStatus( eAIStatus.FightThink, 0 )
					
class AD_CastSpell( AIStatus ):
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
					executor.aiDatas.moveID = executor.navigate_( executor.target.position, self.parent.hoverSpeed, executor.aiCurrentSpell.distance - 1.0, 0xFFFF, 0xFFFF, True, 0, self._onSpellChaseOver )
				
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
		# 只要离出生点太远了，就要脱离战斗
		if self.parent.chaseRadius > 0 and self.parent.chaseRadius < executor.position.flatDistTo( executor.spawnPosition ):
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
				
class AD_Reset( AIStatus ):
	"""
	重置状态
	"""
	TYPE = eAIStatus.Reset
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
			executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
		else:
			executor.aiDatas.moveID = executor.navigate_( executor.spawnPosition, self.parent.hoverSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0,  self._onMoveToSpawnPositionOver )  # 回到出生点
		
			# 走不回去了？？？那直接定位过去吧！！！
			if executor.aiDatas.moveID <= 0:
				ERROR_MSG( "I can't move to spawn position ('%s') use navigate()" % executor.spawnPosition )
				executor.position = executor.spawnPosition
				self.reset( executor )
				executor.changeAIStatus( eAIStatus.Idle, 1.0 )

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
		executor.enemyList.clearAndNotify( executor ) # 清除敌人列表

	def _onMoveToSpawnPositionOver( self, executor, controllerId, state ):
		"""
		@param        executor: 移动的entity
		@param controllerId: 移动控制器的ID
		@param        state: -1 移动结束且失败；0 开始移动；1 移动结束且成功
		"""
		if state != 0:
			self.reset( executor )
			executor.changeAIStatus( eAIStatus.Idle, 1.0 )


