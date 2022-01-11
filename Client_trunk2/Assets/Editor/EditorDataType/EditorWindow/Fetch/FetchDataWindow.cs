using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

using DataSection;
using LitJson;

namespace EditorDataType
{
    public class FetchEditorData
    {
        public string type;         //类型
        public int ID;              //ID
        public string description;  //描述
        public JsonData data;
    }

    public class FetchDataWindow : EditorWindow
    {
        public string dataListPath = "";
        public string dataPath = "Configs/Fetchs/";
        public string serverDataPath = "";

        public List<string> listIDs = new List<string>();
        public Dictionary<int, FetchEditorData> listItems = new Dictionary<int, FetchEditorData>();
        public Dictionary<int, FetchEditorData> listItemCopy = new Dictionary<int, FetchEditorData>();

        int m_delID = 0;
        bool m_delete = false;
        string m_keyword = "";

        Vector2 m_scrolls = Vector2.zero;

        [MenuItem("EditTools/FetchEditorWindow（随机池配置窗口）")]
        static void AddWindow()
        {
            //创建窗口
            Rect wr = new Rect(0, 0, 500, 500);
            FetchDataWindow window = (FetchDataWindow)EditorWindow.GetWindowWithRect(typeof(FetchDataWindow), wr, true, "奖励配置");
            window.Initialize();
            window.Show();
        }

        Vector2 mScrolls = Vector2.zero;
        ConfigTemplateLoader _configTemplateLoader = new ConfigTemplateLoader();
        ConfigTemplate _currentTemplate = null;
        int _currentTemplateIndex = 0;

        public void Initialize()
        {
            // 路径
            string str = System.Environment.CurrentDirectory;
            dataListPath = str + "/Assets/Resources/Configs/Fetchs";
            serverDataPath = str;
            GetServerPath(ref serverDataPath);
            serverDataPath += "/server_trunk/res/configs/Fetchs/";
            // 加载数据
            LoadData();         
        }

        void OnGUI()
        {
            SearchKeyword();
            m_scrolls = GUILayout.BeginScrollView(m_scrolls);


            if (m_delete)
            {
                DeleteData(m_delID);
            }

            foreach (KeyValuePair<int, FetchEditorData> o in listItems)
            {
                DrawObject(o.Value);
            }
            GUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加"))
            {
                FetchEditorWindow window = DrawObjectButton();
            }
            if (GUILayout.Button("关闭"))
            {
                this.Close();
            }

            EditorGUILayout.EndHorizontal();

        }

        public bool LoadData()
        {
            if (listItems.Count != 0)
            {
                listItems.Clear();
                listItemCopy.Clear();
                listIDs.Clear();
            }
            foreach (string filePath in Directory.GetFiles(dataListPath))
            {
                if (filePath.Contains(".json") && !filePath.Contains(".meta"))
                {
                    int i = filePath.LastIndexOf(".");
                    int j = filePath.LastIndexOf("\\");
                    string str = filePath.Substring(j + 1, i - j - 1);
                    FetchEditorData _data = new FetchEditorData();
                    var obj = Resources.Load(dataPath + str);
                    var root = JsonMapper.ToObject<JsonData>(obj.ToString());
                    _data.ID = int.Parse(root["id"].ToString());
                    _data.description = root["description"].ToString();
                    _data.type = root["key"].ToString();
                    _data.data = root;
                    if (listItems.ContainsKey(_data.ID))
                    {
                        if (EditorUtility.DisplayDialog("ID" + _data.ID + "重复，请检查修正后再导入！", "", "关闭"))
                        {
                            listItems.Clear();
                            return false;
                        }
                    }
                    else
                    {
                        listItems.Add(_data.ID, _data);
                        listItemCopy.Add(_data.ID, _data);
                        listIDs.Add(_data.ID.ToString());
                    }
                }
            }

            if (listItems.Count != 0)
            {
                List<KeyValuePair<int, FetchEditorData>> lstorder = listItems.OrderBy(c => c.Key).ToList(); //对数据列表做降序排列
                listItems.Clear();
                foreach (KeyValuePair<int, FetchEditorData> item in lstorder)
                {
                    listItems.Add(item.Key, item.Value);
                }
            }
            return true;
        }

