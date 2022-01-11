# -*- coding: utf-8 -*-
#
"""
"""
import pickle
import time

from KBEDebug import *
from SpellDataType import SpellDataType

class SpellExDataType( SpellDataType ):
	"""
	与SpellEx对应的技能数据类型
	"""
	dataType = 1
	
	def __init__(self):
		"""
		"""
		self.spellID = 0
		self.targetID = 0
		self.posOrDir = (0.0, 0.0, 0.0)
		self.misc = {}

import KST
