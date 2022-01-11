"""
"""
from KBEDebug import *

from .SpaceFinishAction.KillNPC import KillNPC
from .SpaceFinishAction.TimeLimit import TimeLimit

# 类型映射表
# 注意：此表需要与客户端的保持一致
# key = string
# value = class which inherit from ConditionBase

g_FINISHCONDITION_TYPE_MAPPING = {
	"killnpc": KillNPC(),
	"timeLimit" : TimeLimit(), 
}

def createFinishCondition( type ):
	"""
	通过类型标识符创建相应的副本完成条件实例
	"""
	result = None
	c = g_FINISHCONDITION_TYPE_MAPPING.get( type )
	if c is not None:
		result = c
	return result