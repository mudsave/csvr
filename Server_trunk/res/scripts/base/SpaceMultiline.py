# -*- coding: utf-8 -*-
#

import KBEngine
from KBEDebug import *

import KST
from Space import Space

class SpaceMultiline(Space):
	"""
	多线地图场景。
	"""
	def __init__(self):
		"""
		构造函数。
		"""
		Space.__init__(self)
		self.cellData["lineNumber"] = self.params["lineNumber"]