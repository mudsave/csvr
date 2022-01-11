# -*- coding: utf-8 -*-

"""
"""
import weakref
import random

import KBEngine
import time
from KBEDebug import *

from .AIEvent import AIEventDef
from Extra import ECBExtend
from Extra.EntityEvent import EntityEvent
from SrvDef import eAIStatus, eCampRelationship, eEntityStatus, eObjectType, eActionRestrict
from Spell.SpellDef import SpellStatus

class AIStatus(object):
	"""
	AI状态机的状态基础类
	"""
	def __init__( self, parent ):
		"""
		@param parent: 拥有此状态的状态机
		"""
		# 使用弱引用，以避免循环引用引起的内存不释放的问题
		self.parent = weakref.proxy( parent )
	
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
		"""
		pass





class STA_Birth( AIStatus ):
	"""
	出生状态
	"""
	TYPE = eAIStatus.Birth
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		executor.changeEntityStatus( eEntityStatus.Pending )
		executor.aiDatas.birthStart = time.time() #记录开始时间
		
	def leave( self, executor ):
		"""
		virtual method.
		"""
		executor.changeEntityStatus( eEntityStatus.Idle )
		executor.actionFX = "idle"
		executor.actionFX = ""

	def think( self, executor ):
		"""
		virtual method.
		思考下一步行为
		
		@param executor: AI的执行人
		@return: 无
		"""
		if self.parent.birthTime <= time.time() - executor.aiDatas.birthStart:	
			executor.changeAIStatus( eAIStatus.Idle, 0 )
		else:
			executor.think(0.1)

class STA_Idle( AIStatus ):
	"""
	空闲状态
	"""
	TYPE = eAIStatus.Idle
	
	WAIT_MIN = 2.0
	WAIT_MAX = 3.0
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.status != eEntityStatus.Death and executor.status != eEntityStatus.Idle:
			executor.changeEntityStatus( eEntityStatus.Idle )
			
		executor.aiDatas.alertID = 0
		executor.aiDatas.moveID = 0
		
		if self.parent.randomWalkRadius > 0.0:
			if random.random() <= self.parent.activity:
				pos = self.parent.calcRandomWalkPosition( executor )
				executor.aiDatas.moveID = executor.navigate_( pos, self.parent.hoverSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
			if executor.aiDatas.moveID <= 0:
				executor.think( random.uniform( self.WAIT_MIN, self.WAIT_MAX ) )
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
			if random.random() <= self.parent.activity:
				pos = self.parent.calcRandomWalkPosition( executor )
				executor.aiDatas.moveID = executor.navigate_( pos, self.parent.hoverSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
			if executor.aiDatas.moveID <= 0:
				executor.think( random.uniform( self.WAIT_MIN, self.WAIT_MAX ) )

	def _onChaseOver( self, executor, controllerId, state ):
		"""
		选定技能追击目标结束
		"""
		if state != 0:
			executor.aiDatas.moveID = 0
			executor.think( random.uniform( self.WAIT_MIN, self.WAIT_MAX ) )

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


class STA_FightThink( AIStatus ):
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
		# 只要离出生点太远了，就要脱离战斗
		if self.parent.chaseRadius < executor.position.flatDistTo( executor.spawnPosition ):
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

class STA_ChaseEntity( AIStatus ):
	"""
	围绕目标徘徊
	"""
	TYPE = eAIStatus.ChaseEntity

	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
			executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
		else:
			executor.aiDatas.startChaseTime = time.time() #记录开始游荡的时间
			# 太远了，要跟上，加0.1的原因是navigate()移动的结果会有少少的误差
			if self.parent.hoverRadiusMax + 0.1 < executor.position.flatDistTo( executor.target.position ):
				executor.aiDatas.moveID = executor.navigate_( executor.target.position, executor.moveSpeed, self.parent.hoverRadiusMax, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
			else:
				# 在目标附近晃悠（徘徊）
				pos = self.parent.halfCircleCalcHoverPosition( executor.target.position, executor.position )
				executor.aiDatas.moveID = executor.navigate_( pos, self.parent.hoverSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0, self._onChaseOver )
		
			# 徘徊失败，过一会后重新思考下一步行动
			if executor.aiDatas.moveID <= 0:
				executor.changeAIStatus( eAIStatus.FightThink, 1.0 )

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


class STA_CastSpell( AIStatus ):
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
				executor.aiDatas.moveID = executor.navigate_( executor.target.position, executor.moveSpeed, executor.aiCurrentSpell.distance - 1.0, 0xFFFF, 0xFFFF, True, 0, self._onSpellChaseOver )
			
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
		if self.parent.chaseRadius < executor.position.flatDistTo( executor.spawnPosition ):
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

class STA_Reset( AIStatus ):
	"""
	重置状态：脱离战斗，回到出生点
	"""
	TYPE = eAIStatus.Reset
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		if executor.hasActionRestrict( eActionRestrict.ForbidMove ):
			executor.changeAIStatus( eAIStatus.FightThink, 0.5 )
		else:
			executor.aiDatas.moveID = executor.navigate_( executor.spawnPosition, executor.moveSpeed, 0.0, 0xFFFF, 0xFFFF, True, 0,  self._onMoveToSpawnPositionOver )  # 回到出生点
		
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

		# @TODO(phw): 理论上，如果逻辑没有问题的话，这个应该是可以不要的
		#if executor.position.flatDistTo( executor.spawnPosition ) <= 0.001:
		#	self.reset( executor )
		#	executor.changeAIStatus( eAIStatus.Idle, 1.0 )

	def reset( self, executor ):
		"""
		"""
		# @TODO(xh): 这里reset不清除aiDatas的原因是由于AI不同状态之间需要传递数据，甚至是不同AI之间也需要传递aiDatas里的数据，不保存的属性不清理也没关系
		#executor.aiDatas.__dict__.clear()
		executor.target = None                        # Entity; 攻击目标
		executor.aiCurrentSpell = None                # 当前正准备用的技能
		executor.HP = executor.HPMax                  # 回血
		executor.MP = executor.MPMax                  # 回蓝
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
