# -*- coding: utf-8 -*-
#
import json
import KBEngine
from KBEDebug import *
from JsonToPython import *

def loadAllMapConfig(path):
	config = {}
	for f in KBEngine.listPathRes(path, "json"):
		fs = open(f,encoding="utf8")
		data = json.load(fs)
		fs.close()
		config.update(data)
	return config
	
g_mapTriggerConfig = loadAllMapConfig("configs/Map") #场景地图配置
#g_shopItemConfig = loadAllShopConfig("configs/Shop") #商店配置