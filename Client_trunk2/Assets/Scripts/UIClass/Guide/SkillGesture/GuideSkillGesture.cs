using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指引技能
/// </summary>
public class GuideSkillGesture : CUIBaseWin
{
    /// <summary>
    /// 画线状态
    /// </summary>
    private enum DrawLineState
    {
        Waiting,
        Drawing,
        Finished
    }

    //画线间隔
    private const int DRAWLINE_DURATION = 3;

    //手柄选择角度
    private readonly Vector3 controllerEuler = new Vector3(45, -15, -8);

    public float speed = 1.5f;
    public Transform point; //路径点
    public Transform staff; //手柄
    public GameObject prompt;   //技能提示UI
    public SkillGesturePath[] paths; //技能路径

    //private Dictionary<string, SkillGesturePath> gesturePaths;
    private SkillGesturePath currentGesturePath;    //当前绘制路径

    private GuideController controller; //手柄指引
    private DrawLineState drawLineState;
    private int duration;     //帧数间隔
    private int currentIndex;   //当前路径下标
    private bool hasFinishedGuide;  //完成指引

    void Start()
    {
        drawLineState = DrawLineState.Waiting;
        //gesturePaths = new Dictionary<string, SkillGesturePath>();
        //for (int i = 0; i < paths.Length; i++)
        //{
        //    gesturePaths.Add(paths[i].Gesture, paths[i]);
        //}
        controller = staff.GetComponent<GuideController>();

        AttachEvent();
    }

    void OnDestroy()
    {
        DetachEvent();
    }

    void LateUpdate()
    {
        switch (drawLineState)
        {
            case DrawLineState.Waiting:
                break;

            case DrawLineState.Drawing:
                staff.position = point.position;

                //每隔DRAWLINE_DURATION帧画线一次
                if (duration == DRAWLINE_DURATION)
                {
                    duration = 0;
                    currentGesturePath.DrawNextNode(point.position);
                }
                duration++;
                break;

            case DrawLineState.Finished:
                currentGesturePath.TextureOffset();
                break;
        }
    }

    /// <summary>
    /// 添加事件
    /// </summary>
    public void AttachEvent()
    {
        GlobalEvent.register("NotifyGuideSkillGesture", this, "NotifyGuideSkillGesture");
        GlobalEvent.register("NotifyHandleSkillGesture", this, "NotifyHandleSkillGesture");
        GlobalEvent.register("OnFire", this, "NotifySkillFire");
    }

    /// <summary>
    /// 删除事件
    /// </summary>
    public void DetachEvent()
    {
        GlobalEvent.deregister(this);
    }

    /// <summary>
    /// 处理手势指引
    /// </summary>
    public void NotifyHandleSkillGesture()
    {
        currentGesturePath = GetNextGesturePath();
        DrawPath();
    }

    /// <summary>
    /// 绘制指引手势
    /// </summary>
    /// <param name="gesture"></param>
    public void NotifyGuideSkillGesture(int skillID)
    {
        //SkillConfig skillConfig = ClientConst.skillConfigs[skillID];
        if (drawLineState == DrawLineState.Waiting)
        {
            ShowUI();
            if (VRInputManager.Instance.camera == null) return;

            Transform target = VRInputManager.Instance.camera.transform;
            transform.position = target.position;
            Vector3 direction = target.rotation.eulerAngles;
            direction.x = direction.z = 0;
            transform.rotation = Quaternion.Euler(direction);

            //提示UI 转向摄像机
            prompt.transform.LookAt(target);
            Vector3 promptEuler = prompt.transform.localEulerAngles;
            promptEuler.y += 180;
            prompt.transform.localRotation = Quaternion.Euler(promptEuler);

            //SkillGesturePath skillGesture;
            //if (!gesturePaths.TryGetValue(skillConfig.gestureName, out skillGesture))
            //{
            //    Debug.LogError("NotifyGuideSkillGesture: 不存在 " + skillConfig.gestureName);
            //    return;
            //}
            //currentGesturePath = skillGesture;
            controller.IntroduceController();
        }
    }

    /// <summary>
    /// 技能施放成功
    /// </summary>
    /// <param name="gesture"></param>
    public void NotifySkillFire(FireArgs fire)
    {
        if (currentGesturePath == null) return;

        //成功后施放下一个技能
        if (fire.gestureName == currentGesturePath.Gesture)
        {
            if (hasFinishedGuide)
            {
                GlobalEvent.fire("ShowMessage", "完成技能手势指引");
                GlobalEvent.deregister(this);
                HideUI();
                return;
            }

            ClearPath();
            currentGesturePath = GetNextGesturePath();
            DrawPath();
        }
    }

    /// <summary>
    /// 画手势路径
    /// </summary>
    private void DrawPath()
    {
        duration = DRAWLINE_DURATION;
        point.position = currentGesturePath.Path[0].position;
        staff.gameObject.SetActive(true);
        staff.localRotation = Quaternion.Euler(controllerEuler);
        staff.position = point.position;
        drawLineState = DrawLineState.Drawing;

        iTween.MoveTo(point.gameObject, iTween.Hash(
            "path", currentGesturePath.Path,
            "speed", speed,
            "easeType", iTween.EaseType.linear,
            "oncompletetarget", gameObject,
            "oncomplete", "OnComplete"));
    }

    /// <summary>
    /// 清除路径
    /// </summary>
    private void ClearPath()
    {
        drawLineState = DrawLineState.Waiting;
        currentGesturePath.Clear();
    }

    /// <summary>
    /// 获取下一个手势路径
    /// </summary>
    /// <returns></returns>
    private SkillGesturePath GetNextGesturePath()
    {
        return paths[currentIndex++];
    }

    protected override void OnHideUI()
    {
        base.OnHideUI();
        prompt.SetActive(false);
        gameObject.SetActive(false);
    }

    protected override void OnShowUI(params object[] args)
    {
        base.OnShowUI(args);
        gameObject.SetActive(true);
    }

    #region Tween

    /// <summary>
    /// 动画结束
    /// </summary>
    void OnComplete()
    {
        drawLineState = DrawLineState.Finished;
        staff.gameObject.SetActive(false);
        prompt.SetActive(true);

        if (currentIndex == paths.Length)
        {
            hasFinishedGuide = true;
        }
    }

    #endregion Tween
}