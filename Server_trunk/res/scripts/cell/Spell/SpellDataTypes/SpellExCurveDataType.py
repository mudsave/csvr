# -*- coding: utf-8 -*-
#
"""
"""
import pickle
import time

from KBEDebug import *
from Extra import ECBExtend
from SpellDataType import SpellDataType
from SrvDef import eObjectType

class SpellExCurveDataType( SpellDataType ):
	"""
	SpellExCurveFightRelySer对应的技能数据类型
	"""
	dataType = 2
	
	def __init__(self, spell = None, startDelay = 1.0):
		"""
		"""
		self.startDelay = startDelay
		
		self.spellID = 0
		self.targetID = 0
		self.miscData = {}
		if spell is not None:
			self.spellID = spell.id

	def makeMiscData(self):
		"""
		template method.
		"""
		return pickle.dumps( self.miscData, 2 )

	def initMiscData(self, miscData):
		"""
		template method.
		"""
		self.miscData = pickle.loads( miscData )

	def start(self, caster, args):
		"""
		开始监测技能
		@param caster: Entity; 施法者
		@param second: float; 多少秒后触发
		@param callbackName: string; 回调函数名，格式：def callback( caster, args )
		@param args: 任意可序列化成数据流的自定义数据
		"""
		assert self.miscData.get( "timerID", 0) <= 0, "不能重复开启"
		caster.setCurrentSpellDataType( self )
		self.miscData["index"] = 0
		spell = KST.g_spellLoader.getSpell(self.spellID)
		self.miscData["timerID"] = caster.addTimer( spell.curveTriggers[self.miscData["index"]].intervalTime, 0, ECBExtend.TIMER_ON_SPELL_CASTING )
		
	def stop(self, caster):
		"""
		结束技能监测
		"""
		#assert self.miscData.get( "timerID",0 ) > 0, "不能重复结束监测，或技能监测未开启"
		if self.miscData.get( "timerID",0 ) <= 0:
			return

		caster.delTimer( self.miscData["timerID"] )
		self.miscData["timerID"] = 0

		SpellDataType.stop( self, caster )

	def onTimer(self, caster, timerID, userData):
		"""
		virtual method.
		定时器回调
		"""
		spell = KST.g_spellLoader.getSpell(self.spellID)
		index = self.miscData["index"]
		self.miscData["index"] = index + 1
		spell.doTriggerEffect( caster, index, self.targetID )
		if self.miscData["index"] < len( spell.curveTriggers ):
			self.miscData["timerID"] = caster.addTimer( spell.curveTriggers[self.miscData["index"]].intervalTime, 0, ECBExtend.TIMER_ON_SPELL_CASTING )
		else:
			self.stop( caster )


# 放在最后，以避免循环引用
import KST
