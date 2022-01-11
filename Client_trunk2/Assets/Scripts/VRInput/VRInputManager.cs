using System.Collections;
using UnityEngine;
using VRTK;

public class VRInputManager : MonoBehaviour
{
    [Tooltip("左武器尖端")]
    public Transform tip_nib_left = null;

    [Tooltip("右武器尖端")]
    public Transform tip_nib_right = null;

    public GameObject handLeftModel = null;
    public GameObject handRightModel = null;

    public GameObject SelectBeam = null;

    #region VARIABLES

    [HideInInspector]
    public GameObject room { get { return VRTK_SDKManager.instance.actualBoundaries; } }

    [HideInInspector]
    public GameObject head { get { return VRTK_SDKManager.instance.actualHeadset; } }

    [HideInInspector]
    public Camera camera { get { return VRTK_SDKManager.instance.actualHeadset.GetComponent<Camera>(); } }

    [HideInInspector]
    public GameObject handLeft { get { return VRTK_SDKManager.instance.actualLeftController; } }

    [HideInInspector]
    public GameObject handRight { get { return VRTK_SDKManager.instance.actualRightController; } }

    [HideInInspector]
    public VRTK.VRTK_ControllerActions controllerActionsLeft;

    [HideInInspector]
    public VRTK.VRTK_ControllerActions controllerActionsRight;

    [HideInInspector]
    public VRTK.VRTK_ControllerEvents controlleEventsLeft;

    [HideInInspector]
    public VRTK.VRTK_ControllerEvents controlleEventsRight;

    [HideInInspector]
    public PlayerComponent playerComponent { get; set; }

    [HideInInspector]
    public Animator handLeftAnimator = null;

    [HideInInspector]
    public Animator handRightAnimator = null;

    #endregion VARIABLES

    #region SINGLETON

    private static VRInputManager instance = null;

    public static VRInputManager Instance
    {
        get
        {
            if (instance) return instance;
            else
            {
                Debug.LogError("VRInputManager is uninitialized!");
                return null;
            }
        }
    }

    #endregion SINGLETON

    #region Init

    private void Awake()
    {
        instance = this;
        Init();
    }

    private void Init()
    {
        InitController();
    }

    /// <summary>
    /// 初始化VR控制器
    /// </summary>
    private void InitController()
    {
        GameObject hand = VRTK_SDKManager.instance.actualBoundaries.transform.FindChild("Hand").gameObject;
        // 模拟器
        if (!VRInputDefined.isHMDConnected)
        {
            // 头显设置
            Transform tmpHead = VRTK_SDKManager.instance.actualHeadset.transform.parent;
            VRTK_SDKManager.instance.actualHeadset.transform.SetParent(VRTK_SDKManager.instance.actualBoundaries.transform, false);
            tmpHead.SetParent(VRTK_SDKManager.instance.actualHeadset.transform, false);
            tmpHead.GetChild(0).SetParent(VRTK_SDKManager.instance.actualHeadset.transform, false);
            tmpHead.gameObject.SetActive(false);
            VRTK_SDKManager.instance.actualHeadset.transform.localPosition = new Vector3(0, 1.7f, 0);

            // 控制器模型
            GameObject leftHand = Instantiate(hand, VRTK_SDKManager.instance.actualLeftController.transform, false) as GameObject;
            leftHand.name = "Hand";
            GameObject rightHand = Instantiate(hand, VRTK_SDKManager.instance.actualRightController.transform, false) as GameObject;
            rightHand.name = "Hand";
            VRTK_SDKManager.instance.actualLeftController.transform.localPosition = new Vector3(-0.2f, 1.2f, 0.5f);
            VRTK_SDKManager.instance.actualRightController.transform.localPosition = new Vector3(0.2f, 1.2f, 0.5f);

            // 添加模拟器操控脚本
            VRTK_SDKManager.instance.actualLeftController.AddComponent<SDK_ControllerSim>();
            VRTK_SDKManager.instance.actualRightController.AddComponent<SDK_ControllerSim>();
            VRTK_SDKManager.instance.actualBoundaries.AddComponent<VRInput.SDK_InputSimulator>();
        }

        controllerActionsLeft = VRTK_SDKManager.instance.scriptAliasLeftController.GetComponent<VRTK_ControllerActions>();
        if (controllerActionsLeft == null) controllerActionsLeft = VRTK_SDKManager.instance.scriptAliasLeftController.AddComponent<VRTK_ControllerActions>();
        controllerActionsRight = VRTK_SDKManager.instance.scriptAliasRightController.GetComponent<VRTK_ControllerActions>();
        if (controllerActionsRight == null) controllerActionsRight = VRTK_SDKManager.instance.scriptAliasRightController.AddComponent<VRTK_ControllerActions>();

        controlleEventsLeft = VRTK_SDKManager.instance.scriptAliasLeftController.GetComponent<VRTK_ControllerEvents>();
        if (controlleEventsLeft == null) controlleEventsLeft = VRTK_SDKManager.instance.scriptAliasLeftController.AddComponent<VRTK_ControllerEvents>();
        controlleEventsRight = VRTK_SDKManager.instance.scriptAliasRightController.GetComponent<VRTK_ControllerEvents>();
        if (controlleEventsRight == null) controlleEventsRight = VRTK_SDKManager.instance.scriptAliasRightController.AddComponent<VRTK_ControllerEvents>();

        DestroyImmediate(hand);

        if (handLeftModel)
            handLeftAnimator = handLeftModel.GetComponent<Animator>();

        if (handRightModel)
            handRightAnimator = handRightModel.GetComponent<Animator>();
    }

    #endregion Init

    public void Shake(Hand hand, ushort strength, float duration, float pulseInterval)
    {
        VRTK.VRTK_ControllerActions controller = null;
        if (hand == Hand.LEFT)
            controller = controllerActionsLeft;
        else
            controller = controllerActionsRight;
        if (controller != null && controller.gameObject.activeInHierarchy)
            controller.TriggerHapticPulse(strength, duration, pulseInterval);
    }

    public VRTK.VRTK_ControllerEvents GetControllerEvents(Hand hand)
    {
        if (hand == Hand.LEFT)
            return controlleEventsLeft;
        else
            return controlleEventsRight;
    }
}