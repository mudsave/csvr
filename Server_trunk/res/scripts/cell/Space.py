# -*- coding: utf-8 -*-
#

"""
"""
import KBEngine
from KBEDebug import *

import GloballyDefine as GD
import GloballyConst as GC

from SrvDef import eObjectType
import MapDataConfig
import Extra.SpaceEvent as SpaceEvent
import Extra.SpaceEventID as SpaceEventID
from Extra import ECBExtend
from SceneTrigger.SceneTrigger import SceneTrigger

class Space( KBEngine.Entity,
			ECBExtend.ECBExtend,
			SpaceEvent.SpaceEvent,
			SceneTrigger):
	"""
	用于控制Space entity的脚本，所有有需要的Space方法都会调用此脚本(或继承于此脚本的脚本)的接口
	"""
	def __init__( self ):
		"""
		初始化
		"""
		KBEngine.Entity.__init__( self )
		SpaceEvent.SpaceEvent.__init__(self)	
		SceneTrigger.__init__(self)	
		self.objectType = eObjectType.Space
		
		#<todo:yelei> 停止场景所有NPC的AI标示
		self.aiActive = True
		
		# register to KBEngine.cellAppData
		KBEngine.cellAppData[GD.GLOBALDATAPREFIX_SPACE + str(self.spaceID)] = self.base
		
		self.registerEvent( SpaceEventID.SpaceMsgEnd, self )
		
		self.spaceDataConfig = MapDataConfig.get( self.eMetaClass )
		self.spaceDataConfig.initSpace( self )
				
	def onCreateEntity(self, entity):
		"""
		entity创建
		"""
		self.spaceDataConfig.onCreateEntity( self, entity )

	def onInitEntity( self, entity ):
		"""
		entity初始化
		"""
		self.spaceDataConfig.onInitEntity( self, entity )

	def onSpaceGone( self ):
		"""
		space关闭
		"""
		INFO_MSG( "space %s has gone!" % ( self.eMetaClass ) )
		self.destroy()

	def onDestroy( self ):
		"""
		cell 被删除时发生
		"""
		# deregister to KBEngine.cellAppData
		del KBEngine.cellAppData[GD.GLOBALDATAPREFIX_SPACE + str(self.spaceID)]
		SceneTrigger.onDestroy(self)
		self.destroySpace()		
		#self.addTimer( 0.1, 0.0, ECBExtend.TIMER_ON_DESTROY_SPACE )

	def onEnter( self, baseMailbox, params ):
		"""
		define method.
		一个entity进入到space时的通知；
		@param baseMailbox: 进入此space的entity mailbox
		@param params: dict; 进入此space时需要的附加数据。此数据由当前脚本的packedDataOnEnter()接口根据当前脚本需要而获取并传输
		"""
		# 玩家进来了，那就需要通知base増加计数
		self.base.onEnter(baseMailbox, params)
		baseMailbox.onEnterSpace(self.base)

	def onLeave( self, baseMailbox, params ):
		"""
		define method.
		一个entity准备离开space时的通知；
		@param baseMailbox: 要离开此space的entity mailbox
		@param params: dict; 离开此space时需要的附加数据。此数据由当前脚本的packedDataOnLeave()接口根据当前脚本需要而获取并传输
		"""
		# 玩家要离开了，那就需要通知base减少计数
		self.base.onLeave(baseMailbox, params)

	def setSpaceNpcAIActive(self, value):
		if self.aiActive == value:
			return
		
		if not value:
			self.aiActive = False
		else:
			self.aiActive = True
		self.fireEvent(SpaceEventID.AIActive, self.aiActive)

	def onTimer_destroySpace( self, timerID, userArg ):
		"""
		异步销毁space（为了避开一个引起cellapp崩溃的bug）
		"""
		self.destroySpace()
		
	def onRawTimer(self, timerID, cbID):
		"""
		其他无函数的定时器
		"""
		self.spaceDataConfig.onTimer( self, timerID, cbID )

	def onEvent(self, name, *args):
		"""
		"""
		if name == SpaceEventID.SpaceMsgEnd:	
			self.onSpaceMsgEnd(*args)
	
	def onSpaceMsgEnd(self, result):
		"""
		space主动结束
		"""
		self.spaceDataConfig.onSpaceMsgEnd(self, result )
			
	def addNavFlags(self, flag):
		"""
		增加导航的标志位
		"""
		self.navFlags = self.navFlags | flag
		KBEngine.setSpaceData( self.spaceID, GD.SPACEDATA_NAV_FLAGS, str(self.navFlags) )
	
	def subNavFlags(self, flag):
		"""
		减少导航的标志位
		"""
		flag_ = flag ^ 0xFF #取反
		self.navFlags = self.navFlags & flag_
		KBEngine.setSpaceData( self.spaceID, GD.SPACEDATA_NAV_FLAGS, str(self.navFlags) )
	
# 注册timer等回调处理接口
ECBExtend.register_entity_callback( ECBExtend.SPACE_TRIGGER_ID, Space.onTimerTrigger )
ECBExtend.register_entity_callback( ECBExtend.SPACE_TRIGGER_CREATE_TMP, Space.tmpOnTimer )
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_DESTROY_SPACE, Space.onTimer_destroySpace )
