# -*- coding: gb18030 -*-

import random


def enume( env, varList, start = 0 ):
	"""
	向一个环境字典中添加枚举定义
	@param env: dict; 需要添加枚举的目标
	@param varList: array of string; 枚举变量名字符串；例：["State_Move", "State_Jump", ...]
	@param start: int; 枚举值的开始
	@return: 返回evn参数本身
	"""
	for index, varName in enumerate( varList, start ):
		env[varName] = index
	return env

def reverse_enume( env, varList, start = 0 ):
	"""
	向一个环境字典中添加逆反的枚举定义——key 为枚举索引值，value为(varList)参数中的字符串（即变量名）
	这个函数产生的值的key-value与enume()产生的刚好相反。
	
	@param env: dict; 需要添加枚举的目标
	@param varList: array of string; 枚举变量名字符串；例：["State_Move", "State_Jump", ...]
	@param start: int; 枚举值的开始
	@return: 返回evn参数本身
	"""
	for index, varName in enumerate( varList, start ):
		env[index] = varName
	return env

def initRand():
	"""
	初始化随机函数
	"""
	random.seed(int((time.time()*100)%256))

def estimate( odds, precision = 100 ):
	"""
	判断几率odds，精度缺省1%
	@param			odds	  : 出现几率
	@type			odds	  : int16
	@param			precision : 精度参数
	@type			precision : integer
	@return					  : True 存在几率,False 不存在几率
	@rtype					  : boolean
	"""
	if odds <= 0:
		return False
	if odds >= precision:
		return True
	r = int( random.random() * precision + 1 )
	if odds >= r:
		return True
	return False

def getTimestamp():
	"""
	获得时间戳。
	"""
	return int( ( KBEngine.time() * 10 ) % 1024 )

def searchFile( searchPath, exts ):
	"""
	搜索指定的目录(searchPath)，查找所有符合指定的扩展名(exts)的文件（或目录）；
	注意：我们并不判断一个文件是否是文件夹，也不进行递归查找，仅仅以扩展名在指定目录进行查找；

	@param searchPath: STRING or tuple of STRING, 要搜索的路径（列表）
	@param       exts: STRING or tuple of STRING, 要搜索的扩展名（列表），每个扩展名都必须是以点开头的，如：.txt
	@return: array of STRING
	"""
	assert isinstance( exts, (str, tuple, list) )
	assert isinstance( searchPath, (str, tuple, list) )
	if isinstance( exts, str ):
		exts = ( exts, )
	if isinstance( searchPath, str ):
		searchPaths = [ searchPath, ]
	else:
		searchPaths = list( searchPath )

	files = []
	for searchPath in searchPaths:
		section = ResMgr.openSection( searchPath )
		assert section is not None, "can't open section %s." % searchPath

		if searchPath[-1] not in "\\/":
			searchPath += "/"

		for key in section.keys():
			name, ext = os.path.splitext( key )		# 截取扩展名
			if ext in exts:							# 扩展名匹配
				files.append( searchPath + key )
		ResMgr.purge( searchPath )
	return files

def ipToStr( val ):
	"""
	转换int32的ip地址为以点(".")分隔的字符串模式。
	例：ipToStr( 436211884 ) --> '172.16.0.26'
	"""
	return "%i.%i.%i.%i" % ( val & 0xff, ( val >> 8 ) & 0xff, ( val >> 16 ) & 0xff, ( val >> 24 ) & 0xff )


