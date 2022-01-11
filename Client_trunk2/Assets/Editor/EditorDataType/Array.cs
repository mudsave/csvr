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
	public class DT_Array : DataType
	{
		private string _childDataTypeName = "";

		private List<DataType> value_ = new List<DataType>();
		private bool _expanded = true;
		private int _removeIndex = -1;

		public string childDataTypeName
		{
			get { return childDataTypeName; }
			set { _childDataTypeName = value; }
		}

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			_childDataTypeName = section.readString("of");
		}

		public override void OnGUI(string title)
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginHorizontal();
				{
					_expanded = EditorGUILayout.Foldout(_expanded, new GUIContent(title, this.describe));

					if (GUILayout.Button("+", GUILayout.Width(50.0f)))
					{
						var t = MakeDataType(_childDataTypeName);
						if (t == null)
							throw new System.NullReferenceException(string.Format("Invalid data type '{0} : {1}' - {2}.", this.aliasName, _childDataTypeName, title));

						value_.Add(t);
					}
				}
				EditorGUILayout.EndHorizontal();

				if (_removeIndex >= 0 && value_.Count > 0)
				{
					value_.RemoveAt(_removeIndex);
					_removeIndex = -1;
				}

				if (_expanded)
				{
					for (var i = 0; i < value_.Count; ++i)
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.BeginHorizontal();
						{
							var v = value_[i];
							v.OnGUI("item");
							if (GUILayout.Button("-", GUILayout.Width(50.0f)))
							{
								_removeIndex = i;
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUI.indentLevel--;
					}
				}
			}
			EditorGUILayout.EndVertical();
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			foreach (var v in value_)
			{
				var section = root.createSection("item");
				v.ToDataSection(section);
			}
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			for (int i = 0; i < root.childCount; ++i)
			{
				var sec = root.child(i);
				if (sec.name != "item")
					continue;

				var t = MakeDataType(_childDataTypeName);
				if (t == null)
					throw new System.NullReferenceException(string.Format("Invalid data type '{0} : {1}' - {2}.", this.aliasName, _childDataTypeName, root.name));

				t.FromDataSection(sec);
				value_.Add(t);
			}
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Array);
            foreach (var v in value_)
            {
                var section = new JsonData();
                v.ToJsonData(ref section);
                root.Add(section);
            }
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Array);
            for (int i = 0; i < root.Count; ++i)
            {
                var sec = root[i];

                var t = MakeDataType(_childDataTypeName);
                if (t == null)
                    throw new System.NullReferenceException(string.Format("Invalid data type '{0} : {1}'.", this.aliasName, _childDataTypeName));

                t.FromJsonData(sec);
                value_.Add(t);
            }
        }
	}
}
