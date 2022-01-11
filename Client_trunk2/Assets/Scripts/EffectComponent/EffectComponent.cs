using UnityEngine;
using System.Collections;

public class EffectComponent : ObjectComponent
{
    protected string m_effectName = "";    //特效名称
    protected int m_ruleType = 0;          //特效规则类型
    protected int m_type = 0;              //特效类型
    protected float lastTime = 0.0f;       //特效持续时间，当lastTime > 0.0时才有作用
    protected string m_effectAlias = "";   //特效别名
    protected int m_ownerID = 0;           //特效拥有者ID
    protected int m_effectID = 0;          //entity特效管理器分配的ID

    public ComplexBool num = new ComplexBool(false);   //计数

    public virtual void Init(CEffectParameter modelParameter)
    {
        m_effectName = modelParameter.effectConfig.name;
        m_type = modelParameter.effectConfig.type;
        m_ruleType = modelParameter.effectConfig.ruleType;
        m_effectAlias = modelParameter.effectConfig.alias;
        lastTime = (float)modelParameter.effectConfig.lastTime;

        if (lastTime > 0.0f)
        {
            StartCoroutine(DestroyOnTime());
        }
    }

    public string effectName
    {
        get { return m_effectName; }
    }

    public int type
    {
        get { return m_type; }
    }

    public int ruleType
    {
        get { return m_ruleType; }
    }

    public string effectAlias
    {
        get { return m_effectAlias; }
    }

    public int effectID
    {
        get { return m_effectID; }
        set { m_effectID = value; }
    }

    public void SetOwner(int id)
    {
        m_ownerID = id;
    }

    /// <summary>
    /// 设置特效位置
    /// </summary>
    /// <param name="pos"></param>
    public void SetEffectPosition(Vector3 pos)
    {
        if (gameObject)
        {
            gameObject.transform.position = pos;
        }
    }

    /// <summary>
    /// 设置特效绑定
    /// </summary>
    /// <param name="transform"></param>
    public void BindEffect(Transform transform)
    {
        if (gameObject)
        {
            //这里需要保持scale和特效原先世界坐标的scale大小一致，而旋转和朝向应该是保持局部坐标不变
            Quaternion localRotation = gameObject.transform.localRotation;
            Vector3 localPosition = gameObject.transform.position;
            gameObject.transform.SetParent(transform, true);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = localRotation;
        }
    }

    public virtual void DestroyEffect()
    {
        GameObjComponent owner = GameObjComponent.GetEntity(m_ownerID);
        if (owner && owner.effectManager != null)
        {
            owner.effectManager.RemoveEffect(this);
        }
        else
        {
            CompentCreateManager.Instance.RemoveEffectGameObject(this);
        }
    }

    public IEnumerator DestroyOnTime()
    {
        yield return new WaitForSecondsRealtime(lastTime);
        DestroyEffect();
    }

    public virtual void StartEffect()
    {
    }

    public virtual void EndEffect()
    {
    }

    public virtual void Update()
    {
    }
}
