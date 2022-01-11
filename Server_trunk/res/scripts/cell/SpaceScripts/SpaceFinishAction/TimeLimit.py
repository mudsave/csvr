# -*- coding: utf-8 -*-

"""
"""
from KBEDebug import *
from .FinishConditionBase import FinishConditionBase
from Extra import ECBExtend
from SpaceFinishData import SpaceFinishData

class TimeLimit(FinishConditionBase):
	"""
	时间限制，失败
	"""
	def init( self, configData, space ):
		"""
		"""
		if configData > 0:
			data = SpaceFinishData()
			data.index = space.addTimer( configData, 0, ECBExtend.SPACE_ON_FINISH_TIMER )
			data.type = "timeLimit"
			space.finishData.datas[data.index] = data
		else:
			self.onEnd( space, 0, False )

	def verify( self, space, data, params ):
		"""
		"""
		self.onEnd( space, data.parent, False )