# -*- coding: utf-8 -*-
import KBEngine
from KBEDebug import *
from Extra.CellConfig import g_entityConfig
#from Extra.CellConfig import g_entityFightConfig
from SrvDef import eObjectType

"""
在space中创建entity函数
config:触发器配置
"""
def createEntity(spaceID, position, rotation, config, paramDict = None):
	if config["entityType"] == eObjectType.Monster:
		return createMonster(spaceID, position, rotation, config, paramDict)
	elif config["entityType"] == eObjectType.Gear:
		return createGear(spaceID, position, rotation, config, paramDict )
	elif config["entityType"] == eObjectType.RangeTrap:
		return createRangeTrap(spaceID, position, rotation, config, paramDict)
	elif config["entityType"] == eObjectType.NPC:
		return createNPC(spaceID, position, rotation, config, paramDict)
		
	return None

"""
创建怪物函数
"""
def createMonster(spaceID, position, rotation, config, paramDict = None):		
	
	position = tuple(position)
	rotation = tuple(rotation)
	
	params = {
		'eMetaClass' : config['id'],
		'name' : config['name'],
		'modelID' : config['modelID'],
		'weaponID' : config['weaponID'],
		'sceneCampID' : config['sceneCampID'],
		'spawnPosition' : position,
		'actionFX' : config['birthActionFX'],
		'fightID' : config['fightID']
	}
	
	if paramDict is not None:
		params.update(paramDict)
		
	entity = KBEngine.createEntity('Monster', spaceID, position, rotation, params)
	return entity

"""
创建功能性NPC
"""
def createNPC(spaceID, position, rotation, config, paramDict = None):
	
	position = tuple(position)
	rotation = tuple(rotation)
	
	params = {
		'eMetaClass' : config['id'],
		'name' : config['name'],
		'modelID' : config['modelID'],
		'weaponID' : config['weaponID'],
	}
	
	if paramDict is not None:
		params.update(paramDict)
	
	entity = KBEngine.createEntity('NPC', spaceID, position, rotation, params)
	return entity	
	
"""
创建机关
"""
def createGear(spaceID, position, rotation, config, paramDict = None ):
	
	position = tuple(position)
	rotation = tuple(rotation)
	
	params = {
		'eMetaClass' : config['id'],
		'name' : config['name']
	}
	
	if paramDict is not None:
		params.update(paramDict)
		
	entity = KBEngine.createEntity('Gear', spaceID, position, rotation, params)
	return entity

"""
创建范围触发器
"""
def createRangeTrap(spaceID, position, rotation, config, paramDict = None):
	
	position = tuple(position)
	rotation = tuple(rotation)
	
	params = {
		'eMetaClass' : config['id'],
		'name' : config['name'],
		'modelID' : config['modelID'],
		'triggerSize' : config['triggerSize'],
	}

	if paramDict is not None:
		params.update(paramDict)
	
	params['triggerSize'] = tuple(params['triggerSize'])
	params['triggerCenter'] = tuple(params['triggerCenter'])
	
	entity = KBEngine.createEntity('RangeTrap', spaceID, position, rotation, params)
	return entity