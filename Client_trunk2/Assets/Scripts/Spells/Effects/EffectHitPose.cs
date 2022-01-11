using UnityEngine;
using System.Collections;

namespace SPELL
{
	/// <summary>
	/// 受击动作组件
	/// </summary>
	public class EffectHitPose : SpellEffect
	{
		//public static string[] s_animationName = { "hit_normal", "hit_up", "hit_down", "hit_back" };

		public string bindingObjectPath = "";        // 标识技能和声音会在对象的哪个子对象上播放，例如：Bip01/r_h/s014_lod0/WeaponTrail
        public int hitPoseType;                      // animation name which definition in Animator.
		public string effect = "";                   // effect name with prefab
		public string sound = "";                    // sound name

		public override void Init(DataSection.DataSection dataSection)
		{
			base.Init(dataSection);
            hitPoseType = dataSection.readInt("param1");
            bindingObjectPath = dataSection.readString("param2");
            effect = dataSection.readString("param3");
            sound = dataSection.readString("param4");
		}

		/// <summary>
		/// caster do something to dst use by spell
		/// </summary>
		/// <param name="spell">Spell.</param>
		/// <param name="src">spell caster.</param>
		/// <param name="dst">spell receiver.</param>
		public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
		{

			Animator animator = dst.animator;
            if (animator)
            {
                animator.SetInteger("hit_pose", hitPoseType);
                animator.SetTrigger("hit");
            }

			Transform transf = dst.myTransform;
			if (bindingObjectPath != "")
			{
				transf = transf.FindChild(bindingObjectPath);
				if (!transf)
					transf = dst.myTransform;  // set to src position if not found
			}

			// 播放光效
			if (effect != "")
                dst.effectManager.AddEffect(effect, transf.position);

            // 播放声音
            if (sound != "")
				SoundFXManager.Instance.PlaySound(sound, transf);
		}
		
	}
}
