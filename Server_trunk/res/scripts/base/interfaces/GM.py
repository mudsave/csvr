# -*- coding: utf-8 -*-
from KBEDebug import *
import KBEngine
import KST
import json

class GM:
	
	def __init__(self):
		pass
	
	def sendGM(self, str):
		if str == "":		
			return
		
		strlist = str.split(' ')
		try:
			func = getattr(self, "GM_" + strlist[0])
		except:
			return

		del strlist[0]

		for i in range( 0, len( strlist ) ):
			try:
				strlist[i] = json.loads(strlist[i])
			except:
				pass

		func(*strlist)
	
	def GM_test(self, strlist):
		print(strlist[1])
	
