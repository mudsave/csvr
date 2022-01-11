# -*- coding: utf-8 -*-
#

import KBEngine
from KBEDebug import *

import KST
from Space import Space

class SpaceSinglePlayer(Space):
	"""
	单人地图
	"""
	def __init__(self):
		"""
		构造函数。
		"""
		Space.__init__(self)