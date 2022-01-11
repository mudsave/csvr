using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MainMemuWin 
{
    public void Quit()
    {
        //GlobalEvent.fire("Event_EndFight", transform.position, transform.rotation.eulerAngles);   // for test
        PlayerSound(m_audioButtonClick);
        Application.Quit();
    }

    public void SelectMode()
    {
        PlayerSound(m_audioButtonClick);
        UIManager.Instance.CloseUI(CPrefabPaths.MainMenu);
        UIManager.Instance.OpenUI(CPrefabPaths.UILoginModeList, true);
    }

    public void SystemSet()
    {
        PlayerSound(m_audioButtonClick);
        SelectMode();
    }
}
