# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

import time
from Extra import ECBExtend
from BuffDataType import BuffDataType
from ..SpellDef import SpellStatus
from .BuffSimple import BuffSimple
from ..SpellLoader import g_spellLoader
from SrvDef import eObjectType
from ..CastConditions import createCondition
from ..ObjectFinder import createObjectFinder
from ..ObjectSizer import createObjectSizer
from Extra.FightSystem import FightSystem
from Extra.FightSystem import FightResult
from SrvDef import eFightEvent, eCampRelationship
from ..CastConditions.RelationCondition import RelationCondition

class Object(object): pass

class BuffBullet(BuffSimple):
	"""
	直线弹道Buff
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		BuffSimple.init( self, dataSection )

		generalFunction = dataSection["generalFunction"]
		self.collisionNumber = generalFunction.readFloat( "collisionNumber" )   # 碰撞次数

		self.collisionEffects = []
		for effectID in generalFunction["collisionEffect"].readInts( "item" ):
			self.collisionEffects.append( g_spellLoader.getEffect( effectID ) )

		self.collisionTriggers = []
		for section in dataSection["combatFunction"]["collisionTriggers"].values():
			data = Object()

			data.targetConditions = []
			# 过滤目标类型
			relationCondition = RelationCondition()
			relationCondition.init( section )
			data.targetConditions.append( relationCondition )

			if section.has_key( "targetConditions" ):
				for item in section["targetConditions"].values():
					targetCondition = createCondition( item.asInt )
					if targetCondition is not None:
						targetCondition.init( item )
						data.targetConditions.append( targetCondition )

			# 是否判断命中
			data.isHitJudgment = section.readBool( "isHitJudgment" )
			targetArea = section.readInt( "targetFinder" )
			if targetArea > 0:
				data.targetFinder = createObjectFinder( targetArea )
				data.targetFinder.init( section["targetFinder"] )

			# 筛选目标
			SizerType = section.readInt( "targetSizer" )
			data.targetSizer = createObjectSizer ( SizerType )
			if data.targetSizer is not None:
				data.targetSizer.init( section["targetSizer"] )

			data.casterEffects = []
			if section.has_key( "casterEffects" ):
				for effectID in section["casterEffects"].readInts( "item" ):
					data.casterEffects.append( g_spellLoader.getEffect( effectID ) )

			data.spellEffects = []
			if section.has_key( "spellEffects" ):
				for effectID in section["spellEffects"].readInts( "item" ):
					data.spellEffects.append( g_spellLoader.getEffect( effectID ) )

			self.collisionTriggers.append( data )

	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情
		例如：给owner加上10点基础伤害
		"""
		buffData.owner = dst.id
		buffData.misc["collisionNumber"] = self.collisionNumber
		buffData.misc["currentNumber"] = 0  # 初始化已经触发次数
		buffData.misc["judge"] = -1

		# buff拥有者为玩家，则玩家为裁判
		if dst.objectType == eObjectType.Player:
			buffData.misc["judge"] = dst.id
			buffData.misc["castPosition"] = (src.castPosition.x,src.castPosition.y,src.castPosition.z)
			buffData.misc["castDirection"] = (src.castDirection.x,src.castDirection.y,src.castDirection.z)
		# buff拥有者为Monster,则选取附近的一名玩家作为裁判
		elif dst.objectType == eObjectType.Monster:
			objs = dst.entitiesInRange( 30 )
			for obj in objs:
				if obj.objectType == eObjectType.Player:
					buffData.misc["judge"] = obj.id
					break

		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.attachEffect:
			effect.cast( src, dst, None )

	def requestBulletTriggerEffect(self, caster, dstID, buffData):
		"""
		触发子弹碰撞效果
		"""
		
		# 判断当前碰撞次数
		if self.collisionNumber == 0 or buffData.misc["currentNumber"] < self.collisionNumber:

			dst = KBEngine.entities.get(dstID,None)

			# 临时写法，触发碰撞效果
			if buffData.misc["currentNumber"] == 0 and self.collisionNumber == 1:
				for effect in self.collisionEffects:
					effect.cast( caster, dst, self )

			owner = KBEngine.entities.get(buffData.owner,None)
			buffData.misc["currentNumber"] += 1

			for data in self.collisionTriggers:

				# 对自己作用的effects
				for effect in data.casterEffects:
					effect.cast( caster, caster, self )

				objs = data.targetFinder.find(dst)

				# 根据目标条件对目标进行过滤
				for receiver in list( objs ):
					result = SpellStatus.OK
					for cond in data.targetConditions:
						result = cond.verify( receiver, caster )
						if result != SpellStatus.OK:
							objs.remove(receiver)
							break

				# 根据筛选条件对目标进行过滤
				if data.targetSizer is not None:
					objs = data.targetSizer.sizer( objs, caster )

				# 根据筛选条件对目标进行过滤
				if data.targetSizer is not None:
					objs = data.targetSizer.sizer( objs, caster )

				for receiver in objs:
					if data.isHitJudgment == True:
						FightSystem.hitJudgment( caster, receiver, self )
						if caster.result.hit == False:
							# 攻击者被闪避时
							caster.triggerFightEvent( eFightEvent.MissBy, caster, receiver, None )

							# 受击者闪避时
							receiver.triggerFightEvent( eFightEvent.Miss, receiver, caster, None )
					else:
						caster.result.hit = True
					# 对目标作用的effects
					if caster.result.hit == True:

						relation =  caster.checkRelation( receiver )
						# 根据施法者和受术者之间的关系来选择正确的被动技能触发点
						if relation == eCampRelationship.Friendly:
						
							# 攻击者命中友方目标时
							caster.triggerFightEvent( eFightEvent.HitFriendly, caster, receiver, None )
		
							# 受击者被友方目标命中时
							receiver.triggerFightEvent( eFightEvent.HitByFriendly, receiver, caster, None )

						if relation == eCampRelationship.Hostile:

							# 攻击者命中敌方目标时
							caster.triggerFightEvent( eFightEvent.HitHostile, caster, receiver, None )
		
							# 受击者被敌方目标命中时
							receiver.triggerFightEvent( eFightEvent.HitByHostile, receiver, caster, None )

						for effect in data.spellEffects:
							effect.cast( caster, receiver, self )
							
			# 若本次已经达到最大碰撞次数，buff结束
			if self.collisionNumber != 0 and buffData.misc["currentNumber"] == self.collisionNumber:
				 self.detach(owner, buffData)