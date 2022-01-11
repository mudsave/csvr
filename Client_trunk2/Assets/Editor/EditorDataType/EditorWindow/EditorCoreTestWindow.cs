using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using DataSection;
using LitJson;

namespace EditorDataType
{
	public class EditorCoreTestWindow : EditorWindow
	{
        //[MenuItem("Window/EditorCoreTestWindow")]
		static void AddWindow()
		{
			//创建窗口
			Rect wr = new Rect(0, 0, 500, 500);
			EditorCoreTestWindow window = (EditorCoreTestWindow)EditorWindow.GetWindowWithRect(typeof(EditorCoreTestWindow), wr, true, "数据列表");
			window.Initialize();
			window.Show();
		}

		Vector2 mScrolls = Vector2.zero;
		ConfigTemplateLoader _configTemplateLoader = new ConfigTemplateLoader();
		ConfigTemplate _currentTemplate = null;
		int _currentTemplateIndex = 0;

		public void Initialize()
		{
			DataSection.XMLParser parser = new XMLParser();
            var root = parser.loadFile("Configs/Rewards/Template/template");
			DataType.InitDataType(root);

			_configTemplateLoader.Init("className", "Configs/SpellTemplate/Skill/skillType");
			_currentTemplateIndex = 0;
			_currentTemplate = _configTemplateLoader.templates[_currentTemplateIndex].Clone();
		}

		void OnGUI()
		{
			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("生成xml数据测试"))
					{
						DataSection.DataSection root = new XMLSection("root");
                        _currentTemplate.ToDataSection(root);

                        Debug.Log(root.ToString());
					}

					if (GUILayout.Button("还原数据测试"))
					{
                        DataSection.XMLParser parser = new XMLParser();
                        var root = parser.loadFile("Configs/Spells/111001001");

                        var name = root.readString("className");
						var index = _configTemplateLoader.identities2index(name);
						if (index < 0)
							throw new System.Exception(string.Format("unknow type '{0}'", name));

						_currentTemplateIndex = index;
						_currentTemplate = _configTemplateLoader.templates[_currentTemplateIndex].Clone();
						_currentTemplate.FromDataSection(root);
					}
				}
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();
				mScrolls = GUILayout.BeginScrollView(mScrolls);
				{
					//foreach (var v in DataType._dataTypes.Values)
					//	v.OnGUI("test");

					EditorGUILayout.BeginVertical();
					{
						var templates = _configTemplateLoader.templates;
						GUIContent[] opts = new GUIContent[templates.Count];
						for (int i = 0; i < templates.Count; ++i)
						{
							opts[i] = new GUIContent(templates[i].identitiesName);
						}
						var newIndex = EditorGUILayout.Popup(new GUIContent("请选择技能类型", ""), _currentTemplateIndex, opts);
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
