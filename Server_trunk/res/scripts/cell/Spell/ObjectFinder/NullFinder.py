# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .ObjectFinder import ObjectFinder

class NullFinder(ObjectFinder):
	"""
	直接返回NULL的搜索器，用于一些不需要目标的技能
	"""
	def find(self, finder, position = None):
		"""
		"""
		return []

	def init(self, dataSection):
		"""
		virtual method.
		从配置中初始化
		"""
		pass
