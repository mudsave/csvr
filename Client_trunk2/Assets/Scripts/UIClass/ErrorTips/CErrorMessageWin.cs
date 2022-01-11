using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class CErrorMessageWin : CUIBaseWin
{

    class MessageStruct
    {
        public string msg;
        public float time;
    }

    public Text[] messageLabels;

    private float m_delaytime = 1.0f;
    private static CErrorMessageWin s_instance = null;
    private List<MessageStruct> m_msgs = new List<MessageStruct>();

    void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        foreach (Text label in messageLabels)
        {
            label.text = "";
            label.transform.parent.gameObject.SetActive(false);
        }

        CGameObject.instance.TimerManager.addTimer(CheckMessage, 0.0f, 1.0f, null);
        GlobalEvent.register("ShowMessage", this, "ShowMessage");
    }

    void OnDestroy()
    {
        GlobalEvent.deregister(this);
    }

    static public CErrorMessageWin instance
    {
        get { return s_instance; }
    }

    public void ShowMessage(string msg)
    {
        if (msg != "")
        {
            ShowUI();

            MessageStruct message = new MessageStruct();
            message.msg = msg;
            message.time = Time.time;

            m_msgs.Insert(0, message);
            if (m_msgs.Count > messageLabels.Length)
            {
                m_msgs.RemoveRange(m_msgs.Count-1, 1);
            }

            UpdateMessage();
        }
    }

    void UpdateMessage()
    {
        for (int i = 0; i < messageLabels.Length; i++ )
        {
            if (i < m_msgs.Count)
            {
                messageLabels[i].text = m_msgs[i].msg;
                messageLabels[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                messageLabels[i].text = "";
                messageLabels[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    void CheckMessage(object[] objs)
    {
        float time = Time.time;

        for (int i = m_msgs.Count - 1; i >= 0; i--)
        {
            if (time - m_msgs[i].time > 5.0f)
            {
                m_msgs.RemoveAt(i);
            }
        }

        UpdateMessage();
    }

    protected override void OnShowUI(params object[] args)
    {
        base.OnShowUI(args);
        gameObject.SetActive(true);

        if (VRInputManager.Instance.camera == null) return;

        Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
        if (transform.parent != cameraTransform)
        {
            transform.parent = cameraTransform;
            transform.localPosition = Vector3.zero;
            transform.rotation = cameraTransform.rotation;
        }
    }

    protected override void OnHideUI()
    {
        base.OnHideUI();
        gameObject.SetActive(false);
    }
}
