using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using DataSection;

namespace EditorDataType
{
    public class SkillEditorWindow : EditorWindow
    {
        public SkillListWindow dataWindow = null;
        public bool isRevise = false;

        string _dataTypePath = "";
        string _dataTemplatePath = "";

        Vector2 mScrolls = Vector2.zero;
        ConfigTemplateLoader _configTemplateLoader = new ConfigTemplateLoader();
        ConfigTemplate _currentTemplate = null;
        int _currentTemplateIndex = 0;

        SkillEditorData m_data = null;

        public void Initialize(string dataTypePath, string dataTemplatePath)
        {
            _dataTypePath = dataTypePath;
            _dataTemplatePath = dataTemplatePath;

            DataSection.XMLParser parser = new XMLParser();
            var root = parser.loadFile(dataTemplatePath);
            DataType.InitDataType(root);

            _configTemplateLoader.Init("className", dataTypePath);
            _currentTemplateIndex = 0;
            _currentTemplate = _configTemplateLoader.templates[_currentTemplateIndex].Clone();
        }

        public void Initialize(SkillEditorData data, string dataTypePath, string dataTemplatePath)
        {
            _dataTypePath = dataTypePath;
            _dataTemplatePath = dataTemplatePath;

            if (data == null || data.type == null || data.type == "")
            {
                Initialize(dataTypePath, dataTemplatePath);
                return;
            }
            m_data = data;
            DataSection.XMLParser parser = new XMLParser();
            var root = parser.loadFile(dataTemplatePath);
            DataType.InitDataType(root);

            _configTemplateLoader.Init("className", dataTypePath);

            var name = data.type.ToString();
            var index = _configTemplateLoader.identities2index(name);
            if (index < 0)
                throw new System.Exception(string.Format("unknow type '{0}'", name));

            _currentTemplateIndex = index;
            _currentTemplate = _configTemplateLoader.templates[_currentTemplateIndex].Clone();
            _currentTemplate.FromDataSection(data.data);
        }

        void OnGUI()
        {
            if (_currentTemplate == null)
            {
                return;
            }

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("保存"))
                    {
                        DataSection.DataSection root = new XMLSection("root");
                        _currentTemplate.ToDataSection(root);

                        Debug.Log(root.ToString());

                        int ID = int.Parse(root["id"].value);

                        if (ID == 0) //ID为0的时候不能保存
                        {
                            if (EditorUtility.DisplayDialog("ID不能为0，请修改", "", "关闭"))
                            {
                                this.Show();
                            }
                            return;
                        }

                        if (dataWindow.listItems.ContainsKey(ID))
                        {
                            if (isRevise)
                            {
                                if (ID != m_data.ID)
                                {
                                    if (EditorUtility.DisplayDialog("ID已存在，请修改", "", "关闭"))
                                    {
                                        this.Show();
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                if (EditorUtility.DisplayDialog("ID已存在，请修改", "", "关闭"))
                                {
                                    this.Show();
                                }
                                return;
                            }
                        }

                        if (m_data != null)
                        {
                            if (isRevise)
                            {
                                dataWindow.DeleteData(m_data.ID);
                            }
                            m_data = null;
                        }

                        string vd = root.ToString();

                        FileStream fs = File.Open(dataWindow.serverDataPath + root["id"].value + ".xml", FileMode.Create);
                        byte[] info = new UTF8Encoding(true).GetBytes(vd);
                        fs.Write(info, 0, info.Length);
                        fs.Close();

                        FileStream fsclient = File.Open(dataWindow.dataListPath + "/" + root["id"].value + ".xml", FileMode.Create);
                        byte[] infoclient = new UTF8Encoding(true).GetBytes(vd);
                        fsclient.Write(infoclient, 0, infoclient.Length);
                        fsclient.Close();                       
                        

                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                        Close();

                        if (EditorUtility.DisplayDialog("保存成功！！", "", "确定", "关闭"))
                        {
                            SkillListWindow window = (SkillListWindow)EditorWindow.GetWindow(typeof(SkillListWindow));
                            window.LoadData();
                            window.Show();
                        }
                                               
                    }
                    if (GUILayout.Button("还原"))
                    {
                        Initialize(m_data, _dataTypePath, _dataTemplatePath);
                    }
                    if (GUILayout.Button("取消"))
                    {
                        m_data = null;
                        Close();
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
                mScrolls = GUILayout.BeginScrollView(mScrolls);
                {
                    EditorGUILayout.BeginVertical();
                    {
                        var templates = _configTemplateLoader.templates;
                        GUIContent[] opts = new GUIContent[templates.Count];
                        for (int i = 0; i < templates.Count; ++i)
                        {
                            string describe = templates[i].identitiesName;
                            if (templates[i].identitiesDescribe != "")
                            {
                                describe += ":" + templates[i].identitiesDescribe;
                            }
                            opts[i] = new GUIContent(describe);
                        }
                        var newIndex = EditorGUILayout.Popup(new GUIContent("请选择类型", ""), _currentTemplateIndex, opts);
                        if (newIndex != _currentTemplateIndex)
                        {
                            _currentTemplateIndex = newIndex;
                            _currentTemplate = templates[newIndex].Clone();
                        }

                        _currentTemplate.OnGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }
    }

}