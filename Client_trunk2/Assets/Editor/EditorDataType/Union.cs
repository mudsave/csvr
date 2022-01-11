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
	public class DT_Union : DataType
	{
		protected List<DT_UnionItemWarpper> value_ = new List<DT_UnionItemWarpper>();
		protected string _default = "";
		protected int _currentIndex = 0;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			_default = section.readString("Default");
			var prop = section["Properties"];
			for (int i = 0; i < prop.childCount; ++i)
			{
				var sec = prop.child(i);
				var t = new DT_UnionItemWarpper();
				t.InitTypeTemplate(sec);
				value_.Add(t);
				if (t.title == _default)
					_currentIndex = i;
			}
		}

		public override void OnGUI(string title)
		{
			GUIContent[] opts = new GUIContent[value_.Count];
			for (int i = 0; i < value_.Count; ++i)
			{
				opts[i] = new GUIContent(value_[i].title);
			}

			EditorGUILayout.BeginVertical();
			{
				if (title.Length > 0)
					_currentIndex = EditorGUILayout.Popup(new GUIContent(title, this.describe), _currentIndex, opts);
				else
					_currentIndex = EditorGUILayout.Popup(_currentIndex, opts);

				EditorGUI.indentLevel++;
				var v = value_[_currentIndex];
				v.OnGUI("");
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			var section = root.createSection("item");
			var currentVal = value_[_currentIndex];
			currentVal.ToDataSection(section);
            root.asString = currentVal.key;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			var key = root.asString;

			DT_UnionItemWarpper value = null;
			for (int i = 0; i < value_.Count; ++i)
			{
				if (key == value_[i].key)
				{
					_currentIndex = i;
					value = value_[i];
					break;
				}
			}
			
			var section = root["item"];
			value.FromDataSection(section);
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Array);
            var section = new JsonData();
            var currentVal = value_[_currentIndex];
            currentVal.ToJsonData(ref section);
            root.Add(currentVal.key);
            root.Add(section);
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            DT_UnionItemWarpper value = null;
            root.SetJsonType(JsonType.Array);
            for (int i = 0; i < value_.Count; ++i)
            {
                int j = 0;
                if (root.ToString() == value_[i].key)
                {
                    _currentIndex = i;
                    value = value_[i];
                    break;
                }
            }

            value.FromJsonData(root);
        }
	}

	/// <summary>
	/// 非标准联合类型，仅能支持FIXTED_DICT、ENUME、ARRAY
	/// 这个的存在是为了兼容旧的配置格式
	/// </summary>
	[System.Serializable]
	public class DT_NonStandUnion : DT_Union
	{
		public override void ToDataSection(DataSection.DataSection root)
		{
			var currentVal = value_[_currentIndex];

			currentVal.ToDataSection(root);
            root.asString = currentVal.key;
            if (currentVal.value != "" && currentVal.value != null)
            {
                root.attrs["value"] = currentVal.value;
            }
        }

		public override void FromDataSection(DataSection.DataSection root)
		{
			var key = root.asString;

			DT_UnionItemWarpper item = null;
			for (int i = 0; i < value_.Count; ++i)
			{
                if (key == value_[i].key)
				{
					_currentIndex = i;
                    item = value_[i];
					break;
				}
			}

            item.FromDataSection(root);
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            var currentVal = value_[_currentIndex];
            if (currentVal.key != "")
            {
                root["key"] = currentVal.key;
            }
            if (currentVal.value != "")
            {
                root["type"] = currentVal.value;
            }
            currentVal.ToJsonData(ref root);
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            DT_UnionItemWarpper value = null;
            for (int i = 0; i < value_.Count; ++i)
            {
                if (root["key"].ToString() == value_[i].key)
                {
                    _currentIndex = i;
                    value = value_[i];
                    i = value_.Count;
                    break;
                }
            }

            value.FromJsonData(root);
        }
	}
}
