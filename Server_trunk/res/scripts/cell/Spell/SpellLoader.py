# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *
from Singleton import Singleton

from PyDataSection import PyXMLSection
from PyDataSection import PyTabTableSection

class SpellLoader(Singleton):
	"""
	技能配置加载器
	"""
	def __init__(self):
		"""
		"""
		self._spells = {}         # 技能表
		self._effects = {}        # 效果表（这里的效果中包括buff和被动技能）
		self._talentSpells = {}	  # 天赋技能表
		self._spellIDSections = {}
		self._effectIDSections = {}
		self._talentSpellsIDSections = {}		

	def initAll( self ):
		"""
		初始化所有技能相关配置
		"""
		from .SpellExs import createSpell
		from .Effects import createEffect
		from .Talent import createTalent

		# 初始化效果，其中还包括buff和被动技能
		for root in self._effectIDSections.values():
			className = root.readString( "className" )
			effect = createEffect( className )
			if effect is None:
				ERROR_MSG( "Create effect '%s' id: <%s> fault! No such effect type!" % (className, root.readString( "id" )) )
				continue
			effect.init( root )
			self._effects[effect.id] = effect	
			
		# 初始化天赋技能
		for root in self._talentSpellsIDSections.values():
			className = root.readString( "className" )
			talentSpell = createEffect( className )
			if talentSpell is None:
				ERROR_MSG( "Create talentSpell '%s' id: <%s> fault! No such talentSpell type!" % (className, root.readString( "id" )) )
				continue
			talentSpell.init( root )
			self._effects[talentSpell.id] = talentSpell

		# 初始化技能
		for root in self._spellIDSections.values():
			className = root.readString( "className" )
			spell = createSpell( className )
			if spell is None:
				ERROR_MSG( "Create spell '%s' id: <%s> fault! No such spell type!" % (className, root.readString( "id" )) )
				continue
			spell.init( root )
			self._spells[spell.id] = spell

	def scanSpell( self, assetPath ):
		"""
		"""
		for f in KBEngine.listPathRes(assetPath, "xml"):
			#DEBUG_MSG( "Scanning spell '%s'" % f )
			root = PyXMLSection.parse( f )
			self._spellIDSections[root.readInt("id")] = root

	def scanEffect( self, assetPath ):
		"""
		"""
		for f in KBEngine.listPathRes(assetPath, "txt"):
			#DEBUG_MSG( "Scanning spell effect '%s'" % f )
			root = PyTabTableSection.parse( f, "utf-8" )
			for section in root.values():	
				self._effectIDSections[section.readInt("id")] = section

	def scanBuff( self, assetPath ):
		"""
		"""
		for f in KBEngine.listPathRes(assetPath, "xml"):
			#DEBUG_MSG( "Scanning spell buff '%s'" % f )
			root = PyXMLSection.parse( f )
			self._effectIDSections[root.readInt("id")] = root

	def scanPassiveSkill( self, assetPath ):
		"""
		"""
		for f in KBEngine.listPathRes(assetPath, "xml"):
			#DEBUG_MSG( "Scanning PassiveSkill '%s'" % f )
			root = PyXMLSection.parse( f )
			self._effectIDSections[root.readInt("id")] = root

	def scanTalentSpell( self, assetPath ):
		"""
		"""
		for f in KBEngine.listPathRes(assetPath, "xml"):
			#DEBUG_MSG( "Scanning TalentSpell '%s'" % f )
			root = PyXMLSection.parse( f )
			self._talentSpellsIDSections[root.readInt("id")] = root

	def loadSpell( self, spellID ):
		"""
		加载单个技能
		"""
		from .SpellExs import createSpell

		root = self._spellIDSections[spellID]
		className = root.readString("className")
		spell = createSpell( className )
		if spell is None:
			ERROR_MSG( "Create spell '%s' id: <%s> fault! No such spell type!" % (className, spellID) )
			return
		spell.init( root )
		self._spells[spell.id] = spell

	def loadEffect( self, effectID ):
		"""
		加载单个effect
		"""
		from .Effects import createEffect

		root = self._effectIDSections[effectID]
		className = root.readString("className")
		effect = createEffect( className )
		if effect is None:
			ERROR_MSG( "Create spell effect '%s' id: <%s> fault! No such effect type!" % (className, effectID) )
			return
		effect.init( root )
		self._effects[effect.id] = effect

	def loadBuff( self, buffID ):
		"""
		加载单个buff
		"""
		from .Effects import createEffect

		root = self._effectIDSections[buffID]
		className = root.readString("className")
		buff = createEffect( className )
		if buff is None:
			ERROR_MSG( "Create spell buff '%s' id: <%s> fault! No such buff type!" % (className, buffID) )
			return
		buff.init( root )
		self._effects[buff.id] = buff

	def loadPassiveSkill( self, passiveSkillID ):
		"""
		加载单个PassiveSkill
		"""
		from .Effects import createEffect

		root = self._passiveSkillIDSections[passiveSkillID]
		className = root.readString("className")
		passiveSkill = createEffect( className )
		if passiveSkill is None:
			ERROR_MSG( "Create passiveSkill '%s' id: <%s> fault! No such passiveSkill type!" % (className, passiveSkillID) )
			return
		passiveSkill.init( root )
		self._effects[passiveSkill.id] = passiveSkill

	def loadTalentSpell( self, talentSpellID ):
		"""
		加载单个天赋技能
		"""
		from .Talent import createTalent

		root = self._talentSpellsIDSections[talentSpellID]
		className = root.readString("className")
		talentSpellID = createTalent( className )
		if talentSpell is None:
			ERROR_MSG( "Create talentSpell '%s' id: <%s> fault! No such talentSpell type!" % (className, talentSpellID) )
			return
		talentSpell.init( root )
		self._talentSpells[talentSpell.id] = talentSpell


	def getSpell( self, spellID ):
		"""
		"""
		spell = self._spells.get( spellID )
		if spell is None:
			self.loadSpell( spellID )
			spell = self._spells.get( spellID )
			assert spell is not None, "The spellID '%s' is Error !" % spellID
		return spell

	def getBuff( self, buffID ):
		"""
		这里仅仅是为了提供一个获取buff的明确接口，与getEffect无区别。
		"""
		buff = self._effects.get( buffID )
		if buff is None:
			self.loadBuff( buffID )
			buff = self._effects.get( buffID )
			assert buff is not None, "The buffID '%s' is Error !" % buffID
		return buff
			
	def getEffect( self, effectID ):
		"""
		"""
		effect = self._effects.get( effectID )
		if effect is None:
			self.loadEffect( effectID )
			effect = self._effects.get( effectID )
			assert effect is not None, "The effectID '%s' is Error !" % effectID
		return effect

	def getPassiveSkill( self, skillID ):
		"""
		"""
		skill = self._effects.get( skillID )
		if skill is None:
			self.loadPassiveSkill( skillID )
			skill = self._effects.get( skillID )
			assert skill is not None, "The skillID '%s' is Error !" % skillID
		return skill

	def getTalentSpell( self, talentID ):
		
		talent = self._talentSpells.get( talentID )
		if talent is None:
			self.TalentSpell( talentID )
			talent = self._talentSpells.get( talentID )
			assert talent is not None, "The talentID '%s' is Error !" % talentID
		return talent

g_spellLoader = SpellLoader()