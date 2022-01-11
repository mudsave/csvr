# -*- coding: utf-8 -*-

"""
目标搜索器
"""
class ObjectFinder(object):
	"""
	搜索器基础组件，用于定义标准的搜索接口
	"""
	def find(self, finder, position = None):
		"""
		virtual method.
		finder想要搜索目标
		@param finder: Entity; 搜索者
		"""
		assert False, "No Impl"

	def init(self, dataSection):
		"""
		virtual method.
		从配置中初始化
		"""
		assert False, "No Impl"
