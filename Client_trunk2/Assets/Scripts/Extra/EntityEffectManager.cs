using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RuleType
{
    MultipleShow = 0,    //可重复显示多个的特效,相当于可随意添加的特效规则
    OneShow = 1,         //仅显示一个的特效规则
    ReplaceShow = 2,     //互斥显示的特效规则，例如属性增益和减益特效，只会显示一个 
    CastShow = 3,        //技能施展特效，施法结束销毁
}

public class EntityEffectManager
{
    private GameObjComponent m_component;
    int m_effectID = 0;

    private Dictionary<int, EffectComponent> m_effectListMultiple = new Dictionary<int, EffectComponent>();          //用于保存MultipleShow类型的特效
    private Dictionary<string, EffectComponent> m_effectListOnce = new Dictionary<string, EffectComponent>();        //用于保存OneShow类型的特效
    private Dictionary<string, EffectComponent> m_effectListReplace = new Dictionary<string, EffectComponent>();     //用于保存ReplaceShow类型的特效
    private Dictionary<string, EffectComponent> m_effectListCast = new Dictionary<string, EffectComponent>();        //用于保存技能施展特效

    private Dictionary<string, EffectComponent> m_modelEffects = new Dictionary<string, EffectComponent>();          //用于保存模型特效

    private static Dictionary<int, System.Type> m_classTypeMap = new Dictionary<int, System.Type>()
    {
        {0, typeof(EffectComponent)},                //标准特效组件
        {1, typeof(FrozenEffectComponent)},          //冰冻模型特效组件
        {2, typeof(ModelLuminousEffectComponent) },  //外发光模型特效组件
        {3, typeof(WarpEffectComponent) },           //黑洞扭曲特效组件
        {4, typeof(DissolveEffectComponent) },       //溶解效果组件
        {5, typeof(ModelFractureEffectComponent) },  //模型破碎效果组件
    };

    public void init(GameObjComponent component)
    {
        m_component = component;
    }

    public void Destroy()
    {
        ClearAllEffects();
    }

    public void ClearAllEffects()
    {
        EffectListMultipleClear();
        EffectListOnceClear();
        EffectListReplaceClear();
        EffectListCastClear();
        ModelEffectsClear();

        m_effectID = 0;
    }

    private EffectComponent CreateEffectObject(CEffectConfig config)
    {
        EffectComponent eComponent = null;
        CEffectParameter effectParameter = new CEffectParameter(config);
        switch (config.type)
        {
            case 0:   //标准特效
                eComponent = CompentCreateManager.Instance.CreateEffectGameObject(effectParameter, typeof(EffectComponent));
                break;

            case 1:  //模型冰冻特效
            case 2:  //模型发光特效
            case 3:
            case 4:
            case 5:
                eComponent = m_component.gameObject.AddComponent(m_classTypeMap[config.type]) as EffectComponent;
                eComponent.Init(effectParameter);
                eComponent.StartEffect();
                break;
        }
        if (eComponent)
        {
            eComponent.effectID = m_effectID;               //分配特效ID
            eComponent.SetOwner(m_component.id);            //设置特效拥有者ID
            m_effectID++;
        }

        return eComponent;
    }

    private void DestroyEffectObject(EffectComponent eComponent)
    {
        if (eComponent == null)
            return;

        switch (eComponent.type)
        {
            case 0:    //标准特效
                CompentCreateManager.Instance.RemoveEffectGameObject(eComponent);
                break;
            case 1:    //模型冰冻特效
            case 2:    //模型发光特效
            case 3:    //黑洞扭曲效果
            case 4:    //模型溶解效果
            case 5:    //模型破碎效果
                eComponent.EndEffect();
                UnityEngine.Object.Destroy(eComponent);
                break;
        }
    }

