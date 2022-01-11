using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathGenerator))]
public class PathGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PathGenerator scene = target as PathGenerator;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("显示所有控制柄"))
        {
            if (scene.bezierPointList != null && scene.bezierPointList.Count > 1)
            {
                for (int i = 0; i < scene.bezierPointList.Count; i++)
                {
                    BezierPoint bp = scene.bezierPointList[i];
                    bp.isHandle = true;
                }
                if (GUI.changed)
                    EditorUtility.SetDirty(target);
            }
        }

        if (GUILayout.Button("隐藏所有控制柄"))
        {
            if (scene.bezierPointList != null && scene.bezierPointList.Count > 1)
            {
                for (int i = 0; i < scene.bezierPointList.Count; i++)
                {
                    BezierPoint bp = scene.bezierPointList[i];
                    bp.isHandle = false;
                }
                if (GUI.changed)
                    EditorUtility.SetDirty(target);
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("生成"))
        {
            scene.GeneratePoints();
        }
    }

    void OnSceneGUI()
    {
        DoHandles();
    }

    private void DoHandles()
    {
        PathGenerator scene = target as PathGenerator;

        //------------------------------------------
        if (scene.bezierPointList != null && scene.bezierPointList.Count > 1)
        {
            Transform t = scene.transform;
            for (int i = 0; i < scene.bezierPointList.Count; i++)
            {
                BezierPoint bp = scene.bezierPointList[i];

                if (!scene.useWorldPoint)
                {
                    bp.point = t.TransformPoint(bp.point);
                    bp.left = t.TransformPoint(bp.left);
                    bp.right = t.TransformPoint(bp.right);
                }

                // 坐标点移动柄
                if (bp.isHandle)
                {
                    bp.point = Handles.PositionHandle(bp.point, Quaternion.identity);
                    DrawSphere(bp.point, Color.red);
                    Handles.Label(bp.point, "point:" + i);
                }

                // 起止点确定
                bp.isStart = false;
                bp.isEnd = false;
                if (i == 0) bp.isStart = true;
                else if (i == scene.bezierPointList.Count - 1) bp.isEnd = true;

                // 控制柄
                if (!bp.isStart)
                {
                    if (bp.isHandle)
                    {
                        bp.left = Handles.PositionHandle(bp.left, Quaternion.identity);
                        DrawSphere(bp.left, Color.blue, 0.05f);
                        Handles.DrawLine(bp.point, bp.left);
                        Handles.Label(bp.left, "point:" + i + "_left");
                    }

                    if (!bp.isBroken)
                        bp.right = bp.point * 2.0f - bp.left;
                }
                if (!bp.isEnd)
                {
                    if (bp.isHandle)
                    {
                        bp.right = Handles.PositionHandle(bp.right, Quaternion.identity);
                        DrawSphere(bp.right, Color.blue, 0.05f);
                        Handles.DrawLine(bp.point, bp.right);
                        Handles.Label(bp.right, "point:" + i + "_right");
                    }

                    if (!bp.isBroken)
                        bp.left = bp.point * 2 - bp.right;
                }

                // 画Bezier曲线
                if (i < scene.bezierPointList.Count - 1)
                {
                    BezierPoint bpEnd = scene.bezierPointList[i + 1];
                    Vector3 endPoint = bpEnd.point;
                    Vector3 endLeft = bpEnd.left;

                    if (!scene.useWorldPoint)
                    {
                        endPoint = t.TransformPoint(endPoint);
                        endLeft = t.TransformPoint(endLeft);
                    }

                    Handles.DrawBezier(bp.point, endPoint, bp.right, endLeft, Color.green, null, 2f);
                }

                if (!scene.useWorldPoint)
                {
                    bp.point = t.InverseTransformPoint(bp.point);
                    bp.left = t.InverseTransformPoint(bp.left);
                    bp.right = t.InverseTransformPoint(bp.right);
                }
            }
        }
        //------------------------------------------

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private void DrawSphere(Vector3 pos, Color c, float size = 0.1f)
    {
        float circleSize = HandleUtility.GetHandleSize(pos) * size;
        Handles.color = c;
        Handles.CircleCap(0, pos, Quaternion.Euler(0, 0, 0), circleSize);
        Handles.CircleCap(0, pos, Quaternion.Euler(0, 90, 0), circleSize);
        Handles.CircleCap(0, pos, Quaternion.Euler(90, 0, 0), circleSize);
    }
}