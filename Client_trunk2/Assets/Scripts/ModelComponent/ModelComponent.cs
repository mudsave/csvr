using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelComponent : ObjectComponent
{
    public struct NodeClass
    {
        public string nodePath;
        public ModelComponent compent; 

        public NodeClass(string _nodePath,  ModelComponent _compent)
        {
            nodePath = _nodePath;
            compent = _compent;
        }
    }

    protected Transform worldTransform;

    protected bool objVisable = false;

    public GameObject model = null;
    public string modelResName = "";
    public string path = "";

    //记录绑定物品的GameObject
    public Dictionary<string, NodeClass> bindingGameObject = new Dictionary<string, NodeClass>();

    private static Dictionary<CModelType, System.Type> s_classTypeMap = new Dictionary<CModelType, System.Type>()
        {
            //目前buff客户端无表现，直接使用SpellBuff类型
			{CModelType.Avatar, typeof(ModelComponent)},
            {CModelType.Item, typeof(ModelComponent)},
            {CModelType.Gate, typeof(ModelComponent)},
            {CModelType.Equipment, typeof(ModelComponent)},
            {CModelType.CSAvatar, typeof(CSAvaterModelCoponent)},
        };

    //与模型一起创建的特效，一般认为这个特效就是模型的一部分，所以需要模型自己管理
    protected Dictionary<string, EffectComponent> effectModel = new Dictionary<string, EffectComponent>();

    protected Animator mAnimator = null;

    //绑定点位置
    private Transform m_upPos = null;

    private bool isactionCheck = false;

    public virtual Transform upPos
    {
        get { return m_upPos; }
    }

    public Transform WorldTransform
    {
        get
        {
            return this.worldTransform;
        }
    }

    public Vector3 Postion
    {
        get
        {
            return this.worldTransform.localPosition;
        }
        set
        {
            this.worldTransform.localPosition = value;
        }
    }

    public Quaternion Rotate
    {
        get
        {
            return this.worldTransform.localRotation;
        }
        set
        {
            this.worldTransform.localRotation = value;
        }
    }

    public Vector3 Scale
    {
        get
        {
            return this.worldTransform.localScale;
        }
        set
        {
            this.worldTransform.localScale = value;
        }
    }

    public string ObjName
    {
        get
        {
            return base.gameObject.name;
        }
        set
        {
            base.gameObject.name = value;
        }
    }

    public void SetScale(float fScale)
    {
        if (null != base.gameObject)
        {
            this.worldTransform.localScale = Vector3.one * fScale;
        }
    }

    public virtual void SetModelActive(bool active)
    {
        model.SetActive(active);
    }

    //

    private void Awake()
    {
        this.worldTransform = base.transform;
    }

    public void Update()
    {
        if (isactionCheck)
            CurrentAction();
    }

    public static System.Type GetModelType(CModelType type)
    {
        return s_classTypeMap[type];
    }

    public virtual void Destroy()
    {
        //先销毁自身光效
        var enumerator = effectModel.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            element.Value.DestroyEffect();
        }


        //销毁附着在身上的模型资源
        var enumerator_b = bindingGameObject.GetEnumerator();
        while (enumerator_b.MoveNext())
        {
            var element = enumerator_b.Current;
            element.Value.compent.Destroy();
        }

        //最后销毁自身
        Singleton<CompentCreateManager>.GetInstance().ReomoveModelGameObject(this);
    }

    //当前动作未开放
    public void CurrentAction()
    {
        //if (mAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash != stateHash)
        //{
        //    stateHash = mAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        //    int tag = mAnimator.GetCurrentAnimatorStateInfo(0).tagHash;

        //    if (actionCompliex != null)
        //    {
        //        actionCompliex.End();
        //    }

        //    if (CConfigClass.actionEffectHashConfig.ContainsKey(tag))
        //    {
        //        actionCompliex = new ActionCompliex(CConfigClass.actionEffectHashConfig[tag], this);
        //        actionCompliex.Start();
        //    }
        //    else
        //    {
        //        actionCompliex = null;
        //    }

        //}

        //if (actionCompliex != null)
        //{
        //    actionCompliex.Update();
        //}
    }

    public bool IsBodyRenderer(Renderer _Renderer)
    {
        return _Renderer && _Renderer.transform.parent && _Renderer.transform.parent.name == "Model";
    }

    public bool IsWeaponRenderer(Renderer _Renderer)
    {
        return _Renderer && _Renderer.transform.parent && (_Renderer.transform.parent.name == "Weapon_L" || _Renderer.transform.parent.name == "Weapon_R");
    }

    public void InitEffect()
    {
    }

    public void PlayEffect(int effectID, object param = null)
    {
    }

    public void StopEffect(int effectID, bool bStopAll = true)
    {
    }

    public void InitAnimation(string aniPath, string actionFX = "")
    {
        mAnimator = gameObject.GetComponent<Animator>();
        if (mAnimator == null)
            mAnimator = gameObject.AddComponent<Animator>();

        if (model)
        {
            Animator modelAnimator = model.GetComponent<Animator>();
            mAnimator.avatar = modelAnimator.avatar;

            RuntimeAnimatorController con = ResourceManager.LoadAssetBundleResource(aniPath) as RuntimeAnimatorController;

            if (con == null)
            {
                Debug.LogError("acPath is error. path = " + aniPath);
            }

            mAnimator.runtimeAnimatorController = null;
            mAnimator.runtimeAnimatorController = con;

            mAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;

            //this.mAnimator.applyRootMotion = false;
            modelAnimator.enabled = false;
            //DestroyImmediate(modelAnimator);
        }
        else
            Debug.LogError("InitAnimation model is null");

        isactionCheck = true;
    }

    public virtual void Init(CModelParameter modelParameter, GameObject modelArg)
    {
        model = modelArg;
        path = modelParameter.modleConfig.path;
        CModelConfig modleConfig = modelParameter.modleConfig;
        //this.SetVisable(true);
        if (modelParameter.controllerType == CModelParameter.eControllerType.fight && modleConfig.controller != "")
        {
           this.InitAnimation(modleConfig.controller, modelParameter.actionFX);
        }
        else if (modelParameter.controllerType == CModelParameter.eControllerType.show && modleConfig.showController != "")
        {
            this.InitAnimation(modleConfig.showController, modelParameter.actionFX);
        }

        //生成绑定点
        if (modleConfig.up != null && modleConfig.up.Count >= 3) //绑定点
            CreateBindingPoint(new Vector3((float)modleConfig.up[0], (float)modleConfig.up[1], (float)modleConfig.up[2]), "up", ref m_upPos);

        if (modleConfig.boxPosition != null && modleConfig.boxPosition.Count >= 3)
        {
            Vector3 center = new Vector3((float)modleConfig.boxPosition[0], (float)modleConfig.boxPosition[1], (float)modleConfig.boxPosition[2]);

            if (modleConfig.boxSize != null && modleConfig.boxSize.Count == 2)
            {
                CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.isTrigger = true;
                capsuleCollider.center = center;
                capsuleCollider.radius = (float)modleConfig.boxSize[0];
                capsuleCollider.height = (float)modleConfig.boxSize[1];
            }
            else if (modleConfig.boxSize != null && modleConfig.boxSize.Count == 3)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.center = center;
                boxCollider.size = new Vector3((float)modleConfig.boxSize[0], (float)modleConfig.boxSize[1], (float)modleConfig.boxSize[2]);
            }
        }
        else
        {
            Collider collider = GetComponent<Collider>();
            if (collider)
                UnityEngine.Object.Destroy(collider);
        }

        //平移装备绑定点
        if (modleConfig.offset != null && modleConfig.offset.Count >= 3)
        {
            model.transform.Translate(new Vector3((float)modleConfig.offset[0], (float)modleConfig.offset[1], (float)modleConfig.offset[2]));
        }

        //装备旋转
        if (modleConfig.direction != null && modleConfig.direction.Count >= 3)
        {
            model.transform.localRotation = Quaternion.Euler(new Vector3((float)modleConfig.direction[0], (float)modleConfig.direction[1], (float)modleConfig.direction[2])) * model.transform.localRotation;
        }

        //增加初始特效
        if (modleConfig.effectBind != null && modleConfig.effectBind.Count > 0)
        {
            var enumerator = modleConfig.effectBind.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                Transform objTransform = transform.Find(element.Key);
                if (objTransform != null)
                    AddEffect(element.Value, objTransform);
            }
        }

        //根据配置，设置参数值
        SetCompentProperty(modleConfig);
    }

    public void AddEffect(string name, Transform transform)
    {
        if (!CConfigClass.effectConfig.ContainsKey(name))
        {
            Debug.LogError("The EntityEffectManager effect: " + name + " config is missing");
            return;
        }

        CEffectConfig config = CConfigClass.effectConfig[name];

        CEffectParameter effectParameter = new CEffectParameter(config);
        EffectComponent eComponent = CompentCreateManager.Instance.CreateEffectGameObject(effectParameter, typeof(EffectComponent));
        eComponent.BindEffect(transform);

        effectModel.Add(name, eComponent);
    }

    //设置组件属性
    protected void SetCompentProperty(CModelConfig modleConfig)
    {
        //武器法宝配置表中没有这个字段
        if (modleConfig.modelScale != null)
            model.transform.localScale = new Vector3((float)modleConfig.modelScale[0], (float)modleConfig.modelScale[1], (float)modleConfig.modelScale[2]);
    }

    //设置播放状态
    public void Play(string name)
    {
        mAnimator.Play(name);
    }

    //设置播放速度
    public void SetPlaySpeed(float speed)
    {
        mAnimator.speed = speed;
    }

    //设置动作位移
    public void SetApplyRootMotion(bool value)
    {
        mAnimator.applyRootMotion = value;
    }

    public virtual void SetVisable(bool bVisable)
    {
        this.objVisable = bVisable;
        for (int i = 0; i < base.transform.childCount; i++)
        {
            base.transform.GetChild(i).gameObject.SetActive(this.objVisable);
        }
    }

    //生成绑定点
    protected void CreateBindingPoint(Vector3 upPosition, string name, ref Transform tr)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = model.transform;
        obj.transform.localPosition = upPosition;
        obj.name = name;

        tr = obj.transform;
    }

    //绑定主模型下的节点
    public void BindingGameObject(ModelComponent compent, string thingName, string nodename)
    {
        if (compent == null)
        {
            return;
        }

        bool activeSelf = true;
        if (!model.activeSelf)
        {
            activeSelf = model.activeSelf;
            model.SetActive(true);
        }

        //Transform weaponPoint = transform.Find(nodename);
        Transform weaponPoint = gameObject.transform.Find(nodename);

        //Transform[] childrenTransform = gameObject.GetComponentsInChildren<Transform>();
        //for (int i = 0; i < childrenTransform.Length; i++)
        //{
        //    if (childrenTransform[i].gameObject.name == nodename)
        //    {
        //        weaponPoint = childrenTransform[i];
        //        break;
        //    }
        //}

        Quaternion localRotation = compent.transform.localRotation;
        Vector3 scale = compent.transform.localScale;
        compent.transform.parent = weaponPoint;
        compent.transform.localPosition = Vector3.zero;
        compent.transform.localScale = scale;
        compent.transform.localRotation = localRotation;

        Transform[] componentsInChildren = compent.GetComponentsInChildren<Transform>();
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            componentsInChildren[j].gameObject.layer = gameObject.layer;
        }

        bindingGameObject[thingName] = new NodeClass(nodename, compent);

        model.SetActive(activeSelf);
    }

    //移除模型下节点GameObject
    public void RemoveBindingGameObject(string thingName)
    {
        if (bindingGameObject.ContainsKey(thingName))
        {
            CompentCreateManager.Instance.ReomoveModelGameObject(bindingGameObject[thingName].compent);
            bindingGameObject.Remove(thingName);
        }
    }

    //解绑模型下节点GameObject
    public ModelComponent UnBindingGameObject(string thingName)
    {
        if (bindingGameObject.ContainsKey(thingName))
        {
            bindingGameObject[thingName].compent.transform.SetParent(null);
            ModelComponent obj = bindingGameObject[thingName].compent;
            bindingGameObject.Remove(thingName);
            return obj;
        }
        return null;
    }

    //替换主模型
    public void ChangeMainModel(CModelParameter modelParameter)
    {
        model.transform.parent = null; //解开原来模型与主节点的绑定

        Dictionary<string, NodeClass> binding = bindingGameObject;
        bindingGameObject = new Dictionary<string, NodeClass>();

        foreach (KeyValuePair<string, NodeClass> kv in binding)
        {
            ModelComponent obj = kv.Value.compent;

            Quaternion localRotation = obj.transform.localRotation;
            Vector3 scale = obj.transform.localScale;
            obj.transform.parent = null;
            obj.transform.localRotation = localRotation;
            obj.transform.localScale = scale;
        }

        ResourceManager.DestroyResource(model, true);

        CompentCreateManager.Instance.ReloadModel(this.gameObject, modelParameter); //重新绑定模型

        //重新绑定
        foreach (KeyValuePair<string, NodeClass> kv in binding)
        {
            BindingGameObject(kv.Value.compent, kv.Key, kv.Value.nodePath);
        }
    }

    #region Load Weapon

    //加载武器模型（删除原先的武器模型）
    public virtual bool LoadWeapon(CModelParameter modelParameter)
    {
        if (modelParameter.modleConfig.path.Length <= 0)
        {
            return false;
        }

        DeleteWeapon();

        if (modelParameter.modleConfig.equipPoint == CWeaponNode.rightHand)
        {
            ModelComponent weaponObject = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            //ResourceManager.InstantiateResource(modleConfig.path, "Weapon", null) as GameObject;
            BindingGameObject(weaponObject, "rightWeapon", CModelNodePath.r_h);
        }
        else if (modelParameter.modleConfig.equipPoint == CWeaponNode.leftHand)
        {
            ModelComponent weaponObject = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(weaponObject, "leftWeapon", CModelNodePath.l_h);
        }
        else if (modelParameter.modleConfig.equipPoint == CWeaponNode.bothHand)
        {
            ModelComponent rightWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(rightWeapon, "rightWeapon", CModelNodePath.r_h);
            ModelComponent leftWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(leftWeapon, "leftWeapon", CModelNodePath.l_h);
        }
        else if (modelParameter.modleConfig.equipPoint == CWeaponNode.bothHandTurn)
        {
            ModelComponent rightWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            rightWeapon.transform.Rotate(new Vector3(0, 0, 180));
            BindingGameObject(rightWeapon, "rightWeapon", CModelNodePath.r_h);
            ModelComponent leftWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(leftWeapon, "leftWeapon", CModelNodePath.l_h);
        }
        return true;
    }

    //卸载武器模型
    public void DeleteWeapon()
    {
        RemoveBindingGameObject("rightWeapon");
        RemoveBindingGameObject("leftWeapon");
    }

    //获取武器模型
    public ModelComponent GetWeapen(string name)
    {
        if (bindingGameObject.ContainsKey(name))
        {
            return bindingGameObject[name].compent;
        }

        return null;
    }

    #endregion Load Weapon

    #region Change Avater Model

    //改变avater模型
    public virtual void ChangeAvaterModel(CModelParameter modelParameter)
    {
        ChangeMainModel(modelParameter);
    }

    #endregion Change Avater Model


}