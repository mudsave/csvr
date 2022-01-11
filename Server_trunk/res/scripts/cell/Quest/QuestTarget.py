# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time
from SrvDef import eEntityEvent, eObjectType

"""
任务目标
"""

class QuestTarget:
	"""
	升级目标
	@param quest	任务对象
	@param player 	玩家
	@param index	目标序号
	@param lv		目标值
	@param cvalue   当前值		
	"""	
	
	@staticmethod
	def registerQuestTarget(quest, player, index, table, funcName):
		"""
		@param quest	任务对象
		@param player 	玩家
		@param index	目标序号
		@param table	[key1, key2, value, targetValue] 或者是 [key, vlaue, targetValue]. 
						targetValue 目标值
						vlaue 当前值
		@param cvalue   当前值		
		"""	
		if len(table) == 3:
			target = [index, table[1], table[2], quest.id]
			quest.target[index] = target[1]
			QuestTarget.registerTempletToOne(quest, player, target, table[0], funcName)
		else:
			target = [index, table[2], table[3], quest.id]
			quest.target[index] = target[1]
			QuestTarget.registerTempletToTwo(quest, player, target, table[0], table[1], funcName)

	@staticmethod
	def backQuestTarget(player, table, addValue, funcName):
		"""
		@param player 	玩家
		@param table	[key1, key2] 或者是 [key]. 
		@param addValue 增加值		
		"""			
		if len(table) == 1:
			key = table[0]
			player.tickCounter(player.questCounter[key], addValue)
			if len(player.questCounter[key]) == 0:
				del player.questCounter[key]
				player.deregisterEvent(key, QuestTarget, funcName)
		else:
			key1 = table[0]; key2 = table[1]
			if player.questCounter[key1].get(key2) is not None:
				player.tickCounter(player.questCounter[key1][key2], addValue)
				if len(player.questCounter[key1][key2]) == 0:
					del player.questCounter[key1][key2]	
				
				if len(player.questCounter[key1]) == 0:
					del player.questCounter[key1]
					player.deregisterEvent(key1, QuestTarget, funcName)
		
	@staticmethod
	def registerTempletToOne(quest, player, target, key, funcName):
		if target[1] < target[2]:
			if player.questCounter.get(key) is None:
				player.questCounter[key] = []
				player.registerEvent(key, QuestTarget, funcName)
				
			qc = player.questCounter[key]
			qc.append(target)
						
			questData = quest.getQuestData(player)
			questData["counter"] += 1
	
	@staticmethod
	def registerTempletToTwo(quest, player, target, key1, key2, funcName):	
		if target[1] < target[2]:
			if player.questCounter.get(key1) is None:
				player.questCounter[key1] = {}
				player.registerEvent(key1, QuestTarget, funcName)
			
			if player.questCounter[key1].get(key2) is None:
				player.questCounter[key1][key2] = []
				
			qc = player.questCounter[key1][key2]
			qc.append(target)
						
			questData = quest.getQuestData(player)
			questData["counter"] += 1
	
	@staticmethod	
	def levelup(quest, player, index, lv,cvalue):
		"""
		等级升级
		lv = xx
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.Levelup, player.level, lv], "onLevelup")
	
	@staticmethod
	def onLevelup(player, old, new):
		QuestTarget.backQuestTarget(player, [eEntityEvent.Levelup], new - old, "onLevelup")
	
	@staticmethod
	def killnpc(quest, player, index, lv,cvalue):
		"""
		击杀指定数量的NPC
		lv = [xx, xx]
		"""	
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnKilled, lv[0], cvalue, lv[1]], "onKillnpc")	
	
	@staticmethod
	def onKillnpc(player, bykillnpc):
		if bykillnpc.objectType == eObjectType.NPC:
			QuestTarget.backQuestTarget(player, [eEntityEvent.OnKilled, bykillnpc.eMetaClass], 1, "onKillnpc")
		
	@staticmethod	
	def talk(quest, player, index, lv,cvalue):
		"""
		对话
		lv = xx
		"""	
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.Talk, lv, cvalue, 1], "onTalk")
	
	@staticmethod
	def onTalk(player, eMetaClass, talkid):
		QuestTarget.backQuestTarget(player, [eEntityEvent.Talk, talkid], 1, "onTalk")
	
	@staticmethod
	def collect(quest, player, index, lv,cvalue):
		"""
		收集
		lv = [id,num]
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.Collect, lv[0], cvalue, lv[1]], "onCollect")	
	
	@staticmethod
	def onCollect(player, eMetaClass):
		QuestTarget.backQuestTarget(player, [eEntityEvent.Collect, eMetaClass], 1, "onCollect")
		
	@staticmethod	
	def goto(quest, player, index, lv,cvalue):
		"""
		传送
		lv = xx
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnEnterSpace, lv, cvalue, 1], "onGoto")
	
	@staticmethod
	def onGoto(player, id):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnEnterSpace, id], 1, "onGoto")
		
	@staticmethod	
	def sequence(quest, player, index, lv,cvalue):
		"""
		动画
		lv = id
		"""	
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.Sequence, lv, cvalue, 1], "onSequence")
	
	@staticmethod
	def onSequence(player, id):
		QuestTarget.backQuestTarget(player, [eEntityEvent.Sequence, id], 1, "onSequence")
	
	@staticmethod
	def inlayCrystal(quest, player, index, lv,cvalue):
		"""
		晶石镶嵌
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnInlayCrystal, cvalue, lv], "onInlayCrystal")
	
	def onInlayCrystal(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnInlayCrystal], 1, "onInlayCrystal")
	
	@staticmethod
	def petFightOut(quest, player, index, lv,cvalue):
		"""
		幻兽出战
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnPetFightOut, cvalue, lv], "onPetFightOut")
	
	@staticmethod
	def onPetFightOut(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnPetFightOut], 1, "onPetFightOut")
	
	@staticmethod
	def petAbsorb(quest, player, index, lv,cvalue):
		"""
		幻兽吸收
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnPetAbsorb, cvalue, lv], "onPetAbsorb")
	
	@staticmethod
	def onPetAbsorb(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnPetAbsorb], 1, "onPetAbsorb")
	
	@staticmethod
	def equipUpgrade(quest, player, index, lv,cvalue):
		"""
		装备强化
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnEquipUpgrade, cvalue, lv], "onEquipUpgrade")
	
	@staticmethod
	def onEquipUpgrade(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnEquipUpgrade], 1, "onEquipUpgrade")
	
	@staticmethod
	def equipTalisman(quest, player, index, lv,cvalue):
		"""
		装备法宝
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnEquipTalisman, cvalue, lv], "onEquipTalisman")
	
	@staticmethod
	def onEquipTalisman(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnEquipTalisman], 1, "onEquipTalisman")
	
	@staticmethod
	def wieldTalisman(quest, player, index, lv,cvalue):
		"""
		祭起法宝
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnWieldTalisman, cvalue, lv], "onWieldTalisman")
	
	@staticmethod
	def onWieldTalisman(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnWieldTalisman], 1, "onWieldTalisman")

	@staticmethod
	def itemSynthesis(quest, player, index, lv,cvalue):
		"""
		物品合成
		lv = times
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnItemSynthesis, cvalue, lv], "onItemSynthesis")
	
	@staticmethod
	def onItemSynthesis(player):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnItemSynthesis], 1, "onItemSynthesis")

	@staticmethod
	def buildBuilding( quest, player, index, lv, cvalue ):
		"""
		建造建筑
		lv = [id, times]
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnBuildBuilding, lv[0], cvalue, lv[1]], "onBuildBuilding")
	
	@staticmethod
	def onBuildBuilding( player, buildingID ):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnBuildBuilding, buildingID], 1, "onBuildBuilding")

	@staticmethod
	def upgradeBuilding( quest, player, index, lv, cvalue ):
		"""
		升级建筑
		lv = [id, times]
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnUpgradeBuilding, lv[0], cvalue, lv[1]], "onUpgradeBuilding")
	
	@staticmethod
	def onUpgradeBuilding( player, buildingID ):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnUpgradeBuilding, buildingID], 1, "onUpgradeBuilding")

	@staticmethod
	def useItem( quest, player, index, lv, cvalue ):
		"""
		使用物品
		lv = [id, times]
		"""
		QuestTarget.registerQuestTarget(quest, player, index, [eEntityEvent.OnUseItem, lv[0], cvalue, lv[1]], "onUseItem")
	
	@staticmethod
	def onUseItem( player, itemID ):
		QuestTarget.backQuestTarget(player, [eEntityEvent.OnUseItem, itemID], 1, "onUseItem")