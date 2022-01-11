# -*- coding: utf-8 -*-
from KBEDebug import *
from Spell.SpellLoader import g_spellLoader
import KBEngine
import KST
import Extra.SpaceEventID as SpaceEventID
import GloballyConst as GC
import GloballyDefine as GD
import GloballyStatus as GS

"""
陷阱触发事件
"""

class TrapEvent:
		
	@staticmethod
	def openTrap(owner, entity, value):
		"""
		关闭陷阱
		"""
		owner.openTrap = value
	
	@staticmethod		
	def goto(owner, entity, value):
		"""
		传送 value = [id, isShowLoading]
		"""
		entity.gotoMap(value[0], value[1] > 0 )	
	
	@staticmethod
	def gotoPrevSpace(owner,  entity):
		"""
		传送到上一个场景
		"""
		entity.gotoPrevSpace()
	
	@staticmethod
	def gotoTeamNearby(owner, entity, id):
		"""
		同一张地图的队伍传送
		"""		
		if not entity.isTeamCaptain():
			return
		
		members = entity.getAllMemberInMap()
		for member in members:
			member.gotoMap(id)
			
		
	@staticmethod
	def startEGroup(owner, entity, id):
		spaceBase = owner.getCurrentSpaceBase()
		if spaceBase is None:
			return
	
		space = KBEngine.entities[spaceBase.id]
		space.fireEvent(SpaceEventID.StartElmGroup, id) #触发第一个刷怪事件
	
	
	@staticmethod
	def castEffect(owner, entity, id):
		effect = g_spellLoader.getEffect( id )
		effect.cast( owner, entity, None ) #施放一个技能效果
	
	@staticmethod
	def skillGuide(owner, entity, id):
		entity.sendStatusMessage(GS.GUIDE_SKILL_ID, id)

