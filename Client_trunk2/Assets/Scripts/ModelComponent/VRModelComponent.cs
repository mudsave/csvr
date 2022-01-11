using System.Collections;
using UnityEngine;

public class VRModelComponent : ModelComponent
{
    public Transform eyeTransform = null;
    public Transform roomTransform = null;
    public Transform leftController = null;
    public Transform rightController = null;
    public Transform leftHandModel = null;
    public Transform rightHandModel = null;

    private void Awake()
    {
        eyeTransform = VRInputManager.Instance.head.transform;
        roomTransform = VRInputManager.Instance.room.transform;
        leftController = VRInputManager.Instance.handLeft.transform;
        rightController = VRInputManager.Instance.handRight.transform;
        leftHandModel = VRInputManager.Instance.handLeftModel.transform;
        rightHandModel = VRInputManager.Instance.handRightModel.transform;
    }

    //加载武器模型（删除原先的武器模型）
    public override bool LoadWeapon(CModelParameter modelParameter)
    {
        if (modelParameter.modleConfig.path.Length <= 0)
        {
            return false;
        }
        DeleteWeapon();
        ModelComponent weaponObject = CompentCreateManager.Instance.CreateModelGameObject(modelParameter, typeof(ModelComponent));
        weaponObject.tag = "Staff";
        BindingGameObject(weaponObject, "rightWeapon", CModelNodePath.vr_r_h, false);
        weaponObject.transform.localScale = Vector3.one * 0.5f;
        return true;
    }

    //绑定主模型下的节点
    public void BindingGameObject(ModelComponent compent, string thingName, string nodename, bool isRotationCalc = true)
    {
        if (compent == null)
        {
            return;
        }
        Transform weaponPoint = gameObject.transform.Find(nodename);

        Quaternion localRotation = compent.transform.localRotation;
        Vector3 scale = compent.transform.localScale;
        compent.transform.parent = weaponPoint;
        compent.transform.localPosition = Vector3.zero;
        compent.transform.localScale = scale;
        if (isRotationCalc)
            compent.transform.localRotation = Quaternion.Euler(0, -90, -90) * localRotation;
        else
            compent.transform.localRotation = localRotation;

        Transform[] componentsInChildren = compent.GetComponentsInChildren<Transform>();
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            componentsInChildren[j].gameObject.layer = gameObject.layer;
        }

        bindingGameObject[thingName] = new NodeClass(nodename, compent);
    }
}