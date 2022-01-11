# -*- coding: utf-8 -*-
#

"""
"""

import KBEngine
import json
from KBEDebug import *

import KST

from interfaces.GameObject import GameObject
from interfaces.Avatar import Avatar
from interfaces.AI import AI
from SrvDef import eObjectType, eEntityEvent
from Extra import ECBExtend
import GloballyDefine as GD
import GloballyStatus as GS
from Extra.CellConfig import g_entityConfig, g_MonsterConfig, g_entityFightConfig
from NPCAI.AILoader import g_aiLoader
import Extra.SpaceEventID as SpaceEventID

class Monster( GameObject, Avatar, AI ):
	"""
	非玩家对象基础类
	"""
	def __init__(self):
		"""
		构造函数
		"""
		GameObject.__init__(self)
		Avatar.__init__(self)
		self.objectType = eObjectType.Monster

		self.addAttribute( self.getFightConfig() )
		self.attributeOperation()
		
		self.addTimer( 1.0, 0.0, ECBExtend.TIMER_ON_DELAY_INIT )

	def getName( self ):
		"""
		virtual method.
		@return: the name of entity
		@rtype:  STRING
		"""
		return self.name

	def onThink( self ):
		"""
		virtual method.
		AI思考
		"""
		AI.onThink( self )

	def onDead( self, killer ):
		"""
		virtual method.
		我被杀死了
		
		@param killer: Entity; 凶手
		"""	
		Avatar.onDead(self, killer)
		
		#通知场景管理器，NPC已经死亡
		spaceBase = self.getCurrentSpaceBase()
		if spaceBase is None:
			return
		
		space = KBEngine.entities[spaceBase.id]
		space.fireEvent(SpaceEventID.EntityDead, self)
		self.fireEvent( eEntityEvent.OnDead, killer )
		
		self.addTimer( 5.0, 0, ECBExtend.TIMER_ON_NPC_DEAD )
		
	def addAttribute( self, fightConfig):
		Avatar.addAttribute(self, fightConfig)
		self.level = fightConfig['level']
		self.HP =  fightConfig['HPMax_base']
		self.MP = fightConfig['HPMax_base']

	def getFightConfig( self ):
		return g_entityFightConfig[self.fightID]

	def onTimer_destroy( self, timerID, userArg ):
		"""
		死亡销毁定时回调
		"""		
		#注销场景事件
		self.destroy()
	
	def onTimer_delayInit( self, timerID, userArg ):
		"""
		做一些延时初始化。目的地二：
		一是使一些无法在__init__()上初始化的功能得到正确的初始化，例如：npc创建以后需要放个陷阱；
		二是为了让AI晚些触发，以保证客户端上存在这个entity了才做进一步的事情，
		否则有可能导致有些消息发送到客户端后找不到对象接收，使得某些功能可能出现异常。
		"""
		# 初始化AI，放在这里初始化的原因是ai有可能需要放陷阱，这个陷阱必须在__init__()完成后的下一个tick才能做
		config = self.getConfig()
		self.ai = g_aiLoader.get( config['ai'] )
		if self.ai is not None:
			self.ai.attach( self )
		else:
			ERROR_MSG("self.ai = None")
		
	def onEvent(self, name, *args):
		pass

	def onActionRestrictChanged( self, flag ):
		"""
		template method.
		当某个行为限制状态改变时被调用
		
		@param flag: eActionRestrict.*；表示当前有变化的状态，可以通过检查当前状态来确定旧的状态
		"""
		Avatar.onActionRestrictChanged( self, flag )
		if self.ai is not None:
			func = getattr( self.ai, "event_onActionRestrictChanged", None )
			func and func( self, flag )

	def onReceiveDamage( self, attacker, result ):
		"""
		virtual method.
		受到伤害的触发：在减血以后，死亡之前触发
		"""
		self.notifyClientDisplayDamage( attacker, result )
		self.fireEvent( eEntityEvent.OnReceiveDamage, attacker, result.damage )

	def notifyClientDisplayDamage( self, attacker, result ):
		"""
		virtual method.
		通知客户端掉血显示
		"""
		displayResullt = result.hit* GD.FightResultType.Hit | result.crit* GD.FightResultType.Crit

		if attacker.objectType == eObjectType.Player:
			try:
				attacker.clientEntity(self.id).triggerFightResultFS( displayResullt, -result.damage )
			except:
				pass

# 注册timer等回调处理接口
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_NPC_DEAD, Monster.onTimer_destroy )
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_DELAY_INIT, Monster.onTimer_delayInit )

# 注册到事件表中
Monster.registerClass( Monster )
