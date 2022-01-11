using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 投掷电球技能
    /// </summary>
    public class LightningballSkill : MonoBehaviour
    {
       
        private Vector3 m_lastPosition;
        private Rigidbody m_rigidbody;

        private EffectComponent m_onGroundEffect = null;

        void Start()
        {
            GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
            GlobalEvent.register("OnTriggerReleased", this, "OnReleased");
        }

        public void OnPressed(VRControllerEventArgs e)
        {
            //if (e.hand == Hand.LEFT)
            //{
            //}
        }

        public void OnReleased(VRControllerEventArgs e)
        {
            if (e.hand == Hand.LEFT)
            {
                VRInputManager.Instance.handLeftAnimator.SetBool("launch", false);

                GlobalEvent.deregister(this);

                m_rigidbody = gameObject.AddComponent<Rigidbody>();
                gameObject.transform.SetParent(null);

                Transform origin = VRInputManager.Instance.handLeft.transform.parent;
                m_rigidbody.velocity = origin.TransformVector(VRInputManager.Instance.GetControllerEvents(Hand.LEFT).GetVelocity());
                m_rigidbody.angularVelocity = origin.TransformVector(VRInputManager.Instance.GetControllerEvents(Hand.LEFT).GetAngularVelocity());
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == (int)eLayers.Diban)
            {
                EffectComponent e = gameObject.GetComponent<EffectComponent>();
                e.DestroyEffect();

                Vector3 closestPoint = other.ClosestPointOnBounds(gameObject.transform.position);

                VRInputManager.Instance.playerComponent.effectManager.AddEffect("lightningball_ground", closestPoint);
                m_onGroundEffect = VRInputManager.Instance.playerComponent.effectManager.AddEffect("lightning_ground", closestPoint);
                m_onGroundEffect.gameObject.transform.LookAt(VRInputManager.Instance.playerComponent.gameObject.transform.position);

                LightningballOnGround og = m_onGroundEffect.gameObject.AddComponent<LightningballOnGround>();
                og.Init(closestPoint);
            }
        }
    }
}
