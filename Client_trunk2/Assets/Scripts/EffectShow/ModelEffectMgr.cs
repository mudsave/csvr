using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModelEffectMgr 
{
    public GameObjComponent m_entity;
	public Dictionary<string, BaseModelEffect> allEffects = new Dictionary<string, BaseModelEffect>();
    public List<BaseModelEffect> modelEffectsList = new List<BaseModelEffect>();

    private static Dictionary<string, System.Type> s_effectsMap = new Dictionary<string, System.Type>()
    {
        {"FrozenEffect", typeof(FrozenEffect)},                  //从指定颜色恢复到初始状态
    };

    public ModelEffectMgr(GameObjComponent _entity)
    {
        m_entity = _entity;
    }

    protected static BaseModelEffect CreateModelEffect(string type)
    {
        if (!s_effectsMap.ContainsKey(type))
            return null;

        var obj = System.Activator.CreateInstance(s_effectsMap[type]) as BaseModelEffect;
        return obj;
    }

	// Use this for initialization
    //void Start ()
    //{
    //    if (isOpenShield)
    //    {
    //        StartShieldEffect();
    //    }
    //}
	


	// Update is called once per frame
	public void Update () 
	{
        for (int i = 0; i < modelEffectsList.Count; i++)
        {
            modelEffectsList[i].Tick();
        }
	}
	
	public void StartEffect(string effectType)
	{
		//首先查找当前此效果是否正在执行中
		BaseModelEffect effect =  GetEffectInCurrentList(effectType);
		if(effect == null )
		{
			return;
		}
		
		if(effect.isPlaying)
		{
			return;
		}

		effect.StartEffect();
		
	}

    //public void StartColorChangeEffect(string effectType, float fTime, Vector4 colorValue)
    //{
    //    //首先查找当前此效果是否正在执行中
    //    BaseModelEffect effect =  GetEffectInCurrentList(effectType);
    //    if(effect == null )
    //    {
    //        return;
    //    }
		
    //    if(effect.isPlaying)
    //    {
    //        return;
    //    }

    //    effect.StartColorChangeEffect(fTime, colorValue);	
    //}
	
	public void DeleteEffect(BaseModelEffect effect)
	{
	
	}


	public void StopEffect(string effectType)
	{
		//首先查找当前此效果是否正在执行中，是的话停掉
		BaseModelEffect effect =  GetEffectInCurrentList(effectType);
		if(effect != null)
		{
			effect.EndEffect();	
		}
		
	}
	
	public BaseModelEffect GetEffectInCurrentList(string effectType)
	{
		if(allEffects.ContainsKey(effectType))
		{
			return allEffects[effectType];
		}

        var modelEffect = CreateModelEffect(effectType);
        if (modelEffect != null) 
        {
            allEffects[effectType] = modelEffect;
            modelEffectsList.Add(modelEffect);
            modelEffect.Init(this);
            modelEffect.SetOwner(m_entity);
            return modelEffect;
        }
		return null;
	}


}
