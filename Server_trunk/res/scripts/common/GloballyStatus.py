# -*- coding: utf-8 -*-

"""
从baseapp/cellapp反馈状态信息给客户端的状态定义
在这里面定义的数据应该是全局的，所以里面的状态值也应该是全局唯一的

使用方法：
import GloballyStatus
print( GloballyStatus.ACCOUNT_STATE_CREATE_SUCCESS )
print()
"""
import Utility
from JsonToPython import *

def init_globally_status( moduleEnv, msgDict, varList, id2keyDict, start = 0 ):
	"""
	向一个环境字典中添加枚举定义，并且设置key, value到一个变量字典中
	@param moduleEnv: dict; 当前模块的本地变量字典入口
	@param msgDict: dict; 对应的消息定义字典入口
	@param varList: array of string; 枚举变量名字符串；例：[ ( "State_Move", "移动中……"), （"State_Jump", "跳跃中……"), ...]
	@param start: int; 枚举值的开始
	@return: 无
	"""
	for index, (varName, msg) in enumerate( varList, start ):
		moduleEnv[varName] = index
		msgDict[index] = msg
		id2keyDict[index] = varName
	return

def init_globally_ErrorConfig(moduleEnv, msgDict, id2keyDict):
	
	g_errorConfig = loadJsonPathKeyString('configs/MessageConfig.json') 
	for k, v in g_errorConfig.items():
		moduleEnv[v['serverVar']] = v['id']
		msgDict[v['id']] = v
		id2keyDict[v['id']] = v["errorDescription"]
		
	
# 用于存放“{ 状态值 : 状态消息描述 }”的字典
GLOBALLY_STATUS_MSG = {}

# 用于存放“状态值 : 状态名”的字典，主要用于调试时输出错误名用
GLOBALLY_STATUS_MSG_ID2KEY = {}

def id2name( msgID ):
	"""
	通过ID值返回消息名称
	例：
	id2name( 1 ) -> “ACCOUNT_STATE_NAME_NONE”
	
	@param msgID: int; 消息状态值
	@return: string
	"""
	return GLOBALLY_STATUS_MSG_ID2KEY.get( msgID, "" )



GLOBALLY_STATUS_DEF = [

	]	# end of GLOBALLY_STATUS_DEF


# 在这里统一初始化定义的字符串变量的索引值
init_globally_status( locals(), GLOBALLY_STATUS_MSG, GLOBALLY_STATUS_DEF, GLOBALLY_STATUS_MSG_ID2KEY, 0)
del GLOBALLY_STATUS_DEF

init_globally_ErrorConfig(locals(), GLOBALLY_STATUS_MSG, GLOBALLY_STATUS_MSG_ID2KEY)
