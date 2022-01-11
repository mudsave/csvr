using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelFractureEffectComponent : EffectComponent
{
    GameObject fracturedObject;
    public float durationTime;
    public override void Init(CEffectParameter modelParameter)
    {
        base.Init(modelParameter);
        durationTime = (float)(double)modelParameter.effectConfig.args[0];
    }

    public override void StartEffect()
    {
        Object _fractued = ResourceManager.LoadAssetBundleResource("MonsterFractured");
        fracturedObject = Instantiate(_fractued,transform.position, transform.rotation) as GameObject;

        DestroyFractureObjectOnTime _destroyObject = fracturedObject.GetComponent<DestroyFractureObjectOnTime>();
        if(_destroyObject)
        {
            _destroyObject.time = durationTime;
        }   
    }

    public override void DestroyEffect()
    {
        AvatarComponent owner = AvatarComponent.GetAvatar(m_ownerID);
        if (owner && owner.effectManager != null)
        {
            owner.effectManager.RemoveModelEffect(m_effectName);
        }
        else
        {
            Destroy(this);
        }
    }
}
