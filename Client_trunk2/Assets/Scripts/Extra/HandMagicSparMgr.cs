using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMagicSparMgr : MonoBehaviour
{
    public GameObject spawnObject = null;
    public Transform handMagicSparCenter = null;
    private int m_maxCount = 10;
    private int m_currentCount = 0;
    private GameObject[] m_sparArry = null;

    void Start()
    {
        if (spawnObject == null || handMagicSparCenter == null)
            return;

        m_sparArry = new GameObject[m_maxCount];

        Vector3 dir = handMagicSparCenter.up;
        Vector3 pos = handMagicSparCenter.position;

        float angle = 360 / m_maxCount;
        for (int i = 0; i < m_maxCount; i++)
        {
            dir = Quaternion.AngleAxis(angle, handMagicSparCenter.right) * dir;
            GameObject obj = Instantiate(spawnObject, pos + dir * 0.07f, handMagicSparCenter.rotation) as GameObject;
            obj.transform.parent = handMagicSparCenter;
            m_sparArry[i] = obj;
            obj.SetActive(false);
        }

        InstallGlobalEvent();
        PlayerComponent player = VRInputManager.Instance.playerComponent;

        if (player != null)
            Event_ChangeSparCount(player.MagicSpar);
    }

    private void InstallGlobalEvent()
    {
        GlobalEvent.register("Event_ChangeSparCount", this, "Event_ChangeSparCount");
    }

    void Update()
    {
        handMagicSparCenter.Rotate(Vector3.right, Space.Self);
    }

    public void Event_ChangeSparCount(int number)
    {
        int count = System.Math.Abs(number);

        int currentCount = 0;
        if (number > 0)
        {
            for (int i = 0; i < m_maxCount; i++)
            {
                if (m_sparArry[i].activeInHierarchy == false)
                {
                    m_sparArry[i].SetActive(true);
                    ++currentCount;
                    if (currentCount == count)
                        break;
                }
            }
        }
        else if (number < 0)
        {
            for (int i = m_maxCount-1; i >= 0; i--)
            {
                if (m_sparArry[i].activeInHierarchy == true)
                {
                    m_sparArry[i].SetActive(false);
                    ++currentCount;
                    if (currentCount == count)
                        break;
                }
            }
        }
    }
}
