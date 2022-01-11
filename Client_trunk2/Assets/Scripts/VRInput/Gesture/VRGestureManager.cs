using Edwon.VR;
using Edwon.VR.Gesture;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGestureManager : MonoBehaviour
{
    public LineRenderer leftTrailRenderer = null;
    public LineRenderer rightTrailRenderer = null;

    [HideInInspector]
    public Transform room { get { return VRTK.VRTK_SDKManager.instance.actualBoundaries.transform; } }

    [HideInInspector]
    public Transform head { get { return VRTK.VRTK_SDKManager.instance.actualHeadset.transform; } }

    [HideInInspector]
    public Transform handLeft { get { return VRTK.VRTK_SDKManager.instance.actualLeftController.transform; } }

    [HideInInspector]
    public Transform handRight { get { return VRTK.VRTK_SDKManager.instance.actualRightController.transform; } }

    private GestureRecognizer currentRecognizer;
    private Transform perpTransform;
    private VRCaptureHand leftCapture;
    private VRCaptureHand rightCapture;

    private VRGestureSettings gestureSettings;

    private VRGestureSettings GestureSettings
    {
        get
        {
            if (gestureSettings == null)
            {
                gestureSettings = Utils.GetGestureSettings();
            }
            return gestureSettings;
        }
    }

    #region SINGLETON

    private static VRGestureManager instance = null;

    public static VRGestureManager Instance
    {
        get
        {
            if (instance) return instance;
            else
            {
                Debug.LogError("VRGestureManager is uninitialized!");
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
        perpTransform = transform.Find("Perpindicular Head");
        if (perpTransform == null)
        {
            perpTransform = new GameObject("Perpindicular Head").transform;
            perpTransform.parent = this.transform;
        }
        VRGestureTrail leftTrail = gameObject.AddComponent<VRGestureTrail>();
        leftTrail.currentRenderer = leftTrailRenderer;
        VRGestureTrail rightTrail = gameObject.AddComponent<VRGestureTrail>();
        rightTrail.currentRenderer = rightTrailRenderer;

        leftCapture = new VRCaptureHand(perpTransform, head, handLeft, Handedness.Left, leftTrail);
        rightCapture = new VRCaptureHand(perpTransform, head, handRight, Handedness.Right, rightTrail);

        currentRecognizer = new GestureRecognizer(GestureSettings.currentNeuralNet);
        //currentRecognizer = new GestureRecognizer("VRNET");
    }

    #endregion Init

    private void OnEnable()
    {
        GlobalEvent.register("OnTriggerPressed", this, "StartCapturing");
        GlobalEvent.register("OnTriggerReleased", this, "StopCapturing");
        GestureRecognizer.GestureDetectedEvent += OnGestureDetected;
        GestureRecognizer.GestureRejectedEvent += OnGestureRejected;
    }

    private void OnDisable()
    {
        GlobalEvent.deregister(this);
        GestureRecognizer.GestureDetectedEvent -= OnGestureDetected;
        GestureRecognizer.GestureRejectedEvent -= OnGestureRejected;
    }

    private void Update()
    {
        if (leftCapture != null)
        {
            leftCapture.Update();
        }
        if (rightCapture != null)
        {
            rightCapture.Update();
        }
    }

    public void RecognizeLine(List<Vector3> capturedLine, Handedness hand)
    {
        if (currentRecognizer != null)
            currentRecognizer.RecognizeLine(capturedLine, hand, null);
    }

    public void StartCapturing(VRControllerEventArgs e)
    {
        if (e.hand == Hand.LEFT)
        {
            leftCapture.StartRecording();
        }
        else
        {
            rightCapture.StartRecording();
        }
    }

    public void StopCapturing(VRControllerEventArgs e)
    {
        if (e.hand == Hand.LEFT)
        {
            leftCapture.StopRecording();
        }
        else
        {
            rightCapture.StopRecording();
        }
    }

    public void OnGestureDetected(string gestureName, double confidence, Handedness hand, bool isDouble = false)
    {
        Debug.Log(string.Format("OnGestureDetected=>gestureName:{0},confidence:{1},hand:{2},isDouble:{3}", gestureName, confidence, hand, isDouble));
    }

    public void OnGestureRejected(string error, string gestureName, double confidence)
    {
        Debug.Log(string.Format("OnGestureRejected=>error:{0},gestureName:{1},confidence:{2}", error, gestureName, confidence));
    }
}