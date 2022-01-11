# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import json
from Extra.CellConfig import g_entityConfig
import Extra.Common as Common
import cProfile
import Extra.SpaceEventID as SpaceEventID
import GloballyStatus as GS
pr = None

class GM:
	
	def __init__(self):
		pass
	
	def sendGM(self, exposed, str):
		if exposed != self.id:
			return	
	
		if str == "":		
			return
		
		strlist = str.split(' ')
		try:
			func = getattr(self, "GM_" + strlist[0])
		except:
			return

		del strlist[0]

		for i in range( 0, len( strlist ) ):
			try:
				strlist[i] = json.loads(strlist[i])
			except:
				pass

		func(*strlist)
	
	def GM_test(self, args1, args2, args3):
		print(args1)
		print(args2)
		print(args3)
	
	def GM_goto(self, id):
		self.gotoMap(id)
	
	def GM_goton(self, id):
		self.gotoMap(id, False)
	
	def GM_createEntity(self, id):
		config = g_entityConfig[id]
		Common.createEntity(self.spaceID, self.position, self.direction, config)
	
	def GM_sendGearMsg(self, msgName):
		spaceBase = self.getCurrentSpaceBase()
		space = KBEngine.entities[spaceBase.id]
		space.fireEvent(SpaceEventID.GearSyncMsg, 1001, msgName, "")
		
	def GM_setEHP(self, id, hp):
		entity = KBEngine.entities[id]
		entity.HP = hp
	
	def GM_spaceMsgEnd(self):
		spaceBase = self.getCurrentSpaceBase()
		space = KBEngine.entities[spaceBase.id]		
		space.fireEvent( SpaceEventID.SpaceMsgEnd, 1 )
		
	def GM_skillGuide(self, id):
		self.sendStatusMessage(GS.GUIDE_SKILL_ID, id)