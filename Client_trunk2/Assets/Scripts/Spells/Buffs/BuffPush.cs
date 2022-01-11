using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

namespace SPELL
{
    /// <summary>
    /// 将目标往一个方向推的buff
    /// </summary>
    public class BuffPush : SpellBuff
    {

        public int hitPoseType;  // 击退类型
        public int referenceType;
        public float offsetAngle = 0.0f;
        public float distance; //击退距离
        public float pushSpeed = 0.0f;
        public float pushTime = 0.0f;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            DataSection.DataSection generalFunction = dataSection["generalFunction"];
            referenceType = generalFunction.readInt("referenceType");
            offsetAngle = generalFunction.readFloat("offsetAngle");
            distance = generalFunction.readFloat("distance");
            pushTime = generalFunction.readFloat("pushTime");
            hitPoseType = generalFunction.readInt("hitPoseType");
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            pushSpeed = distance / pushTime;

            if (pushSpeed > 0.0)
            {
                AvatarComponent caster = AvatarComponent.GetAvatar(buffData.casterID);
                
                Vector3 endPoint = Vector3.zero;
                if (referenceType == 0) 
                {
                    Vector3 dir = Quaternion.Euler(0, -offsetAngle, 0) * caster.myTransform.forward;
                    endPoint = owner.transform.position + dir * distance;
                }
                else if (referenceType == 1)
                {
                    Vector3 dir = Quaternion.Euler(0, -offsetAngle, 0) * owner.transform.forward;
                    endPoint = owner.transform.position + dir * distance;
                }
                else if (referenceType == 2)
                {
                    Vector3 dir = Quaternion.Euler(0, -offsetAngle, 0) * ((owner.transform.position - caster.transform.position).normalized);
                    endPoint = owner.transform.position + dir * distance;
                }

                UnityEngine.AI.NavMeshHit navMeshHit;
                if (UnityEngine.AI.NavMesh.Raycast(owner.transform.position, endPoint, out navMeshHit, -1)) 
                {
                    endPoint = navMeshHit.position;
                }
                else if (UnityEngine.AI.NavMesh.SamplePosition( endPoint, out navMeshHit, 2.0f, -1)) 
                {
                    endPoint = navMeshHit.position;
                }

                //将目标点存在buff本地的临时数据里
                buffData.localBuffData["endPoint"] = endPoint;
            }

            Animator animator = owner.animator;
            if (animator)
            {
                animator.SetInteger("hit_pose", hitPoseType);
                animator.SetTrigger("hit");
            }

            owner.EffectStatusCounterIncr((int)eEffectStatus.HitBy);
        }

        protected override bool OnTick(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            if (pushSpeed > 0.0f)
            {
                if (Vector3.Distance(owner.transform.position, (Vector3)buffData.localBuffData["endPoint"]) > 0.1f)
                {
                    owner.transform.position = Vector3.MoveTowards(owner.transform.position, (Vector3)buffData.localBuffData["endPoint"], Time.deltaTime * pushSpeed);
                    return true;
                }
            }
            return false;
        }

        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.EffectStatusCounterDecr((int)eEffectStatus.HitBy);
        }

    }
}