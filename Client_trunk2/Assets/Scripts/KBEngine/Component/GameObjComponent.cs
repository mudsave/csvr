using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameObjComponent : MonoBehaviour {

    private Transform m_myTransform = null;
    private Animator m_animator = null;
    private ModelComponent m_modelCompent = null;
    private ComplexBool m_isHide = new ComplexBool(false);
    
    private KBEngine.Entity m_entity;
    private ModelEffectMgr m_effectMgr;
    //private eAnimatPosSyncFlag m_animatPosSyncFlag = eAnimatPosSyncFlag.NotSync;     // 位移（位置）同步标志

    public CEntityType objectType = CEntityType.None;
    public static Dictionary<int, GameObjComponent> entityComponentList = new Dictionary<int, GameObjComponent>();

    public delegate void updateDelegate();
    public Dictionary<string, updateDelegate> updateFun = new Dictionary<string, updateDelegate>();
    private ActionCompliex actionCompliex = null;

    private EntityEffectManager m_effectManager = null;

    private int s_raycastIgnore;

    //GameObjComponent的唯一id
    [HideInInspector]
    public Int32 id;

    private static Int32 m_index = 0;

    private EntityEvent m_event = new EntityEvent();

    public EntityEvent eventObj
    {
        get { return m_event; }
    }

    public static Int32 GetIndex()
    {
        return ++ m_index;
    }

    public static void ResetIndex()
    {
        m_index = 0;
    }

    public ModelComponent modelCompent
    {
        get { return m_modelCompent; }
        set
        {
            m_modelCompent = value;
            if (isHide)
                OnHide();
            else
                OnShow();
        }
    }

    public EntityEffectManager effectManager
    {
        get { return m_effectManager; }
    }


    public ModelEffectMgr effectMgr
    {
        get { return m_effectMgr; }
        set { m_effectMgr = value; }
    }

    public virtual Transform myTransform
    {
        get { return m_myTransform; }
    }

    public Animator animator
    {
        get { return m_animator; }
    }

    public bool isHide
    {
        get { return m_isHide.Value(); }
    }

    public void Awake()
    {
        m_myTransform = GetComponent<Transform>();
        m_modelCompent = GetComponent<ModelComponent>();
        m_animator = GetComponent<Animator>();

        s_raycastIgnore = (1 << (int)eLayers.Entity) | (1 << (int)eLayers.Trap) | (1 << (int)eLayers.Bullet) | (1 << (int)eLayers.Shield) | (1 << (int)eLayers.UI);

        id = GetIndex();

        if (m_effectManager == null)
        {
            m_effectManager = new EntityEffectManager();
            m_effectManager.init(this);
        }
    }

    public virtual void Destroy()
    {
        GlobalEvent.deregister(this);
        if (m_effectManager != null)
        {
            m_effectManager.Destroy();
            m_effectManager = null;
        }

        if (m_modelCompent != null)
        {
            m_modelCompent.Destroy();
        }
    }

    public void Start()
    {
    }

    public virtual void Update()
    {
        var enumerator = updateFun.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            element.Value();
        }
    }

    public bool Hide(string tag = null, bool isSystemHideOrder = false)
    {
        bool oldValue = m_isHide.Value();

        if (tag != null)
        {
            m_isHide.addComplex(tag);
        }
        else
        {
            m_isHide.addComplex();
        }

        if (oldValue == m_isHide.Value())
        {
            return false;
        }

        OnHide();
        return true;
    }

    public virtual void OnHide()
    {
        //隐藏模型
        m_modelCompent.SetModelActive(false);
    }

    public bool Show(string tag = null)
    {
        bool oldValue = m_isHide.Value();

        if (tag != null)
        {
            m_isHide.subComplex(tag);
        }
        else
        {
            m_isHide.subComplex();
        }

        if (oldValue == m_isHide.Value())
        {
            return false;
        }

        OnShow();
        return true;

    }

    public virtual void OnShow()
    {
        //显示模型
        m_modelCompent.SetModelActive(true);
    }


    public void AddActionComliex(string id)
    {
        if (CConfigClass.actionEffectConfig.ContainsKey(id))
        {
            actionCompliex = new ActionCompliex(CConfigClass.actionEffectConfig[id], this);
            actionCompliex.Start();
        }
    }


    public void HitMe()
    {
        GlobalEvent.fire("Event_HitMe", new object[] { this.gameObject });
    }

    /* 延时设置animator的标志值，以简化代码 */
    public void AnimatorFlagSetDelay(string key, int value)
    {
        StartCoroutine(_AnimatorFlagSetDelay(key, value));
    }

    private IEnumerator _AnimatorFlagSetDelay(string key, float value)
    {
        yield return new WaitForEndOfFrame();
        m_animator.SetFloat(key, value);
    }

    /* 延时设置animator的标志值，以简化代码 */
    public void AnimatorFlagSetDelay(string key, float value)
    {
        StartCoroutine(_AnimatorFlagSetDelay(key, value));
    }

    private IEnumerator _AnimatorFlagSetDelay(string key, bool value)
    {
        yield return new WaitForEndOfFrame();
        m_animator.SetBool(key, value);
    }

    /* 延时设置animator的标志值，以简化代码 */
    public void AnimatorFlagSetDelay(string key, bool value)
    {
        StartCoroutine(_AnimatorFlagSetDelay(key, value));
    }

    #region entityManage
    /* 搜索对象半径x米内的游戏对象
	 */
    public static Dictionary<int, GameObjComponent> EntitiesInRange(float radius, Vector3 center, Transform transform, List<CEntityType> entityTypes = null)
    {
        Vector3 pos = center;
        if (pos == Vector3.zero)
            pos = transform.position;

        Dictionary<int, GameObjComponent> objs = entityComponentList;
        Dictionary<int, GameObjComponent> result = new Dictionary<int, GameObjComponent>();
        var ide = objs.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<int, GameObjComponent> obj = ide.Current;

            if (entityTypes != null && entityTypes.Count > 0)
            {
                if (!entityTypes.Contains(obj.Value.objectType))
                {
                    continue;
                }
            }
            if (Vector3.Distance(obj.Value.transform.position, pos) <= radius)
            {
                result[obj.Key] = obj.Value;
            }

        }

        return result;
    }

    public static Dictionary<int, GameObjComponent> EntitiesInRange(List<CEntityType> entityTypes = null)
    {
        if (entityTypes == null || entityTypes.Count == 0)
            return entityComponentList;
        else
        {
            Dictionary<int, GameObjComponent> result = new Dictionary<int, GameObjComponent>();
            var ide = entityComponentList.GetEnumerator();
            while (ide.MoveNext())
            {
                KeyValuePair<int, GameObjComponent> obj = ide.Current;
                if (entityTypes.Contains(obj.Value.objectType))
                {
                    result.Add(obj.Key, obj.Value);
                }
            }
            return result;
        }
    }

    public Dictionary<int, GameObjComponent> EntitiesInRange(float radius, List<CEntityType> entityTypes = null)
    {
        return EntitiesInRange(radius, Vector3.zero, transform, entityTypes);
    }

    /* 搜索对象半径x米内朝向为中心y度扇形内的游戏对象。
     * 思路：
     * 1.让每一个找到的对象坐标减去搜索者坐标，得到相对于搜索者的矢量A
     * 2.使用搜索者的朝向矢量B与矢量A做夹角计算
     * 3.得出的夹角如果大于参数给于的角度的一半，则表示不在扇形内
     * Cos(θ) = A . B /(|A||B|)
     * @param radius: float, 半径
     * @param angle: float, 搜索角度
     */
    public static Dictionary<int, GameObjComponent> EntitiesInRange(float radius, float angle, Vector3 dir, Transform transform, List<CEntityType> entityTypes = null)
    {
        Dictionary<int, GameObjComponent> objs = EntitiesInRange(radius, Vector3.zero, transform);
        Vector3 srcPos = transform.position;
        Vector3 p1 = dir;
        p1.y = 0.0f;  // 去掉高度的影响
        p1.Normalize();
        Dictionary<int, GameObjComponent> result = new Dictionary<int, GameObjComponent>();
        var ide = objs.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<int, GameObjComponent> obj = ide.Current;
            if (obj.Value.transform == transform)
            {
                // 忽略自己
                continue;
            }

            Vector3 p2 = obj.Value.transform.position - srcPos;
            p2.y = 0.0f;  // 去掉高度的影响

            // 计算两矢量的角度
            // 因为计算出来的角度是以当前对象朝向为中心的，所以需要乘2
            float angleV = Vector3.Angle(p1, p2) * 2;

            if (angleV <= angle)
            {
                if (entityTypes == null || entityTypes.Count == 0)
                    result[obj.Key] = obj.Value;
                else
                {
                    if (entityTypes.Contains(obj.Value.objectType))
                    {
                        result[obj.Key] = obj.Value;
                    }
                }
            }
        }
        return result;
    }

    public Dictionary<int, GameObjComponent> EntitiesInRange(float distance, float radius, float angle, List<CEntityType> entityTypes = null)
    {
        Dictionary<int, GameObjComponent> finder = new Dictionary<int, GameObjComponent>();
        Dictionary<int, GameObjComponent> result = new Dictionary<int, GameObjComponent>();
        finder = EntitiesInRange(radius, Vector3.zero, transform);
        var ide = finder.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<int, GameObjComponent> obj = ide.Current;
            if (obj.Value.transform == transform)
            {
                // 忽略自己
                continue;
            }
            if (Vector3.Distance(transform.position, obj.Value.transform.position) <= distance)
            {
                result[obj.Key] = obj.Value;
            }
            else
            {
                Vector3 p1 = transform.forward;
                p1.y = 0.0f;  // 去掉高度的影响
                p1.Normalize();

                if (obj.Value.transform == transform)
                {
                    // 忽略自己
                    continue;
                }

                Vector3 p2 = obj.Value.transform.position - transform.position;
                p2.y = 0.0f;  // 去掉高度的影响

                // 计算两矢量的角度
                // 因为计算出来的角度是以当前对象朝向为中心的，所以需要乘2
                float angleV = Vector3.Angle(p1, p2) * 2;

                if (angleV <= angle)
                {
                    if (entityTypes == null || entityTypes.Count == 0)
                        result[obj.Key] = obj.Value;
                    else
                    {
                        if (entityTypes.Contains(obj.Value.objectType))
                        {
                            result[obj.Key] = obj.Value;
                        }
                    }
                }
            }
        }
        return result;
    }

    public Dictionary<int, GameObjComponent> EntitiesInRange(float radius, float angle, List<CEntityType> entityTypes = null)
    {
        return EntitiesInRange(radius, angle, transform.forward, transform, entityTypes);
    }

    /* 搜索对象前方x米宽w米内的游戏对象。
     * 两点求一条直线:点斜式
     * 点到直线距离公式:d=|Ax0+By0+C|/√(A²+B²)
     * @param length: float, 搜索者正前方搜索距离
     * @param width: float, 以搜索者为中心左右两边宽度
     */
    public static Dictionary<int, GameObjComponent> EntitiesInRange(Vector3 center, Vector3 dir, float width, float length, Transform transform, List<CEntityType> entityTypes = null)
    {
        Vector3 a = center;
        if (a == Vector3.zero)
            a = transform.position;

        a.y = 0.0f;
        Vector3 b = a + dir * length;
        Vector3 ver = Vector3.zero;             //方向向量的垂直向量
        if (dir.z == 0 || dir.z < -0.00001f)   //数过小则舍弃
        {
            ver.z = 0.1f;
            ver.x = (-dir.z * 0.1f) / dir.x;
        }
        else
        {
            ver.x = 0.1f;
            ver.z = (-dir.x * 0.1f) / dir.z;
        }
        ver.Normalize();
        Vector3 o = new Vector3((a.x + b.x) / 2, 0, (a.z + b.z) / 2);
        Vector3 c = o + ver * width;
        Vector3 d = o - ver * width;
        float A1 = 0;
        float B1 = -1f;
        if (b.x == a.x)
        {
            A1 = -1f;
            B1 = 0;
        }
        else if (b.z == a.z)
        {
            A1 = 0;
        }
        else
        {
            A1 = (b.z - a.z) / (b.x - a.x);              //求直线斜率
        }
        float C1 = a.z - A1 * a.x;                      //求直线常量
        float A2 = 0;
        float B2 = -1f;
        if (d.x == c.x)
        {
            A2 = -1f;
            B2 = 0;
        }
        else if (d.z == c.z)
        {
            A2 = 0;
        }
        else
        {
            A2 = (d.z - c.z) / (d.x - c.x);
        }
        float C2 = c.z - A2 * c.x;
        Dictionary<int, GameObjComponent> objs = entityComponentList;
        Dictionary<int, GameObjComponent> result = new Dictionary<int, GameObjComponent>();

        var ide = objs.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<int, GameObjComponent> obj = ide.Current;

            Vector3 p2 = obj.Value.transform.position;
            p2.y = 0.0f;
            float WidthDistance = (Mathf.Abs(A1 * p2.x + B1 * p2.z + C1) / Mathf.Sqrt(Mathf.Pow(A1, 2) + Mathf.Pow(B1, 2)));
            float LengthDistance = (Mathf.Abs(A2 * p2.x + B2 * p2.z + C2) / Mathf.Sqrt(Mathf.Pow(A2, 2) + Mathf.Pow(B2, 2))) * 2;
            if (LengthDistance <= length && WidthDistance <= width)
            {
                if (entityTypes == null || entityTypes.Count == 0)
                    result[obj.Key] = obj.Value;
                else
                {
                    if (entityTypes.Contains(obj.Value.objectType))
                    {
                        result[obj.Key] = obj.Value;
                    }
                }
            }
        }
        return result;
    }

    public Dictionary<int, GameObjComponent> EntitiesInRange(Vector3 dir, float width, float length, List<CEntityType> entityTypes = null)
    {
        return EntitiesInRange(Vector3.zero, dir, width, length, transform, entityTypes);
    }

    public static Dictionary<int, GameObjComponent> GetEntities(List<CEntityType> entityTypes = null)
    {
        Dictionary<int, GameObjComponent> result = new Dictionary<int, GameObjComponent>();
        PlayerComponent player = GetPlayer();

        var ide = entityComponentList.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<int, GameObjComponent> kv = ide.Current;

            if (kv.Value == player)
            {
                // 忽略自己
                continue;
            }

            if (entityTypes == null || entityTypes.Contains(kv.Value.objectType))
            {
                result[kv.Key] = kv.Value;
            }
        }

        return result;
    }

    /*
     *通过entityID获取entity 
     */
    public static GameObjComponent GetEntity(int id)
    {
        if (entityComponentList.ContainsKey(id))
            return entityComponentList[id];

        return null;
    }

    /*
     * 增加entityComponent
     */
    public static void AddEntityComponent(GameObjComponent component)
    {
        //entityComponentList[component.entity.id] = component;

    }

    /*
     * 移除entityComponent
     */
    public static void RemoveEntityComponent(int id)
    {
        entityComponentList.Remove(id);
    }

    /*
     * 获取player玩家的component 
     */
    public static PlayerComponent GetPlayer()
    {
        return (PlayerComponent)GetEntity(KBEngine.KBEngineApp.app.player().id);
    }
    #endregion entityManage

    /// <summary>
    /// This callback method is called when the local entity control by the client has been enabled or disabled. 
    /// See the Entity.controlledBy() method in the CellApp server code for more infomation.
    /// </summary>
    /// <param name="isControlled">
    /// 对于玩家自身来说，它表示是否自己被其它玩家控制了；
    /// 对于其它entity来说，表示我本机是否控制了这个entity
    /// </param>
    public virtual void OnControlled(bool isControlled)
    {

    }


    /// <summary>
    /// 当地图数据加载完成后，GameObject正式开始进入地图时被调用
    /// </summary>
    public virtual void EnterWorld()
    {

    }

    public virtual void LeaveWorld()
    {

    }

    /// <summary>
    /// 重置函数
    /// </summary>
    public static void OnClear()
    {
        entityComponentList.Clear();
    }

}
