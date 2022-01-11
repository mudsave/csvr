using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CErrorWin : MonoBehaviour {

	public Text message;
    public GameObject error;

    private static CErrorWin s_instance;
    public static CErrorWin instance
    {
        get
        {
            return s_instance;
        }
    }

    void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        InstallErrorEvent();
    }

	public void InitUI(object[] args)
	{
		HideUI ();
	}

    public void InstallErrorEvent()
    {
		GlobalEvent.register("ShowLoadingErrorMessage", this, "OnShowLoadingErrorMessage");
    }

    public void OnShowLoadingErrorMessage(string str)
    {
        ShowMessage(str);
    }

    void ShowMessage(string str)
    {
        ShowUI();

		if (Camera.main == null) 
		{
			return;
		}

		Transform cameraTransform = Camera.main.transform;
		transform.position = cameraTransform.position;
		transform.rotation = cameraTransform.rotation;

        message.text = str;
    }

    public void ShowUI()
    {
        if (!error.activeSelf)
            error.SetActive(true);
    }

    public void HideUI()
    {
        if (error.activeSelf)
            error.SetActive(false);
        message.text = "";
    }
}
