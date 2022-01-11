# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *

from SrvDef import eEntityEvent

class AIEventDef:
	"""
	"""
	registerD = {
		eEntityEvent.OnHPChanged      : lambda executor, event: executor.registerEvent(event, executor.__class__, "aiEvent_onHPChanged"),
		eEntityEvent.OnReceiveDamage  : lambda executor, event: executor.registerEvent(event, executor.__class__, "aiEvent_onReceiveDamage"),
		eEntityEvent.OnKilled         : lambda executor, event: executor.registerEvent(event, executor.__class__, "aiEvent_onKilled"),
		eEntityEvent.OnDead           : lambda executor, event: executor.registerEvent(event, executor.__class__, "aiEvent_onDead"),
		eEntityEvent.OnEnterFight     : lambda executor, event: executor.registerEvent(event, executor.__class__, "aiEvent_onEnterFight"),
	}
	
	deregisterD = {
		eEntityEvent.OnHPChanged      : lambda executor, event: executor.deregisterEvent(event, executor.__class__, "aiEvent_onHPChanged"),
		eEntityEvent.OnReceiveDamage  : lambda executor, event: executor.deregisterEvent(event, executor.__class__, "aiEvent_onReceiveDamage"),
		eEntityEvent.OnKilled         : lambda executor, event: executor.deregisterEvent(event, executor.__class__, "aiEvent_onKilled"),
		eEntityEvent.OnDead           : lambda executor, event: executor.deregisterEvent(event, executor.__class__, "aiEvent_onDead"),
		eEntityEvent.OnEnterFight     : lambda executor, event: executor.deregisterEvent(event, executor.__class__, "aiEvent_onEnterFight"),
	}
	
	@classmethod
	def register( cls, executor, event ):
		"""
		@param executor: 执行AI行为的entity（例如：NPC、宠物）
		@param    event: eEntityEvent.*
		"""
		f = cls.registerD.get( event )
		if f:
			f(executor, event)

	@classmethod
	def deregister( cls, executor, event ):
		"""
		@param executor: 执行AI行为的entity（例如：NPC、宠物）
		@param    event: eEntityEvent.*
		"""
		f = cls.deregisterD.get( event )
		if f:
			f(executor, event)

