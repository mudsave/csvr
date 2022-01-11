# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time

"""
任务奖励
"""

class QuestReward:	
	
	@staticmethod	
	def gold(player, lv, itemsDataStream):
		itemsDataStream.addMoney(lv)
	
	@staticmethod
	def exp(player, lv, itemsDataStream):
		itemsDataStream.addExp(lv)
	
	@staticmethod
	def money(player, lv, itemsDataStream):
		itemsDataStream.addMoney(lv)
	
	@staticmethod
	#lv = [id, num]
	def item(player, lv, itemsDataStream):
		itemsDataStream.addItem(lv[0], lv[1])
	
	def rewardID(player, lv, itemsDataStream):
		value = g_fetchsMgr.fetch(lv, player)
		itemsDataStream.addList(value)
	
# from ItemScripts.ItemFactory import g_items
# from FetchsMgr import g_fetchsMgr