# -*- coding: gb18030 -*-
import Utility

SpaceEventID = [
	'EntityDead',
	'ElmGroupPass',
	'StartElmGroup',
	'SpaceStart',
	'AIActive',
	'SpaceMsgEnd',
	"GearSyncMsg",
	]
				
Utility.enume( locals(), SpaceEventID, 0 )
del SpaceEventID		