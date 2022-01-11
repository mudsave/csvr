using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPoint
{
    public bool isStart = false;
    public bool isEnd = false;
    public bool isBroken = false;
    public bool isHandle = true;
    public Vector3 point;
    public Vector3 left;
    public Vector3 right;
}

public class PathGenerator : MonoBehaviour
{
    public bool useWorldPoint = false;
    public List<BezierPoint> bezierPointList;
    public GameObject prefab;
    public int pointCount;

    public void GeneratePoints()
    {
        if (bezierPointList != null && bezierPointList.Count > 1)
        {
            List<float> lengthList = GetLengthList();
            float length = 0;
            for (int i = 0; i < lengthList.Count; i++)
            {
                length += lengthList[i];
            }

            // 每段长度占总长比
            List<float> percentLengthList = new List<float>();
            for (int i = 0; i < lengthList.Count; i++)
            {
                percentLengthList.Add(lengthList[i] / length);
            }

            // 生成路径点
            Transform pathObj = transform.FindChild("Path");
            if (pathObj != null) DestroyImmediate(pathObj.gameObject);
            pathObj = (new GameObject("Path")).transform;
            pathObj.SetParent(transform, false);

            for (int i = 0; i < pointCount; i++)
            {
                float t = (i + 0.0f) / (pointCount - 1.0f);
                for (int j = 0; j < percentLengthList.Count; j++)
                {
                    if (t > percentLengthList[j]) t -= percentLengthList[j];
                    else
                    {
                        BezierPoint start = bezierPointList[j];
                        BezierPoint end = bezierPointList[j + 1];
                        t /= percentLengthList[j];
                        Vector3 point = CalculateCubicBezierPoint(t, start.point, end.point, start.right, end.left);
                        GameObject go = Instantiate(prefab) as GameObject;
                        go.transform.SetParent(pathObj, false);
                        go.transform.localPosition = point;
                        go.name = i + 1 + "";
                        go.SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    public List<float> GetLengthList()
    {
        List<float> lengthList = new List<float>();
        for (int i = 0; i < bezierPointList.Count - 1; i++)
        {
            lengthList.Add(CalculateLength(bezierPointList[i], bezierPointList[i + 1]));
        }
        return lengthList;
    }

    public float CalculateLength(BezierPoint start, BezierPoint end)
    {
        float length = 0;
        int maxNum = 100;
        Vector3 startPos;
        Vector3 endPos;
        startPos = start.point;
        for (int i = 1; i < maxNum; i++)
        {
            endPos = CalculateCubicBezierPoint((i + 0f) / maxNum, start.point, end.point, start.right, end.left);
            length += Vector3.Distance(startPos, endPos);
            startPos = endPos;
        }
        return length;
    }

    public Vector3 CalculateCubicBezierPoint(float t, Vector3 startPoint, Vector3 endPoint, Vector3 startTangent, Vector3 endTangent)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * startPoint;
        p += 3 * uu * t * startTangent;
        p += 3 * u * tt * endTangent;
        p += ttt * endPoint;

        return p;
    }
}