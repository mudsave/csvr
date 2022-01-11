using UnityEngine;
using System.Collections;

namespace LevelDesign
	{
	/// <summary>
	/// 基础的怪物出生点,仅出生一个怪物，怪物死后即通关
	/// </summary>
	[AddComponentMenu("LevelDesign/LevelElement/SpawnPoint")]
	public class ElemSpawnPoint : LevelElement
	{
		public GameObject spawnObject;        // 欲出生的对象预制体
        public float respawnDelay = 0.0f;     // 下一次重生时的延迟时间（单位：秒）
        public int respawnCount = 0;          // 共重生几次，在重生次数未完结时，不会通关


        private int m_respawnCounter = 0;
        private float m_respawnTime = 0.0f;

        /// <summary>
        /// 关卡元素被激活时调用，用于自定义关卡执行具体事务
        /// </summary>
        public override void OnActive()
        {
            var obj = SpawnObject();
            if (obj == null)
            {
                LevelPass();
                return;
            }
		}

		public override void ChildLevelPassed( LevelElement child )
		{
            if (m_respawnCounter >= respawnCount)
                LevelPass();
            else
                m_respawnTime = Time.time + respawnDelay;

        }

        public void FixedUpdate()
        {
            if (m_respawnTime > 0.0f && Time.time >= m_respawnTime)
            {
                m_respawnTime = 0.0f;
                m_respawnCounter++;
                SpawnObject();
            }
        }

        public GameObject SpawnObject()
        {
            if (spawnObject == null)
                return null;

            GameObject obj = Instantiate(spawnObject, transform.position, transform.rotation) as GameObject;
            ElemPassOnDestroy component = obj.AddComponent<ElemPassOnDestroy>();
            component.parent = this;
            component.Active();
            return obj;
        }

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            base.OnDrawGizmos();

            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f);
#endif
        }
    }
}

