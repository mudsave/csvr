# -*- coding: utf-8 -*-

from KBEDebug import *
from .ObjectSizer import ObjectSizer

import random

"""
随机目标筛选器
"""
class RandomSizer(ObjectSizer):
	"""
	随机筛选出指定数量的目标
	"""

	def init(self, dataSection):
		"""
		"""
		# 上限人数
		self.maxNumber = dataSection.readInt( "maxNumber" )

	def sizer(self, objects, caster):
		"""
		"""
		# 若筛选的目标数量不超过最大数量，就不进行筛选了
		if len(objects) <= self.maxNumber:
			return objects
		return random.sample( objects, self.maxNumber )



