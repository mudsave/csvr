using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;


public class CNPCPathEditorWindow : EditorWindow
{

    RaycastHit _hitInfo;
    SceneView.OnSceneFunc _delegate;
    static CNPCPathEditorWindow _windowInstance;

    [@MenuItem("EditTools/SetNPCPathConfig（打开布置NPC路径的窗口）")]
    static void Init()
    {
        if (_windowInstance == null)
        {
            Rect wr = new Rect(0, 0, 700, 150);

            _windowInstance = (CNPCPathEditorWindow)EditorWindow.GetWindowWithRect(typeof(CNPCPathEditorWindow), wr, false, "布置NPC路径界面");
            _windowInstance._delegate = new SceneView.OnSceneFunc(OnSceneFunc);
            SceneView.onSceneGUIDelegate += _windowInstance._delegate;
        }
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }

    void OnDestroy()
    {
        if (_delegate != null)
        {
            SceneView.onSceneGUIDelegate -= _delegate;
        }
    }

    void OnGUI()
    {
    }

    void OnInspectorGUI()
    {
        Debug.Log("OnInspectorGUI");
    }

    static public void OnSceneFunc(SceneView sceneView)
    {
        _windowInstance.CustomSceneGUI(sceneView);
    }

    void CustomSceneGUI(SceneView sceneView)
    {
        Camera _camera = sceneView.camera;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(ray, out _hitInfo, 1000, -1))
        {
            Transform _tranform = _hitInfo.transform;
            if (_tranform != null)
            {
                CNPCPathBase npcPathBase = _tranform.GetComponent<CNPCPathBase>();
                if (npcPathBase != null)
                {
                    Debug.Log("selecet exit point");
                }
            }
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 1)
            {
                Debug.Log("Right mouse button down ");
                OnMouseButtonClick();
            }
        }

        SceneView.RepaintAll();
    }

    void OnMouseButtonClick()
    {
        Transform selected = Selection.activeTransform;
        if (selected == null)
            return;

        CNPCPathBase npcPathBase = selected.GetComponent<CNPCPathBase>();
        if (npcPathBase == null)
            return;

        npcPathBase.SetNPCPath(selected, _hitInfo.point);
    }

}
