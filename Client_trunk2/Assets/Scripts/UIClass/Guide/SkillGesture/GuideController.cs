using System.Collections;
using UnityEngine;
using VRTK;

/// <summary>
/// 指引控制器
/// </summary>
public class GuideController : MonoBehaviour
{
    public GameObject menuObject;
    public GameObject tipObject;
    public GameObject controllerModel;
    public UICircle uicircle;

    private readonly Color fromColor = Color.green;
    private readonly Color toColor = Color.white;

    private bool hasNotifyGuide = false;    

    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// 介绍手柄
    /// </summary>
    public void IntroduceController()
    {
        gameObject.SetActive(true);
        StartCoroutine(Guide());
    }

    private IEnumerator Guide()
    {
        tipObject.SetActive(true);
        menuObject.SetActive(false);
        yield return new WaitForSeconds(2.5f);
        tipObject.SetActive(false);

        iTween.RotateTo(controllerModel, iTween.Hash(
            "rotation", new Vector3(90, 180, 0),
            "time", 1.2f,
            "islocal", true,
            "oncompletetarget", gameObject,
            "oncomplete", "OnRotateComplete"
            ));

        yield return new WaitForSeconds(1.8f);
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", fromColor,
            "to", toColor,
            "time", 0.2f,
            "looptype", iTween.LoopType.pingPong,
            "onupdate", "OnUpdateValue",
            "onupdatetarget", gameObject
            ));

        GlobalEvent.register("OnTouchpadPressed", this, "OnTouchpadPressed");
    }

    #region iTween Event

    public void OnRotateComplete()
    {
        menuObject.SetActive(true);
    }

    public void OnUpdateValue(Color value)
    {
        //ColorBlock colorBlock = new ColorBlock();
        //colorBlock.normalColor = value;
        //colorBlock.highlightedColor = value;
        //colorBlock.pressedColor = value;
        //colorBlock.disabledColor = value;
        //startButton.GetComponent<Button>().colors = colorBlock;

        uicircle.color = value;
    }

    #endregion iTween Event

    /// <summary>
    /// 按下触控板
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnTouchpadPressed(VRControllerEventArgs e)
    {
        if (hasNotifyGuide) return;

        if (e.hand == Hand.RIGHT)
        {
            GlobalEvent.deregister(this);
            hasNotifyGuide = true;
            menuObject.SetActive(false);
            GlobalEvent.fire("NotifyHandleSkillGesture");
        }
    }
}