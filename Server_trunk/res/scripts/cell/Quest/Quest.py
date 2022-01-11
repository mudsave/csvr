# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time
from Quest.QuestTarget import QuestTarget
from Quest.QuestAction import QuestAction
from Quest.QuestReward import QuestReward
from Quest.QuestGoneCondition import QuestGoneCondition

from ItemsDataStreamType import ItemsDataStreamType
from ItemDistribution import ItemDistribution

"""
任务基类
"""

class Quest:
	
	def __init__(self, config, info=None):	
		
		if info is None:
			self.status = GD.QuestStatus.receive  #任务状态
			self.target = [0] * len(config.target) #任务现有值
		else:
			self.status = info["status"]
			self.target = info["target"]
		self.id = config.id #任务ID
		self.config = config #任务配置		
	
	def createReset(self):
		"""
		初始化重置,只能在登录的时候调用
		"""
		self.status = GD.QuestStatus.receive  #任务状态
		self.target = [0] * len(self.config.target) #任务现有值		
	
	@staticmethod
	def create(config, info=None):
		if config.type == GD.QuestType.mainQuest:
			return MainQuest(config, info)
		elif config.type == GD.QuestType.branchQuest:
			return BranchQuest(config, info)
		elif config.type == GD.QuestType.dayQuest:
			return DayQuest(config, info)
		
		ERROR_MSG( "Quest.create create quest is None. Type = %s" % (config.type))
		return None
	
	def getDict(self):
		data = {}
		data["id"] = self.config.id
		data["status"] = self.status
		data["target"] = self.target
		return data
	
	
	def getQuestData(self, player):
		return player.questData[self.id]
			
	
	def addTarget(self, player):
		player.questData[self.id] = {"counter":0}
		
		#添加目标
		target = self.config.target
		selftarget = self.target
		self.target = []
		for m in range(len(target)):	
			func = getattr(QuestTarget, target[m][0])
			if len(selftarget) < m+1:
				self.target.append(0)
			else:
				self.target.append(selftarget[m])
			func(self, player, m, target[m][1], self.target[m])
		
		if player.questData[self.id]["counter"] == 0:
			self.complete(player)
	
	def givenEvent(self, player):
		#任务接取成功后，执行的回调
		action = self.config.givenEvent
		for value in action:
			func = getattr(QuestAction, value[0])
			func(player, value[1])		
	
	def complete(self, player):
		#任务完成的回调
		self.status = GD.QuestStatus.complete
	
	def reward(self, player):
		
		if self.status != GD.QuestStatus.complete:
			ERROR_MSG("dont have GD.QuestStatus.complete")
			return False
		
		self.status = GD.QuestStatus.reward
		
		reward = self.config.reward		
		
		itemsDataStream = ItemsDataStreamType()
		for value in reward:
			func = getattr(QuestReward, value[0])
			func(player, value[1], itemsDataStream)
		
		if not ItemDistribution.distributionItemDatStream( player, itemsDataStream ):
			return False
		
		return True
	
	def action(self, player):
		#执行完成任务后的动作
		print("action %s " % (self.status))
		if self.status == GD.QuestStatus.reward:
			self.status = GD.QuestStatus.action
			action = self.config.action
			for value in action:
				func = getattr(QuestAction, value[0])
				func(player, value[1])
			return True
		else:
			return False
	
	def goneCondition( self, player ):
		condition = self.config.goneCondition
		if len(condition) == 0:
			return False
		
		for k,v in condition.items():
			func = getattr(QuestGoneCondition, k)
			if not func(player, self, v):
				return False
		return True
	
	def resetCondition( self, player ):
		condition = self.config.resetCondition
		if len(condition) == 0:
			return False
		
		for k,v in condition.items():
			func = getattr(QuestGoneCondition, k)
			if not func(player, self, v):
				return False
		return True		
	
	def destroy(self, player):
		#任务销毁
		if player.questData.get(self.id) is not None:
			del player.questData[self.id]
	
from Quest.MainQuest import MainQuest
from Quest.DayQuest import DayQuest
from Quest.BranchQuest import BranchQuest