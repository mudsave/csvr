using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 精力系统
/// </summary>
public class EnergyMgr : MonoBehaviour {

    private int currentEnergyValue = 100;          //当前精力值
    public int CurrentEnergyValue
    {
        get { return currentEnergyValue; }
        set { currentEnergyValue = value; }
    }

    private int energyMax = 100;                   //最大精力值
    public int EnergyMax
    {
        get { return energyMax; }
        set { energyMax = value; }
    }

    private int m_tickRecoveryValue = 20;         //心跳恢复基础值
    private float m_recoveryValuePercent = 0.0f;  //心跳恢复值加成百分比
    private float m_tickTime = 1.0f;              //心跳间隔
    private int m_tickExtraValue = 0;
    private int m_tickValue;                      //实际每次心跳精力的改变值

    private float m_stopDuration = 0.0f;
    private float m_stopEndTime = 0.0f;

    private AvatarComponent m_avatar;             // parent

    public int tickValue
    {
        get { return m_tickValue; }
    }

    public float recoveryValuePercent
    {
        get { return m_recoveryValuePercent; }
    }

    private void CalcTickValue()
    {
        m_tickValue = (int)(m_tickRecoveryValue * (1 + m_recoveryValuePercent)) + m_tickExtraValue;
    }

    public void Init(AvatarComponent p_parent)
    {
        m_avatar = p_parent;
        CalcTickValue();
        InvokeRepeating("Tick", m_tickTime, m_tickTime); //开启心跳
    }

    public void Tick()
    {
        ChangeEnergy(m_tickValue);
    }

    public void ChangeEnergy(int value)
    {

        int oldValue = CurrentEnergyValue;
        CurrentEnergyValue += value;

        if (CurrentEnergyValue > EnergyMax)
        {
            CurrentEnergyValue = EnergyMax;
        }
        if (CurrentEnergyValue < 0)
        {
            CurrentEnergyValue = 0;
        }

        if (oldValue != CurrentEnergyValue)
            OnEnergyValueChange(oldValue,CurrentEnergyValue);
    }

    protected void OnEnergyValueChange(int oldValue, int newValue)
    {
        m_avatar.OnEnergyChanged(newValue, oldValue, EnergyMax);
    }

    public void ChangeTickRecoveryValue(int value)
    {
        m_tickRecoveryValue = value;
        CalcTickValue();
    }

    public void ChangeRecoveryValuePercent(float value)
    {
        m_recoveryValuePercent += value;
        CalcTickValue();
    }

    public void ChangeTickExtraValue(int value)
    {
        m_tickExtraValue += value;
        CalcTickValue();
    }

    //停止恢复
    public void StopRecovery(float lastTime)
    {
        if (m_stopDuration > 0.0f)
        {
            if (m_stopEndTime - Time.time < lastTime)
            {
                m_stopEndTime = Time.time + lastTime;
            }
        }
        else
        {
            StartCoroutine(_StopRecovery(lastTime));
        }
    }

    private IEnumerator _StopRecovery(float lastTime)
    {
        int temp = m_tickRecoveryValue;
        m_tickRecoveryValue = 0;
        CalcTickValue();

        m_stopDuration = lastTime;
        m_stopEndTime = Time.time + lastTime;

        while (m_stopEndTime > Time.time)
        {
            yield return new WaitForEndOfFrame();
        }

        m_tickRecoveryValue = temp;
        CalcTickValue();

        m_stopDuration = 0.0f;
    }

    /// <summary>
    /// 判断精力是否足够用
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HasEnoughEnergy(int value)
    {
        if (value > CurrentEnergyValue)
            return false;

        return true;
    }

}
