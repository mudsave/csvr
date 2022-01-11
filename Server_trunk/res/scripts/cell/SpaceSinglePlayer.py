# -*- coding: utf-8 -*-
#

"""
标准场景，也可以作为场景基类
"""

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

	def onEnter( self, baseMailbox, params ):
		"""
		define method.
		一个entity进入到space时的通知；
		@param baseMailbox: 进入此space的entity mailbox
		@param params: dict; 进入此space时需要的附加数据。此数据由当前脚本的packedDataOnEnter()接口根据当前脚本需要而获取并传输
		"""
		entity = KBEngine.entities[baseMailbox.id]
		Space.onEnter(self, baseMailbox, params)

	def onLeave( self, baseMailbox, params ):
		"""
		define method.
		一个entity准备离开space时的通知；
		@param baseMailbox: 要离开此space的entity mailbox
		@param params: dict; 离开此space时需要的附加数据。此数据由当前脚本的packedDataOnLeave()接口根据当前脚本需要而获取并传输
		"""
		Space.onLeave(self, baseMailbox, params)