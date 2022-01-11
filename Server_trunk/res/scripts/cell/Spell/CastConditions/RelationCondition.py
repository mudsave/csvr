# -*- coding: utf-8 -*-

"""
"""
from KBEDebug import *
from .ConditionBase import ConditionBase
from ..SpellDef import SpellStatus
from SrvDef import eCampRelationship, eTargetRelationship, eObjectType, eEntityStatus

class RelationCondition( ConditionBase ):
	"""
	两者关系判断条件
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		self.relation = dataSection.readIntArray( "relation", ',' )

	def verify( self, target, otherTarget ):
		"""
		判断施法者或受术者自身是否满足条件
		@param target: Entity; 施法者或受术者
		@param otherTarget: Entity; 与target判断关系的目标对象（若只判断自身是否满足条件，该参数可以为None）
		"""
		if self.checkCampRelation( otherTarget, target ) in self.relation:
			return SpellStatus.OK

		return SpellStatus.INVALID_TARGET_TYPE

	def checkCampRelation(self, src, dst ):
		"""
		判断目标与施法者关系
		@param src: Entity; 施法者
		@param dst: Entity; 检测目标
		@return 1:敌对玩家, 2：友方玩家, 3:施法者自身, 5:敌对NPC, 6：友方NPC
		"""
		if dst == src:
			return eTargetRelationship.Own

		relation = src.checkRelation(dst)
		if relation == eCampRelationship.Irrelative:
			return None

		if dst.status == eEntityStatus.Pending:
			return None

		objectType = dst.objectType
		if relation == eCampRelationship.Friendly:
			if objectType == eObjectType.Player:
				return eTargetRelationship.FriendlyPlayers
			elif objectType == eObjectType.Monster: 
				return eTargetRelationship.FriendlyMonster
		elif relation == eCampRelationship.Hostile:
			if objectType == eObjectType.Player:
				return eTargetRelationship.HostilePlayers
			elif objectType == eObjectType.Monster: 
				return eTargetRelationship.HostileMonster
		elif relation == eCampRelationship.neutrality:
			if objectType == eObjectType.Player:
				return eTargetRelationship.NeutralityPlayers
			elif objectType == eObjectType.Monster:
				return eTargetRelationship.NeutralityMonster

		return None
			
				


			


