# -*- coding: utf-8 -*-
from JsonToPython import *
import ConfigLoadFun

g_entityFightConfig = loadJsonPathKeyInt('configs/EntityFightConfig.json') #entity战斗Config配置

g_duplicateMapConfig = loadJsonPathKeyString('configs/DuplicateMapConfig.json') #副本地图信息配置表
g_commonMapConfig = loadJsonPathKeyString('configs/CommonMapConfig.json') #公共地图信息配置表
g_mapDataConfig = combineMap(g_commonMapConfig, g_duplicateMapConfig) #地图信息配置表

g_heroConfig = loadJsonPathKeyInt('configs/HeroConfig.json') #英雄配置
