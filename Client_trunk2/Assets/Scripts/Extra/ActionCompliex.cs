using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class ActionCompliex
{
    public float startTime;
    public CActionEffectConfig config;
    public GameObjComponent owner;
    protected int updateIndex = 0;

    protected Dictionary<string, EffectComponent> effectCompent = new Dictionary<string,EffectComponent>();

    public ActionCompliex(CActionEffectConfig _config, GameObjComponent _owner)
    {
        if (_owner == null)
            Debug.LogError("ActionCompliex is null");
        config = _config;
        owner = _owner;
        startTime = Time.time;
        updateIndex = 0;
    }

    public void Start()
    {
        DoEvent(config.enterEvent);
        owner.StartCoroutine(Update());
    }

    public IEnumerator Update()
    {
        while (true)
        {
            if (Time.time - startTime >= config.totalTime)
            {
                End();
                break;
            }

            TimeUpdate();
            yield return new WaitForEndOfFrame();
        }
    }

    public void TimeUpdate()
    {
        if (config.timeEvent.Count == 0 || updateIndex > config.timeEvent.Count)
            return;

        float time = Time.time - startTime;
        for (int i = updateIndex; i < config.timeEventKey.Count; i++)
        {
            if (config.timeEventKey[i] > time)
            {
                updateIndex = i;
                return;
            }

            float key = config.timeEventKey[i];

            DoEvent(config.timeEventInt[key]);
        }

        updateIndex = config.timeEventKey.Count + 1;
    }

    public void End()
    {
        DoEvent(config.leaveEvent);
    }

    protected void DoEvent(Dictionary<string, JsonData> events)
    {
        foreach (KeyValuePair<string, JsonData> kv in events)
        {
            System.Reflection.MethodInfo method = null;
            method = this.GetType().GetMethod("Event_" + kv.Key);

            if (method == null)
            {
                continue;
            }

            try
            {
                method.Invoke(this, new object[] { kv.Value });
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    #region Event

    public void Event_play(JsonData data)
    {
        string actionname = (string)data;
        owner.modelCompent.Play(actionname);
    }

    /// <summary>
    /// 增加光效，
    /// nodeName : 节点名
    /// effectName : 特效名
    /// aliasName : 别名
    /// </summary>
    /// <param name="data"></param>
    public void Event_addEffect(JsonData data)
    {
        string nodeName = (string)data[0];
        string effectName = (string)data[1];
        string aliasName = (string)data[2];

        if (effectCompent.ContainsKey(aliasName))
            return;

        Transform bindNode = null;
        if (nodeName == "")
            bindNode = owner.transform;
        else
            bindNode = owner.transform.FindChild(nodeName);

        EffectComponent compent = owner.effectManager.AddEffect(effectName, bindNode);
        effectCompent[aliasName] = compent;
    }

    public void Event_removeEffect(JsonData data)
    {
        string aliasName = (string)data;

        if (!effectCompent.ContainsKey(aliasName))
            return;

        EffectComponent compent = effectCompent[aliasName];
        compent.DestroyEffect();

        effectCompent.Remove(aliasName);
    }

    public void Event_setScale(JsonData data)
    {
        owner.gameObject.transform.localScale = new Vector3((float)((double)data[0]), (float)((double)data[1]), (float)((double)data[2]));
    }

    public void Event_setActive(JsonData data)
    {
        if ((int)data == 0)
        {
            owner.Hide("actionCompliex");
        }
        else
        {
            owner.Show("actionCompliex");
        }
    }

    /// <summary>
    /// 头顶信息
    /// </summary>
    /// <param name="data"></param>
    public void Event_headInfoActive(JsonData data)
    {
        if ((int)data == 0)
        {
            //owner.entity.eventObj.fire("HeadInfoActive", new object[] { false, "ActionCompliex" });
        }
        else
        {
            //owner.entity.eventObj.fire("HeadInfoActive", new object[] { true, "ActionCompliex" });
        }
    }

    #endregion Event
}