        void SearchKeyword()
        {
            EditorGUILayout.BeginHorizontal();
            {
                string search = EditorGUILayout.TextField("", m_keyword, "SearchTextField", GUILayout.Width(Screen.width - 35f));

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    search = "";
                    GUIUtility.keyboardControl = 0;
                }

                if (m_keyword != search)
                {
                    m_keyword = search;
                }
            }
            EditorGUILayout.EndHorizontal();
            if (m_keyword != "")
            {
                listItems.Clear();
                foreach (string s in listIDs)
                {
                    if (s.IndexOf(m_keyword, System.StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        if (listItemCopy.ContainsKey(int.Parse(s)))
                        {
                            FetchEditorData ss;
                            if (listItemCopy.TryGetValue(int.Parse(s), out ss))
                            {
                                listItems.Add(int.Parse(s), ss);
                            }
                        }
                    }
                }
            }
            else if (m_keyword == "")
            {
                listItems.Clear();
                listIDs.Sort();
                foreach (string s in listIDs)
                {
                    if (listItemCopy.ContainsKey(int.Parse(s)))
                    {
                        FetchEditorData ss;
                        if (listItemCopy.TryGetValue(int.Parse(s), out ss))
                        {
                            listItems.Add(int.Parse(s), ss);
                        }
                    }
                }
            }
        }

        void DrawObject(FetchEditorData value)
        {
            if (value == null) return;
            GUILayout.BeginHorizontal();
            {
                GUI.contentColor = new Color(0.6f, 0.8f, 1f);
                GUILayout.Label(value.ID.ToString(), "ButtonLeft", GUILayout.Width(100f), GUILayout.Height(20f));
                GUILayout.Label(value.description, "ButtonLeft", GUILayout.Height(20f));
                GUI.contentColor = Color.white;
                if (GUILayout.Button("复制", "ButtonLeft", GUILayout.Width(40f), GUILayout.Height(20f)))
                {
                    FetchEditorWindow window = DrawObjectButton(value);
                }
                if (GUILayout.Button("修改", "ButtonLeft", GUILayout.Width(40f), GUILayout.Height(20f)))
                {
                    FetchEditorWindow window = DrawObjectButton(value);
                    window.isRevise = true;
                }
                if (m_delID == value.ID)
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("删除", GUILayout.Width(40f), GUILayout.Height(20f)))
                    {
                        m_delete = true;
                    }
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("X", GUILayout.Width(22f), GUILayout.Height(20f)))
                    {
                        m_delID = 0;
                        m_delete = false;
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    if (GUILayout.Button("X", GUILayout.Width(22f), GUILayout.Height(20f)))
                    {
                        m_delID = value.ID;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        FetchEditorWindow DrawObjectButton()
        {
            FetchEditorWindow window = (FetchEditorWindow)EditorWindow.GetWindow(typeof(FetchEditorWindow));
            window.dataWindow = this;
            window.Initialize();
            window.title = "技能数据编辑";

            window.Show();
            return window;
        }

        FetchEditorWindow DrawObjectButton(FetchEditorData value)
        {
            FetchEditorWindow window = (FetchEditorWindow)EditorWindow.GetWindow(typeof(FetchEditorWindow));
            window.dataWindow = this;
            window.Initialize(value);
            window.title = "技能数据编辑";

            window.Show();
            return window;
        }

        public void DeleteData(int currentID)
        {
            listItems.Remove(currentID);
            listItemCopy.Remove(currentID);
            listIDs.Remove(currentID.ToString());
            if (File.Exists(dataListPath + "//" + currentID + ".json"))
            {
                File.Delete(dataListPath + "//" + currentID + ".json");
                File.Delete(dataListPath + "//" + currentID + ".json.meta");
            }

            if (File.Exists(serverDataPath + "//" + currentID + ".json"))
            {
                File.Delete(serverDataPath + "//" + currentID + ".json");
                File.Delete(serverDataPath + "//" + currentID + ".json.meta");
            }
            m_delID = 0;
            m_delete = false;
        }

        void GetServerPath(ref string path)
        {
            bool isFlag = false;
            int idx = path.TrimEnd('\\').LastIndexOf('\\');
            if (idx > 0)
            {
                string updir = path.Substring(0, idx);
                path = updir;
                foreach (string filePath in Directory.GetDirectories(updir))
                {
                    if (filePath.Contains("server_trunk"))
                    {
                        isFlag = true;
                        path = updir;
                    }
                }
                if (isFlag == false)
                {
                    GetServerPath(ref path);
                }
            }
        }
    }
}