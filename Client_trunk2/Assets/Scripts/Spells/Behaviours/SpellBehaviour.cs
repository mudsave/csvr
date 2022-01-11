using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public class EffectsData
    {
        // 目前只存放光效，以后可能会存放音效等资源实例
        public GameObject effect = null; //光效
        public AudioSource audioSource = null; //音效播放组件

        // 播放光效
        public void EffectPlay(string effectName, Transform ObjTransform, bool bindEffect = false)
        {
            effect = EffectManager.Instance.PlayEffect(effectName, ObjTransform, bindEffect);
        }

        public void EffectPlay(string effectName, Vector3 position)
        {
            effect = EffectManager.Instance.PlayEffect(effectName, position);
        }

        // 销毁光效
        public void EffectDestroy()
        {
            if (effect != null)
            {
                effect.Recycle();
            }
        }

        // 播放音效
        public void AudioPlay(GameObject obj, string soundName)
        {
            audioSource = obj.AddComponent<AudioSource>();
            audioSource.clip = (AudioClip)SoundFXManager.Instance.GetSoundMetaData(soundName).soundFX;
            audioSource.Play();
        }

        // 停止播放音效并销毁AudioSource组件
        public void AudioStop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                GameObject.Destroy(audioSource);
            }
        }

        // 销毁整个效果实例，包括了光效和音效
        public void destroyAll(AvatarComponent caster, float dalayTime)
        {
            if (dalayTime > 0.0f)
            {
                caster.StartCoroutine(delayDestroy(dalayTime));
            }
            else
            {
                EffectDestroy();
                AudioStop();
            }

        }

        IEnumerator delayDestroy(float dalayTime)
        {
            yield return new WaitForSeconds(dalayTime);
            EffectDestroy();
            AudioStop();
        }

    }

    [System.Serializable]
    public class SpellAnimation
    {
        public string bindingObjectPath = "";  // 标识技能和声音会在对象的哪个子对象上播放，例如：Bip01/r_h/s014_lod0/WeaponTrail
        public string animation = "";  // animation name which definition in Animator.
        public string effect = "";  // effect name with prefab
        public string sound = "";  // sound name
        public bool bindEffect = false; //Whether or not bind effects on object
        //public eAnimatPosSyncFlag collisionType = eAnimatPosSyncFlag.NotSync;

        public void Init(DataSection.DataSection dataSection)
        {
            bindingObjectPath = dataSection.readString("bindingObjectPath");
            animation = dataSection.readString("animation");
            effect = dataSection.readString("effect");
            sound = dataSection.readString("sound");
            bindEffect = dataSection.readBool("bindEffect");
            //collisionType = (eAnimatPosSyncFlag)dataSection.readInt("collisionType");
        }

        public EffectsData Do(AvatarComponent obj)
        {
            // 播放动作
            if (animation != "")
            {
                //EntityComponent entity = obj.GetComponent<EntityComponent>();
                Animator animator = obj.GetComponent<Animator>();
                //animator.speed = entity.attackSpeed;
                if (animator != null)
                    animator.Play(animation);
                //obj.BroadcastMessage("WeaponAnimationPlayMessage", animation, SendMessageOptions.DontRequireReceiver);
            }

            Transform transf = obj.transform;
            if (bindingObjectPath != "")
            {
                transf = transf.FindChild(bindingObjectPath);
                if (!transf)
                    transf = obj.transform;  // set to src position if not found
            }

            EffectsData effectsData = new EffectsData();
            // 播放光效
            if (effect != "")
            {
                if (bindEffect)
                    obj.effectManager.AddEffect(effect, transf);
                else
                    obj.effectManager.AddEffect(effect, transf.position);
            }

            // 播放声音
            if (sound != "" && SoundFXManager.Instance != null)
            {
                effectsData.AudioPlay(obj.gameObject, sound);
            }
            return effectsData;
        }

    }

    [System.Serializable]
    public class SpellLightEffect
    {
        public string bindingObjectPath = "";  // 标识技能和声音会在对象的哪个子对象上播放，例如：Bip01/r_h/s014_lod0/WeaponTrail
        public string effect = "";  // effect name with prefab
        public string sound = "";  // sound name
        public bool bindEffect = false; //Whether or not bind effects on object

        public void Init(DataSection.DataSection dataSection)
        {
            bindingObjectPath = dataSection.readString("bindingObjectPath");
            effect = dataSection.readString("effect");
            sound = dataSection.readString("sound");
            bindEffect = dataSection.readBool("bindEffect");
        }

        public EffectsData Do(AvatarComponent obj)
        {
            Transform transf = obj.transform;
            if (bindingObjectPath != "")
            {
                transf = transf.FindChild(bindingObjectPath);
                if (!transf)
                    transf = obj.transform;  // set to src position if not found
            }

            EffectsData effectsData = new EffectsData();
            // 播放光效
            if (effect != "")
            {
                if (bindEffect)
                    obj.effectManager.AddEffect(effect, transf);
                else
                    obj.effectManager.AddEffect(effect, transf.position);
            }

            // 播放声音
            if (sound != "")
                effectsData.AudioPlay(obj.gameObject, sound);

            return effectsData;
        }
    }


    public abstract class SpellBehaviour
    {
        /* @param spell: 使用这个行为的技能使用
		 * @param receiver: 接受这个行为的对象
		 * @return: true = 需要继续调用Update()
		 */
        public virtual bool BehaviourStart(SpellEx spell, AvatarComponent caster)
        {
            return false;
        }

        /* @param spell: 使用这个行为的技能使用
		 * @param receiver: 接受这个行为的对象
		 * @return: true = 需要继续调用Update()
		 */
        public virtual bool BehaviourUpdate(SpellEx spell, AvatarComponent caster)
        {
            return false;
        }

    }
}
