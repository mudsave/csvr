# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from ..Spell import SpellEffect

class EffectHitPose(SpellEffect):
	"""
	受击效果：击中、击退、击飞、击倒等
	"""
	def init(self, dataSection):
		"""
		"""
		SpellEffect.init(self, dataSection)
		
		# 标识技能和声音会在对象的哪个子对象上播放，例如：Bip01/r_h/s014_lod0/WeaponTrail
		#self.bindingObjectPath = dataSection.readString( "bindingObjectPath" )
		
		# 击中的类型：详看eHitPose定义
		#self.hitPoseType = dataSection.readString( "hitPoseType" )
		
		# 击中的光效
		#self.effect = dataSection.readString( "effect" )
		
		# 击中的声音
		#self.sound = dataSection.readString( "sound" )
	
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		# 允许没有src
		srcID = -1
		if src is not None:
			srcID = src.id

		dst.allClients.seeSpellEffectFS( self.id, srcID )

