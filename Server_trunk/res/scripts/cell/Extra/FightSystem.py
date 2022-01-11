# -*- coding: utf-8 -*-
#
"""
"""
import random

from KBEDebug import *
import GloballyDefine as GD

from SrvDef import eEntityStatus
from SrvDef import eFightEvent

class FightResult(object):
	"""
	"""
	def __init__( self ):
		"""
		"""
		self.hit = False        # 是否命中敌人
		self.crit = False       # 是否打出了爆击
		self.parry = False      # 目标是否招架住了
		self.shielded = False   # 护盾是否生效
		self.damageType = 0     # 伤害类型
		self.damage = 0         # 最终产生的伤害值
		self.suckBlood = 0      # 攻击吸血
		self.realDamage = 0     # 真实产生的伤害值（当目标剩余血量小于最终产生的伤害值时，该值会小于最终产生的伤害值）

class FightSystem(object):
	"""
	战斗系统结算代码
	"""
	@classmethod
	def damageType2ATK(SELF, damageType, avatar):
		"""
		根据伤害类型（damageType）获取avatar的最终攻击力
		"""
		if damageType == GD.DamageType.Physics:
			return avatar.physicsATK
		else:
			return avatar.magicATK
	
	@classmethod
	def damageType2DEF(SELF, damageType, avatar):
		"""
		根据提供的伤害类型（damageType）获取avatar的对应防御值
		"""
		if damageType == GD.DamageType.Physics:
			return avatar.physicsDEF
		else:
			return avatar.magicDEF

	@classmethod
	def hitJudgment( SELF, srcEntity, dstEntity, spell ):
		"""
		@param srcEntity: Entity; 攻击者
		@param dstEntity: Entity; 受术者
		@param spell: instance SpellEx; 使用的什么技能打的
		"""
		result = srcEntity.result

		"""
		◎	命中率 = 攻击者命中/（攻击者命中 + 受术者闪避）
		"""		
		hitRate = srcEntity.hitPoint / ( srcEntity.hitPoint + dstEntity.dodgePoint)

		if hitRate < random.random():
			result.hit = False
			# 向客户端广播闪避消息
			dstEntity.notifyClientDisplayDamage( srcEntity, result )
			return

		result.hit = True

	@classmethod
	def fight( SELF, srcEntity, dstEntity, spell, damageType, damagePercent, damageValue ):
		"""
		@param srcEntity: Entity; 攻击者
		@param dstEntity: Entity; 受术者
		@param spell: instance SpellEx; 使用的什么技能打的
		@param damageType: value of GloballyDefine.DmageType -> Physics or Magic；表示伤害类型
		@param damagePercent：float; 伤害效果加成
		@param damageValue: int; 修正值修正
		"""
		result = srcEntity.result
		# 进来这里就算是命中
		result.hit = True
		result.damageType = damageType

		"""
		◎	伤害＝（攻击者(对应)攻击力终值×伤害效果加成值＋伤害效果附加值）
		"""
		srcAttack = SELF.damageType2ATK(damageType, srcEntity)
		result.damage = (srcAttack * damagePercent + damageValue)
		
		"""
		◎	暴击率=攻击者暴击值/（攻击者暴击值+受术者韧性值）
		◎	若暴击成功，则：技能伤害×＝基准暴击倍率，并进行暴击表现反馈
		"""
		critRate = srcEntity.critPoint / (srcEntity.critPoint + dstEntity.toughPoint)
		if critRate >= random.random():
			result.crit = True
			srcEntity.triggerFightEvent( eFightEvent.Crit, srcEntity, dstEntity, spell )
			dstEntity.triggerFightEvent( eFightEvent.CritBy, dstEntity, srcEntity, spell )
			result.damage *= GD.COEF_CRIT
		else:
			result.crit = False	

		"""
		○	招架率＝受击者招架点终值÷(受击者招架点终值×(1﹣基准招架倍率)＋受击者等级标准招架参数)
		◎	若招架成功，则：技能伤害×＝基准招架倍率，并进行招架表现反馈
		"""
		"""
		parryRate = dstEntity.parryPoint / ( dstEntity.parryPoint * ( 1 - GD.COEF_PARRY) + dstAmend.parry )
		if parryRate >= random.random():
			result.parry = True
			dstEntity.triggerFightEvent( eFightEvent.Parried, dstEntity, srcEntity, spell )
			srcEntity.triggerFightEvent( eFightEvent.ParriedBy, srcEntity, dstEntity, spell )
			result.damage *= GD.COEF_PARRY
		else:
			result.parry = False

		result.damage = int( result.damage )
		"""
		"""
		◎	护甲伤害减免率＝受术者防御力/（攻击者攻击力+受术者防御力)
		◎	技能伤害×＝(1 - 护甲伤害减免率)
		"""
		dstDefense = SELF.damageType2DEF(damageType, dstEntity)
		reduceRate = dstDefense / ( srcAttack + dstDefense )
		result.damage *= (1 - reduceRate)

		result.damage = int( result.damage )

		# 开始最终的结算
		result.realDamage = dstEntity.receiveDamage( srcEntity, result )
		
		# 触发对目标产生伤害的事件
		srcEntity.triggerFightEvent( eFightEvent.Damaged, srcEntity, dstEntity, spell )

		# 触发被伤害的事件
		dstEntity.triggerFightEvent( eFightEvent.DamagedBy, dstEntity, srcEntity, spell )
		
		if dstEntity.status == eEntityStatus.Death:
			# 告诉攻击者：你把谁搞死了
			srcEntity.triggerFightEvent( eFightEvent.Killed, srcEntity, dstEntity, spell )
			# 告诉受击者：你被谁搞死了
			dstEntity.triggerFightEvent( eFightEvent.KilledBy, dstEntity, srcEntity, spell )

			return

		elif result.suckBlood > 0:
			srcEntity.changeHP( result.suckBlood );  # 吸血处理

	@classmethod
	def healHP( SELF, srcEntity, dstEntity, healPercent, healValue ):
		"""
		治疗
		@param srcEntity: Entity; 攻击者
		@param dstEntity: Entity; 受术者
		@param healPercent：float; 伤害效果加成
		@param healValue: int; 修正值修正
		"""
		"""
		6.1.1	治疗初值计算（根据目标生命值上限进行治疗）
		○	读取作用目标属性：生命值上限终值
		○	读取技能属性：技能效果（治疗百分比、治疗固定值）
		○	治疗＝ 作用目标的生命值上限终值×治疗百分比＋治疗固定值
		"""
		value = dstEntity.HPMax * healPercent + healValue
		dstEntity.changeHP( value )

	@classmethod
	def healMP( SELF, srcEntity, dstEntity, healPercent, healValue ):
		"""
		MP回复
		@param srcEntity: Entity; 攻击者
		@param dstEntity: Entity; 受术者
		@param healPercent：float; 伤害效果加成
		@param healValue: int; 修正值修正
		"""
		value = dstEntity.MPMax * healPercent + healValue
		dstEntity.changeMP( value )
