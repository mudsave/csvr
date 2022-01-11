# -*- coding: utf-8 -*-
from JsonToPython import *
import ConfigLoadFun

g_entityFightConfig = loadJsonPathKeyInt('configs/EntityFightConfig.json') #entity战斗Config配置
g_fightSystemConfig = loadJsonKeyIntProperty('configs/FightSystemAmend.json') #战斗公式参数
g_duplicateMapConfig = loadJsonPathKeyString('configs/DuplicateMapConfig.json') #副本地图信息配置表
g_commonMapConfig = loadJsonPathKeyString('configs/CommonMapConfig.json') #公共地图信息配置表
g_mapDataConfig = combineMap(g_commonMapConfig, g_duplicateMapConfig) #地图信息配置表

g_heroConfig = loadJsonPathKeyInt('configs/HeroConfig.json') #英雄配置
g_NPCConfig = loadJsonPathKeyInt('configs/NPCConfig.json') #功能性entity配置
g_MonsterConfig = loadJsonPathKeyInt('configs/MonsterConfig.json') #NPC配置
g_triggerEConfig = loadJsonPathKeyInt('configs/TriggerEConfig.json') #触发器entity配置

g_entityConfig = combineMap(g_heroConfig, g_NPCConfig, g_MonsterConfig, g_triggerEConfig) #entityConfig配置

g_mainQuestConfig = loadJsonKeyIntProperty('configs/MainQuestConfig.json') #主线任务配置表
g_branchQuestConfig = loadJsonKeyIntProperty('configs/BranchQuestConfig.json') #支线任务配置表
g_dayQuestConfig = loadJsonKeyIntProperty('configs/DayQuestConfig.json') #日常任务配置表
g_questConfig = combineMap(g_mainQuestConfig, g_branchQuestConfig, g_dayQuestConfig) #任务系统配置表

