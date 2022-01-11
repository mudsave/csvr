using KBEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    public GameObject GMWin;
    public InputField gmInput;
    public RectTransform scrollContent;
    public GameObject gmLabelPrefab;
    //Player player;

    void Awake()
    {
        gmInput.text = "";
        Init();
    }

    public void UpdateInputField(Text content)
    {
        string[] str = content.text.Split('/');
        if (str.Length > 0)
            gmInput.text = str[0];
    }

    private void Init()
    {
        string str = Resources.Load("Configs/GM").ToString();
        var enumerator = str.Split('\n', '\r').GetEnumerator();
        while (enumerator.MoveNext())
        {
            string element = enumerator.Current.ToString();
            if (!string.IsNullOrEmpty(element))
            {
                GMLabel label = Instantiate(gmLabelPrefab).GetComponent<GMLabel>();
                label.transform.SetParent(scrollContent);
                label.Init(element.Replace("\t", ""), this);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (GMWin.activeSelf)
                OnSubmit();
            else
                GMWin.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            GMWin.SetActive(false);
        }
    }

    public void OnSubmit()
    {
        string inputValue = gmInput.text;
        if (!string.IsNullOrEmpty(inputValue))
        {
            //gmInput.text = "";
            //player = (Player)KBEngineApp.app.player();
            //player.baseCall("sendGM", new object[] { inputValue });
            //player.cellCall("sendGM", new object[] { inputValue });
            //Debug.LogError("GM Send");
        }
    }

    public void OnClose()
    {
        GMWin.SetActive(false);
    }
}