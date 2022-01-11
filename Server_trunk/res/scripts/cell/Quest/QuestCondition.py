# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time

"""
任务条件检测
"""

class QuestCondition:
		
	@staticmethod	
	def level(player, lv):
		return player.level == lv
	
	@staticmethod
	def levelgreat(player, lv):
		return player.level > lv
	
	@staticmethod
	def levelless(player, lv):
		return player.level < lv
		
	@staticmethod
	def sex(player, lv):
		return player.sex == lv	