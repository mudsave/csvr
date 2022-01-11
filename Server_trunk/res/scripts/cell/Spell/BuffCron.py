# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

import time
from Extra import ECBExtend
from .SpellDef import SpellStatus
from BuffDataType import BuffDataType
from .Spell import SpellBuff
from SrvDef import eBuffSaveType
from .CastConditions import createCondition

class BuffCron(SpellBuff):
	"""
	使用timer定时触发效果的timer
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		SpellBuff.init( self, dataSection )
		
		generalFunction = dataSection["generalFunction"]
		self.stayTime = generalFunction.readFloat( "stayTime" )        # 持续时间
		self.tickTime = generalFunction.readFloat( "tickTime" )        # 每次心跳间隔（小于等于0则不心跳）
		self.interruptCodes = set( generalFunction.readIntArray( "interruptCode", "," ) )   # 中断码
		self.dataSaveType = generalFunction.readInt( "saveType" )      # buff保存类型
		self.syncType = generalFunction.readInt( "syncType" )      	   # buff同步类型

		self.conditions = []
		if generalFunction.has_key( "conditions" ):
			for item in generalFunction["conditions"].values():
				targetCondition = createCondition( item.asInt )
				if targetCondition is not None:
					targetCondition.init( item )
					self.conditions.append( targetCondition )

		if self.tickTime <= 0:
			self.tickCounts = 0
		else:
			if self.stayTime > 0:
				self.tickCounts = int( self.stayTime / self.tickTime )      # 共心跳几次
			else:
				self.tickCounts = 1
	
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
			if cond.verify( dst, src ) != SpellStatus.OK:
				return
		
		# 检查添加条件是否满足
		if self.onCast(src, dst, spell) != SpellStatus.OK:
			return
			
		return self.attach(src, dst)

	def attach(self, src, dst):
		"""
		virtual method.
		把buff附到owner身上
		"""
		buffData = BuffDataType()
		buffData.buffID = self.id
		if src is not None:
			buffData.casterID = src.id
		buffData.endTime = time.time() + self.stayTime
		buffData.counter = self.tickCounts
		buffData.saveType = self.dataSaveType
		buffData.buff = self
		buffData.misc["haveTick"] = False  # BUFF是否还有心跳次数
		
		if self.tickTime > 0.0:
			buffData.timerID = dst.addTimer( self.tickTime, self.tickTime, ECBExtend.TIMER_ON_BUFFTICK )
			buffData.misc["haveTick"] = True
		elif self.stayTime > 0.0:
			buffData.timerID = dst.addTimer( self.stayTime, 0, ECBExtend.TIMER_ON_BUFFTICK )
		self.onAttach(src, dst, buffData)  # 先执行onAttach，再执行addBuff()是为了满足不同buff在onAttach中对buffData做些事情
		dst.addBuff( buffData )
		return buffData.index

	def reattach(self, dst, buffData):
		"""
		virtual method.
		重新把buff作用到owner（玩家）身上
		"""
		buffData = dst.refreshBuffData( buffData )

		if buffData == None:
			return

		oldTimerID = buffData.timerID
		if self.tickTime > 0.0:
			buffData.timerID = dst.addTimer( self.tickTime, self.tickTime, ECBExtend.TIMER_ON_BUFFTICK )
		elif self.stayTime > 0.0:
			buffData.timerID = dst.addTimer( buffData.buff.stayTime, 0, ECBExtend.TIMER_ON_BUFFTICK )
		if oldTimerID != buffData.timerID:
			dst.changeBuffTimerID( oldTimerID, buffData.timerID )
		self.onAttach( None, dst, buffData )

	def refreshBuff(self, dst, buffData):
		"""
		刷新一个buff的时间
		"""
		oldTimerID = buffData.timerID
		buffData.endTime = time.time() + buffData.buff.stayTime
		dst.delTimer( buffData.timerID )
		if buffData.buff.tickTime > 0.0:
			buffData.counter = buffData.buff.tickCounts
			buffData.misc["haveTick"] = True
			buffData.timerID = dst.addTimer( buffData.buff.tickTime, buffData.buff.tickTime, ECBExtend.TIMER_ON_BUFFTICK )
		elif buffData.buff.stayTime > 0.0:
			buffData.timerID = dst.addTimer( buffData.buff.stayTime, 0, ECBExtend.TIMER_ON_BUFFTICK )

		dst.changeBuffTimerID( oldTimerID, buffData.timerID )

	def removeFrom_(self, owner, buffData):
		"""
		从owner身上移除自己，进行移除清理
		"""
		# 移除buff记录
		owner.removeBuff( buffData )
		if buffData.timerID > 0:
			owner.delTimer( buffData.timerID )

	def detach(self, owner, buffData):
		"""
		virtual method.
		把buff从owner身上卸下来
		"""
		self.removeFrom_( owner, buffData )
		self.onDetach( owner,buffData )

	def tick(self, owner, buffData):
		"""
		virtual method.
		buff的心跳，每一跳都有可能会做一些事情
		@return: None
		"""

		if self.tickCounts > 0 and buffData.misc["haveTick"]:     # 先判断心跳次数
			if buffData.counter > 0:
				if self.stayTime > 0:
					buffData.counter -= 1
				if not self.onTick(owner, buffData):
					self.detach(owner, buffData)  # 心跳结束
					return
			if buffData.counter <= 0:                # 虽然心跳结束了，但还需要等待持续时间结束（这里暂时）
				if time.time() >= buffData.endTime:
					self.detach(owner, buffData)  # 心跳结束
				else:
					buffData.misc["haveTick"] = False
					oldTimerID = buffData.timerID
					# 先结束掉旧的timer
					owner.delTimer( oldTimerID )
					# 再开启新的timer
					newTimerID = owner.addTimer( buffData.endTime - time.time() + 0.01, 0.0, ECBExtend.TIMER_ON_BUFFTICK )
					# 并且需要通知buff管理器更新timerID
					owner.changeBuffTimerID(oldTimerID, newTimerID)
				return
		
		else:
			self.detach(owner, buffData)  # 心跳结束
		return

	def interrupt(self, owner, buffData, interruptCode):
		"""
		virtual method.
		中断当前的buff
		"""
		if interruptCode in self.interruptCodes:
			self.removeFrom_( owner, buffData )
			self.onInterrupt( owner, buffData )

	def interruptByID(self, owner, buffData, buffID):
		"""
		通过ID中断当前的buff
		"""
		if buffID == self.id:
			self.removeFrom_( owner, buffData )
			self.onInterrupt( owner, buffData )

	def interruptOnDead(self, owner, buffData):
		"""
		virtual method.
		死亡时中断当前的buff
		"""
		if buffData.saveType == eBuffSaveType.NotSave:
			self.removeFrom_( owner, buffData )
			self.onInterrupt( owner, buffData )

	def actionByQTE(self, owner, buffData):
		"""
		virtual method.
		QTE行为触发
		"""
		pass

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
		return SpellStatus.OK;
	
	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情
		例如：给owner加上10点基础伤害
		"""
		pass
	
	def onDetach(self, owner, buffData):
		"""
		template method.
		当buff从owner身上取下来时，此接口被调用（仅调用一次）
		可以在此处做一些buff结束时的事情
		例如：给owner减去10点基础伤害
		"""
		pass
	
	def onTick(self, owner, buffData):
		"""
		template method.
		buff的心跳回调，每一跳都有可能会做一些事情
		@return: bool; true: buff需要继续心跳；false: buff不需要继续心跳了，亦即应该从玩家身上卸去了
		"""
		return False

	def onInterrupt(self, owner, buffData):
		"""
		template method.
		buff中断回调，继承于此类的buff可以根据实际情况做自己想做的事情
		"""
		pass
