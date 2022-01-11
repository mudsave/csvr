# -*- coding: utf-8 -*-
import os
import KBEngine
from KBEDebug import *
import GloballyDefine as GD
import GloballyConst as GC

def onInit(isReload):
	"""
	KBEngine method.
	当引擎启动后初始化完所有的脚本后这个接口被调用
	"""
	DEBUG_MSG('onInit::isReload:%s' % isReload)
	groupID = os.getenv("KBE_BOOTIDX_GROUP")
	key = GD.GLOBALDATAPREFIX_CELLAPP + str(groupID)
	KBEngine.globalData[key] = groupID
	
	if int(groupID) <= GC.CELLAPP_NOT_BALCANING_AMOUNT:
		KBEngine.setAppFlags(KBEngine.APP_FLAGS_NOT_PARTCIPATING_LOAD_BALANCING)	
	
def onGlobalData(key, value):
	"""
	KBEngine method.
	globalData改变 
	"""
	DEBUG_MSG('onGlobalData: %s' % key)
	
def onGlobalDataDel(key):
	"""
	KBEngine method.
	globalData删除 
	"""
	DEBUG_MSG('onDelGlobalData: %s' % key)

def onCellAppData(key, value):
	"""
	KBEngine method.
	cellAppData改变 
	"""
	DEBUG_MSG('onCellAppData: %s' % key)
	
def onCellAppDataDel(key):
	"""
	KBEngine method.
	cellAppData删除 
	"""
	DEBUG_MSG('onCellAppDataDel: %s' % key)
	
def onSpaceData( spaceID, key, value ):
	"""
	KBEngine method.
	spaceData改变
	"""
	pass
	
def onAllSpaceGeometryLoaded( spaceID, isBootstrap, mapping ):
	"""
	KBEngine method.
	space 某部分或所有chunk等数据加载完毕
	具体哪部分需要由cell负责的范围决定
	"""
	pass
	
	
import MapDataConfig
from Extra.CellConfig import g_mapDataConfig
MapDataConfig.init(g_mapDataConfig)

from Spell.SpellLoader import g_spellLoader
g_spellLoader.scanEffect( "configs/SpellEffects/" )			# 这个必须先初始化
g_spellLoader.scanPassiveSkill( "configs/PassiveSkills/" )	# 这个必须在buff前面初始化，因为被动技能通过buff添加或者移除
g_spellLoader.scanBuff( "configs/Buffs/" )					# effect，PassiveSkills先初始化这个才能初始化
g_spellLoader.scanSpell( "configs/Spells/" )
g_spellLoader.scanTalentSpell( "configs/TalentSpells/" )
g_spellLoader.initAll()

# init AI datas
from NPCAI.AILoader import g_aiLoader, initAIType
initAIType()
g_aiLoader.init( "configs/AIConfig.json" )