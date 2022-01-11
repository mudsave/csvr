# -*- coding: gb18030 -*-

"""
规则：
	使用事件前必须为你所确立的事件定义一个“消息宏”，并将消息宏归类地写到本模块的开头处

样例：
class test:
	def __init__( self ):
		registerEvent( "EVENT_STRING", self )	# 在初始化的时候或需要的时候注册某个事件

	def __del__( self ):
		unregisterEvent( "EVENT_STRING", self )	# 在实例被删除的时候或不再需要的时候取消对某个事件的注册

	def onEvent( self, name, *args ):			# 所有注册实例都必须有这个方法，它会在被触发的消息产生时自动调用注册实例的onEvent()方法
		if name == "EVENT_STRING":
			do some thing in here
		else:
			do other
"""

"""
2006.02.24: writen by penghuawei
2009.02.26: tidy up by huangyongwei
注意：
	被注册的类实例或类必须包含方法：onEvent
	当消息发出时，onEvent 将会被触发，onEvent 的第一个参数是上面所定义的消息宏，后面接着可以有若干个参数（不同的消息，其参数不一样）
"""

import sys
import weakref
from KBEDebug import *


# --------------------------------------------------------------------
# 实现事件类客户端全局事件类( 每个消息宏对应一个事件实例 )
# --------------------------------------------------------------------
class _Event:									# 改为模块私有（hyw--2009.02.26）
	def __init__( self, name ):
		self._name = name						# 事件名称
		self._receivers = []					# 事件接收者，注意每条消息可以有多个接收者( renamed from 'handlers' to 'receiver' by hyw--2009.02.26 )

	def fire( self, *argv ):
		"""
		触发事件
		"""
		for index in range( len( self._receivers ) - 1, -1, -1 ):
			receiver = self._receivers[index]()
			if receiver:
				#try:
					receiver.onEvent( self._name, *argv )
				#except Exception as errstr:
				#	err = "error take place when event '%s' received by %s:\n" % ( self._name, str( receiver ) )
				#	DEBUG_MSG( err )
			else:
				self._receivers.pop( index )

	def addReceiver( self, receiver ):
		"""
		添加消息接收者
		"""
		wr = weakref.ref( receiver )
		if wr not in self._receivers :
			self._receivers.append( wr )

	def removeHandler( self, receiver ):
		"""
		删除消息接收者
		"""
		receive = weakref.ref( receiver )
		if receive in self._receivers :
			self._receivers.remove( receive )

	def clearReceivers( self ):
		"""
		清除所有事件接收者
		"""
		self._receivers=[]

class _EventExtend:
	def __init__( self, name ):
		self._name = name						# 事件名称
		self._receivers = {}					# 事件接收者，注意每条消息可以有多个接收者( renamed from 'handlers' to 'receiver' by hyw--2009.02.26 )
	
	def fire( self, fireObj , *argv ):
		"""
		触发事件
		"""
		for id in list(self._receivers.keys()):
			eventFun = self._receivers[id]
			func = getattr(fireObj, eventFun)
			if func != None:
				func(id, *argv)	
	
	def addReceiver( self, id, funcName ):
		"""
		添加消息接收者
		"""
		self._receivers[id] = funcName

	def removeHandler( self, id ):
		"""
		删除消息接收者
		"""
		del self._receivers[id]

	def clearReceivers( self ):
		"""
		清除所有事件接收者
		"""
		self._receivers={}	
	
class SpaceEvent:
	
	def __init__(self):
		self.g_events = {}				# key：消息宏，类型为 str，value：是 _Event 类实例
		self.g_eventsExtend = {}		# 事件系统扩展
		
	# --------------------------------------------------------------------
	# 实现事件注册和吊销接口
	# --------------------------------------------------------------------
	def registerEvent(self, eventKey, receiver ):
		"""
		注册一个事件
		@type			eventKey : str
		@param			eventKey : 消息宏
		@type			reveiver : class instance
		@param			reveiver : 消息接收者（注意：该事件接收者必须包含方法：onEvent）
		"""
		try:
			event = self.g_events[eventKey]
		except KeyError:
			event = _Event( eventKey )
			self.g_events[eventKey] = event
		event.addReceiver( receiver )

	def unregisterEvent(self, eventKey, receiver ):
		"""
		删除一个消息接收者
		@type			eventKey : str
		@param			eventKey : 消息宏
		@type			reveiver : class instance
		@param			reveiver : 要删除的消息接收者
		"""
		try:
			self.g_events[eventKey].removeHandler( receiver )
		except KeyError:
			err = "receiver is not in list of enevt '%s''" % eventKey

	def registerEventID(self, eventKey, id, funcName ):
		"""
		注册一个事件
		@type			eventKey : str
		@param			eventKey : 消息宏
		@param			id 		 : int 数据类型ID
		@param			funcName : 函数名称（接受者为自身，专门为单例提供的事件方案）
		"""
		try:
			event = self.g_eventsExtend[eventKey]
		except KeyError:
			event = _EventExtend( eventKey )
			self.g_eventsExtend[eventKey] = event
		event.addReceiver( id, funcName )
	
	def unregisterEventID(self, eventKey, id ):
		"""
		删除一个消息接收者
		@type			eventKey : str
		@param			eventKey : 消息宏
		@type			id 		 : int
		@param			id		 : 数据类型ID
		"""	
		try:
			self.g_eventsExtend[eventKey].removeHandler( id )
		except KeyError:
			err = "receiver is not in list of enevt '%s''" % eventKey
		
	
	def fireEvent(self, eventKey, *args ):
		"""
		触发指定事件
		@type			eventKey : str
		@param			eventKey : 要触发的消息类型
		@type			*args	 : all types
		@param			*args	 : 消息参数
		"""
		try:
			if eventKey in self.g_events:
				self.g_events[eventKey].fire( *args )
		except KeyError:
			pass
		
		try:		
			if eventKey in self.g_eventsExtend:
				self.g_eventsExtend[eventKey].fire( self, *args )
		except KeyError:
			pass
