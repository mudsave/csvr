# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from SrvDef import eFightEvent, eActionRestrict, eEntityEvent, eObjectType
from .SpellDef import SpellStatus, TargetType
from .Spell import Spell
from .CastConditions import createCondition
import GloballyStatus as GS

class SpellEx( Spell ):
	"""
	"""
	def __init__( self ):
		"""
		"""
		pass
	
	def init( self, dataSection ):
		"""
		"""
		Spell.init( self, dataSection )
		self.name = dataSection["generalPerformance"].readString( "name" )

		data = dataSection["generalFunction"]
		self.targetType = data.readFloat( "targetType" )
		self.distance = data.readFloat( "distance" )
		self.isIntoFightStatus = data.readBool( "isIntoFightStatus" )
		self.cooldownID = data.readInt( "cooldownID" )
		self.cooldownTime = data.readFloat( "cooldownTime" )
		self.mpCost = data.readInt( "mpCost" )
		self.conditions = []
		for section in data["conditions"].values():
			condition = createCondition( section.asInt )
			if condition is not None:  # 允许某些条件在服务器端不进行判断
				condition.init( section )
				self.conditions.append( condition )
		
	def cast( self, caster, targetData ):
		"""
		virtual method.
		开始施法
		@param caster: 施法者
		@param targeData: instance of SpellTargetData
		@return: value of SpellStatus
		"""

		status = SpellStatus.OK
		
		# 判断施法条件
		status = self.canStart( caster, targetData )
		if status != SpellStatus.OK:
			return status;
		
		# 绑定施法数据存储实例
		self.bindSpellDataType(caster, targetData)
		
		# 判断技能是否要进入战斗状态
		if self.isIntoFightStatus:
			caster.setFightStatus()

		# 处理cooldown
		self.doCooldown( caster )

		# 消耗MP
		caster.changeMP( -self.mpCost )

		# 开始施法
		#caster.fireEvent( eEntityEvent.OnCast, self )
		self.fire( caster, targetData )
		return status;
	
	def canStart( self, caster, targetData ):
		"""
		检查施法者是否允许施法，此接口应该在施法前进行判断
		1.检查技能cd；
		2.检查施展目标是否正确；
		3.more...
		@param caster: Entity; 施法者
		@return: value of SpellStatus
		"""
		# 是否禁止施法
		if caster.hasActionRestrict( eActionRestrict.ForbidSpell ):
			return SpellStatus.FORBID_ACTION_LIMIT
		
		# 是否正在施展技能
		if caster.currentSpell is not None and caster.currentSpell.spellID > 0:
			return SpellStatus.CASTING;
		
		# 检查CD
		if not caster.cooldownIsOut( self.cooldownID ):
			if caster.objectType == eObjectType.Player:
				caster.sendStatusMessage( GS.FIGHT_SKILL_CDING, self.name )
			return SpellStatus.COOLDOWNING
		
		# 检测MP是否足够施法
		if caster.MP < self.mpCost:
			return SpellStatus.LACK_OF_MP

		# 施法者条件检查
		result = SpellStatus.OK
		for cond in self.conditions:
			result = cond.verify( caster, None )
			if result != SpellStatus.OK:
				return result
		return result
	
	def notifyClientCastSpell(self, caster, targetData):
		"""
		通知客户端施展技能
		"""
		if self.targetType == TargetType.none:
			caster.allClients.startSpellFS( self.id )
		elif self.targetType == TargetType.Entity:
			caster.allClients.startSpellToEntityFS( self.id, targetData.gameObject.id )
		elif self.targetType == TargetType.Position:
			caster.allClients.startSpellToPosFS( self.id, targetData.posOrDir )
		elif self.targetType == TargetType.Direction:
			caster.allClients.startSpellToDirFS( self.id, targetData.posOrDir )
		else:
			assert False, "unknow target type '%s'" % self.targetType
	
	def bindSpellDataType(self, caster, targetData):
		"""
		virtual method.
		向施法者身上绑定SpellDataType实例（不需要绑定这个实例时可以不重载这个接口）
		注意：需要不同的数据类型时，需要重载这个接口返回与自己对应的数据类型实例
		"""
		pass
	
	def doCooldown(self, caster):
		"""
		virtual method.
		设置cooldown
		"""
		caster.setCooldown( self.cooldownID, self.cooldownTime )
	
	def fire( self, caster, targetData ):
		"""
		template method.
		正式开始处理技能施展的行为，能进来这里表示前面的施法前判定都通过了。
		"""
		pass
