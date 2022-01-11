using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using DataSection;

namespace EditorDataType
{
    public class SkillEditorData
    {
        public string type;         //类型
        public int ID;              //ID
        public string description;  //描述
        public DataSection.DataSection data;
    }

    public class SkillListWindow : EditorWindow
    {
        public string dataListPath = "";
        public string dataPath = "";
        public string serverDataPath = "";

        public List<string> listIDs = new List<string>();
        public Dictionary<int, SkillEditorData> listItems = new Dictionary<int, SkillEditorData>();
        public Dictionary<int, SkillEditorData> listItemCopy = new Dictionary<int, SkillEditorData>();

        int m_delID = 0;
        bool m_delete = false;
        string m_keyword = "";
        bool isFlag = false;
        string dataTypePath = "";
        string dataTemplatePath = "";

        Vector2 m_scrolls = Vector2.zero;

        [MenuItem("EditTools/SkillEditorWindow（技能编辑器窗口）")]
        static void AddWindow()
        {
            //创建窗口
            Rect wr = new Rect(0, 0, 500, 500);
            SkillListWindow window = (SkillListWindow)EditorWindow.GetWindowWithRect(typeof(SkillListWindow), wr, true, "技能数据列表");
            window.Show();
        }

        Vector2 mScrolls = Vector2.zero;
        ConfigTemplateLoader _configTemplateLoader = new ConfigTemplateLoader();
        ConfigTemplate _currentTemplate = null;
        int _currentTemplateIndex = 0;


        void Initialize()
        {
            dataListPath = "";
            dataPath = "";
            serverDataPath = "";
            listIDs.Clear();
            listItems.Clear();
            listItemCopy.Clear();
            m_delID = 0;
            m_delete = false;
            m_keyword = "";
            isFlag = false;
            dataTypePath = "";
            dataTemplatePath = "";
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("技能"))
            {
                SkillEditorWindow window = (SkillEditorWindow)EditorWindow.GetWindow(typeof(SkillEditorWindow));
                window.Close();

                Initialize();

                // 路径
                string str = System.Environment.CurrentDirectory;
                dataListPath = str + "/Assets/Resources/Configs/Spells";
                serverDataPath = str;
                GetServerPath(ref serverDataPath);
                serverDataPath += "/Server_trunk/res/configs/Spells/";
                dataPath = "Configs/Spells/";

                dataTypePath = "Configs/SpellTemplate/Skill/skilltype";
                dataTemplatePath = "Configs/SpellTemplate/Skill/skilltemplate";

                // 加载数据
                LoadData();
            }

            if (GUILayout.Button("被动技能"))
            {
                SkillEditorWindow window = (SkillEditorWindow)EditorWindow.GetWindow(typeof(SkillEditorWindow));
                window.Close();

                Initialize();

                // 路径
                string str = System.Environment.CurrentDirectory;
                dataListPath = str + "/Assets/Resources/Configs/PassiveSkills";
                serverDataPath = str;
                GetServerPath(ref serverDataPath);
                serverDataPath += "/server_trunk/res/configs/PassiveSkills/";
                dataPath = "Configs/PassiveSkills/";                

                dataTypePath = "Configs/SpellTemplate/PassiveSkills/passiveSkillsType";
                dataTemplatePath = "Configs/SpellTemplate/PassiveSkills/passiveSkillstemplate";

                // 加载数据
                LoadData();
            }

            if (GUILayout.Button("Buff"))
            {
                SkillEditorWindow window = (SkillEditorWindow)EditorWindow.GetWindow(typeof(SkillEditorWindow));
                window.Close();

                Initialize();

                // 路径
                string str = System.Environment.CurrentDirectory;
                dataListPath = str + "/Assets/Resources/Configs/Buffs";
                serverDataPath = str;
                GetServerPath(ref serverDataPath);
                serverDataPath += "/server_trunk/res/configs/Buffs/";
                dataPath = "Configs/Buffs/";

                dataTypePath = "Configs/SpellTemplate/Buff/bufftype";
                dataTemplatePath = "Configs/SpellTemplate/Buff/bufftemplate";

                // 加载数据
                LoadData();
            }
            EditorGUILayout.EndHorizontal();

            SearchKeyword();
            m_scrolls = GUILayout.BeginScrollView(m_scrolls);


            if (m_delete)
            {
                DeleteData(m_delID);
            }

