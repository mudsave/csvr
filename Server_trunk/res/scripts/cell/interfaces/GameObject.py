# -*- coding: gb18030 -*-

"""
"""
import KBEngine
from KBEDebug import *

from Extra import ECBExtend
from Extra.EntityEvent import EntityEvent
from Extra.CellConfig import g_entityConfig
import GloballyDefine as GD
import Math
import math

class GameObject( KBEngine.Entity, ECBExtend.ECBExtend, EntityEvent  ):
	"""
	"""
	def __init__( self ):
		"""
		"""
		KBEngine.Entity.__init__( self )
		EntityEvent.__init__(self)

	def onSpaceGone( self ):
		"""
		space关闭
		"""
		INFO_MSG( "GameObject %s has gone!" % ( self.eMetaClass ) )
		self.destroy()		
		
	def getName( self ):
		"""
		virtual method.
		@return: the name of entity
		@rtype:  STRING
		"""
		return ""

	def getConfig(self):
		return g_entityConfig.get(self.eMetaClass)

	def getCurrentSpaceBase( self ):
		"""
		取得entity当前所在的space的space entity base
		@return: 如果找到了则返回相应的base，找不到则返回None.
				找不到的原因通常是因为space处于destoryed中，而自己还没有收到转移通知或destroy.
		"""
		try:
			return KBEngine.cellAppData[GD.GLOBALDATAPREFIX_SPACE + str(self.spaceID)]
		except KeyError:
			return None

	def forward(self):
		return Math.Vector3(math.sin(self.direction.z),0, math.cos(self.direction.z))
	
	# -------------------------------------------------
	# flags about
	# -------------------------------------------------
	def setFlag( self, flag ):
		"""
		重新设置标志

		@param flag: ENTITY_FLAG_* 左移后组合的值
		@type  flag: INT
		"""
		self.flags = flag

	def addFlag( self, flag ):
		"""
		重新设置标志

		@param flag: ENTITY_FLAG_*
		@type  flag: INT
		"""
		self.flags |= 1 << flag

	def removeFlag( self, flag ):
		"""
		重新设置标志

		@param flag: ENTITY_FLAG_*
		@type  flag: INT
		"""
		# 第32位不使用，那是标志位，如果使用了则必须要用UINT32，当前用的是INT32
		# 不使用UINT32的其中一个原因是我们可能不会有这么多标志，
		# 另一个原因是如果使用UINT32，python会使用是INT64来保存这个值
		self.flags &= ~(1 << flag)

	def hasFlag( self, flag ):
		"""
		判断一个entity是否有指定的标志

		@param flag: ENTITY_FLAG_*
		@type  flag: INT
		@return: BOOL
		"""
		flag = 1 << flag
		return ( self.flags & flag ) == flag

	# -------------------------------------------------
	# mapping about
	# -------------------------------------------------
	def getMapping( self ):
		return self.persistentMapping

	def getTempMapping( self ):
		return self.tempMapping

	def mappingQuery( self, key, default = None ):
		"""
		根据关键字查询mapping中与之对应的值

		@return: 如果关键字不存在则返回default值
		"""
		try:
			return self.persistentMapping[key]
		except KeyError:
			return default

	def mappingSet( self, key, value ):
		"""
		往一个key里写一个值

		@param   key: 任何PYTHON原类型(建议使用字符串)
		@param value: 任何PYTHON原类型(建议使用数字或字符串)
		"""
		self.persistentMapping[key] = value

	def mappingPop( self, key, default = None ):
		"""
		移除并返回一个与key相对应的值
		"""
		return self.persistentMapping.pop( key, default )

	def mappingRemove( self, key ):
		"""
		移除一个与key相对应的值
		"""
		self.persistentMapping.pop( key, None )

	def mappingAdd( self, key, value ):
		"""
		放一个key相对应的值里加一个值；
		注意：此方法并不检查源和目标的值是否匹配或正确
		"""
		v = self.mappingQuery( key, 0 )
		self.set( key, value + v )

	def tempMappingQuery( self, key, default = None ):
		"""
		根据关键字查询临时mapping中与之对应的值

		@return: 如果关键字不存在则返回default值
		"""
		try:
			return self.tempMapping[key]
		except KeyError:
			return default

	def tempMappingSet( self, key, value ):
		"""
		往一个key里写一个值

		@param   key: 任何PYTHON原类型(建议使用字符串)
		@param value: 任何PYTHON原类型(建议使用数字或字符串)
		"""
		self.tempMapping[key] = value

	def tempMappingPop( self, key, default = None ):
		"""
		移除并返回一个与key相对应的值
		"""
		return self.tempMapping.pop( key, default )

	def tempMappingRemove( self, key ):
		"""
		移除一个与key相对应的值
		"""
		self.tempMapping.pop( key, None )

	def tempMappingAdd( self, key, value ):
		"""
		放一个key相对应的值里加一个值；
		注意：此方法并不检查源和目标的值是否匹配或正确
		"""
		v = self.tempMappingQuery( key, 0 )
		self.setTemp( key, value + v )

	# ------------------------------------------------
	# think about
	# ------------------------------------------------
	def onThink( self ):
		"""
		virtual method.
		AI思考
		"""
		pass

	def think( self, delay = 0.0 ):
		"""
		设置心跳。
		@param delay:	等待下次触发think的时间，如果delay为0则立即触发
		@type  delay:	FLOAT
		"""
		if delay > 0.0:
			if self.thinkControlID < 0:	# 当thinkControlID值小于0时表示不再think，除非此值不再小于0
				return
			t = KBEngine.time()
			if self.thinkControlID != 0:
				if self.thinkWaitTime - t <= delay:
					return	# 当前剩余时间如果小于新的触发等待时间，则使用当前timer。
				self.delTimer( self.thinkControlID )
			self.thinkWaitTime = t + delay
			self.thinkControlID = self.addTimer( delay, 0.0, ECBExtend.TIMER_ON_THINKING )
		else:
			# stop if we waiting think
			if self.thinkControlID > 0:
				self.delTimer( self.thinkControlID )
				self.thinkControlID = 0
			elif self.thinkControlID < 0:	# 当thinkControlID值小于0时表示不再think，除非此值不再小于0
				return
			self.onThink()

	def pauseThink( self, stop = True ):
		"""
		中止/开启think行为；
		think行为被中止以后除非再次开启，否则所有对think的调用行为都会被忽略。
		"""
		if stop:
			if self.thinkControlID > 0:
				self.delTimer( self.thinkControlID )
			self.thinkControlID = -1
		else:
			self.thinkControlID = 0

	def onTimer_think( self, timerID, cbID ):
		"""
		ECBExtend timer callback.
		"""
		self.think( 0 )	
	
	def onRawTimer(self, timerID, cbID):
		"""
		其他无函数的定时器
		"""
		pass
	
	def navigate_( self, destination, velocity, distance, maxMoveDistance, maxSearchDistance, faceMovement, layer, userData ):
		"""
		导航
		"""
		flags = KBEngine.getSpaceData(self.spaceID, GD.SPACEDATA_NAV_FLAGS)
		return KBEngine.Entity.navigate( self, destination, velocity, distance, maxMoveDistance, maxSearchDistance, faceMovement, layer, int(flags), userData )
	
	def navigatePathPoints_( self, destination, maxSearchDistance, layer ):
		"""
		导航路径点
		"""
		flags = KBEngine.getSpaceData(self.spaceID, GD.SPACEDATA_NAV_FLAGS)
		return KBEngine.Entity.navigatePathPoints( self, destination, maxSearchDistance, layer, int(flags) )
		
# 注册timer等回调处理接口
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_THINKING, GameObject.onTimer_think )


# GameObject.py
