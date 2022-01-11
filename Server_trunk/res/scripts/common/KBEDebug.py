# -*- coding: utf-8 -*-
import sys
import KBEngine

# 是否打印调用者的程序信息(Source FileName + LineNo)
printPath = True


def _printErrorInfo():
	print( str( sys.exc_info()[1] ) )

def _getClassName( f, funcName ):
	"""
	取得对应堆栈帧(Frame)的类名字。
	@param 			f : 调用者的堆栈帧(Frame)
	@type 			f : Frame
	@return			  : 对应的类名字
	@rtype			  : string
	"""
	try:
		selfClass = f.f_locals[ 'self' ].__class__					# Note: This only works if self argument is self.
		mro = getattr( selfClass, "__mro__", [] )					# Only new style classes have __mro__ ( inherit object class )
		if mro == []:												# get all grandsire classes
			stack = [selfClass]
			while stack:
				curr = stack.pop( 0 )
				mro.append(curr)									# get all grandsire classes and my self
				stack += curr.__bases__								# base classes
		for bases in mro:
			method = bases.__dict__.get( funcName, None )
			if method is None :										# private method
				prvFunc = "_%s%s" % ( bases.__name__, funcName )	# private method name
				method = bases.__dict__.get( prvFunc, None )		# get private method
			if ( method is not None ) and \
				method.__code__ == f.f_code :						# if find out the method, in class
					return bases.__name__ + "."						# return the class name
	except : pass
	return ""

def _printMessage( args ):
	"""
	按照输出信息。
	@param 			args		: 输出的信息
	@type 			args		: tuple
	@return						: None
	"""
	f = sys._getframe(2)
	if printPath:
		print( f.f_code.co_filename + "(" + str( f.f_lineno ) + ") :" )
	funcName = f.f_code.co_name
	className = _getClassName( f, funcName )
	print( "%s%s: " % ( className, funcName ), end = "" )
	print( *args, sep = "" )

def STREAM_MSG( stream, *args ):
	"""
	输出Hack信息。
	@param 			stream	: 输出的二进制流
	@type 			stream	: String
	@param 			args	: 输出的信息
	@type 			args	: 可变参数列表
	@return					: 无
	@rtype					: None
	"""
	dumpStream = "0x"
	for char in stream:
		hexString = hex( ord( char ) )[2:]
		pad = "0" * ( 2 - len( hexString ) )
		dumpStream = dumpStream + pad + hexString
	sargs = list( args )
	sargs.append( dumpStream )
	_printMessage( sargs )

def TRACE_MSG( *args ):
	"""
	输出Trace信息。
	@param 				args : 输出的信息
	@type 				args : 可变参数列表
	@return					 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_NORMAL)
	_printMessage( args )

def DEBUG_MSG( *args ):
	"""
	输出Debug信息。
	@param 				args : 输出的信息
	@type 				args : 可变参数列表
	@return					 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_DBG)
	_printMessage( args )

def INFO_MSG( *args ):
	"""
	输出Info信息。
	@param 				args : 输出的信息
	@type 				args : 可变参数列表
	@return					 : 无
	@rtype					 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_INFO)
	_printMessage( args )

def NOTICE_MSG( *args ):
	"""
	输出Notice信息。
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : None
	"""
	_printMessage( args )

def WARNING_MSG( *args ):
	"""
	输出Warning信息。
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_WAR)
	_printMessage( args )

def ERROR_MSG( *args ):
	"""
	输出Error信息。
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : 无
	@rtype				 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_ERR)
	_printMessage( args )
	_printErrorInfo()

def CRITICAL_MSG( *args ):
	"""
	输出Critical信息。
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_ERR)
	_printMessage( args )

def HACK_MSG( *args ):
	"""
	输出Hack信息。
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_ERR)
	_printMessage( args )

def HOOK_MSG( *args ) :
	"""
	输出Hook信息。
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_ERR)
	excStr = "Hook: "
	for i in xrange( len( args ) ) :
		excStr += "%s, "
	try :
		raise excStr % args
	except:
		sys.excepthook( *sys.exc_info() )

def EXCEHOOK_MSG( *args ) :
	"""
	输出当前栈帧错误信息，常用于输出异常信息
	
	@param 			args : 输出的信息
	@type 			args : 可变参数列表
	@return				 : None
	"""
	KBEngine.scriptLogType(KBEngine.LOG_TYPE_ERR)
	exceInfo = sys.exc_info()
	if exceInfo is None or exceInfo == ( None, None, None ) :
		ERROR_MSG( "no exception in stack!" )
	else :
		print( "EXCEHOOK_MSG: " )
		for arg in args :
			print( str( arg ) )
		sys.excepthook( *exceInfo )