    public EffectComponent AddEffect(string name, Transform transform)
    {
        if (!CConfigClass.effectConfig.ContainsKey(name))
        {
            Debug.LogError("The EntityEffectManager effect: " + name + " config is missing");
            return null;
        }

        CEffectConfig config = CConfigClass.effectConfig[name];

        if (config.ruleType == (int)RuleType.OneShow)
        {
            if (m_effectListOnce.ContainsKey(config.name))
            {
                m_effectListOnce[config.name].num.addComplex();
                return m_effectListOnce[config.name];
            }

            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.num.addComplex();
            eComponent.BindEffect(transform);
            m_effectListOnce[config.name] = eComponent;
            return eComponent;
        }
        else if (config.ruleType == (int)RuleType.ReplaceShow)
        {
            if (m_effectListOnce.ContainsKey(config.alias))
            {
                DestroyEffectObject(m_effectListReplace[config.alias]);
                m_effectListReplace.Remove(config.alias);
            }

            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.BindEffect(transform);
            m_effectListReplace[config.alias] = eComponent;
            return eComponent;
        }
        else if (config.ruleType == (int)RuleType.CastShow)
        {
            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.BindEffect(transform);
            m_effectListCast[config.name] = eComponent;
            return eComponent;
        }
        else
        {
            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.BindEffect(transform);
            m_effectListMultiple[eComponent.effectID] = eComponent;
            return eComponent;
        }
    }

    public EffectComponent AddEffect(string name, Vector3 position)
    {
        if (!CConfigClass.effectConfig.ContainsKey(name))
        {
            Debug.LogError("The EntityEffectManager effect: " + name +" config is missing");
            return null;
        }

        CEffectConfig config = CConfigClass.effectConfig[name];

        if (config.ruleType == (int)RuleType.OneShow)
        {
            if (m_effectListOnce.ContainsKey(config.name))
            {
                m_effectListOnce[config.name].num.addComplex();
                return m_effectListOnce[config.name];
            }

            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.num.addComplex();
            eComponent.SetEffectPosition(position);
            m_effectListOnce[config.name] = eComponent;
            return eComponent;
        }
        else if (config.ruleType == (int)RuleType.ReplaceShow)
        {
            if (m_effectListOnce.ContainsKey(config.alias))
            {
                DestroyEffectObject(m_effectListReplace[config.alias]);
                m_effectListReplace.Remove(config.alias);
            }

            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.SetEffectPosition(position);
            m_effectListReplace[config.alias] = eComponent;
            return eComponent;
        }
        else if (config.ruleType == (int)RuleType.CastShow)
        {
            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.SetEffectPosition(position);
            m_effectListCast[config.name] = eComponent;
            return eComponent;
        }
        else
        {
            EffectComponent eComponent = CreateEffectObject(config);
            eComponent.SetEffectPosition(position);
            m_effectListMultiple[eComponent.effectID] = eComponent;
            return eComponent;
        }
    }

    public void RemoveEffect(EffectComponent eComponent)
    {
        if (eComponent == null)
            return;

        if (eComponent.ruleType == (int)RuleType.OneShow)
        {
            if (!m_effectListOnce.ContainsKey(eComponent.effectName))
            {
                return;
            }
            eComponent.num.subComplex();
            if (!eComponent.num.Value())
            {
                m_effectListOnce.Remove(eComponent.effectName);
                DestroyEffectObject(eComponent);
            }
        }
        else if (eComponent.ruleType == (int)RuleType.ReplaceShow)
        {
            if (!m_effectListReplace.ContainsKey(eComponent.effectAlias))
            {
                return;
            }

            m_effectListReplace.Remove(eComponent.effectAlias);
            DestroyEffectObject(eComponent);
        }
        else if (eComponent.ruleType == (int)RuleType.CastShow)
        {
            if (!m_effectListCast.ContainsKey(eComponent.effectName))
            {
                return;
            }

            m_effectListCast.Remove(eComponent.effectName);
            DestroyEffectObject(eComponent);
        }
        else
        {
            if (!m_effectListMultiple.ContainsKey(eComponent.effectID))
            {
                return;
            }

            m_effectListMultiple.Remove(eComponent.effectID);
            DestroyEffectObject(eComponent);
        }
    }

