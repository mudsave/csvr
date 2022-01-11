using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CUpdateWin : MonoBehaviour {

    public Slider downLoadSlider;
    public Text downLoadInfo;

    void Start()
    {
        GlobalEvent.register("ShowDownLoadProgressInfo", this, "ShowDownLoadProgressInfo");
    }

    void OnDestroy()
    {
        GlobalEvent.deregister("ShowDownLoadProgressInfo", this, "ShowDownLoadProgressInfo");
    }

    public void ShowDownLoadProgressInfo(int downLoaded,int total)
    {
        float percent = (float)(downLoaded *100/ total);
        downLoadSlider.value = (float)(percent/100);
        downLoadInfo.text = (percent).ToString() + "/100";
    }

    public void InitUI(object[] obj)
    {
        
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }
}
