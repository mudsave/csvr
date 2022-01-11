# -*- coding: utf-8 -*-
#
"""
"""
import json

from KBEDebug import *
from Singleton import Singleton


# AI类型映射表
_g_type = {}



def initAIType():
	from .NPCAI_A import NPCAI_A
	from .NPCAI_AD import NPCAI_AD
	from .StandingAI import StandingAI
	from .ShaChongAI import ShaChongAI
	from .NPCAI_Patrol import NPCAI_Patrol

	global _g_type
	_g_type = {
		"NPCAI_A"                  : NPCAI_A,
		"NPCAI_AD"                 : NPCAI_AD,
		"StandingAI"			   : StandingAI,
		"ShaChongAI"               : ShaChongAI,
		"NPCAI_Patrol"             : NPCAI_Patrol,
	}






class AILoader(Singleton):
	"""
	AI配置加载器
	"""
	def init( self, assetPath ):
		"""
		"""
		self._aiDatas = {}
		
		f = open( KBEngine.getResFullPath( assetPath ), encoding="utf-8" )
		data = json.load(f)
		f.close()
		
		DEBUG_MSG( "loading AI config from '%s'" % f )
		for root in data.values():
			className = root["className"]
			aiLogic = _g_type[className]()
			aiLogic.init( root )
			self._aiDatas[aiLogic.id] = aiLogic
	
	def get( self, id ):
		"""
		"""
		return self._aiDatas.get( id )



g_aiLoader = AILoader()
