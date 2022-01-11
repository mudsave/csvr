# -*- coding: utf-8 -*-

"""
"""

import KBEngine
from KBEDebug import *
from SpaceScripts.SpaceData import SpaceData

class MonglineSpaceData( SpaceData ) :
	"""
	单线空间
	"""
	def init(self, config):
		SpaceData.init(self, config)

		self.loadOnStart = config["loadOnStart"]						# 是否在space manager初始化的时候就需要把这个地图加载起来，通常普通地图会这么做
