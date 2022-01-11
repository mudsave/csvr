"""
"""
from KBEDebug import *
import Extra.SpaceEventID as SpaceEventID

class FinishConditionBase(object):
	"""
	初始化注册一些entity事件或场景事件，
	当事件触发时，回调相应的函数
	"""	
	def __init__(self):
		"""
		"""
		pass
		
	def init( self, configData, space ):
		"""
		@param configData: 配置数据
		@param space: space
		@param parent: parent
		@return: None
		"""
		pass

	def verify( self, space, data ):
		"""
		验证是否完成

		@param space: space
		@param data: finishData
		@return: None
		"""
		pass

	def onEnd( self, space, parentID, result ):
		"""
		完成

		@param space: space
		@param parentID: parentID
		@param result: 结果
		@return: None
		"""
		if parentID > 0:
			data = self.finishData.datas[id]
			finishCondition = createFinishCondition( data.type )
			finishCondition.verify( self, data )
		else:
			space.fireEvent( SpaceEventID.DuplicationEnd, result )