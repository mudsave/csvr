using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSAvaterModelCoponent : ModelComponent
{

    //加载武器模型（删除原先的武器模型）
    public override bool LoadWeapon(CModelParameter modelParameter)
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
            BindingGameObject(weaponObject, "rightWeapon", CModelNodePath.cs_r_h);
        }
        else if (modelParameter.modleConfig.equipPoint == CWeaponNode.leftHand)
        {
            ModelComponent weaponObject = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(weaponObject, "leftWeapon", CModelNodePath.cs_l_h);
        }
        else if (modelParameter.modleConfig.equipPoint == CWeaponNode.bothHand)
        {
            ModelComponent rightWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(rightWeapon, "rightWeapon", CModelNodePath.cs_r_h);
            ModelComponent leftWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(leftWeapon, "leftWeapon", CModelNodePath.cs_l_h);
        }
        else if (modelParameter.modleConfig.equipPoint == CWeaponNode.bothHandTurn)
        {
            ModelComponent rightWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            rightWeapon.transform.Rotate(new Vector3(0, 0, 180));
            BindingGameObject(rightWeapon, "rightWeapon", CModelNodePath.cs_r_h);
            ModelComponent leftWeapon = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
            BindingGameObject(leftWeapon, "leftWeapon", CModelNodePath.cs_l_h);
        }
        return true;
    }
}
