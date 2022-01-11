# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .ObjectFinder import ObjectFinder

class OneSelfFinder(ObjectFinder):
	"""
	用于统一查找范围的方式获取自身
	"""
	def init(self, dataSection):
		"""
		virtual method.
		从配置中初始化
		"""
		pass

	def find(self, finder, position = None):
		"""
		"""
		objs = []
		objs.append(finder)
		return objs