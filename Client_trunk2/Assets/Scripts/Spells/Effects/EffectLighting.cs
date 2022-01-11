using UnityEngine;
using System.Collections;
namespace SPELL
{
	/// <summary>
	/// 播放特效声音的技能效果
	/// </summary>
	public class EffectLighting : SpellEffect
	{		
		public string bindingObjectPath = "";                      
		public string effect = "";
		public string sound = "";
		public bool isBind = false;
        public float posOffset = 0;
        public bool setRotation = false; //强制朝向

        public override void Init(DataSection.DataSection dataSection)
		{
			base.Init(dataSection);
			bindingObjectPath = dataSection.readString("param1");
			effect = dataSection.readString("param2");
			sound = dataSection.readString("param3");
			isBind = dataSection.readBool("param4");
            posOffset = dataSection.readFloat("param5");
            setRotation = dataSection.readBool("param6");
        }
		
		/// <summary>
		/// caster do something to dst use by spell
		/// </summary>
		/// <param name="spell">Spell.</param>
		/// <param name="src">spell caster.</param>
		/// <param name="dst">spell receiver.</param>
		public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
		{
			Transform transf = dst.myTransform;
			if (bindingObjectPath != "")
			{
				transf = transf.FindChild(bindingObjectPath);
				if (!transf)
					transf = dst.myTransform;
			}

            EffectComponent eComponent = null;
            // 播放光效
            if (effect != "")
            {
                if (isBind)
                    eComponent = dst.effectManager.AddEffect(effect, transf);
                else
                {
                    Vector3 pos = transf.position + transf.forward * posOffset;
                    eComponent = dst.effectManager.AddEffect(effect, pos);
                }

                //强制特效朝向    
                if (setRotation)
                {
                    eComponent.gameObject.transform.rotation = dst.myTransform.rotation;
                }
                    
            }

            // 播放声音
            if (sound != "")
                AudioManager.Instance.SoundPlay(sound);

            BulletMotionSkill bs = eComponent.gameObject.GetComponent<BulletMotionSkill>();
            if (bs)
                bs.Init(src);

        }
		
	}
}