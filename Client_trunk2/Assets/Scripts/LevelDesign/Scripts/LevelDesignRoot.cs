using UnityEngine;
using System.Collections;

namespace LevelDesign
{
	[AddComponentMenu("LevelDesign/LevelDesignRoot")]
	public class LevelDesignRoot : ElemQueue {
		public bool activeOnStart = true;

		// Use this for initialization
		public void Start ()
        {
            this.Init();

			if (activeOnStart)
				Active();
            KBEngine.Event.registerOut("Event_LevelEnd", this, "StoryLevelEnd");
		}

        //关卡结束反注册事件
        public void StoryLevelEnd(bool result)
        {
            BroadcastMessage("ReceiveLevelEndMessage", null, SendMessageOptions.DontRequireReceiver);
        }

        void OnDestroy()
        {
            KBEngine.Event.deregisterOut("Event_LevelEnd", this, "StoryLevelEnd");
        }

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            base.OnDrawGizmos();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
#endif
        }
    }
}
