using Edwon.VR;
using Edwon.VR.Gesture;
using Edwon.VR.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCaptureHand
{
    private Transform playerHead;
    private Transform playerHand;
    private Transform perpTransform;

    private Handedness hand;

    private VRGestureTrail myTrail;
    private List<Vector3> currentCapturedLine;

    private float nextRenderTime = 0;
    private float renderRateLimit = Config.CAPTURE_RATE;

    private bool capturing = false;

    public string lastGesture;
    public DateTime lastDetected;

    public delegate void StartCapture();

    public event StartCapture StartCaptureEvent;

    public delegate void ContinueCapture(Vector3 capturePoint);

    public event ContinueCapture ContinueCaptureEvent;

    public delegate void StopCapture();

    public event StopCapture StopCaptureEvent;

    public VRCaptureHand(Transform _perp, Transform _playerHead, Transform _playerHand, Handedness _hand, VRGestureTrail _myTrail = null)
    {
        hand = _hand;
        playerHand = _playerHand;
        playerHead = _playerHead;
        perpTransform = _perp;
        currentCapturedLine = new List<Vector3>();
        if (_myTrail != null)
        {
            myTrail = _myTrail;
            myTrail.AssignHand(this);
        }

        Start();
    }

    // Use this for initialization
    private void Start()
    {
        if (myTrail != null)
        {
            //myTrail.AssignHand(this);
        }
    }

    //This will get points in relation to a users head.
    public Vector3 getLocalizedPoint(Vector3 myDumbPoint)
    {
        perpTransform.position = playerHead.position;
        perpTransform.rotation = Quaternion.Euler(0, playerHead.eulerAngles.y, 0);
        return perpTransform.InverseTransformPoint(myDumbPoint);
    }

    #region UPDATE

    // Update is called once per frame
    public void Update()
    {
        if (capturing)
        {
            CapturePoint();
        }
    }

    public void StartRecording()
    {
        //Debug.Log("StartRecording");
        capturing = true;
        nextRenderTime = Time.time + renderRateLimit / 1000;
        if (StartCaptureEvent != null)
        {
            StartCaptureEvent();
        }
        CapturePoint();
    }

    private void CapturePoint()
    {
        Vector3 handPoint = playerHand.position;
        Vector3 localizedPoint = getLocalizedPoint(handPoint);
        currentCapturedLine.Add(localizedPoint);
        if (ContinueCaptureEvent != null)
        {
            //ContinueCaptureEvent(handPoint);
            ContinueCaptureEvent(VRGestureManager.Instance.transform.InverseTransformPoint(handPoint));
        }
        //Debug.Log("CapturePoint:" + handPoint);
    }

    public void StopRecording()
    {
        //Debug.Log("StopRecording");
        capturing = false;
        if (currentCapturedLine.Count > 0)
        {
            //识别计算
            VRGestureManager.Instance.RecognizeLine(currentCapturedLine, hand);
            currentCapturedLine.RemoveRange(0, currentCapturedLine.Count);
            currentCapturedLine.Clear();

            if (StopCaptureEvent != null)
                StopCaptureEvent();
        }
        myTrail.ClearTrail();
    }

    #endregion UPDATE
}