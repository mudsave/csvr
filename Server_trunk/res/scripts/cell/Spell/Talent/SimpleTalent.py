# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from ..Spell import TalentSpell
from ..SpellLoader import g_spellLoader

class SimpleTalent(TalentSpell):
	"""
	天赋技能类
	效果得到天赋技能就直接生效的
	如：增加10点最大生命值
	"""
	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		TalentSpell.init( self, dataSection )

		self.attachEffectIDs = dataSection["attachEffect"].readInts( "item" )					# 附上时执行的效果
		self.detachEffectIDs = dataSection["attachEffect"].readInts( "item" )					# 正常卸下时执行的效果
		
		self.attachEffects = []
		for id in self.attachEffectIDs:
			self.attachEffects.append( g_spellLoader.getEffect( id ) )

		self.detachEffects = [] 
		for id in self.detachEffectIDs:
			self.detachEffects.append( g_spellLoader.getEffect( id ) )

		self.interruptCodes = dataSection.readInts( "interruptCodes" )					# 中断码			

	def add(self, entity):
		"""
		virtual method.
		把天赋添加到entity身上
		@param entity: Entity; 受术者
		@return: None
		"""
		if not entity:
			ERROR_MSG( "No entity for talent '%s:%s'" % (self.id, self.name) )
			return
		
		self.attach(entity)

	def attach(self, entity):
		"""
		virtual method.
		把天赋的效果附到entity身上
		"""
		for attachEffect in self.attachEffects:
			attachEffect.cast(entity, entity, self)

		self.onAttach(entity)

	def onAttach(self, entity):
		"""
		把天赋的效果附到entity身上时做一些处理
		"""

	def remove(self, entity):
		"""
		virtual method.
		把buff从entity身上卸下来
		"""
		self.onRemove(entity)

	def onRemove(self, entity):
		"""
		从entity身上移除自己，进行移除清理
		"""
		for detachEffect in self.detachEffects:
			detachEffect.cast(entity, entity, self)	

	def interrupt(self, entity):
		"""
		template method.
		天赋中断回调，被中断时做一些事情
		如：移除自身
		"""
		for talentID in entity.talentSpells:
			if talentID == self.id:
				entity.talentSpells.remove(talentID)
				self.remove(entity)

