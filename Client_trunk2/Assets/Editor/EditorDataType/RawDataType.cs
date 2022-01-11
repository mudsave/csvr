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
	public class DT_Bool : DataType
	{
		private bool value_;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readBool("Default");
		}

		public override void ParseDefaultValue(string val)
		{
			value_ = val.Length == 0 || val.ToLower() == "false" || int.Parse(val) == 0;
		}

		public override void OnGUI(string title)
		{
			if (title.Length > 0)
			{
				if (cantModify)
                    EditorGUILayout.LabelField(new GUIContent(title, describe), new GUIContent(value_.ToString()));
				else
					value_ = EditorGUILayout.Toggle(new GUIContent(title, describe), value_);
			}
			else
			{
				if (cantModify)
					EditorGUILayout.LabelField(value_.ToString());
				else
					value_ = EditorGUILayout.Toggle(value_);
			}
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asBool = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asBool;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Boolean);
            root = value_;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            value_ = (bool)root;
        }
	}

	[System.Serializable]
	public class DT_Int32 : DataType
	{
		private int value_;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readInt("Default");
		}

		public override void ParseDefaultValue(string val)
		{
			value_ = int.Parse(val);
		}

		public override void OnGUI(string title)
		{
			if (title.Length > 0)
			{
				if (cantModify)
                    EditorGUILayout.LabelField(new GUIContent(title, describe), new GUIContent(value_.ToString()));
				else
					value_ = EditorGUILayout.IntField(new GUIContent(title, describe), value_);
			}
			else
			{
				if (cantModify)
					EditorGUILayout.LabelField(value_.ToString());
				else
					value_ = EditorGUILayout.IntField(value_);
			}
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asInt = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asInt;			
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Int);
            root = value_;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            value_ = (int)root;
        }
	}

	[System.Serializable]
	public class DT_Float : DataType
	{
		private float value_;

		public override void ParseDefaultValue(string val)
		{
			value_ = float.Parse(val);
		}

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readFloat("Default");
		}

		public override void OnGUI(string title)
		{
			if (title.Length > 0)
			{
				if (cantModify)
                    EditorGUILayout.LabelField(new GUIContent(title, describe), new GUIContent(value_.ToString()));
				else
					value_ = EditorGUILayout.FloatField(new GUIContent(title, describe), value_);
			}
			else
			{
				if (cantModify)
					EditorGUILayout.LabelField(value_.ToString());
				else
					value_ = EditorGUILayout.FloatField(value_);
			}
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asFloat = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asFloat;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Double);
            root = (double)value_;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            value_ = (float)(double)root;
        }
	}

	[System.Serializable]
	public class DT_String : DataType
	{
		private string value_;

		public override void ParseDefaultValue(string val)
		{
			value_ = val;
		}

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readString("Default");
		}

		public override void OnGUI(string title)
		{
			if (title.Length > 0)
			{
                if (cantModify)
                    EditorGUILayout.LabelField(new GUIContent(title, describe), new GUIContent(value_.ToString()));
				else
					value_ = EditorGUILayout.TextField(new GUIContent(title, describe), value_);
			}
			else
			{
                if (cantModify)
					EditorGUILayout.LabelField(value_);
				else
					value_ = EditorGUILayout.TextField(value_);
			}
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asString = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asString;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.String);
            root = value_;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            value_ = root.ToString();
        }
	}

	[System.Serializable]
	public class DT_Vector2 : DataType
	{
		private Vector2 value_;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readVector2("Default");
		}

		public override void OnGUI(string title)
		{
			value_ = EditorGUILayout.Vector2Field(new GUIContent(title, describe), value_);
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asVector2 = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asVector2;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Array);
            root.Add((double)value_.x);
            root.Add((double)value_.y);
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            int number = 0;
            foreach (var item in root)
            {
                switch (number)
                {
                    case 0: { value_.x = (float)item; break; }
                    case 1: { value_.y = (float)item; break; }
                    default: { break; }
                }
            }
        }
	}

	[System.Serializable]
	public class DT_Vector3 : DataType
	{
		private Vector3 value_;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readVector3("Default");
		}

		public override void OnGUI(string title)
		{
			value_ = EditorGUILayout.Vector3Field(new GUIContent(title, describe), value_);
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asVector3 = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asVector3;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Array);
            root.Add((double)value_.x);
            root.Add((double)value_.y);
            root.Add((double)value_.z);
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            int number = 0;
            foreach (var item in root)
            {
                switch (number)
                {
                    case 0: { value_.x = (float)item; break; }
                    case 1: { value_.y = (float)item; break; }
                    case 3: { value_.z = (float)item; break; }
                    default: { break; }
                }
            }
        }
	}

	[System.Serializable]
	public class DT_Vector4 : DataType
	{
		private Vector4 value_;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
			value_ = section.readVector4("Default");
		}

		public override void OnGUI(string title)
		{
			value_ = EditorGUILayout.Vector4Field(title, value_);
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			root.asVector4 = value_;
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			value_ = root.asVector4;
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            root.SetJsonType(JsonType.Array);
            root.Add((double)value_.x);
            root.Add((double)value_.y);
            root.Add((double)value_.z);
            root.Add((double)value_.w);
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            int number = 0;
            foreach (var item in root)
            {
                switch (number)
                {
                    case 0: { value_.x = (float)item; break; }
                    case 1: { value_.y = (float)item; break; }
                    case 3: { value_.z = (float)item; break; }
                    case 4: { value_.w = (float)item; break; }
                    default: { break; }
                }
            }
        }
	}
}
