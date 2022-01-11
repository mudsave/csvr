# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from .Trap import Trap
from .TrapEvent import TrapEvent
from .TrapCondition import TrapCondition
from SrvDef import eObjectType
import Math 
import math
import Vector

class TrapBase(Trap): 
	"""
	客户端和服务器联动陷阱基础类
	"""
	def __init__( self ):
		"""
		"""
		Trap.__init__(self)
	
	def init( self, dataSection ):
		"""
		"""
		pass
	
	def enter( self, owner, entity ):
		"""
		进入陷阱
		@param owner: Entity；表示此陷阱依附的对象
		@param entity: Entity；表示谁进入了这个陷阱
		"""
		if entity.objectType != eObjectType.Player:
			return
		
		if not self.pointInRangeSide(owner, entity.position):
			return
		
		for k,v in owner.config['condition'].items():
			func = getattr(TrapCondition, k)
			if not func(owner, entity,  v):
				return
		
		owner.trapEntity.append(entity.id)
		
		for k,v in owner.config['enterEvent'].items():
			func = getattr(TrapEvent, k)
			func(owner, entity,  v)
	
	def leave( self, owner, entity ):
		"""
		离开陷阱
		@param owner: Entity；表示此陷阱依附的对象
		@param entity: Entity；表示谁进入了这个陷阱
		"""
		if entity.objectType != eObjectType.Player:
			return
		
		if self.pointInRange(owner, entity.position):
			return
		
		owner.trapEntity.remove(entity.id)
		
		for k,v in owner.config['leaveEvent'].items():
			func = getattr(TrapEvent, k)
			func(owner, entity,  v)

	
	def pointInRange(self, owner, point):		
		"""
		判断此点是否在立方体内
		"""
		forward = owner.forward()
		#将高度变成0，借用forward的高度为0的特点，简化算法
		p0 = Math.Vector3(point[0]-owner.position[0]-owner.triggerCenter[0], 0, point[2]-owner.position[2]-owner.triggerCenter[2])
		rad = Vector.radian(p0, forward)
		len = Vector.magnitude(p0)
		p = Math.Vector3(math.sin(rad)*len, point[1]-owner.position[1]-owner.triggerCenter[1], math.cos(rad)*len)
		
		if (-owner.triggerSize[0]/2 <= p[0]) and (p[0] <= owner.triggerSize[0]/2 ) and \
			(-owner.triggerSize[1]/2 <= p[1]) and (p[1] <= owner.triggerSize[1]/2 ) and \
			(-owner.triggerSize[2]/2 <= p[2]) and (p[2] <= owner.triggerSize[2]/2 ):
			return True
		return False
	
	def pointInRangeSide(self, owner,  point):
		"""
		判断此点是否在立方体内
		"""
		len = 2.5
		forward = owner.forward()
		#将高度变成0，借用forward的高度为0的特点，简化算法
		p0 = Math.Vector3(point[0]-owner.position[0]-owner.triggerCenter[0], 0, point[2]-owner.position[2]-owner.triggerCenter[2])
		rad = Vector.radian(p0, forward)
		p0_len = Vector.magnitude(p0)
		p = Math.Vector3(math.sin(rad)*p0_len, point[1]-owner.position[1]-owner.triggerCenter[1], math.cos(rad)*p0_len)
		
		if (-(owner.triggerSize[0]+len)/2 <= p[0]) and (p[0] <= (owner.triggerSize[0]+len)/2 ) and \
			(-(owner.triggerSize[1]+len)/2 <= p[1]) and (p[1] <= (owner.triggerSize[1]+len)/2 ) and \
			(-(owner.triggerSize[2]+len)/2 <= p[2]) and (p[2] <= (owner.triggerSize[2]+len)/2 ):
			return True
		
		return False
