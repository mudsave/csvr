# -*- coding: utf-8 -*-
from KBEDebug import *
import sys
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import GloballyConst as GC
import time
import json
from Extra.CellConfig import g_questConfig
from Quest.QuestCondition import QuestCondition
from Quest.Quest import Quest

class PlayerQuest:
	def __init__(self):
		#初始化任务
		for key in list(self.questList.keys()):
			if self.questList[key].goneCondition( self ):
				self.missQuestList.append(key)
				self.questList[key].destroy( self )
				del self.questList[key]
			elif self.questList[key].resetCondition( self ):
				self.questList[key].createReset()
				self.questList[key].addTarget(self)
			else: 
				self.questList[key].addTarget(self)
				if self.questList[key].config.type == GD.QuestType.dayQuest:
					self.dayQuestList[key] = True
				

	def reqQuestData(self, exposed):
		"""
		请求任务数据
		"""
		print(sys._getframe().f_code.co_name)
		if exposed != self.id:
			return
		
		self.client.onReqQuestData( self.questList, self.missQuestList, self.missDayQuestList, self.nextRefreshQuestTime)
	

	def fixedUpdateQuest(self):
		"""
		定点更新(二点钟更新)
		"""		
		self.dayQuestCompNum = 0
		

	def hasQuest(self, questid, qeustStatus = None):
		"""
		是否有此未完成的任务
		"""
		quest = self.questList.get(questid)
		if quest is None:
			return False
		
		if qeustStatus != None and (self.status not in qeustStatus):
			return False
		
		return True
	

	def giveUpQuest(self, exposed, questid):
		"""
		放弃任务
		"""
		if exposed != self.id:
			return	
		
		quest = self.questList.get(questid)
		if quest is None:
			return
		
		if quest.config.type == GD.QuestType.mainQuest and quest.config.meetNpc == 0:
			#主线任务不可以放弃
			return
		
		self.questList[questid].destroy( self )
		self._delQuestList(questid)
		
		if quest.config.type == GD.QuestType.mainQuest or quest.config.type == GD.QuestType.branchQuest:
			self.addMissQuestList([questid])		
		elif quest.config.type == GD.QuestType.dayQuest:
			self.addMissDayQuestList([questid])
		
		del quest
			
		self.client.delQuest([questid])
	

	def getQuestRewardFromNPC(self, exposed, entityID, questid):
		"""
		领取奖励
		"""
		if exposed != self.id:
			return
		
		quest = self.questList.get(questid)
		if quest is None:
			return	
		
		entity = KBEngine.entities.get(entityID, None)
		if entity is None:
			ERROR_MSG("Entity isn't exist, entity ID = %i" %(entityID))
			return			
		
		if entity.eMetaClass != quest.config.givenNpc:
			return
		
		if not self.isEntityInRange(entity, GC.MAX_CONST_RANGE):
			return	
		
		if quest.reward(self):
			quest.action(self)
			self.questList[questid].destroy( self )
			self._delQuestList(questid)
			del quest
		
		self.client.delQuest([questid])			
	

	def getDayQuestReward(self, exposed, questid):
		"""
		领取奖励
		"""
		if exposed != self.id:
			return	
		
		if self.dayQuestCompNum >= GC.MAX_COMP_DAY_QUEST_AMOUNT:
			return
		
		quest = self.questList.get(questid)
		if quest is None:
			return
		
		if quest.reward(self):
			quest.action(self)
			self.questList[questid].destroy( self )
			self._delQuestList(questid)
			del quest
		else:
			return
		
		self.dayQuestCompNum = self.dayQuestCompNum + 1
		self.client.delQuest([questid])
		
	
	def givenQuestFromNPC(self, exposed, entitiyID, questid):
		"""
		领取任务
		"""			
		if exposed != self.id:
			return
		
		if len(self.questList) >= GC.MAX_QUEST_AMOUNT:
			self.sendStatusMessage(GS.QUEST_AMOUNT_FULL, GC.MAX_QUEST_AMOUNT)
			return
		
		#在未接任务中，是否有此任务
		if questid not in self.missQuestList:
			return

		config = g_questConfig[questid]

		entity = KBEngine.entities.get(entitiyID, None)
		if entity is None:
			ERROR_MSG("Entity isn't exist, entity ID = %i" %(entitiyID))
			return			

		if entity.eMetaClass != config.meetNpc:
			return
		
		if not self.isEntityInRange(entity, GC.MAX_CONST_RANGE):
			return		
		
		quest = self.addQuest(questid) 
		if (quest is not None):
			self.delMissQuestList([questid])
			self.client.addQuest([quest])
	

	def givenDayQuest(self, exposed, questid):
		"""
		领取日常任务
		"""	
		if exposed != self.id:
			return	
		
		if len(self.questList) >= GC.MAX_QUEST_AMOUNT:
			self.sendStatusMessage(GS.QUEST_AMOUNT_FULL, GC.MAX_QUEST_AMOUNT)
			return
		
		config = g_questConfig[questid]
		
		#在日常可接任务中，是否有此任务
		if questid not in self.missDayQuestList:
			return
		
		quest = self.addQuest(questid)
		if (quest is not None):		
			self.delMissDayQuestList([questid])
			self.client.addQuest([quest])			
			

	def refreshAllDayQuest(self, exposed, subGold):
		if exposed != self.id:
			return			
		
		if self.nextRefreshQuestTime > time.time():
			if not subGold:
				#不扣除金币，直接返回
				return
			
			if (not self.subGoldCoin( GC.Quest_Day_Need_GoldCoin )):
				return
			
			self.nextRefreshQuestTime = time.time() + GC.Quest_DAY_INTERVAL_TIME
		else:
			self.nextRefreshQuestTime = self.nextRefreshQuestTime + (int(( time.time() - self.nextRefreshQuestTime )/ GC.Quest_DAY_INTERVAL_TIME) + 1)*GC.Quest_DAY_INTERVAL_TIME
		
		needNum = GC.MAX_DAY_QUEST_AMOUNT - len(self.dayQuestList)
		
		if needNum <= 0:
			self.sendStatusMessage(GS.QUEST_AMOUNT_FULL, GC.MAX_QUEST_AMOUNT)
			return
		
		self.missDayQuestList = []
		
		randomID = 0
		for (k,v) in g_dayQuestRandomConfig.items():
			if self.level < k:
				randomID = v.randomID
		
		if randomID == 0:
			ERROR_MSG("g_dayQuestRandomConfig not level = %s " %(self.level))
			return
		
		result = g_fetchsMgr.fetch(randomID, self) 
		for i in result:
			if self.dayQuestList.get(i):
				continue
			
			self.missDayQuestList.append(i)
			needNum = needNum - 1
			if needNum == 0:
				break
		
		self.client.updateMissDayQuest(self.missDayQuestList)
		self.client.refreshAllDayQuestBack(self.nextRefreshQuestTime)
		

	def givenAllDayQuest(self, exposed):
		"""
		全部接取日常任务
		"""	
		if exposed != self.id:
			return	
		
		if len(self.questList) >= GC.MAX_QUEST_AMOUNT:
			self.sendStatusMessage(GS.QUEST_AMOUNT_FULL, GC.MAX_QUEST_AMOUNT)
			return
		
		num = GC.MAX_QUEST_AMOUNT - len(self.questList)
		if num >= len(self.missDayQuestList):
			self.addQuestList(self.missDayQuestList)
			INFO_MSG(self.missDayQuestList)
			self.clearMissDayQuestList()
		else:
			questList = self.missDayQuestList[0:num]
			INFO_MSG(questList)
			self.delMissDayQuestList(questList)
			self.addQuestList(questList)
		

	def addMissQuestList(self, questIdList):
		addquestid = []
		for id in questIdList:
			if id not in self.missQuestList:
				self.missQuestList.append(id)
				addquestid.append(id)
		
		if len(addquestid) > 0:
			self.client.addMissQuestList(addquestid)
		

	def delMissQuestList(self, questIdList):
		self.missQuestList = list(set(self.missQuestList).difference(set(questIdList)))
		
		if len(questIdList) > 0:
			self.client.delMissQuestList(questIdList)
			

	def addMissDayQuestList(self, questIdList):
		addquestid = []
		for id in questIdList:
			if id not in self.missDayQuestList:
				self.missDayQuestList.append(id)
				addquestid.append(id)
		
		if len(addquestid) > 0:
			self.client.addMissDayQuestList(addquestid)
		

	def delMissDayQuestList(self, questIdList):
		self.missDayQuestList = list(set(self.missDayQuestList).difference(set(questIdList)))
		
		if len(questIdList) > 0:
			self.client.delMissDayQuestList(questIdList)
	

	def clearMissDayQuestList(self):
		self.missDayQuestList = []
		self.client.updateMissDayQuest(self.missDayQuestList)		
		

	def addQuestList(self, questIdList):
		"""
		增加多个任务
		"""
		questlist = []
		for questid in questIdList:
			quest = self.addQuest(questid)
			if quest is not None:
				questlist.append(quest)
		
		if len(questlist) > 0:
			self.client.addQuest(questlist)
	

	def addQuest(self, questid):
		"""
		增加任务
		"""
		config = g_questConfig[questid]
		
		if self.questList.get(questid) is not None:
			print("questid is add")
			return None

		for k,v in config.condition.items():
			func = getattr(QuestCondition, k)
			if not func(self, v):
				print("%s condition is fail" % (k))
				return None
			
		quest = Quest.create(config)
		quest.addTarget(self)
		quest.givenEvent(self)
		self._addQuestList(quest)
		return quest
	

	def _delQuestList(self, questid):
		"""
		从列表中删除任务变量
		"""
		quest = self.questList[questid]
		del self.questList[questid]
		if quest.config.type == GD.QuestType.dayQuest:
			del self.dayQuestList[questid]
		

	def _addQuestList(self, quest):
		"""
		从列表中增加任务变量
		"""		
		self.questList[quest.id] = quest
		
		if quest.config.type == GD.QuestType.dayQuest:
			self.dayQuestList[quest.id] = True
		

	def _clearQuestList(self):
		"""
		清空任务变量列表
		"""
		self.questList.clear()
		self.dayQuestList.clear()
	

	def tickCounter(self, counterList, addnum):
		"""
		计数器
		"""
		questUpdate = [] #需要通知客户端更新的任务列表
		
		for m in range(len(counterList)-1, -1, -1):
			questTarget = counterList[m]
			quest = self.questList.get(questTarget[3])
			
			if quest is not None:
				questTarget[1] += addnum
				quest.target[questTarget[0]] = questTarget[1]
				if questTarget[1] < 0:
					questTarget[1] = 0
				elif questTarget[1] > questTarget[2]:
					questTarget[1] = questTarget[2]
				
				if quest.status == GD.QuestStatus.receive:
					quest.status = GD.QuestStatus.doing
				
				if questTarget[1] == questTarget[2]:
					del counterList[m]
					questData = quest.getQuestData(self)
					questData["counter"] = questData["counter"] - 1
					if questData["counter"] <= 0:
						quest.complete(self)
				
				questUpdate.append(quest)
			else: 
				#此任务已经被删除
				del counterList[m]
		
		if len(questUpdate) > 0:
			#通知客户端更新任务信息
			self.client.updateQuest(questUpdate)


	def onFireEvent( self, eventKey, value ):
		"""
		Define method.
		发送消息
		"""
		value = json.loads( value )
		self.fireEvent( eventKey, *value )
			
			
from FetchsMgr import g_fetchsMgr