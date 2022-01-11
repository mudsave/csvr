using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign
{
    /// <summary>
    /// 回合测试关卡
    /// </summary>
    public class ElemTurnBased : LevelElement
    {
        public int rows = 4;                // 行数
        public int columns = 4;　　　　　　 // 列数
        public GameObject[] Objects;        // 欲出生的对象预制体
        public float respawnDelay = 0.0f;   // 下一次重生时的延迟时间（单位：秒）

        private int maxNumber;
        private List<Vector3> positionList = new List<Vector3>();

        private int currentPassCount = 0;
        private float m_respawnTime = 0.0f;
        private int level = 0;

        public override void OnActive()
        {
            maxNumber = rows * columns;

            float x = 0;
            float z = 0;
            float y = transform.position.y;

            for (int i = 0; i < rows; i++)
            {
                z = transform.position.z + i * 2.0f;
                for (int j = 0; j < columns; j++)
                {
                    x = transform.position.x + j * 2.0f;
                    positionList.Add(new Vector3(x, y, z));
                }
            }
            SpawnObjects();
        }

        public void SpawnObjects()
        {
            if (Objects.Length == 0)
                return;

            TurnManager.Instance.Init();

            for (int i = 0; i < maxNumber; i++)
            {
                GameObject obj = Instantiate(Objects[Random.Range(0, Objects.Length)], positionList[i], transform.rotation) as GameObject;

                AvatarComponent avatarComponent = obj.GetComponent<AvatarComponent>();
                if (avatarComponent)
                {
                    avatarComponent.MaxHP += (int)(avatarComponent.MaxHP * level * 0.1f);
                    avatarComponent.HP = avatarComponent.MaxHP;
                    avatarComponent.actionTime = Random.Range(5, 10);
                    TurnManager.Instance.AddBattleList(avatarComponent);
                }

                ElemPassOnDestroy component = obj.AddComponent<ElemPassOnDestroy>();
                component.parent = this;
                component.Active();
            }

            TurnManager.Instance.StartBattle();
        }

        public override void ChildLevelPassed(LevelElement child)
        {
            currentPassCount++;
            if (currentPassCount >= maxNumber)
            {
                currentPassCount = 0;
                m_respawnTime = Time.time + respawnDelay;
            }
        }

        public void FixedUpdate()
        {
            if (m_respawnTime > 0.0f && Time.time >= m_respawnTime)
            {
                m_respawnTime = 0.0f;
                SpawnObjects();
            }
        }

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            base.OnDrawGizmos();
            float x = 0;
            float z = 0;
            float y = transform.position.y;

            for (int i = 0; i < rows; i++)
            {
                z = transform.position.z + i * 2.0f;
                for (int j = 0; j < columns; j++)
                {
                    x = transform.position.x + j * 2.0f;
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(new Vector3(x, y, z), 0.5f);
                }
            }
#endif
        }
    }
}