            foreach (KeyValuePair<int, SkillEditorData> o in listItems)
            {
                DrawObject(o.Value);
            }
            GUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加"))
            {
                SkillEditorWindow window = DrawObjectButton();
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
            }
            if (listItemCopy.Count != 0)
            {
                listItemCopy.Clear();
            }
            if (listIDs.Count != 0)
            {
                 listIDs.Clear();
            }
            foreach (string filePath in Directory.GetFiles(dataListPath))
            {
                if (filePath.Contains(".xml") && !filePath.Contains(".meta"))
                {
                    int i = filePath.LastIndexOf(".");
                    int j = filePath.LastIndexOf("\\");
                    string str = filePath.Substring(j + 1, i - j - 1);
                    SkillEditorData _data = new SkillEditorData();

                    var xmlParser = new DataSection.XMLParser();
                    DataSection.DataSection root = xmlParser.loadFile(dataPath + str);

                    _data.ID = int.Parse(root["id"].value);
                    
                    DataSection.DataSection publicNode = root["generalPerformance"];
                    if (publicNode != null)
                    {
                        _data.description = publicNode["name"].value;
                    }  
                    _data.type = root["className"].value;
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
                List<KeyValuePair<int, SkillEditorData>> lstorder = listItems.OrderBy(c => c.Key).ToList(); //对数据列表做降序排列
                listItems.Clear();
                foreach (KeyValuePair<int, SkillEditorData> item in lstorder)
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
                isFlag = true;
                listItems.Clear();
                foreach (string s in listIDs)
                {
                    if (s.IndexOf(m_keyword, System.StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        if (listItemCopy.ContainsKey(int.Parse(s)))
                        {
                            SkillEditorData ss;
                            if (listItemCopy.TryGetValue(int.Parse(s), out ss))
                            {
                                listItems.Add(int.Parse(s), ss);
                            }
                        }
                    }
                }

                if (listItems.Count != 0)
                {
                    List<KeyValuePair<int, SkillEditorData>> lstorder = listItems.OrderBy(c => c.Key).ToList(); //对数据列表做降序排列
                    listItems.Clear();
                    foreach (KeyValuePair<int, SkillEditorData> item in lstorder)
                    {
                        listItems.Add(item.Key, item.Value);
                    }
                }
            }

            if (m_keyword == "" && isFlag)
            {
                isFlag = false;
                listItems.Clear();
                listIDs.Sort();
                foreach (string s in listIDs)
                {
                    if (listItemCopy.ContainsKey(int.Parse(s)))
                    {
                        SkillEditorData ss;
                        if (listItemCopy.TryGetValue(int.Parse(s), out ss))
                        {
                            listItems.Add(int.Parse(s), ss);
                        }
                    }
                }
                if (listItems.Count != 0)
                {
                    List<KeyValuePair<int, SkillEditorData>> lstorder = listItems.OrderBy(c => c.Key).ToList(); //对数据列表做降序排列
                    listItems.Clear();
                    foreach (KeyValuePair<int, SkillEditorData> item in lstorder)
                    {
                        listItems.Add(item.Key, item.Value);
                    }
                }
            }
        }

        void DrawObject(SkillEditorData value)
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
                    SkillEditorWindow window = DrawObjectButton(value);
                    window.isRevise = false;
                }
                if (GUILayout.Button("修改", "ButtonLeft", GUILayout.Width(40f), GUILayout.Height(20f)))
                {
                    SkillEditorWindow window = DrawObjectButton(value);
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

        SkillEditorWindow DrawObjectButton()
        {
            SkillEditorWindow window = (SkillEditorWindow)EditorWindow.GetWindow(typeof(SkillEditorWindow));
            window.dataWindow = this;
            window.Initialize(dataTypePath, dataTemplatePath);
            window.title = "技能数据编辑";

            window.Show();
            return window;
        }

        SkillEditorWindow DrawObjectButton(SkillEditorData value)
        {
            SkillEditorWindow window = (SkillEditorWindow)EditorWindow.GetWindow(typeof(SkillEditorWindow));
            window.dataWindow = this;
            window.Initialize(value, dataTypePath, dataTemplatePath);
            window.title = "技能数据编辑";

            window.Show();
            return window;
        }

        public void DeleteData(int currentID)
        {
            listItems.Remove(currentID);
            listItemCopy.Remove(currentID);
            listIDs.Remove(currentID.ToString());
            if (File.Exists(dataListPath + "//" + currentID + ".xml"))
            {
                File.Delete(dataListPath + "//" + currentID + ".xml");
                File.Delete(dataListPath + "//" + currentID + ".xml.meta");
            }

            if (File.Exists(serverDataPath + "//" + currentID + ".xml"))
            {
                File.Delete(serverDataPath + "//" + currentID + ".xml");
                File.Delete(serverDataPath + "//" + currentID + ".xml.meta");
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
                    if (filePath.Contains("Server_trunk"))
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
