using UnityEngine;
using System.Collections;

namespace SPELL
{
    /// <summary>
    /// 闪电链Buff
    /// </summary>
    public class BuffLightning : SpellBuff
    {

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            AudioManager.Instance.SoundPlay("电流-发射");
            EffectComponent eComponent = owner.effectManager.AddEffect("Lightning", owner.transform.position);
            owner.effectManager.AddEffect("LightningHand02", VRInputManager.Instance.handLeft.transform);

            GameObjComponent dstComponent = GameObjComponent.GetEntity((int)buffData.misc["targetID"]);
            UVChainLightning[] uvs = eComponent.gameObject.GetComponentsInChildren<UVChainLightning>();

            Transform dstTransform = null;
            if (dstComponent == null)
            {
                GameObject empty = new GameObject();
                Vector3 dir = new Vector3((float)(double)(buffData.misc["castDirection"][0]), (float)(double)(buffData.misc["castDirection"][1]), (float)(double)(buffData.misc["castDirection"][2]));
                empty.transform.position = VRInputManager.Instance.tip_nib_right.transform.position + dir * 3;
                empty.transform.SetParent(eComponent.gameObject.transform);
                dstTransform = empty.transform;
                buffData.localBuffData["empty"] = empty;
                owner.effectManager.AddEffect("LightningHitEmpty", dstTransform);
            }
            else
            {
                dstTransform = dstComponent.myTransform;
                foreach (UVChainLightning uv in uvs)
                {
                    uv.dstYOffset = 1.0f;
                }
            }

            if (uvs.Length > 0)
            {
                foreach (UVChainLightning uv in uvs)
                {
                    uv.target = dstTransform;
                    uv.start = VRInputManager.Instance.handLeft.transform;
                    //uv.displacement = 1;
                }
            }

        }

        /// <summary>
        /// 把buff从owner身上取下来
        /// </summary>
        /// <param name="owner">拥有这个buff的人</param>
        /// <param name="buffData">存储的buff数据</param>
        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
        }

        //protected override void OnSpecificAction(AvatarComponent owner, Alias.BuffDataType buffData, Alias.BuffDataType newData)
        //{
        //    AudioManager.Instance.SoundPlay("电流-发射");
        //    EffectComponent eComponent = owner.effectManager.AddEffect("Lightning", owner.transform.position);
        //    GameObjComponent srcComponent = GameObjComponent.GetEntity((int)newData.misc["srcObj"]);
        //    GameObjComponent dstComponent = GameObjComponent.GetEntity((int)newData.misc["dstObj"]);

        //    UVChainLightning[] uvs = eComponent.gameObject.GetComponentsInChildren<UVChainLightning>();
        //    if (uvs.Length > 0)
        //    {
        //        foreach (UVChainLightning uv in uvs)
        //        {
        //            uv.target = srcComponent.myTransform;
        //            uv.start = dstComponent.myTransform;
        //            uv.srcYOffset = 1.0f;
        //            uv.dstYOffset = 1.0f;

        //        }

        //    }
        //}


    }
}
