# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..SpellDef import SpellStatus
from ..ObjectFinder import createObjectFinder
from ..ObjectSizer import createObjectSizer
from ..SpellLoader import g_spellLoader
from Extra.FightSystem import FightSystem
from Extra.FightSystem import FightResult
from SrvDef import eFightEvent, eCampRelationship
from ..SpellEx import SpellEx
from ..CastConditions import createCondition
from ..SpellDataTypes.SpellExCurveDataType import SpellExCurveDataType
from ..CastConditions.RelationCondition import RelationCondition

class Object(object): pass

class SpellExCurveFightRelySer( SpellEx ):
	"""
	一个根据时间间隔来触发效果的技能基础类。
	"""
	def __init__( self ):
		"""
		"""
		SpellEx.__init__( self )

	def init( self, dataSection ):
		"""
		"""
		SpellEx.init( self, dataSection )

		# 与curves触发点一一对应，表示触发点触发时的效果
		self.curveTriggers = []

		for section in dataSection["combatFunction"]["curveTriggers"].values():
			data = Object()
			data.intervalTime = section.readFloat( "intervalTime" ) # 触发效果的时间间隔

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

			self.curveTriggers.append( data )

		self.isTriggerDo = dataSection["generalFunction"].readBool( "isTriggerDo" )

	def bindSpellDataType(self, caster, targetData):
		"""
		virtual method.
		向施法者身上绑定SpellDataType实例（不需要绑定这个实例时可以不重载这个接口）
		注意：需要不同的数据类型时，需要重载这个接口返回与自己对应的数据类型实例
		"""
		# 由于使用动画curve来触发效果，因此需要记下当前正在施展的技能，以互斥、校验
		spellDataType = SpellExCurveDataType()
		spellDataType.spellID = self.id
		if targetData.gameObject != None:
			spellDataType.targetID = targetData.gameObject.id
		spellDataType.posOrDir = targetData.posOrDir
		spellDataType.start(caster, targetData)

	def fire( self, caster, targetData ):
		"""
		template method.
		正式开始处理技能施展的行为，能进来这里表示前面的施法前判定都通过了
		"""
		self.notifyClientCastSpell( caster, targetData )  # 通知客户端施展技能

		# 是否触发"施法时"事件，主要是来区别是技能还是普通攻击
		if self.isTriggerDo == True:
			# 攻击者释放技能时
			caster.triggerFightEvent( eFightEvent.Do, caster, caster, self )

		caster.result = FightResult()

	def doTriggerEffect( self, caster, index, targetID ):
		"""
		根据指定的index选择触发的技能效果
		"""
		data = self.curveTriggers[index]

				# 对自己作用的effects
		for effect in data.casterEffects:
			effect.cast( caster, caster, self )

		objs = data.targetFinder.find( caster )
		
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

		for receiver in objs:
			if data.isHitJudgment == True:
				FightSystem.hitJudgment( caster, receiver, self )
				if caster.result.hit == False:
					# 攻击者被闪避时
					caster.triggerFightEvent( eFightEvent.MissBy, caster, receiver, self )

					# 受击者闪避时
					receiver.triggerFightEvent( eFightEvent.Miss, receiver, caster, self )
			else:
				caster.result.hit = True
			# 对目标作用的effects
			if caster.result.hit == True:

				relation =  caster.checkRelation( receiver )
				# 根据施法者和受术者之间的关系来选择正确的被动技能触发点
				if relation == eCampRelationship.Friendly:
						
					# 攻击者命中友方目标时
					caster.triggerFightEvent( eFightEvent.HitFriendly, caster, receiver, self )
		
					# 受击者被友方目标命中时
					receiver.triggerFightEvent( eFightEvent.HitByFriendly, receiver, caster, self )
					
				if relation == eCampRelationship.Hostile:

					# 攻击者命中敌方目标时
					caster.triggerFightEvent( eFightEvent.HitHostile, caster, receiver, self )
		
					# 受击者被敌方目标命中时
					receiver.triggerFightEvent( eFightEvent.HitByHostile, receiver, caster, self )

				for effect in data.spellEffects:
					effect.cast( caster, receiver, self )

