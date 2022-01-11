using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintPath : MonoBehaviour
{
    private List<Transform> path;
    private int index;
    public bool showLine = false;
    public LineRenderer lineRenderer = null;
    public bool completed = false;

    public event Action onPaintCompleted;
    public event Action onFirstTouch;

    public Transform fillTransform = null;
    public float fillMax = 1;
    public float fillMix = 0;
    public Transform mirrorTransform = null;
    public Transform pointTransform = null;
    public float pointPositionScal = 1f;

    private Material fillMaterial = null;
    private Material mirrorMaterial = null;
    //private bool increase = true;

    // Use this for initialization
    private void Start()
    {
        index = -1;
        Transform[] p = GetComponentsInChildren<Transform>();
        path = new List<Transform>(p);
        path.RemoveAt(0);

        //lineRenderer.SetVertexCount(path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            //path[i].GetComponent<MeshRenderer>().enabled = false;
            path[i].gameObject.AddComponent<PaintPathPoint>().path = this;
            //lineRenderer.SetPosition(i, path[i].localPosition);
        }

        fillMaterial = fillTransform.GetComponent<MeshRenderer>().material;
        Vector2 offset = fillMaterial.GetTextureOffset("_MainTex");
        offset.x = fillMix;
        fillMaterial.SetTextureOffset("_MainTex", offset);

        if (mirrorTransform != null)
        {
            mirrorMaterial = mirrorTransform.GetComponent<MeshRenderer>().material;
            offset = mirrorMaterial.GetTextureOffset("_MainTex");
            offset.x = fillMix;
            mirrorMaterial.SetTextureOffset("_MainTex", offset);
        }

        // if (fillMax < fillMix) increase = false;
    }

    public void CheckNext(Transform go)
    {
        int i = path.IndexOf(go);
        if (i < 0) return;

        if (i == index + 1)
        {
            if (index == 0)
            {
                if (onFirstTouch != null)
                {
                    onFirstTouch();
                }
            }

            index++;
            if (showLine)
            {
                Debug.Log(i);
                lineRenderer.SetVertexCount(index + 1);
                lineRenderer.SetPosition(index, go.localPosition);
            }

            float x = fillMix + i * 1f / path.Count * (fillMax - fillMix);
            Vector2 offset = fillMaterial.GetTextureOffset("_MainTex");
            offset.x = x;
            fillMaterial.SetTextureOffset("_MainTex", offset);

            if (mirrorTransform != null)
            {
                offset = mirrorMaterial.GetTextureOffset("_MainTex");
                offset.x = x;
                mirrorMaterial.SetTextureOffset("_MainTex", offset);
            }

            pointTransform.gameObject.SetActive(true);
            pointTransform.localPosition = go.localPosition * pointPositionScal + Vector3.forward * 5 + Vector3.up * 1.5f;

            Destroy(go.gameObject);
        }
        if (index == path.Count - 1)
        {
            PaintCompleted();
        }
    }

    private void PaintCompleted()
    {
        Debug.Log("-------------- PaintCompleted");

        if (mirrorTransform != null)
        {
            for (int i = 0; i < mirrorTransform.childCount; i++)
            {
                mirrorTransform.GetChild(i).gameObject.SetActive(true);
            }
        }

        completed = true;
        if (onPaintCompleted != null)
            onPaintCompleted();
    }
}