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
	public class DT_FixedDict : DataType
	{
		private List<DT_FixedItemWarpper> value_ = new List<DT_FixedItemWarpper>();
		private bool _expanded = true;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			var properSec = section["Properties"];
			foreach (var sec in properSec.values())
			{
				var t = new DT_FixedItemWarpper();
				t.InitTypeTemplate(sec);
				value_.Add(t);
			}
		}

		public override void OnGUI(string title)
		{
			EditorGUILayout.BeginVertical();
			{
				if (title.Length > 0)
					_expanded = EditorGUILayout.Foldout(_expanded, new GUIContent(title, this.describe));

				if (_expanded)
				{
					EditorGUI.indentLevel++;
					foreach (var v in value_)
					{
						v.OnGUI("");
					}
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			foreach (var v in value_)
			{
				v.ToDataSection(root);
			}
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			foreach (var v in value_)
			{
				v.FromDataSection(root);
			}
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            foreach (var v in value_)
            {
                v.ToJsonData(ref root);
            }
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            foreach (var v in value_)
            {
                v.FromJsonData(root);
            }
        }
	}

}
