# -*- coding: utf-8 -*-

from KBEDebug import *

import time
from Extra import ECBExtend
from BuffDataType import BuffDataType
from ..SpellDef import SpellStatus
from ..BuffCron import BuffCron
from ..SpellLoader import g_spellLoader
from Extra.CellConfig import g_entityConfig
from SrvDef import eObjectType, eEntityStatus, eEffectStatus, eCampRelationship
import Extra.Common as Common


class BuffLightning(BuffCron):
	"""
	闪电链buff
	"""
	def init(self, dataSection):
		BuffCron.init( self, dataSection )

		combatFunction = dataSection["combatFunction"]
		
		self.times = combatFunction.readInt("times")    #弹射次数
		self.radius = combatFunction.readInt("radius")	#弹射的最大半径
		self.effectIDs = combatFunction["effect"].readInts( "item" )

		self.effects = []
		for id in self.effectIDs:
			self.effects.append( g_spellLoader.getEffect( id ) )

	def cast(self, src, dst, spell, targetID):
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
			
		return self.attach(src, dst, targetID)

	def attach(self, src, dst, targetID):
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
		buffData.misc["targetID"] = targetID
		
		if self.tickTime > 0.0:
			buffData.timerID = dst.addTimer( self.tickTime, self.tickTime, ECBExtend.TIMER_ON_BUFFTICK )
			buffData.misc["haveTick"] = True
		elif self.stayTime > 0.0:
			buffData.timerID = dst.addTimer( self.stayTime, 0, ECBExtend.TIMER_ON_BUFFTICK )
		self.onAttach(src, dst, buffData)  # 先执行onAttach，再执行addBuff()是为了满足不同buff在onAttach中对buffData做些事情
		dst.addBuff( buffData )
		return buffData.index	

	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情
		"""
		buffData.misc["attackTimes"] = self.times
		buffData.misc["objList"] = []  #记录打击过的目标

		target = None
		target = KBEngine.entities.get(buffData.misc["targetID"],None)
		if target:
			buffData.misc["objList"].append(target.id) #记录打击过的目标
			buffData.misc["srcObj"] = target.id

			for effect in self.effects:
				effect.cast( src, target, None )
		else:
			buffData.misc["attackTimes"] = 0
			buffData.misc["castDirection"] = (src.castDirection.x,src.castDirection.y,src.castDirection.z)

	def onTick(self, owner, buffData):
		"""
		template method.
		buff的心跳，每一跳都有可能会做一些事情
		@return: bool; true: buff需要继续心跳；false: buff不需要继续心跳了，亦即应该从玩家身上卸去了
		"""
		if buffData.misc["attackTimes"] == 0:
			return False

		caster = KBEngine.entities.get(buffData.casterID,None)             #找到buff施法者
		srcObj = KBEngine.entities.get(buffData.misc["srcObj"],None)

		target = None
		objs = caster.entitiesInRange(self.radius, None, srcObj.position)

		length = len(objs)

		for i in range( length-1 ):
			for j in range( i,length ):
				if objs[j].position.flatDistTo( caster.position ) < objs[i].position.flatDistTo( caster.position ):
					objs[j], objs[i] = objs[i], objs[j]

		for obj in objs:
			if caster.checkRelation(obj) == eCampRelationship.Hostile and obj.status != eEntityStatus.Death:
				target = obj
				for id in buffData.misc["objList"]:
					if obj.id == id:
						target = None
						break

				if target:
					break

		if target is not None:
			buffData.misc["dstObj"] = target.id
			buffData.misc["objList"].append(target.id) #记录打击过的目标
			caster.buffSpecificAction(buffData)        #通知客户端做表现
			buffData.misc["srcObj"] = target.id
			buffData.misc["attackTimes"] -= 1

			for effect in self.effects:
				effect.cast( caster, target, None )

		return True




