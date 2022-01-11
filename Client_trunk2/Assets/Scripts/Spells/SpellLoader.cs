using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellLoader
{
    static SpellLoader s_instance;

    public static SpellLoader instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new SpellLoader();
            }
            return s_instance;
        }
    }

    public void init()
    {
        s_instance.ScanEffect("Configs/SpellEffects/EffectsTable");
        s_instance.ScanBuff("Configs/Buffs");
        s_instance.ScanPassive("Configs/PassiveSkills");
        s_instance.ScanSpell("Configs/Spells");
        s_instance.InitAll();
    }

    private Dictionary<int, SPELL.Spell> m_spells;          //技能
    private Dictionary<int, SPELL.SpellEffect> m_effects;   //效果和buff
    private Dictionary<int, SPELL.PassiveSkill> m_passive;  //被动技能

    private Dictionary<int, DataSection.DataSection> m_spellsDataSection;  //技能数据
    private Dictionary<int, DataSection.DataSection> m_effectsDataSection; //效果和buff数据
    private Dictionary<int, DataSection.DataSection> m_passiveDataSection; //被动技能数据

    public SpellLoader()
    {
        if (s_instance != null)
            throw new System.Exception("SpellLoader instance reduplicate!!!");
    }

    public void InitAll()
    {
        //初始化效果和buff
        if (m_effectsDataSection == null)
            throw new System.Exception("SpellLoader::InitAll(), ScanEffect error!!!");

        if (m_effects == null)
            m_effects = new Dictionary<int, SPELL.SpellEffect>();

        foreach (var dataSection in m_effectsDataSection.Values)
        {
            var className = dataSection.readString("className");
            var effect = SPELL.SpellEffect.CreateSpellEffect(className);
            //临时写法，服务器效果类型未完全移植到客户端，所以这里暂时允许为空
            if (effect == null)
                continue;
            effect.Init(dataSection);
            m_effects[effect.id] = effect;
        }

        //初始化技能
        if (m_spellsDataSection == null)
            throw new System.Exception("SpellLoader::InitAll(), ScanSpell error!!!");

        if (m_spells == null)
            m_spells = new Dictionary<int, SPELL.Spell>();

        foreach (var dataSection in m_spellsDataSection.Values)
        {
            var className = dataSection.readString("className");
            var spell = SPELL.Spell.CreateSpell(className);
            spell.Init(dataSection);
            m_spells[spell.id] = spell;
        }

        //初始化被动技能
        if (m_passiveDataSection == null)
            throw new System.Exception("SpellLoader::InitAll(), ScanSpell error!!!");

        if (m_passive == null)
            m_passive = new Dictionary<int, SPELL.PassiveSkill>();

        foreach (var dataSection in m_passiveDataSection.Values)
        {
            var className = dataSection.readString("className");
            var passive = SPELL.PassiveSkill.CreatePassiveSkill(className);
            passive.Init(dataSection);
            m_passive[passive.id] = passive;
        }
    }

    #region Scan Function 
    public void ScanEffect(string asset)
    {
        if (m_effectsDataSection == null)
            m_effectsDataSection = new Dictionary<int, DataSection.DataSection>();

        DataSection.TabTableSection tableSection = DataSection.TabTableLoader.loadFile(asset);

        foreach (DataSection.TabTableSection dataSection in tableSection.values())
        {
            m_effectsDataSection[dataSection.readInt("id")] = dataSection;
        }
    }

    public void ScanBuff(string asset)
    {
        if (m_effectsDataSection == null)
            m_effectsDataSection = new Dictionary<int, DataSection.DataSection>();

        var objs = Resources.LoadAll(asset);
        var xmlParser = new DataSection.XMLParser();
        foreach (var text in objs)
        {
            var dataSection = xmlParser.loadXML(text.ToString());
            m_effectsDataSection[dataSection.readInt("id")] = dataSection;
        }
    }

    public void ScanPassive(string asset)
    {
        if (m_passiveDataSection != null)
            throw new System.Exception("SpellLoader::InitSpell(), Don't re-init.");

        m_passiveDataSection = new Dictionary<int, DataSection.DataSection>();

        var objs = Resources.LoadAll(asset);
        var xmlParser = new DataSection.XMLParser();
        foreach (var text in objs)
        {
            var dataSection = xmlParser.loadXML(text.ToString());
            m_passiveDataSection[dataSection.readInt("id")] = dataSection;
        }
    }

    public void ScanSpell(string asset)
    {
        if (m_spellsDataSection != null)
            throw new System.Exception("SpellLoader::InitSpell(), Don't re-init.");

        m_spellsDataSection = new Dictionary<int, DataSection.DataSection>();

        var objs = Resources.LoadAll(asset);
        var xmlParser = new DataSection.XMLParser();
        foreach (var text in objs)
        {
            var dataSection = xmlParser.loadXML(text.ToString());
            m_spellsDataSection[dataSection.readInt("id")] = dataSection;
        }
    }
    #endregion Scan Function 

    #region Get Function 
    public SPELL.SpellEffect GetEffect(int id)
    {
        if (m_effects.ContainsKey(id))
        {
            return m_effects[id];
        }
        else
        {
            var dataSection = m_effectsDataSection[id];
            var className = dataSection.readString("className");
            var effect = SPELL.SpellEffect.CreateSpellEffect(className);
            if (effect == null)
            {
                //临时写法，服务器效果类型未完全移植到客户端，所以这里暂时允许为空
                return null;
                //throw new System.Exception(string.Format("effect '{0}' is Error!", id));
            }
            effect.Init(dataSection);
            m_effects[id] = effect;
            return m_effects[id];
        }
    }

    public SPELL.Spell GetSpell(int id)
    {
        if (m_spells.ContainsKey(id))
        {
            return m_spells[id];
        }
        else
        {
            var dataSection = m_spellsDataSection[id];
            var className = dataSection.readString("className");
            var spell = SPELL.Spell.CreateSpell(className);
            if (spell == null)
            {
                throw new System.Exception(string.Format("get spell '{0}' is Error!", id));
            }
            spell.Init(dataSection);
            m_spells[id] = spell;
            return m_spells[id];
        }
    }

    public SPELL.PassiveSkill GetPassive(int id)
    {
        if (m_passive.ContainsKey(id))
        {
            return m_passive[id];
        }
        else
        {
            var dataSection = m_passiveDataSection[id];
            var className = dataSection.readString("className");
            var passive = SPELL.PassiveSkill.CreatePassiveSkill(className);
            if (passive == null)
            {
                throw new System.Exception(string.Format("get passive '{0}' is Error!", id));
            }
            passive.Init(dataSection);
            m_passive[id] = passive;
            return m_passive[id];
        }
    }
    #endregion Get Function 

}