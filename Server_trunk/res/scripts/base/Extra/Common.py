# -*- coding: utf-8 -*-
import KBEngine
from KBEDebug import *
from Extra.BaseConfig import g_heroConfig, g_entityFightConfig
import GloballyConst as GC
import time

"""
创建新Player函数
"""
def createNewPlayerLocally( eMetaClass, paramDict):
	
	_heroConfig = g_heroConfig.get(eMetaClass)
	if _heroConfig is None:
		ERROR_MSG( "createNewPlayerLocally HeroConfig has not key => key = %s" % (profession))
		return None

	_fightConfig = g_entityFightConfig[_heroConfig['fightID']]

	params = {
		'eMetaClass' : _heroConfig['id'],
		"profession":	_heroConfig['profession'],
		'modelID' : _heroConfig['modelID'],
		'weaponID' : _heroConfig['weaponID'],
		'defaultModelID' : _heroConfig['modelID'],
		'defaultWeaponID' : _heroConfig['weaponID'],
		'HP' : _fightConfig['HPMax_base'],
		'MP' : _fightConfig['MPMax_base'],
	}
	
	paramDict.update(params)
	entity = KBEngine.createBaseLocally( "Player", paramDict )
	return entity
