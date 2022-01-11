using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GMLabel : MonoBehaviour
{
    private GM gmInstance;
    public Text gmContent;

    public void Init(string text, GM _gmInstance)
    {
        gmContent.text = text;
        gmInstance = _gmInstance;
    }

    public void OnClickLabel()
    {
        gmInstance.UpdateInputField(gmContent);
    }
}