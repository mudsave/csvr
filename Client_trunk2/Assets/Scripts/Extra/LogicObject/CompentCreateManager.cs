using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct CModelParameter
{
    public enum eControllerType
    {
         fight, //战斗状态机
         show, //显示状态机
    }

    public CModelParameter(CModelConfig _modleConfig, eLayers _layer = eLayers.Default, eControllerType _controllerType = eControllerType.fight,string _actionFX = "")
    {
        modleConfig = _modleConfig;
        controllerType = _controllerType;
        layer = _layer;
        actionFX = _actionFX;
    }

    public CModelConfig modleConfig;
    public eControllerType controllerType;
    public eLayers layer; //层
    public string actionFX; //要求动作播放
}

public struct CEffectParameter
{
    public CEffectConfig effectConfig;

    public CEffectParameter(CEffectConfig _effectConfig)
    {
        effectConfig = _effectConfig;
    }
}

/// <summary>
/// 提供创建模型和特效的接口
/// </summary>
public class CompentCreateManager : Singleton<CompentCreateManager>
{
    //加载模型，模型节点名为"Model"
    private bool LoadModel(GameObject objRoot, GameObject objModel, CModelParameter modelParameter)
    {
        if (null == objRoot)
        {
            return false;
        }

        Transform transform = objRoot.transform.FindChild("Model");
        if (transform != null)
        {
            UnityEngine.Object.DestroyImmediate(transform.gameObject);
        }
        if (objModel != null)
        {
            Vector3 scale = objModel.transform.localScale;
            objModel.transform.parent = objRoot.transform;
            objModel.transform.localPosition = Vector3.zero;
            objModel.transform.localRotation = Quaternion.identity;
            objModel.transform.localScale = scale;

            Transform[] componentInChild = objModel.GetComponentsInChildren<Transform>();
            for (int i = 0; i < componentInChild.Length; i++)
            {
                componentInChild[i].gameObject.layer = objRoot.layer;

            }

            ModelComponent component = objRoot.GetComponent<ModelComponent>();
            if (component != null)
            {
                component.Init(modelParameter, objModel);
            }

        }
        return true;
    }


    //主要针对PC，NPC，加载绑定模型资源，模型节点名为"Model"
    public bool ReloadModel(GameObject objRoot, CModelParameter modelParameter)
    {
        if (null == objRoot || modelParameter.modleConfig.path.Length <= 0)
        {
            return false;
        }

        //GameObject gameObject = ResourceManager.InstantiateResource(modelParameter.modleConfig.path);
        GameObject gameObject = ResourceManager.InstantiateAssetBundleResource(modelParameter.modleConfig.path);
     
        gameObject.name = "Model";
        if (gameObject)
        {
            return this.LoadModel(objRoot, gameObject, modelParameter);
        }
        //TODO 如果从Resources路径下无法找到对应的资源，则从assetBoundle中加载资源
        //BundleManager.Load;
        return false;
    }

    //改变模型，重新生成modelCompent
    public ModelComponent ChangeModel(GameObject objRoot, CModelParameter modelParameter, System.Type type = null)
    {
        ModelComponent component = objRoot.GetComponent<ModelComponent>();
        component.model.transform.parent = null; //解开原来模型与主节点的绑定

        ////解开右手武器绑定
        //GameObject rightWeapon = null;
        //if (component.rightWeapon != null)
        //{
        //    Quaternion localRotation = component.rightWeapon.transform.localRotation;
        //    Vector3 scale = component.rightWeapon.transform.localScale;
        //    component.rightWeapon.transform.parent = null;
        //    rightWeapon = component.rightWeapon;
        //    rightWeapon.transform.localRotation = localRotation;
        //    rightWeapon.transform.localScale = scale;
        //}

        ////解开左手武器绑定
        //GameObject leftWeapon = null;
        //if (component.leftWeapon != null)
        //{
        //    Quaternion localRotation = component.leftWeapon.transform.localRotation;
        //    Vector3 scale = component.leftWeapon.transform.localScale;
        //    component.leftWeapon.transform.parent = null;
        //    leftWeapon = component.leftWeapon;
        //    leftWeapon.transform.localRotation = localRotation;
        //    leftWeapon.transform.localScale = scale;
        //}

        //GameObject.DestroyImmediate(component.model);
        ResourceManager.DestroyResource(component.model, true);

        if (type == null)
            type = component.GetType();

        GameObject.DestroyImmediate(component);

        component = (ModelComponent)objRoot.AddComponent(type); //模型类重新生成
        this.ReloadModel(objRoot, modelParameter); //重新绑定模型

        //LoadRightWeapon(objRoot, rightWeapon);
        //LoadLeftWeapon(objRoot, leftWeapon);

        //少发送一个自身的事件，也可能不发

        return component;
    }

