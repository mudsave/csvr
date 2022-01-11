# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from .SpellDef import SpellStatus
from SrvDef import eFightEvent

class SpellTargetData(object):
	"""
	施法目标数据存储
	"""
	def __init__( self ):
		"""
		"""
		self.gameObject = None           # Entity; 表示施法目标实例
		self.posOrDir = (0.0, 0.0, 0.0)  # Vector3; 表示施法目标位置或方向

class SpellBase(object):
	"""
	"""
	def __init__( self ):
		"""
		"""
		self.id = 0            # spell uid

	def init( self, dataSection ):
		"""
		@param DataSection: instance of PyDataSection
		@return: None
		"""
		self.id = dataSection.readInt( "id" )

class Spell(SpellBase):
	"""
	"""
	def init( self, dataSection ):
		"""
		"""
		SpellBase.init( self, dataSection )

		data = dataSection["generalPerformance"]
		self.name = data.readString( "name" )
		self.level = data.readInt( "level" )

	def isCasting( self, caster ):
		"""
		@param caster: instance of Entity;
		@return: bool; 表示caster是否正在释放技能
		"""
		assert False
	
	def cast( self, caster, targetData ):
		"""
		virtual method.
		开始施法
		@param caster: 施法者
		@param targeData: instance of SpellTargetData
		@return: value of SpellStatus
		"""
		return SpellStatus.OK



class SpellEffect(SpellBase):
	"""
	"""
	def cast(self, src, dst, spell):
		"""
		src向dst施放一个效果
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@param spell: instance of Spell; 表示是由哪个技能触发的效果
		@return: None
		"""
		pass

class SpellBuff(SpellEffect):
	"""
	buff基础类，仅声明基础的功能接口
	"""
	def init( self, dataSection ):
		"""
		"""
		SpellEffect.init( self, dataSection )

		data = dataSection["generalPerformance"]
		self.name = data.readString( "name" )
		self.level = data.readInt( "level" )

	def cast(self, src, dst, spell = None):
		"""
		virtual method.
		把buff效果作用在target身上，允许失败
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@return: None
		"""
		pass

	def attach(self, src, dst):
		"""
		virtual method.
		把buff附到owner身上
		"""
		pass

	def reattach(self, dst, buffData):
		"""
		virtual method.
		重新把buff作用到owner身上
		"""
		pass

	def detach(self, owner, buffData):
		"""
		virtual method.
		把buff从owner身上卸下来
		"""
		pass

class PassiveSkill(SpellEffect):
	"""
	被动技能基础类，仅声明基础的功能接口
	"""
	def init( self, dataSection ):
		"""
		"""
		SpellEffect.init( self, dataSection )

		data = dataSection["generalPerformance"]
		self.name = data.readString( "name" )
		self.level = data.readInt( "level" )

	def cast(self, src, dst, spell):
		"""
		virtual method.
		把被动技能作用在target身上，允许失败
		@param src: Entity; 施法者
		@param dst: Entity; 受术者
		@return: None
		"""
		dst.passiveSkillBag.addPassiveSkill(self)

	def trigger(self, eventType, responderEntity, thirdEntity, spell):
		"""
		virtual method.
		事件触发接口，表示有谁触发了事件
		@param eventType: 来自eFightEvent的事件定义类型
		@param responderEntity: Entity; 响应事件的人（有可能是攻击者，也有可能是受击者）
		@param thirdEntity: Entity; 与事件有关的第三方人士（有可能是攻击者，也有可能是受击者）
		@param spell: SpellEx; 产生这个事件的技能实例
		@param fightData: instance of FightResult；通过修改这个参数里的值以达到改变战斗结果的目的
		"""
		# 忽略不属于自己的事件
		if self.eventType != eventType:
			return
		self.onTrigger(responderEntity, thirdEntity, spell)
		
	def onTrigger(self, responderEntity, thirdEntity, spell):
		"""
		template method.
		@return: None
		"""
		pass

class TalentSpell(SpellBase):
	"""
	天赋技能基类
	"""	
	def init( self, dataSection ):
		"""
		"""
		SpellBase.init( self, dataSection )

		data = dataSection["generalPerformance"]
		self.name = data.readString( "name" )
		self.level = data.readInt( "level" )

	def add(self, entity):
		"""
		virtual method.
		给对象附加天赋技能前处理，如顶替掉其他天赋效果
		@param entity: Entity; 把天赋技能附加到此对象身上
		@return: None
		"""
		self.attach(entity)	

	def attach(self, entity):
		"""
		virtual method.
		给对象附加天赋技能
		@param src: Entity; 把天赋技能附加到此对象身上
		@return: None
		"""
		pass

	def remove(self, entity):
		"""
		virtual method.
		给天赋技能从对象身上移除
		@param src: Entity; 把天赋技能从此对象身上移除
		@return: None
		"""
		pass