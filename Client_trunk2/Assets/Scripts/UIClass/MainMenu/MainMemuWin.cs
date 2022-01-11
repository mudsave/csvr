using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMemuWin : UIWindow 
{
    [Header("Sounds name")]
    public string m_audioButtonClick;
    public string m_audioButtonHover;

    protected void PlayerSound(string p_soundName)
    {
        if (p_soundName != "")
            AudioManager.Instance.SoundPlay(p_soundName);
    }

    public virtual void Button_hoverBegin()
    {
        PlayerSound(m_audioButtonHover);
    }

    protected void CloseMainMenu()
    {
        UIManager.Instance.CloseUI(CPrefabPaths.MainMenu);
        UIManager.Instance.CloseUI(CPrefabPaths.UILoginModeList);
        UIManager.Instance.CloseUI(CPrefabPaths.UILoginPlotList);
    }
}
