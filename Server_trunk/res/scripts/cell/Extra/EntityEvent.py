# -*- coding: utf-8 -*-
import sys
from KBEDebug import *

"""
规则：
	
样例：
class GM(EntityEvent):	
	def __init__(self):
		global g_testEvent
		注册事件
		self.registerEvent("dddd", g_testEvent)
	
	def GM_test(self, strlist):
		事件
		self.fireEvent("dddd", strlist[1])

"""
	
class EntityEvent:
	
	_classList = {}
	
	def __init__(self):
		#self.events = {}
		pass
		
	@staticmethod
	def registerClass(targetClass):
		"""
		注册类
		"""
		if type(targetClass) is type:
			EntityEvent._classList[targetClass.__name__] = targetClass
	
	@staticmethod
	def deregisterClass(targetClass):
		"""
		反注册类
		"""
		try:
			del EntityEvent._classList[targetClass.__name__]
		except KeyError:
			ERROR_MSG( "EntityEvent classList no class = %s." % targetClass.__class__.__name__ )
			pass
	
	def registerEvent(self, eventKey, targetClass, funcName):
		"""
		注册事件
		"""
		if type(funcName) is not str:
			ERROR_MSG( "funcName is not string")
			return
		
		if type(targetClass) is not type:
			ERROR_MSG( "targetClass is not type")
			return
		
		className = targetClass.__name__
		if className not in EntityEvent._classList:
			ERROR_MSG( "registerEvent error. EntityEvent classList no class = '%s'." % className )
			return
		
		try:
			event = self.events[eventKey]
		except KeyError:
			event = {}
			self.events[eventKey] = event
		
		try:
			event_class = event[className]
		except KeyError:
			event_class = []
			event[className] = event_class
		
		event_class.append(funcName)
		
	def deregisterEvent(self, eventKey, targetClass, funcName):
		"""
		反注册事件
		"""
		className = targetClass.__name__
		try:
			event = self.events[eventKey]
		except KeyError:
			return
			
		try:
			event_class = event[className]
		except KeyError:
			return
		
		event_class.remove(funcName)
		if len(event_class) == 0:
			del event[className]
		
	def fireEvent(self, eventKey, *args):
		"""
		触发指定事件
		"""
		if eventKey not in self.events:
			#ERROR_MSG( "fireEvent error. events no key = '%s'." % eventKey )
			return
		
		events = self.events[eventKey]

		
		for key in list(events.keys()):
			targetClass = EntityEvent._classList[key]
			for funcName in events[key]:
				func = getattr(targetClass, funcName)
				try:
					func(self, *args)
				except:
					sys.excepthook( *sys.exc_info() )

def registerEntityClass():
	pass
	
registerEntityClass()