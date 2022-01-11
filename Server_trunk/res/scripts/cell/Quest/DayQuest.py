# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import GloballyDefine as GD
from Functor import Functor
import GloballyStatus as GS
import time
from Quest.Quest import Quest

"""
日常任务类
"""

class DayQuest(Quest):
	
	"""
	接取多个任务		
	"""	
	def __init__(self, config, info=None):	
		Quest.__init__(self, config, info)
	
	def goneCondition( self, player ):
		return False
		
	def resetCondition( self, player ):
		return False