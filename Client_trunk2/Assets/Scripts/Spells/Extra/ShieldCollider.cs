using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public class ShieldCollider : MonoBehaviour
    {
        private AvatarComponent _caster = null;
        private int m_index;
        private float waveSpeed = 0.0f;
        
        public SPELL.SpellEffect effect1;
        public SPELL.SpellEffect effect2;
        public SPELL.SpellEffect effect3;

        private MeshRenderer meshRenderer;

        private Material shield;
        private Material shieldlie1;
        private Material shieldlie2;
        private Material shieldlie3;

        private List<AvatarComponent> TriggerList = new List<AvatarComponent>();

        public AvatarComponent caster
        {
            get { return _caster; }
        }

        public void Init(AvatarComponent component, int index)
        {
            _caster = component;
            m_index = index;

            effect1 = SpellLoader.instance.GetEffect(3001001); //击退2米
            effect2 = SpellLoader.instance.GetEffect(3001002); //击退5米

            effect3 = SpellLoader.instance.GetEffect(3003001);

            meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
            shield = meshRenderer.material;

            Object materialObj = Resources.Load("Effects/VR_Shield/Materials/T_VR_dun_D_1");

            if (materialObj != null)
            {
                shieldlie1 = (Material)UnityEngine.Object.Instantiate(materialObj);
            }

            materialObj = Resources.Load("Effects/VR_Shield/Materials/T_VR_dun_D_2");

            if (materialObj != null)
            {
                shieldlie2 = (Material)UnityEngine.Object.Instantiate(materialObj);
            }

            materialObj = Resources.Load("Effects/VR_Shield/Materials/T_VR_dun_D_3");

            if (materialObj != null)
            {
                shieldlie3 = (Material)UnityEngine.Object.Instantiate(materialObj);
            }
        }

        void Start()
        {
        }

        void OnTriggerEnter(Collider other)
        {
            ColliderComponent component = other.gameObject.GetComponent<ColliderComponent>();
      
            if (component)
            {

                if (caster.energyMgr)
                {
                    caster.energyMgr.ChangeEnergy(-25);
                    caster.energyMgr.StopRecovery(1);
                }
                //TriggerList.Add(component.caster);
                effect3.Cast(caster, component.caster, null, null);
                VRInputManager.Instance.Shake(Hand.LEFT, 1500, 0.1f, 0.01f);
                return;
            }

            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst)
            {
                if (caster.CheckRelationship(dst) == eTargetRelationship.HostileMonster && dst.status != eEntityStatus.Death)
                {
                    //if (waveSpeed > 5.0f)
                    //    effect2.Cast(caster, dst, null, null);
                    //else if (waveSpeed > 1.5f)
                    //    effect1.Cast(caster, dst, null, null);

                    //if (caster.energyMgr)
                    //{
                    //    caster.energyMgr.ChangeEnergy(-25);
                    //    caster.energyMgr.StopRecovery(1);
                    //}
                    TriggerList.Add(dst);
                    VRInputManager.Instance.Shake(Hand.LEFT, 1500, 0.1f, 0.01f);
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            waveSpeed = VRInputManager.Instance.GetControllerEvents(Hand.LEFT).GetVelocity().magnitude;
            if (waveSpeed > 2.0f)
            {
                foreach (AvatarComponent dst in TriggerList)
                {
                    effect2.Cast(caster, dst, null, null);
                }
                TriggerList.Clear();
            }
        }

        void OnTriggerExit(Collider other)
        {
            ColliderComponent component = other.gameObject.GetComponent<ColliderComponent>();
            if (component)
            {
                TriggerList.Remove(component.caster);
                return;
            }

            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst)
            {
                TriggerList.Remove(dst);
            }
        }

        void Update()
        {
            if (caster.energyMgr.CurrentEnergyValue <= 75 && caster.energyMgr.CurrentEnergyValue >= 50)
            {
                meshRenderer.material = shieldlie1;
            }
            else if (caster.energyMgr.CurrentEnergyValue <= 50 && caster.energyMgr.CurrentEnergyValue >= 25)
            {
                meshRenderer.material = shieldlie2;
            }
            else if (caster.energyMgr.CurrentEnergyValue <= 25)
            {
                meshRenderer.material = shieldlie3;
            }
            else
            {
                meshRenderer.material = shield;
            }
        }
    }
}