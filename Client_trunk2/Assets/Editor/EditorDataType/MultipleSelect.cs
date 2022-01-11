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
	public class DT_MultipleSelect : DataType
	{
		[System.Serializable]
		class NumInputData
		{
			public string name;
			public string value;
			public bool sellected = false;
		}


		private List<NumInputData> value_ = new List<NumInputData>();
		private string _default = "";
		private bool _expanded = true;
		private int selectedCount = 0;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			_default = section.readString("Default");
			var prop = section["Properties"];
			for (int i = 0; i < prop.childCount; ++i)
			{
				var sec = prop.child(i);
				var t = new NumInputData();
				t.name = sec.name;
				t.value = sec.asString;
				value_.Add(t);
				if (t.value == _default)
					t.sellected = true;
			}
		}

		public override void OnGUI(string title)
		{
			EditorGUILayout.BeginVertical();
			{
				_expanded = EditorGUILayout.Foldout(_expanded, new GUIContent(title, this.describe));

				if (_expanded)
				{
					EditorGUI.indentLevel++;
					{
						var oldStatus = selectedCount == value_.Count;
						if (EditorGUILayout.Toggle("all", oldStatus))
						{
							foreach (var t in value_)
							{
								t.sellected = true;
							}
						}
						else if (oldStatus)
						{
							foreach (var t in value_)
							{
								t.sellected = false;
							}
						}

						selectedCount = 0;
						foreach (var t in value_)
						{
							t.sellected = EditorGUILayout.Toggle(t.name, t.sellected);
							if (t.sellected)
								selectedCount++;
						}
					}
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			var temp = new List<string>();
			foreach (var v in value_)
			{
				if (v.sellected)
					temp.Add(v.value);
			}
            temp.Sort(string.Compare);

			root.asString = string.Join(",", temp.ToArray());
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			foreach (var t in value_)
			{
                t.sellected = false;
			}

			var ss = root.asString.Split(',');
			foreach (var v in ss)
			{
				var nv = v.Trim();
				foreach (var t in value_)
				{
					if (t.value == nv)
						t.sellected = true;
				}
			}
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            var temp = new List<string>();
            foreach (var v in value_)
            {
                if (v.sellected)
                    temp.Add(v.value);
            }

            root.SetJsonType(JsonType.String);
            root = string.Join(",", temp.ToArray());
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            foreach (var t in value_)
            {
                t.sellected = false;
            }

            var ss = root.ToString().Split(',');
            foreach (var v in ss)
            {
                var nv = v.Trim();
                foreach (var t in value_)
                {
                    if (t.value == nv)
                        t.sellected = true;
                }
            }
        }
	}
}
