# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD

"""
陷阱触发条件
"""

class TrapCondition:
		
	@staticmethod
	def hasUnQuest(owner, entity, questid):
		"""
		是否有此未完成的任务
		"""
		return entity.hasQuest(questid, [GD.QuestStatus.receive, GD.QuestStatus.doing])

