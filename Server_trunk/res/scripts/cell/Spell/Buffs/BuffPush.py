# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from BuffDataType import BuffDataType
from SrvDef import eEffectStatus, eObjectType, eDirChange
from .BuffSimple import BuffSimple
import Math 
import math
import Vector


class BuffPush(BuffSimple):
	"""
	位移buff(将目标往指定方向推动)
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		BuffSimple.init( self, dataSection )

		generalFunction = dataSection["generalFunction"]
		self.referenceType = generalFunction.readInt( "referenceType" ) # 参照类型
		self.offsetAngle = generalFunction.readFloat( "offsetAngle" )* math.pi / 180.0 # 先换成弧度
		self.distance = generalFunction.readFloat( "distance" ) # 击退距离
		self.pushTime = generalFunction.readFloat( "pushTime" ) # 击退时间

	def onAttach(self, src, dst, buffData):
		"""
		template method.
		当buff附到owner身上时，此接口被调用（仅调用一次）
		可以在此处做一些前期初始化的事情

		"""
		dst.effectStatusCounterIncr(eEffectStatus.HitBy)
			
		src = KBEngine.entities.get(buffData.casterID,None)

		for effect in self.attachEffect:
			effect.cast( src, dst, None )

		if self.distance <= 0.0 or self.pushTime <= 0.0:
			buffData.misc["pushSpeed"] = 0.0
			return
		else:
			buffData.misc["pushSpeed"] = self.distance/self.pushTime

		# 判断buff作用目标之间的关系，若施法者为玩家且受术者为NPC时，需要设置受术者的controlledBy属性
		if dst.objectType == eObjectType.Monster:
			if src.objectType == eObjectType.Player:
				dst.controlledBy = src.base

		"""
			endPos = dst.position
			if self.referenceType == 0:
				srcDir = Math.Vector3(src.direction)
				offSetDir = Math.Vector3(math.sin(srcDir.z - self.offsetAngle), 0, math.cos(srcDir.z - self.offsetAngle)) # 偏移后的朝向
				endPos = dst.position + offSetDir * self.distance

			elif self.referenceType == 1:
				dstDir = Math.Vector3(dst.direction)
				offSetDir = Math.Vector3(math.sin(dstDir.z - self.offsetAngle), 0, math.cos(dstDir.z - self.offsetAngle)) # 偏移后的朝向
				endPos = dst.position + offSetDir * self.distance

			elif self.referenceType == 2:
				direction = dst.position - src.position
				direction.normalise()
				offSetDir = Vector.rotalteXZ(direction, self.offsetAngle) # 偏移后的朝向
				endPos = dst.position + offSetDir * self.distance

			listPos = KBEngine.raycast( dst.spaceID, 0, dst.position, endPos )

			if listPos is not None:
				endPos = listPos[len(listPos)-1]

			pushSpeed = self.distance / self.pushTime   # 速度
			dst.moveToPoint( endPos, pushSpeed, 0.0, None, False, False )
		"""

	def onDetach(self, owner, buffData):
		"""
		template method.
		当buff从owner身上取下来时，此接口被调用（仅调用一次）
		可以在此处做一些buff结束时的事情
		例如：给owner减去10点基础伤害
		"""
		if owner.controlledBy != owner.base:
			owner.controlledBy = owner.base

		owner.effectStatusCounterDecr(eEffectStatus.HitBy)

		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.detachEffect:
			effect.cast( src, owner, None )

	def onInterrupt(self, owner, buffData):
		"""
		template method.
		buff中断回调，继承于此类的buff可以根据实际情况做自己想做的事情
		"""
		if owner.controlledBy != owner.base:
			owner.controlledBy = owner.base

		owner.effectStatusCounterDecr(eEffectStatus.HitBy)

		src = KBEngine.entities.get(buffData.casterID,None)
		for effect in self.interruptEffect:
			effect.cast( src, owner, None )

