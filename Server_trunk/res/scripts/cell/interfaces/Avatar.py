# -*- coding: utf-8 -*-
import KBEngine
from KBEDebug import *

import time
import Math
import math
import Vector
import json
from Extra import ECBExtend
from SrvDef import eObjectType
import GloballyDefine as GD
import MapDataConfig

from SrvDef import eEntityStatus, eEffectStatus, eActionRestrict, Status2Action, EntityStatus2Action, eObjectType, eCampRelationship, eEntityEvent, eBuffSyncType
from Spell.SpellLoader import g_spellLoader
from Spell.Spell import SpellTargetData
from CooldownMgrDataType import CooldownDataType

from Spell.SpellDef import SpellStatus, EffectStatus2InterruptCode, ActionRestrict2InterruptCode
class Avatar:
	"""
	"""
	_validRelationType = set( [eObjectType.Player, eObjectType.Monster] )

	def __init__(self):
		"""
		"""
		# 初始化状态
		self.effectStatusCounter = [0] * eEffectStatus.Max
		self.actionRestrictCounter = [0] * eActionRestrict.Max

	def actionRestrictCounterIncr( self, flag ):
		"""
		记数器加一
		@param flag: eActionRestrict下定义的值之一
		"""
		self.actionRestrictCounter[flag] += 1
		if self.actionRestrictCounter[flag] == 1:
			self.actionRestrict |= ( 1 << flag )
			self.onActionRestrictChanged( flag )

	def actionRestrictCounterDecr( self, flag ):
		"""
		记数器减一
		@param flag: eActionRestrict下定义的值之一
		"""
		self.actionRestrictCounter[flag] -= 1
		if self.actionRestrictCounter[flag] == 0:
			self.actionRestrict &= ~( 1 << flag )
			try:
				self.onActionRestrictChanged( flag )
			except Exception as err:
				EXCEHOOK_MSG( err )

	def hasActionRestrict( self, flag ):
		"""
		当前某个行为是否被限制了
		"""
		return self.actionRestrictCounter[flag] > 0

	def onActionRestrictChanged( self, flag ):
		"""
		template method.
		当某个行为限制状态改变时被调用
		
		@param flag: eActionRestrict.*；表示当前有变化的状态，可以通过检查当前状态来确定旧的状态
		"""
		if self.hasActionRestrict( flag ):
			# 当前获得了这个标志
			code = ActionRestrict2InterruptCode[1].get( flag, 0 )
			if flag == eActionRestrict.ForbidMove:
				self.cancelController( "Movement" )
			if flag == eActionRestrict.ForbidSpell:
				# 打断当前施法
				if self.currentSpell is not None:
					self.currentSpell.stop( self )

		else:
			# 当前失去了这个标志
			code = ActionRestrict2InterruptCode[0].get( flag, 0 )
		
		if code > 0:
			self.buffMgr.interruptBuff( self, code )

	def effectStatusCounterIncr( self, flag ):
		"""
		记数器加一
		@param flag: eEffectStatus下定义的值之一
		"""
		self.effectStatusCounter[flag] += 1
		if self.effectStatusCounter[flag] == 1:
			self.effectStatus |= ( 1 << flag )
			try:
				self.onEffectStatusChanged( flag )
			except Exception as err:
				EXCEHOOK_MSG( err )
				
			# 状态的改变导致行为的改变
			for s in Status2Action[flag]:
				self.actionRestrictCounterIncr( s )

	def effectStatusCounterDecr( self, flag ):
		"""
		记数器减一
		@param flag: eEffectStatus下定义的值之一
		"""
		self.effectStatusCounter[flag] -= 1
		if self.effectStatusCounter[flag] == 0:
			self.effectStatus &= ~( 1 << flag )
			try:
				self.onEffectStatusChanged( flag )
			except Exception as err:
				EXCEHOOK_MSG( err )

			# 状态的改变导致行为的改变
			for s in Status2Action[flag]:
				self.actionRestrictCounterDecr( s )
	
	def hasEffectStatus( self, flag ):
		"""
		当前是否获得了某个状态
		"""
		return self.effectStatusCounter[flag] > 0

	def onEffectStatusChanged( self, flag ):
		"""
		template method.
		当某个效果状态改变时被调用
		
		@param flag: eEffectStatus.*；表示当前有变化的状态，可以通过检查当前状态来确定旧的状态
		"""
		if self.hasEffectStatus( flag ):
			# 当前获得了状态
			code = EffectStatus2InterruptCode[1].get( flag, 0 )
		else:
			# 当前失去了状态
			code = EffectStatus2InterruptCode[0].get( flag, 0 )

		if code > 0:
			self.buffMgr.interruptBuff( self, code )

	def isEntityStatus( self, flag ):
		"""
		判断是否处于某种状态中
		"""
		return self.status == flag
	
	def changeEntityStatus( self, flag ):
		"""
		@param flag: see also eEntityStatus
		"""
		# 没有什么理由在状态一致的时候还需要调这个接口
		assert flag != self.status
		
		assert flag >= 0 and flag < eEntityStatus.Max

		old = self.status
		self.status = flag
		
		# 状态的改变导致行为的改变：恢复旧的状态所影响的行为
		for s in EntityStatus2Action[old]:
			self.actionRestrictCounterDecr( s )
			
		# 状态的改变导致行为的改变：根据新的状态重新设置所影响的行为
		for s in EntityStatus2Action[flag]:
			self.actionRestrictCounterIncr( s )

		try:
			self.onEntityStatusChanged( old, flag )
		except Exception as err:
			EXCEHOOK_MSG( err )

	def onEntityStatusChanged( self, old, new ):
		"""
		template method.
		"""
		pass
	
	def getSpaceConfig_(self):
		"""
		"""
		return MapDataConfig.get( KBEngine.getSpaceData( self.spaceID, GD.SPACEDATA_SPACE_IDENT ) )

	def getCurrentSpaceData( self, key ):
		"""
		see also function KBEngine.getSpaceDataFirstForKey() from KBEngine cellApp python API;
		Gets space data for a key in the space which entity live in. Only appropriate for keys that should have only one value.

		@param key: the key to get data for; KEY_SPACEDATA_*
		@return: the value of the first entry for the key
		"""
		return KBEngine.getSpaceData( self.spaceID, key )
	
	@staticmethod
	def findSpaceDomain( domainName ):
		"""
		查找指定的地图领域管理器
		"""
		try:
			key = GD.GLOBALDATAPREFIX_SPACE_DOMAIN + domainName
			spaceDM = KBEngine.globalData[key]
			return spaceDM
		except KeyError:
			return None
	
	def subAttribute(self, config):
		self.HPMax_base = self.HPMax_base - config.get("HPMax_base", 0)
		self.MPMax_base = self.MPMax_base - config.get("MPMax_base", 0)
		self.moveSpeed_base = self.moveSpeed_base - config.get("moveSpeed_base", 0)
		self.physicsATK_base = self.physicsATK_base - config.get("physicsATK_base", 0)
		self.physicsDEF_base = self.physicsDEF_base - config.get("physicsDEF_base", 0)
		self.magicATK_base = self.magicATK_base - config.get("magicATK_base", 0)
		self.magicDEF_base = self.magicDEF_base - config.get("magicDEF_base", 0)
		self.hitPoint_base = self.hitPoint_base - config.get("hitPoint_base", 0)
		self.dodgePoint_base = self.dodgePoint_base - config.get("dodgePoint_base", 0)
		self.critPoint_base = self.critPoint_base - config.get("critPoint_base", 0)
		self.toughPoint_base = self.toughPoint_base - config.get("toughPoint_base", 0)

	def addAttribute(self, config):
		self.HPMax_base = self.HPMax_base + config.get("HPMax_base", 0)
		self.MPMax_base = self.MPMax_base + config.get("MPMax_base", 0)
		self.moveSpeed_base = self.moveSpeed_base + config.get("moveSpeed_base", 0)
		self.physicsATK_base = self.physicsATK_base + config.get("physicsATK_base", 0)
		self.physicsDEF_base = self.physicsDEF_base + config.get("physicsDEF_base", 0)
		self.magicATK_base = self.magicATK_base + config.get("magicATK_base", 0)
		self.magicDEF_base = self.magicDEF_base + config.get("magicDEF_base", 0)
		self.hitPoint_base = self.hitPoint_base + config.get("hitPoint_base", 0)
		self.dodgePoint_base = self.dodgePoint_base + config.get("dodgePoint_base", 0)
		self.critPoint_base = self.critPoint_base + config.get("critPoint_base", 0)
		self.toughPoint_base = self.toughPoint_base + config.get("toughPoint_base", 0)

	def attributeOperation(self):
		"""
		战斗属性计算
		"""
		self.calcHPMax()
		self.calcMPMax()
		self.calcMoveSpeed()
		self.calcPhysicsATK()
		self.calcPhysicsDEF()
		self.calcMagicATK()
		self.calcMagicDEF()
		self.calcHitPoint()
		self.calcDodgePoint()
		self.calcCritPoint()
		self.calcToughPoint()

	def calcHPMax(self):
		"""
		计算HP最大值
		"""
		old = self.HPMax
		value = int(( self.HPMax_base + self.HPMax_added ) * ( 1.0 + self.HPMax_percent / GD.COEF_PERCENT ) + self.HPMax_appended)

		if value < 1:
			self.HPMax = 1
		else:
			self.HPMax = value
		
		if old != 0:
			self.HP = math.ceil((self.HP*self.HPMax)/old)

	def calcMPMax(self):
		"""
		计算MP最大值
		"""
		old = self.MPMax
		value = int(( self.MPMax_base + self.MPMax_added ) * ( 1.0 + self.MPMax_percent / GD.COEF_PERCENT ) + self.MPMax_appended)

		if value < 1:
			self.MPMax = 1
		else:
			self.MPMax = value
		
		if old != 0:
			self.MP = math.ceil((self.MP*self.MPMax)/old)

	def calcMoveSpeed(self):
		"""
		计算移动速度
		"""
		value = ( ( self.moveSpeed_base + self.moveSpeed_added ) * ( 1.0 + self.moveSpeed_percent / GD.COEF_PERCENT ) + self.moveSpeed_appended ) / GD.COEF_PERCENT

		if value < 0:
			self.moveSpeed = float(0)
		else:
			self.moveSpeed = value

	def calcPhysicsATK(self):
		"""
		计算物理攻击力
		"""
		value = int(( self.physicsATK_base + self.physicsATK_added ) * ( 1.0 + self.physicsATK_percent / GD.COEF_PERCENT ) + self.physicsATK_appended)

		if value < 0:
			self.physicsATK = 0
		else:
			self.physicsATK = value

	def calcPhysicsDEF(self):
		"""
		计算物理防御力
		"""
		value = int(( self.physicsDEF_base + self.physicsDEF_added ) * ( 1.0 + self.physicsDEF_percent / GD.COEF_PERCENT ) + self.physicsDEF_appended)

		if value < 0:
			self.physicsDEF = 0
		else:
			self.physicsDEF = value

	def calcMagicATK(self):
		"""
		计算法术攻击力
		"""
		value = int(( self.magicATK_base + self.magicATK_added ) * ( 1.0 + self.magicATK_percent / GD.COEF_PERCENT ) + self.magicATK_appended)

		if value < 0:
			self.magicATK = 0
		else:
			self.magicATK = value

	def calcMagicDEF(self):
		"""
		计算法术防御力
		"""
		value = int(( self.magicDEF_base + self.magicDEF_added ) * ( 1.0 + self.magicDEF_percent / GD.COEF_PERCENT ) + self.magicDEF_appended)

		if value < 0:
			self.magicDEF = 0
		else:
			self.magicDEF = value

	def calcHitPoint(self):
		"""
		计算命中点
		"""
		value = int(( self.hitPoint_base + self.hitPoint_added ) * ( 1.0 + self.hitPoint_percent / GD.COEF_PERCENT ) + self.hitPoint_appended)

		if value < 0:
			self.hitPoint = 0
		else:
			self.hitPoint = value

	def calcDodgePoint(self):
		"""
		计算闪避点
		"""
		value = int(( self.dodgePoint_base + self.dodgePoint_added ) * ( 1.0 + self.dodgePoint_percent / GD.COEF_PERCENT ) + self.dodgePoint_appended)

		if value < 0:
			self.dodgePoint = 0
		else:
			self.dodgePoint = value

	def calcCritPoint(self):
		"""
		计算爆击点
		"""
		value = int(( self.critPoint_base + self.critPoint_added ) * ( 1.0 + self.critPoint_percent / GD.COEF_PERCENT ) + self.critPoint_appended)

		if value < 0:
			self.critPoint = 0
		else:
			self.critPoint = value

	def calcToughPoint(self):
		"""
		计算韧性点
		"""
		value = int(( self.toughPoint_base + self.toughPoint_added ) * ( 1.0 + self.toughPoint_percent / GD.COEF_PERCENT ) + self.toughPoint_appended)

		if value < 0:
			self.toughPoint = 0
		else:
			self.toughPoint = value






	def requestCastSpell(self, entityID, spellID):
		"""
		Exposed method.
		无目标、无对象施法
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		spell = g_spellLoader.getSpell( spellID )
		if spell is None:
			ERROR_MSG( "%s(%i): invalid spell id '%s'" % (self.name, self.id, spellID) )
			return

		targetData = SpellTargetData()
		result = spell.cast( self, targetData )
		#DEBUG_MSG( "entity '%s' cast spell '%s', status = '%s'" % ( entityID, spellID, result ) )

	def requestCastSpellSyncDir(self, entityID, spellID, direction):
		"""
		Exposed method.
		无目标、无对象施法(同步方向)
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		spell = g_spellLoader.getSpell( spellID )
		if spell is None:
			ERROR_MSG( "%s(%i): invalid spell id '%s'" % (self.name, self.id, spellID) )
			return

		self.setDirectionForOthers(direction)
		targetData = SpellTargetData()
		result = spell.cast( self, targetData )
		#DEBUG_MSG( "entity '%s' cast spell '%s', status = '%s'" % ( entityID, spellID, result ) )

	def requestCastSpellToEntity(self, entityID, spellID, dstEntityID):
		"""
		Exposed method.
		对目标对象施法
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		spell = g_spellLoader.getSpell( spellID )
		if spell is None:
			ERROR_MSG( "%s(%i): invalid spell id '%s'" % (self.name, self.id, spellID) )
			return

		targetData = SpellTargetData()
		targetData.gameObject = KBEngine.entities.get(dstEntityID, None)
		result = spell.cast( self, targetData )
		#DEBUG_MSG( "entity '%s' cast spell '%s', status = '%s'" % ( entityID, spellID, result ) )

	def requestCastSpellToPos(self, entityID, spellID, position):
		"""
		Exposed method.
		对某位置施法
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		spell = g_spellLoader.getSpell( spellID )
		if spell is None:
			ERROR_MSG( "%s(%i): invalid spell id '%s'" % (self.name, self.id, spellID) )
			return

		targetData = SpellTargetData()
		targetData.posOrDir = position
		if self.objectType == eObjectType.Player:
			self.castPosition = position
		result = spell.cast( self, targetData )
		#DEBUG_MSG( "entity '%s' cast spell '%s', status = '%s'" % ( entityID, spellID, result ) )

	def requestCastSpellToDir(self, entityID, spellID, direction):
		"""
		Exposed method.
		对某方向施法
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		spell = g_spellLoader.getSpell( spellID )
		if spell is None:
			ERROR_MSG( "%s(%i): invalid spell id '%s'" % (self.name, self.id, spellID) )
			return

		targetData = SpellTargetData()
		targetData.posOrDir = direction
		if self.objectType == eObjectType.Player:
			self.castDirection = direction
		result = spell.cast( self, targetData )
		#DEBUG_MSG( "entity '%s' cast spell '%s', status = '%s'" % ( entityID, spellID, result ) )

	def requestCastSpellToPosDir(self, entityID, spellID, position, direction):
		"""
		Exposed method.
		从指定位置向指定方向施法
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		spell = g_spellLoader.getSpell( spellID )
		if spell is None:
			ERROR_MSG( "%s(%i): invalid spell id '%s'" % (self.name, self.id, spellID) )
			return

		targetData = SpellTargetData()
		if self.objectType == eObjectType.Player:
			self.castPosition = position
			self.castDirection = direction
		result = spell.cast( self, targetData )
		#DEBUG_MSG( "entity '%s' cast spell '%s', status = '%s'" % ( entityID, spellID, result ) )

	def requestTriggerSpellEffect(self, entityID, spellID):
		"""
		Exposed method.
		请求触发技能效果；
		这是由施法的客户端判断已“击中”目标而发出的结算请求
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		if self.currentSpell is None:
			ERROR_MSG( "No spell casting current! entityID: %s, spellID: %s" % (entityID, spellID) )
			return

		if self.currentSpell.spellID != spellID:
			ERROR_MSG( "current casting spell not match! entityID: %s, spellID: %s, current: %s" % (entityID, spellID, self.currentSpell.spellID) )
			return

		spell = g_spellLoader.getSpell( self.currentSpell.spellID )
		spell.requestTriggerEffect( self )

	def requestTriggerEnd(self, entityID, spellID):
		"""
		Exposed method.
		请求技能結束；
		这是由施法的客户端判断已“結束而发出的结算请求
		"""
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		if self.currentSpell is None:
			ERROR_MSG( "No spell casting current! entityID: %s, spellID: %s" % (entityID, spellID) )
			return

		if self.currentSpell.spellID != spellID:
			ERROR_MSG( "current casting spell not match! entityID: %s, spellID: %s, current: %s" % (entityID, spellID, self.currentSpell.spellID) )
			return

		spell = g_spellLoader.getSpell( self.currentSpell.spellID )
		spell.requestTriggerEnd( self )

	def requestDetachBuff(self, entityID, buffIndex):
		"""
		Exposed method.
		请求拿下buff
		"""
		buffData = self.buffMgr.getByIndex(buffIndex)
		if buffData is None:
			return

		buff = g_spellLoader.getBuff( buffData.buffID )
		buff.detach(self, buffData)

	def requestBulletTriggerEffect(self, entityID, buffIndex, dstID):
		"""
		Exposed method.
		请求触发子弹作用效果；
		这是由施法裁判的客户端判断子弹碰撞到了满足条件的目标而发出的请求
		"""
		buffData = self.buffMgr.getByIndex(buffIndex)

		if buffData is None:
			return
			
		if entityID != buffData.misc["judge"]:
			ERROR_MSG( "The caller '%s' must be judge '%s'" % (entityID, self.id) )
			return
			
		# 找到buff的施法者
		caster = KBEngine.entities.get(buffData.casterID,None)
		if caster is not None:
			buff = g_spellLoader.getBuff( buffData.buffID )
			buff.requestBulletTriggerEffect( caster, dstID, buffData )

	def requestBuffFunc(self, entityID, buffIndex, funcName, str):
		"""
		Exposed method
		请求执行技能方法
		"""
		buffData = self.buffMgr.getByIndex(buffIndex)

		if buffData is None:
			return

		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		strList = json.loads(str)
		strList.append(buffData)
		buff = g_spellLoader.getBuff( buffData.buffID )
		func = getattr(buff, funcName)
		func(*strList)

	def requestSpellFunc(self, entityID, spellID, funcName, str):
		"""
		Exposed method
		请求执行技能方法
		"""	
		if entityID != self.id:
			ERROR_MSG( "The caller '%s' must be self '%s'" % (entityID, self.id) )
			return

		strList = json.loads(str)
		spell = g_spellLoader.getSpell( spellID )
		func = getattr(spell, funcName)
		func(*strList)

	def spellFunc(self, spellID, funcName, *args):
		"""
		技能分发
		"""
		spell = g_spellLoader.getSpell( spellID )
		func = getattr(spell, funcName)
		func(*args)

	def changeHP( self, value ):
		"""
		改变HP的值，正为加，负为减
		"""
		old = self.HP;
		maxHP = self.HPMax
		newHP = old + int(value)
		if newHP < 0:
			newHP = 0
		elif (newHP > maxHP):
			newHP = maxHP
		self.HP = newHP

	def changeMP(self, value):
		"""
		改变MP的值，正为加，负为减
		"""
		old = self.MP
		maxMP = self.MPMax
		newMP = old + int(value);
		if newMP < 0:
			newMP = 0
		elif newMP > maxMP:
			newMP = maxMP
		self.MP = newMP

	def receiveDamage( self, attacker, result ):
		"""
		virtual method.
		接收伤害
		@param attacker: Entity; 攻击者
		@param damage: int; 伤害值
		@return:int; 目标受到的真实伤害
		"""
		# 没有什么理由在死的时候还会受到伤害
		assert self.status != eEntityStatus.Death

		# 处于未决状态时，不受到伤害
		if self.status == eEntityStatus.Pending:
			return

		currentHP = self.HP
		self.changeHP( -result.damage )
		self.onReceiveDamage( attacker, result )

		if self.HP <= 0:
			self.changeEntityStatus( eEntityStatus.Death )
			self.onDead( attacker )
			attacker.onKilled( self )
			return currentHP
		return result.damage

	def onReceiveDamage( self, attacker, damage ):
		"""
		virtual method.
		受到伤害的触发：在减血以后，死亡之前触发
		"""
		pass

	def onDead( self, killer ):
		"""
		virtual method.
		我被杀死了
		
		@param killer: Entity; 凶手
		"""
		for buffData in list(self.buffMgr.getAllBuffs()):
			buffData.buff.interruptOnDead(self, buffData)

	def onKilled( self, victim ):
		"""
		virtual method.
		我杀死了谁
		
		@param victim: 受害者
		"""
		self.fireEvent(eEntityEvent.OnKilled, victim)

	def isDead( self ):
		"""
		判断是否已死亡
		"""
		return self.status == eEntityStatus.Death

	def onTimer_buffTick( self, timerID, userArg ):
		"""
		处理buff心跳;
		"""
		buffData = self.buffMgr.getByTimerID( timerID )
		buffData.buff.tick( self, buffData )

	def setCooldown(self, cooldownID, duration):
		"""
		@param cooldownID: int; 需要设置的cooldown编号
		@param duration: float; 持续时间
		"""
		cooldown = self.cooldownMgr.datas.get(cooldownID)
		if cooldown is None:
			cooldown = CooldownDataType(cooldownID)
		cooldown.beginTime = time.time()
		cooldown.endTime = cooldown.beginTime + duration
		self.cooldownMgr.datas[cooldownID] = cooldown

		if self.objectType == eObjectType.Player:
			self.client.updateCooldownFS( cooldown.cooldownID, cooldown.beginTime, cooldown.endTime )

	def cooldownIsOut(self, cooldownID):
		"""
		判断指定的cooldown是否已过期
		@return: bool
		"""
		cooldown = self.cooldownMgr.datas.get(cooldownID)
		if cooldown is None:
			return True
		return cooldown.endTime <= time.time()

	def onTimer_spellCasting( self, timerID, userArg ):
		"""
		技能施展定时回调
		"""
		if self.currentSpell == None:
			self.delTimer( timerID )
			return

		try:
			self.currentSpell.onTimer( self, timerID, userArg )
		except:
			self.setCurrentSpellDataType( None )
			EXCEHOOK_MSG( "spell cast except!" )

	def triggerFightEvent(self, eventType, src, dst, spell):
		"""
		事件触发接口，表示有谁触发了事件
		@param eventType: 来自eFightEvent的事件定义类型
		@param src: Entity; 攻击者
		@param dst: Entity; 受击者
		@param spell: SpellEx; 产生这个事件的技能实例
		@param fightData: instance of FightResult；通过修改这个参数里的值以达到改变战斗结果的目的
		"""
		for key in list(self.passiveSkillBag.datas.keys()):
			self.passiveSkillBag.datas[key].trigger( eventType, src, dst, spell )		

	def requestInitBuffs( self, entityID ):
		"""
		Exposed method.
		客户端entityID请求获取所有的buff
		"""
		entity = KBEngine.entities[entityID]
		if self.id == entityID:
			for buffData in self.buffMgr.getAllBuffs():
				if buffData.buff.syncType == eBuffSyncType.Sync:
					entity.client.updateBuffFS( buffData )
		else:
			for buffData in self.buffMgr.getAllBuffs():
				if buffData.buff.syncType == eBuffSyncType.Sync:
					entity.clientEntity( self.id ).updateBuffFS( buffData )

	def addBuff( self, buffData ):
		"""
		増加一个buff
		@param buffData: BuffDataType
		"""
		index = self.buffMgr.addBuff( buffData )
		self.allClients.updateBuffFS( buffData )  # 通知周围的玩家
		return index

	def buffSpecificAction( self, buffData):
		"""
		执行buff的特殊作用效果
		"""
		self.allClients.buffSpecificActionFS(buffData)  # 通知周围的玩家

	def removeBuff( self, buffData ):
		"""
		删除一个buff
		@param buffData: BuffDataType
		"""
		self.buffMgr.removeBuff( buffData )
		self.allClients.removeBuffFS( buffData.index )  # 通知周围的玩家

	def changeBuffTimerID( self, old, new ):
		"""
		改变某个buff的timerID
		"""
		self.buffMgr.changeBuffTimerID( old, new )

	def setCurrentSpellDataType( self, spllDataType ):
		"""
		设置当前施展的技能的数据存储实例
		"""
		flag = 0
		if self.currentSpell is not None:
			flag -= 1
		
		self.currentSpell = spllDataType
		if spllDataType is not None:
			flag += 1

		if flag > 0:
			self.effectStatusCounterIncr( eEffectStatus.SpellCasting )
		elif flag < 0:
			self.effectStatusCounterDecr( eEffectStatus.SpellCasting )

	def checkRelation( self, entityB ):
		"""
		判断两个entity之间的关系（无关系、友好的、敌对的、中立的）
		"""
		# 自己与自己为友好关系
		if self is entityB:
			return eCampRelationship.Friendly

		if self.objectType not in self._validRelationType or entityB.objectType not in self._validRelationType:
			return eCampRelationship.Irrelative
		
		
		# 检查地图相关的阵营关系
		sceneCamp = self.getSpaceConfig_().campMap
		try:
			relation = sceneCamp[self.sceneCampID][entityB.sceneCampID]
			if relation == 0:
				return eCampRelationship.Friendly
			elif relation == 1:
				return eCampRelationship.Hostile
			else:
				return eCampRelationship.neutrality
		except:
			# 默认关系是“无关系”
			return eCampRelationship.Irrelative

	def lookAt( self, position ):
		"""
		把脸朝向某个世界坐标
		"""
		dst = position - self.position
		dst.y = 0.0
		try:
			ang = Vector.angle( Vector.forward, dst )
		except ZeroDivisionError:
			return
			
		v = Vector.cross( Vector.forward, dst )
		if v[1] < 0:
			ang *= -1
		self.direction.z = ang

	def setFightStatus( self ):
		"""
		virtual method.
		切换到战斗状态或者重置战斗状态时间
		"""
		pass

	def notifyClientDisplayDamage( self, attacker, result ):
		"""
		virtual method.
		通知客户端掉血显示
		"""
		pass


# 注册timer等回调处理接口
ECBExtend.register_entity_callback(	ECBExtend.TIMER_ON_BUFFTICK, Avatar.onTimer_buffTick )
ECBExtend.register_entity_callback( ECBExtend.TIMER_ON_SPELL_CASTING, Avatar.onTimer_spellCasting)

