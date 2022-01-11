# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from BuffDataType import BuffDataType
from ..SpellDef import SpellStatus
from ..BuffCron import BuffCron
from ..SpellLoader import g_spellLoader


class BuffSimple(BuffCron):
	"""
	基础buff类，仅实现基础的功能
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		BuffCron.init( self, dataSection )

		combatFunction = dataSection["combatFunction"]
		self.attachEffectIDs = combatFunction["attachEffect"].readInts( "item" )                 # 附上时执行的效果
		self.detachEffectIDs = combatFunction["detachEffect"].readInts( "item" )                 # buff正常卸下（非中断结束）时执行的效果
		self.tickEffectIDs = combatFunction["tickEffect"].readInts( "item" )                     # 每心跳一次执行一次的效果
		self.battlecryEffectIDs = combatFunction["battlecryEffect"].readInts( "item" )           # buff准备附上时执行的效果（用于如去掉指定编号的buff）
		self.interruptEffectIDs = combatFunction["interruptEffect"].readInts( "item" )           # buff被中断（非正常卸下）时执行的效果
		self.castFailEffectIDs = combatFunction["castFailEffect"].readInts( "item" )             # buff添加失败时执行的效果
				
		self.attachEffect = []
		for id in self.attachEffectIDs:
			self.attachEffect.append( g_spellLoader.getEffect( id ) )

		self.detachEffect = [] 
		for id in self.detachEffectIDs:
			self.detachEffect.append( g_spellLoader.getEffect( id ) )

		self.tickEffect = []
		for id in self.tickEffectIDs:
			self.tickEffect.append( g_spellLoader.getEffect( id ) )
		
		self.battlecryEffect = []
		for id in self.battlecryEffectIDs:
			self.battlecryEffect.append( g_spellLoader.getEffect( id ) )
	
		self.interruptEffect = []
		for id in self.interruptEffectIDs:
			self.interruptEffect.append( g_spellLoader.getEffect( id ) )

		self.castFailEffect = []
		for id in self.castFailEffectIDs:
			self.castFailEffect.append( g_spellLoader.getEffect( id ) )

	def cast(self, src, dst, spell):
		"""
		virtual method.
		把buff效果作用在target身上，允许失败
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		if not dst:
			ERROR_MSG( "No target for buff '%s:%s'" % (self.id, self.name) )
			return

		for cond in self.conditions:
			# 条件不满足，那就施加失败效果
			if cond.verify( dst, src ) != SpellStatus.OK:
				for effect in self.castFailEffect:
					effect.cast( src, dst, None )
				return
		
		# 检查添加条件是否满足
		if self.onCast(src, dst, spell) != SpellStatus.OK:
			return
			
		return self.attach(src, dst)
	
	def onCast(self, src, dst, spell):
		"""
		template method.
		这个接口有以下作用：
		1.检查是否有足够的条件添加到目标；
		2.处理Attach()到目标前必须做的一些事情。
		2.1.例如如果需要覆盖现有的buff，可以在这里把旧buff删除;
		2.2.又例如要増量当前的buff（叠加）效果，也可以在这里把要増量的效果进行处理，完毕后直接返回false，以避免自身添加到目标身上；
		
		@return: SpellStatus
		"""
		for effect in self.battlecryEffect:
			effect.cast( src, dst, None )
		return SpellStatus.OK
	
	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情
		例如：给owner加上10点基础伤害
		"""
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.attachEffect:
			effect.cast( src, dst, None )
	
	def onDetach(self, owner, buffData):
		"""
		template method.
		当buff从owner身上取下来时，此接口被调用（仅调用一次）
		可以在此处做一些buff结束时的事情
		例如：给owner减去10点基础伤害
		"""
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.detachEffect:
			effect.cast( src, owner, None )
	
	def onTick(self, owner, buffData):
		"""
		template method.
		buff的心跳，每一跳都有可能会做一些事情
		@return: bool; true: buff需要继续心跳；false: buff不需要继续心跳了，亦即应该从玩家身上卸去了
		"""
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.tickEffect:
			effect.cast( src, owner, None )
		return True

	def onInterrupt(self, owner, buffData):
		"""
		template method.
		buff中断回调，继承于此类的buff可以根据实际情况做自己想做的事情
		"""
		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.interruptEffect:
			effect.cast( src, owner, None )
