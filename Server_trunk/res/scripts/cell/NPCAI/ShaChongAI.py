# -*- coding: utf-8 -*-

"""
"""

import KBEngine
import Math 
import math
from KBEDebug import *

from SrvDef import eAIStatus, eEntityEvent, eObjectType, eEffectStatus
import AIRefDataType

from .AIEvent import AIEventDef
from .AIStatus import AIStatus
from Extra import ECBExtend
from SrvDef import eAIStatus, eCampRelationship, eEntityStatus
from Spell.SpellLoader import g_spellLoader
from .AIStatus import STA_Birth
import random

class ShaChongAI( AIRefDataType.AIRefDataType ):
	"""
	沙虫BOSS
	"""
	def init( self, dataSection ):
		"""
		"""
		AIRefDataType.AIRefDataType.init( self, dataSection )
		
		self.skillList = dataSection["skillList"]               # 可用的攻击技能
		self.chaseRadius = dataSection["chaseRadius"]           # 追击半径（超过此半径，怪物应脱离战斗并回到出生点），目标一旦远离这个距离，就复位
		self.alertRadius = dataSection["alertRadius"]           # 警戒半径（有玩家进入此范围则自动攻击），小于或等于0表示不警戒
		self.activity = dataSection["activity"]					# 进攻积极性
		self.birthTime = dataSection["birthTime"]               # 出生状态时间

		self.statusD = {
			eAIStatus.Birth          : STA_Birth( self ),
			eAIStatus.Idle           : SC_Idle( self ),
			eAIStatus.FightThink     : SC_FightThink( self ),
			eAIStatus.CastSpell      : SC_CastSpell( self ),
			eAIStatus.Reset          : SC_Reset( self ),
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

		#临时写法，默认该AI的怪物有霸体状态
		executor.effectStatusCounterIncr(eEffectStatus.SuperBody)

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

class SC_Idle( AIStatus ):
	"""
	空闲状态
	"""
	TYPE = eAIStatus.Idle
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		executor.aiDatas.alertID = 0
		
		if self.parent.alertRadius > 0.0:
			executor.aiDatas.alertID = executor.addProximity( self.parent.alertRadius, self.parent.alertRadius, ECBExtend.TRAP_ON_ALERT_RADIUS )
			
		
	def leave( self, executor ):
		"""
		virtual method.
		"""
		if executor.aiDatas.alertID:
			executor.cancelController( executor.aiDatas.alertID )
			executor.aiDatas.alertID = 0

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

class SC_FightThink( AIStatus ):
	"""
	战斗思考
	"""
	TYPE = eAIStatus.FightThink
	
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
		# 目标太远了，复位
		if executor.target is not None and self.parent.chaseRadius > 0 and self.parent.chaseRadius < executor.position.flatDistTo( executor.target.position ):
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
		else:
			executor.think(1.0)

		executor.think(0.5)

class SC_CastSpell( AIStatus ):
	"""
	施法
	"""
	TYPE = eAIStatus.CastSpell

	WAIT_MIN = 0.0
	WAIT_MAX = 2.0
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
		# 进来就直接放技能
		executor.lookAt( executor.target.position )
		executor.castSpell( executor.aiCurrentSpell, None )
		executor.aiCurrentSpell = None
		
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
		# 没有技能在施展了，切回思考状态
		if executor.aiCurrentSpell is None and executor.currentSpell is None:
			executor.changeAIStatus( eAIStatus.FightThink, random.uniform( self.WAIT_MIN, self.WAIT_MAX ))
			return

		executor.think( 0.1 )

class SC_Reset( AIStatus ):
	"""
	重置状态：脱离战斗
	"""
	TYPE = eAIStatus.Reset
	
	def enter( self, executor ):
		"""
		virtual method.
		"""
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
		executor.HP = executor.HPMax                  # 回血
		executor.MP = executor.MPMax                  # 回蓝
		executor.enemyList.clearAndNotify( executor ) # 清除敌人列表
