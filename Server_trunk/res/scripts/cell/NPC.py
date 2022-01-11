# -*- coding: gb18030 -*-
#

"""
"""

import KBEngine
from KBEDebug import *

import KST

from interfaces.GameObject import GameObject
from SrvDef import eObjectType

class NPC( GameObject):
	"""
	非玩家对象基础类
	"""
	def __init__(self):
		"""
		构造函数。
		"""
		GameObject.__init__(self)
		self.objectType = eObjectType.ActNPC						

