# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time

"""
断线重新接取的条件检测
"""

class QuestGoneCondition:
	
	@staticmethod
	def questStatus( player, quest, lv):
		if quest.status in lv:
			return True
		return False