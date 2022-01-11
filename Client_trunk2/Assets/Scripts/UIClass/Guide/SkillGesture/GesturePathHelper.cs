using UnityEngine;

public class GesturePathHelper : MonoBehaviour
{
    public float scale = 1f;

    // Use this for initialization
    void Start()
    {
    }

    [ContextMenu("Scale Gesture Path")]
    public void ScalePath()
    {
        foreach (Transform obj in transform)
        {
            obj.position = obj.position * scale;
        }
    }
}