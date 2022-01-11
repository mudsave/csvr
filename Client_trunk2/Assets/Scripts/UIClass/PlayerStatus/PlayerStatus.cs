using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : UIWindow 
{
    private readonly string ENERGY_TEXT_FORMAT = "{0}/{1}";

    private Text m_crystalText;
    private Text m_energyText;
    private Slider m_energy;
    //private Image m_bloodImage;
    //private Material m_bloodMaterial;
    private GameObject[] m_bloodImages = new GameObject[4];

    public float[] m_bloodPercent = new float[4];

    protected override void OnInit()
    {
        InitItem();
        InstallGlobalEvent();
    }

    private void InitItem()
    {
        Transform crystalTextTrans = transform.Find("CrystalInfo/Text");
        m_crystalText = crystalTextTrans.GetComponent<Text>();

        Transform energyslider = transform.Find("Slider");
        m_energy = energyslider.GetComponent<Slider>();
        Transform energytext = transform.Find("Slider/Text");
        m_energyText = energytext.GetComponent<Text>();

        //Transform image = transform.Find("BloodSplash/Image");
        //m_bloodImage = image.GetComponent<Image>();
        //m_bloodMaterial = m_bloodImage.material;

        m_bloodImages[0] = transform.Find("BloodSplash/4").gameObject;
        m_bloodImages[1] = transform.Find("BloodSplash/3").gameObject;
        m_bloodImages[2] = transform.Find("BloodSplash/2").gameObject;
        m_bloodImages[3] = transform.Find("BloodSplash/1").gameObject;

        //Debug.Assert((m_crystalText != null) && (m_energy != null) && (m_energyText != null));

        PlayerComponent player = VRInputManager.Instance.playerComponent;
        if(player == null)
        {
            Debug.LogWarning("PlayerStatus::InitItem:PlayerComponent has not init...");
            return;
        }
        //Event_crystalChange(player.MagicSpar);
        //Event_energyChange(player.energyMgr.CurrentEnergyValue, player.energyMgr.EnergyMax);
        Event_playerHPChange(player.HP, player.MaxHP);
    }

    private void InstallGlobalEvent()
    {
        //GlobalEvent.register("EVENT_playerInited", this, "EVENT_playerInited");
        //GlobalEvent.register("Event_crystalChange", this, "Event_crystalChange");
        //GlobalEvent.register("Event_energyChange", this, "Event_energyChange");
        GlobalEvent.register("Event_playerHPChange", this, "Event_playerHPChange");
    }

    public void EVENT_playerInited(PlayerComponent p_player)
    {
        //Event_crystalChange(p_player.MagicSpar);
        //Event_energyChange(p_player.energyMgr.CurrentEnergyValue, p_player.energyMgr.EnergyMax);
        Event_playerHPChange(p_player.HP, p_player.MaxHP);
    }

    public void Event_crystalChange(int p_num)
    {
        m_crystalText.text = Convert.ToString(p_num);
    }

    public void Event_energyChange(int p_currVal, int p_maxVal)
    {
        m_energy.value = Convert.ToSingle(p_currVal) / p_maxVal;
        m_energyText.text = string.Format(ENERGY_TEXT_FORMAT, p_currVal, p_maxVal);
    }

    private void SetBloodActive(GameObject p_img, bool p_enable)
    {
        if (p_img.activeSelf != p_enable)
            p_img.SetActive(p_enable);
    }

    public void Event_playerHPChange(int p_currVal, int p_maxVal)
    {
        float percent = Convert.ToSingle(p_currVal) / p_maxVal;

        for (int i = 0; i < m_bloodPercent.Length; ++i )
        {
            if(percent < m_bloodPercent[i])
            {
                SetBloodActive(m_bloodImages[i], true);
                for(int j = i + 1; j < m_bloodPercent.Length; ++j)
                {
                    SetBloodActive(m_bloodImages[j], false);
                }
                break;
            }
            else
                SetBloodActive(m_bloodImages[i], false);
        }
    }

    void OnDestroy()
    {
        GlobalEvent.deregister(this);
    }
}
