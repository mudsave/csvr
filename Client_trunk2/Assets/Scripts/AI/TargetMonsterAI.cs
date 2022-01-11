using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMonsterAI : MonoBehaviour {
    public enum BornType //出生类型
    {
        NUll,
        Born_Dissolve, //溶解出生
    }
    public BornType bornType = BornType.Born_Dissolve;
    public GameObject cuttingDeathObject;
    Animator m_animator;
    private AvatarComponent own;
    float lastTime = 0.0f;
    bool bornFlag = false;

    // Use this for initialization
    void Awake()
    {
        if (bornType == BornType.Born_Dissolve)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
    }


    void Start()
    {
        lastTime = Time.time;
        own = gameObject.GetComponent<AvatarComponent>();
        m_animator = this.GetComponent<Animator>();
        born();
        own.eventObj.register("Event_OnDead", this, "onDead");
    }


    public void onDead(CDeadType deadType)
    {
        switch (deadType)
        {
            case CDeadType.None:
                break;
            case CDeadType.Normal:
                m_animator.SetBool("die", true);
                StartCoroutine(DelayDestroy(3.0f));
                break;
            case CDeadType.Dissolution:
                m_animator.SetBool("die", true);
                own.effectManager.AddModelEffect("DissolveEffect");
                StartCoroutine(DelayDestroy(6.0f));
                break;
            case CDeadType.Cutting:
                own.HideModel();
                GameObject obj = Instantiate(cuttingDeathObject, transform.position, transform.rotation) as GameObject;
                StartCoroutine(DelayDestroy(6.0f));
                break;
            case CDeadType.Fracture:
                own.HideModel();
                own.effectManager.AddModelEffect("FractureEffect");
                StartCoroutine(DelayDestroy(6.0f));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 延迟销毁gameObject
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        own.Destroy();
    }

    void born()
    {
        if (!bornFlag)
        {
            m_animator.SetInteger("runInt", 0);
            bornFlag = true;
            if (own)
            {
                if (bornType == BornType.Born_Dissolve)
                {
                    own.effectManager.AddModelEffect("InverseDissolveEffect");
                }
            }
        }
    }
}
