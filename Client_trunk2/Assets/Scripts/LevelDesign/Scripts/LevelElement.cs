using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LevelDesign
{
	public abstract class LevelElement : MonoBehaviour {

        /// <summary>
        ///  父对象
        /// </summary>
		public LevelElement parent = null;

        /// <summary>
        /// 标识这个关卡元素是否已被激活，只有被激活的对象才能自动处理一些事情.
        /// </summary>
        public bool isActive
		{
            get;
            protected set;
		}

        // 标识玩家是否通过这个关卡
        public bool isPassed
		{
            get;
            protected set;
		}

		/// <summary>
		/// 激活此元素的功能
		/// </summary>
		public void Active()
		{
			if (isActive)
				throw new System.Exception( this + "::Active(), I was actived, may be somewhere has logic error." );

            isActive = true;
			gameObject.SetActive( true );
			OnActive();
		}

		/// <summary>
		/// 关卡元素被激活时调用，用于自定义关卡执行具体事务
		/// </summary>
		public virtual void OnActive()
		{
		}

        public virtual void Init()
        {
        }

		/// <summary>
		/// 通关
		/// </summary>
		public void LevelPass()
		{
			if (isPassed)
				throw new System.Exception( this + "::LevelPass(), I was passed, may be somewhere has logic error." );

            isActive = false;
            isPassed = true;
			SendMessage( "SignalOnLevelPass", SendMessageOptions.DontRequireReceiver );
			if (parent)
				parent.ChildLevelPassed( this );
		}

		/// <summary>
		/// 由子关卡元素调用，告诉其父类自己已被通关。
		/// </summary>
		/// <param name="child">Child of level element.</param>
		public virtual void ChildLevelPassed( LevelElement child )
		{

		}

        public void ReceiveLevelEndMessage()
        {
            KBEngine.Event.deregisterOut(this);
        }

        public virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (transform.parent)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, transform.parent.position);

                //TextMesh text = transform.GetComponentInChildren<TextMesh>();
                ////if (!text)
                ////{
                ////    text = this.gameObject.AddComponent<TextMesh>();
                ////    text.text = "出生点";
                ////    text.characterSize = 0.25f;
                ////    text.anchor = TextAnchor.LowerCenter;
                ////}
                //Camera targetCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
                //if (targetCamera != null && text != null)
                //{
                //    text.transform.rotation = targetCamera.transform.rotation;

                //    float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
                //    float scale = distance / 8;
                //    text.transform.localScale = new Vector3(scale, scale, 1);
                //}
            }
#endif
        }
    }

}
