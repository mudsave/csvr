using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using DataSection;
using LitJson;

namespace EditorDataType
{
	[System.Serializable]
	public class DT_Enume : DataType
	{
		[System.Serializable]
		class EnumeData
		{
			public string name;
			public string value;
		}

		private List<EnumeData> value_ = new List<EnumeData>();
		private string _default = "";
		private int _currentIndex = 0;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			_default = section.readString("Default");
			var prop = section["Properties"];
			for (int i = 0; i < prop.childCount; ++i)
			{
				var sec = prop.child(i);
				if (sec.name != "Arg")
					continue;

				var t = new EnumeData();
				t.name = sec.readString("Name");
				t.value = sec.readString("Value");
				value_.Add(t);
				if (t.value == _default)
					_currentIndex = i;
			}
		}

		public override void OnGUI(string title)
		{
			GUIContent[] opts = new GUIContent[value_.Count];
			for (int i = 0; i < value_.Count; ++i)
			{
				opts[i] = new GUIContent(value_[i].name);
			}

			EditorGUILayout.BeginVertical();
			{
                if (title.Length > 0)
                {
                    if (cantModify)
                    {
                        GUIContent[] fixedOpts = new GUIContent[1];
                        int fixedData = int.Parse(_default);
                        fixedOpts[0] = opts[_currentIndex];
                        EditorGUILayout.Popup(new GUIContent(title, this.describe), 0, fixedOpts);
                    }
                    else
                    {
                        _currentIndex = EditorGUILayout.Popup(new GUIContent(title, this.describe), _currentIndex, opts);
                    }
                    
                }
                else
                {
                    _currentIndex = EditorGUILayout.Popup(_currentIndex, opts);
                }					
			}
			EditorGUILayout.EndVertical();
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asString = value_[_currentIndex].value;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			var v = root.asString;
			for (int i = 0; i < value_.Count; ++i)
			{
				if (value_[i].value == v)
				{
					_currentIndex = i;
					return;
				}
			}
			_currentIndex = 0;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.String);
            root = value_[_currentIndex].value;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            var v = root.ToString();
            for (int i = 0; i < value_.Count; ++i)
            {
                if (value_[i].value == v)
                {
                    _currentIndex = i;
                    return;
                }
            }
            _currentIndex = 0;
        }
	}

}
