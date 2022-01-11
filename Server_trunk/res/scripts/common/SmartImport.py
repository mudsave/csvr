# -*- coding: utf-8 -*-
#
"""
自已的模块导入函数，在所有的服务器端都有可能被调用；
"""

import re
from KBEDebug import *

def smartImport( mname ) :
	"""
	导入一个模块。
	@type         mname: STRING
	@param        mname: 模块名称。具体格式为"mod1.mod2.modN:className"
	                     其中":className"是可选的，表示要导入哪个类，如果存在则只允许有一个，类似于 "from mod import className"
	                     也就是说可以直接导入一个模块里指定的类。
	@rtype             : object of module or class
	@return            : 返回指定的模块
	@raise  ImportError: 如果指定的模块不存在则产生此异常
	"""
	compons = re.split( "\.|:", mname )						# 拆分路径和模块
	assert len( compons ) > 0, "wrong module name!"			# 排除空路径
	moduleName = mname.split( ":" )[0]						# 取出模块部分
	try :
		mod = __import__( moduleName )						# import 模块的最顶层(注意：这样写只会 import 最顶层，但它会对全路径进行检查，路径不存在会 import 失败)
	except ImportError as err :
		raise ImportError( "modult path '%s' is not exist!" % mname )

	for com in compons[1:] :								# 按模块路径顺序，循环深入地获取模块的子模块
		try :
			mod = getattr( mod, com )
		except AttributeError as err :
			EXCEHOOK_MSG( err )
			mpath = ".".join( compons[:-1] )
			raise ImportError( "module '%s' has no class or attribute '%s'!" % ( mpath, compons[-1] ) )
	return mod

