# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time
import Extra.SpaceEventID as SpaceEventID

"""
执行任务完成动作
"""
from Extra.CellConfig import g_heroConfig

class QuestAction:
		
	@staticmethod	
	def acceptquest(player, v):
		"""
		接取单个任务		
		"""	
		player.addQuestList([v])
	
	@staticmethod
	def lookEntity(player, configID):
		"""
		可以查看到entity
		"""	
		pass
		
	@staticmethod
	def maissquest(player, v):
		"""
		增加未接任务
		"""	
		player.addMissQuestList([v])
	
	@staticmethod
	def startSequence(player, sequenceID):
		"""
		开始播放一段剧情
		"""	
		player.addSequence(sequenceID)
	
	@staticmethod
	def activeSkillIndex(player, index):
		"""
		激活第一个序列的技能
		"""
		config = g_heroConfig.get(player.eMetaClass)
		player.requestUpgradeSpell(player.id, config['skilllist'][index])
	
	@staticmethod
	def startEGroup(player, id):
		"""
		刷新一组怪物
		"""
		spaceBase = player.getCurrentSpaceBase()
		if spaceBase is None:
			return
			
		space = KBEngine.entities[spaceBase.id]
		space.fireEvent(SpaceEventID.StartElmGroup, id) #触发第一个刷怪事件

	@staticmethod
	def startGuidance(player, id):
		"""
		指引
		"""
		player.startGuidance(id)
		
	@staticmethod
	def goto(player, id):
		"""
		跳转地图
		"""		
		player.gotoMap(id)
		

	@staticmethod
	def moduleActivate(player, str):
		"""
		激活模块
		"""
		player.setModuleActivate(str)	

	@staticmethod
	def addPet(player, id):
		"""
		奖励幻兽
		"""
		player.addPet(id)	

	@staticmethod
	def addTalisman(player, id):
		"""
		奖励法宝
		"""
		player.addTalisman(id)		