# -*- coding: gb18030 -*-

"""
游戏功能相关的全局常量值定义；
用于base/cell/client等app，以便在需要调整某些功能时，可以方便的通过这些定义来调整；
理论上，这个配置是可以由策划调整的。例如玩家所持有的金钱上限等等
"""

import GloballyDefine as GDef

# 主城生出点位置和朝向
SPAWN_SPACE = "99999999"
SPAWN_POSITION = (-0.37, 3.808, 0.6395)
SPAWN_DIRECTION = (0.0, 0.0, 3.14)

# --------------------------------------------------------------------
# about login
# --------------------------------------------------------------------
LOGIN_ROLE_UPPER_LIMIT		= 3			# 最多可以创建多少个角色


#-------------------------------------------------------------------------------
#about duplication
#--------------------------------------------------------------------------------
SPACE_CONST_DISTANCE = 10000		# 单位：米 在space的cell中，查找某个space中某个范围内的entity
SPACE_HYST = 2.0					# 指定超过AOI区域的滞后区域的大小
SPACE_CREATE_ENTITY_TIME = 0.1     # 产生entitiy最小间隔


#----------------------------------------------------------------
# about cellapp
#----------------------------------------------------------------
CELLAPP_INIT_AMOUNT = 0				#多少个cellapp起来后，开始初始化spaceManager
CELLAPP_NOT_BALCANING_AMOUNT = 0	#默认多少个cellapp不参与负载均衡(当此值大于上面的值时，服务器会等到不参与负载均衡的cellapp都起来后，才开始创建space)




MAIN_QUEST_FIRST_ID = 101000
MAX_QUEST_AMOUNT = 20
MAX_DAY_QUEST_AMOUNT = 20
MAX_COMP_DAY_QUEST_AMOUNT = 30
Quest_DAY_INTERVAL_TIME = 1800
Quest_Day_Need_GoldCoin = 15