    public EffectComponent AddModelEffect(string name)
    {
        if (!CConfigClass.effectConfig.ContainsKey(name))
        {
            Debug.LogError("The EntityEffectManager ModelEffect: " + name + " config is missing");  //找不到该模型特效的配置
            return null;
        }

        CEffectConfig config = CConfigClass.effectConfig[name];
        if (!m_classTypeMap.ContainsKey(config.type))
        {
            Debug.LogError("The EntityEffectManager ModelEffect: " + name + " classType is error");  //没有该类型的模型特效
            return null;
        }

        if (config.ruleType == (int)RuleType.OneShow)
        {
            EffectComponent eComponent = null;
            if (m_modelEffects.TryGetValue(config.name, out eComponent))
            {
                eComponent.num.addComplex();
            }
            else
            {
                m_modelEffects[config.name] = CreateEffectObject(config);
                m_modelEffects[config.name].num.addComplex();
            }

            return m_modelEffects[config.name];
        }
        else if (config.ruleType == (int)RuleType.ReplaceShow)
        {
            EffectComponent eComponent = null;
            if (m_modelEffects.TryGetValue(config.alias, out eComponent))
            {
                if (eComponent.effectName == config.name)
                {
                    eComponent.num.addComplex();
                }
                else
                {
                    DestroyEffectObject(eComponent);
                    m_modelEffects[config.alias] = CreateEffectObject(config);
                    m_modelEffects[config.alias].num.addComplex();
                }
            }
            else
            {
                m_modelEffects[config.alias] = CreateEffectObject(config);
                m_modelEffects[config.alias].num.addComplex();
            }

            return m_modelEffects[config.alias];
        }
        Debug.LogError("The EntityEffectManager ModelEffect: " + name + " ruleType is error");  //不支持添加该类型的模型特效
        return null;
    }

    public void RemoveModelEffect(string name)
    {
        if (!CConfigClass.effectConfig.ContainsKey(name))
        {
            return;
        }

        CEffectConfig config = CConfigClass.effectConfig[name];
        if (config.ruleType == (int)RuleType.OneShow)
        {
            EffectComponent eComponent = null;
            if (!m_modelEffects.TryGetValue(config.name, out eComponent))
            {
                return;
            }
            eComponent.num.subComplex();
            if (!eComponent.num.Value())
            {
                DestroyEffectObject(eComponent);
                m_modelEffects.Remove(config.name);
            }
        }
        else if (config.ruleType == (int)RuleType.ReplaceShow)
        {
            EffectComponent eComponent = null;
            if (!m_modelEffects.TryGetValue(config.alias, out eComponent))
            {
                return;
            }

            if (eComponent.effectName != config.name)
            {
                return;
            }
            eComponent.num.subComplex();
            if (!eComponent.num.Value())
            {
                DestroyEffectObject(eComponent);
                m_modelEffects.Remove(config.alias);
            }
        }
    }

    /// <summary>
    /// 清理MultipleShow类型的特效
    /// </summary>
    public void EffectListMultipleClear()
    {
        List<int> keyList = new List<int>(m_effectListMultiple.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            if (m_effectListMultiple.ContainsKey(keyList[i]))
            {
                DestroyEffectObject(m_effectListMultiple[keyList[i]]);
            }
        }
        m_effectListMultiple.Clear();
    }

    /// <summary>
    /// 清理OneShow类型的特效
    /// </summary>
    public void EffectListOnceClear()
    {
        List<string> keyList = new List<string>(m_effectListOnce.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            if (m_effectListOnce.ContainsKey(keyList[i]))
            {
                DestroyEffectObject(m_effectListOnce[keyList[i]]);
            }
        }
        m_effectListOnce.Clear();
    }

    /// <summary>
    /// 清理ReplaceShow类型的特效
    /// </summary>
    public void EffectListReplaceClear()
    {
        List<string> keyList = new List<string>(m_effectListReplace.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            if (m_effectListReplace.ContainsKey(keyList[i]))
            {
                DestroyEffectObject(m_effectListReplace[keyList[i]]);
            }
        }
        m_effectListReplace.Clear();
    }

    /// <summary>
    /// 清理CastShow类型的特效
    /// </summary>
    public void EffectListCastClear()
    {
        List<string> keyList = new List<string>(m_effectListCast.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            if (m_effectListCast.ContainsKey(keyList[i]))
            {
                DestroyEffectObject(m_effectListCast[keyList[i]]);
            }
        }
        m_effectListCast.Clear();
    }

    /// <summary>
    /// 清理模型特效
    /// </summary>
    public void ModelEffectsClear()
    {
        List<string> keyList = new List<string>(m_modelEffects.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            if (m_modelEffects.ContainsKey(keyList[i]))
            {
                DestroyEffectObject(m_modelEffects[keyList[i]]);
            }
        }
        m_modelEffects.Clear();
    }

    public void OnEntityDestroy()
    {
        ClearAllEffects();
    }
}