    //根据配置的情况,增加主控脚本，创建ModelCompent对象
    public ModelComponent CreateModelGameObject(CModelParameter modelParameter, Vector3 position, Quaternion rotation)
    {
        System.Type type = ModelComponent.GetModelType(modelParameter.modleConfig.type);
        return CreateModelGameObject( modelParameter, type,  position, rotation);
    }

    //创建ModelCompent对象
    public ModelComponent CreateModelGameObject(CModelParameter modelParameter, System.Type type, Vector3 position, Quaternion rotation)
    {
        GameObject gameObject = ResourceManager.InstantiateResource(position, rotation, "EntityPrefab/Common");
        gameObject.name = modelParameter.modleConfig.name;
        gameObject.layer = (int)modelParameter.layer;

        ModelComponent compent = null;
        if (type != null)
            compent = gameObject.AddComponent(type) as ModelComponent;

        this.ReloadModel(gameObject, modelParameter);

        return compent;
    }

    //创建ModelCompent对象
    public ModelComponent CreateModelGameObject(CModelParameter modelParameter, System.Type type)
    {
        GameObject gameObject = ResourceManager.InstantiateAssetBundleResource("EntityPrefab/Common");
        gameObject.name = modelParameter.modleConfig.name;
        gameObject.layer = (int)modelParameter.layer;

        ModelComponent compent = null;
        if (type != null)
            compent = gameObject.AddComponent(type) as ModelComponent;

        this.ReloadModel(gameObject, modelParameter);
        return compent;
    }

    //创建GameObject对象
    public ModelComponent CreateModelGameObject(GameObject obj, CModelParameter modelParameter, System.Type type)
    {
        obj.layer = (int)modelParameter.layer;

        obj.AddComponent(type);

        this.ReloadModel(obj, modelParameter);

        return obj.GetComponent<ModelComponent>();
    }

    public EffectComponent CreateEffectGameObject(CEffectParameter effectParameter, System.Type type)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(effectParameter.effectConfig.path)) as GameObject;

        EffectComponent compent = null;
        if (type != null)
            compent = gameObject.AddComponent(type) as EffectComponent;

        compent.Init(effectParameter);
        return compent;
    }

    public bool RemoveEffectGameObject(EffectComponent compent)
    {
        if (compent == null)
            return false;

        GameObject obj = compent.gameObject;
        obj.transform.parent = null;
        UnityEngine.Object.Destroy(compent);
        ResourceManager.DestroyResource(obj, false);

        return true;
    }

    //从场景中删除指定的对象
    public bool ReomoveModelGameObject(ModelComponent component)
    {     
        if (component)
        {
            if (component.model != null)
            {

                component.model.transform.parent = null; //解开原来模型与主节点的绑定    
                ResourceManager.Destroy(component.model,component.path,true);
                component.model = null;
            }

            GameObject obj = component.gameObject;

            UnityEngine.Object.Destroy(obj);
            UnityEngine.Object.Destroy(component);
            return true;
        }

        return false;
    }

    //改变GameObject层级
    public void ChangeGameObjectLayer(GameObject objModel, int layer)
    {
        Transform[] componentInChild = objModel.GetComponentsInChildren<Transform>();
        for (int i = 0; i < componentInChild.Length; i++)
        {
            componentInChild[i].gameObject.layer = layer;

        }
    }
}
