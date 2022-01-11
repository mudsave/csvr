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
主线任务类
"""

class MainQuest(Quest):
	
	"""
	接取多个任务		
	"""	
	def __init__(self, config, info=None):	
		Quest.__init__(self, config, info)
	
	