using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 选择可战斗目标
/// </summary>
public class VRInputSelectTarget : MonoBehaviour
{
    private static VRInputSelectTarget s_instance;

    private AvatarComponent s_avatarTarget = null;
    private Transform head = null;

    private bool isSelect = false;

    public static VRInputSelectTarget Instance
    {
        get
        {
            if (s_instance == null)
            {
                Debug.LogError("VRInputSelectTarget is uninitialized!");
                return null;
            }
            return s_instance;
        }
    }

    public AvatarComponent AvatarTarget
    {
        get { return s_avatarTarget; }
    }

    void Awake()
    {
        if (s_instance != null)
        {
            throw new System.Exception("VRInputSelectTarget类不允许有超过一份以上的实例!");
        }

        s_instance = this;
    }

    void Start()
    {
        head = VRInputManager.Instance.head.transform;

        SelectOpen();
    }

    void Update()
    {
        if (isSelect)
        {
            FindAvatarComponent();
        }
    }

    public void SelectOpen()
    {
        isSelect = true;
    }

    public void SelectClose()
    {
        isSelect = false;
    }

    /// <summary>
    /// 点到线的距离
    /// </summary>
    /// <param name="point"></param>
    /// <param name="linePoint1"></param>
    /// <param name="linePoint2"></param>
    /// <returns></returns>
    public static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
    {
        Vector3 vec1 = point - linePoint1;
        Vector3 vec2 = linePoint2 - linePoint1;
        Vector3 vecProj = Vector3.Project(vec1, vec2);
        float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
        return dis;
    }

    public List<AvatarComponent> Find(AvatarComponent src, float radius, float angle)
    {
        List<AvatarComponent> objs = AvatarComponent.AvatarInRange(radius, src, Vector3.zero);
        List<AvatarComponent> result = new List<AvatarComponent>();

        foreach (AvatarComponent obj in objs)
        {
            Vector3 desDir = obj.gameObject.transform.position - src.gameObject.transform.position;
            desDir.y = 0;
            desDir.Normalize();

            float an = Vector3.Dot(src.gameObject.transform.forward, desDir);

            if (an < -1)
                an = -1;
            if (an > 1)
                an = 1;

            int angleTemp = (int)(Mathf.Acos(an) / Mathf.PI * 180);
            if (angleTemp <= angle / 2.0)
            {
                result.Add(obj);
            }
        }
        return result;
    }

    private void FindAvatarComponent()
    {
        AvatarComponent tempComponent = null;
        List<AvatarComponent> objs = Find(VRInputManager.Instance.playerComponent, 30, 60);
        foreach (AvatarComponent obj in objs)
        {
            if (obj.status == eEntityStatus.Death || obj.status == eEntityStatus.Pending)
                continue;

            if (tempComponent == null)
                tempComponent = obj;
            else if (DisPoint2Line(obj.transform.position, head.position, head.position + head.forward) <
                     DisPoint2Line(tempComponent.transform.position, head.position, head.position + head.forward))
            {
                tempComponent = obj;
            }
        }

        if (tempComponent != s_avatarTarget)
        {
            if (tempComponent != null)
            {
                if (VRInputManager.Instance.head.GetComponent<HighlightingSystem.HighlightingRenderer>() == null)
                {
                    VRInputManager.Instance.head.AddComponent<HighlightingSystem.HighlightingRenderer>();
                }
                HighlightingSystem.Highlighter lighter = tempComponent.GetComponent<HighlightingSystem.Highlighter>();
                if (lighter == null)
                    lighter = tempComponent.gameObject.AddComponent<HighlightingSystem.Highlighter>();
                lighter.ConstantOn();
            }
            if (s_avatarTarget != null)
            {
                HighlightingSystem.Highlighter lighter = s_avatarTarget.GetComponent<HighlightingSystem.Highlighter>();
                if (lighter != null)
                {
                    lighter.ConstantOff();
                }
            }
            s_avatarTarget = tempComponent;
        }
    }



}
