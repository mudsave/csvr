# -*- coding: utf-8 -*-
#
import json
import KBEngine

class ConfigObject : 
	def init(self): pass
	
from KBEDebug import *
"""
转换后python的字典，key为字符串
"""
def loadJsonPathKeyString(configPath):
	f = open(KBEngine.getResFullPath( configPath ),encoding="utf8")
	data = json.load(f)
	f.close()
	return data

"""
转换后python的对象，key为字符串
"""
def loadJsonKeyStringProperty(configPath, cobj = None):
	f = open(KBEngine.getResFullPath( configPath ),encoding="utf8")
	data = json.load(f)
	f.close()
	
	dataPro = {}
	for (k,v) in data.items():
		if cobj is not None:
			value = cobj()
		else:
			value = ConfigObject()
		value.__dict__.update(v)
		value.init()
		dataPro[k] = value	
	
	return dataPro	

"""
转换后python的字典，key为数字
"""	
def loadJsonPathKeyInt(configPath):
	f = open(KBEngine.getResFullPath( configPath ),encoding="utf8")
	data = json.load(f)
	f.close()
	
	dataint = {}
	for (k,v) in data.items():
		dataint[int(k)] = v

	return dataint

"""
转换后python的对象，key为数字
"""	
def loadJsonKeyIntProperty(configPath, cobj = None):
	f = open(KBEngine.getResFullPath( configPath ),encoding="utf8")
	data = json.load(f)
	f.close()
	
	dataPro = {}
	for (k,v) in data.items():
		if cobj is not None:
			value = cobj()
		else:
			value = ConfigObject()
		value.__dict__.update(v)
		value.init()
		dataPro[int(k)] = value
		
	return dataPro	

"""
多个map链接
"""	
def combineMap(*args):
	config = {}
	for arg in args :
		config.update(arg)
	return config