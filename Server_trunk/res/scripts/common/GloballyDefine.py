# -*- coding: gb18030 -*-

"""
代码内部相关的全局定义。
可用于base/cell/client等app。
理论上，这里定义的变量仅应该由程序自己改变，属于程序内部或与策划商讨后确定的固定定义。
例如：性别、状态、NPC类型的定义等
"""
# 定义全局数据前缀，以区分不同的功能
GLOBALDATAPREFIX_SPACE_DOMAIN = "SpaceDomain."  # space domain的前缀
GLOBALDATAPREFIX_SPACE = "Space."               # space 的前缀
GLOBALDATAPREFIX_CELLAPP = "cellapp."			


SPACEDATA_SPACE_IDENT = "MetaClass"   # 地图的唯一标识，也就是meta class名称，标识地图的唯一性
SPACEDATA_SPACE_NAME = "Name"         # 地图名称标识
SPACEDATA_CLIENT_PATH = "ClientPath"  # 用于客户端加载地图用的参数
SPACEDATA_LINE_NUMBER = "LineNumber"  # 用于多线地图，注册当前地图属于几线
SPACEDATA_NAV_FLAGS = "NavFlags"	  # 场景导航标志位
SPACEDATA_REVIVE_TYPE_DISABLE = "ReviveTypeDisable"  # 被禁止的复活类型

# --------------------------------------------------------------------
# about server state
# --------------------------------------------------------------------
SERVERST_STATUS_COMMON				= 1 	# 状态一般
SERVERST_STATUS_WELL				= 2		# 状态良好
SERVERST_STATUS_BUSY				= 3		# 服务器繁忙
SERVERST_STATUS_HALTED				= 4		# 服务器暂停

COEF_PERCENT = 10000        # 加乘值修正系数
COEF_ATK = 10000            # 2.6.1	战斗力缩放系数
COEF_COPY_MP_REGAIN = 0.5   # 2.6.2	基准副本法力恢复系数
COEF_SKILL_DMG = 2.0        # 2.6.3	基准技能伤害倍数
COEF_COPY_FIGHT_TIME = 120  # 2.6.4	基准副本时长
COEF_PARRY = 0.5            # 2.6.5	基准招架倍率
COEF_CRIT = 2.0             # 2.6.6	基准暴击倍率
COEF_HIT = 1.0              #       基准命中率


class DamageType:
	"""
	攻击力（伤害）类型：物理伤害、法术伤害
	"""
	Physics = 0  # 物理伤害
	Magic = 1    # 法术伤害

class FightResultType:
	Hit = 0x01   # 命中
	Crit = 0x02  # 暴击

class QuestStatus:
	receive = 0     #已接受
	doing = 1		#进行中
	complete = 2	#已完成
	lose = 3		#已失败
	reward = 4		#已奖励
	action = 5  	#已执行

class QuestType:
	mainQuest = 1	#主线任务
	branchQuest = 2 #支线任务
	dayQuest = 3	#日常任务
	
class DialogType:
	QuestDialog = 0		#剧情对话
	NormalDialog = 1	#闲聊