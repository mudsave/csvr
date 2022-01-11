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
    public class DT_FixedArray : DataType
	{
        private List<DT_FixedArrayItemWarpper> value_ = new List<DT_FixedArrayItemWarpper>();
        private bool _expanded = true;

        public override void InitTypeTemplate(DataSection.DataSection section)
        {
            base.InitTypeTemplate(section);
            var properSec = section["Properties"];
            foreach (var sec in properSec.values())
            {
                var t = new DT_FixedArrayItemWarpper();
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
            for (int i = 0; i < value_.Count; i++)
            {
                var sec = root.child(i);
                var v = value_[i];
                if (sec.name != "item")
                    continue;

                if (v == null)
                    throw new System.NullReferenceException(string.Format("Invalid data type '{0} : {1}' - {2}.", this.aliasName, v.type, root.name));

                v.FromDataSection(sec);
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
            for (int i = 0; i < value_.Count; i++)
			{
                var sec = root[i];
                var v = value_[i];
                if (v == null)
                    throw new System.NullReferenceException(string.Format("Invalid data type '{0} : {1}'.", this.aliasName, v.type));

                v.FromJsonData(sec);
            }
        }
    }

    [System.Serializable]
    public class DT_FixedArrayItemWarpper : DataType
    {
        public string type;
        public string title;
        public string defaultValue;
        public DataType inst;

        public override void InitTypeTemplate(DataSection.DataSection section)
        {
            base.InitTypeTemplate(section);

            type = section.asString;
            title = section.name;
            defaultValue = section.readString("Default");
        }

        public virtual void initInst()
        {
            if (inst != null)
                throw new System.Exception(string.Format("'{0} - {1}' is already inited!", type, title));

            inst = DataType.MakeDataType(type);
            if (inst == null)
                throw new System.Exception(string.Format("invalid typ '{0} - {1}'!", type, title));

            if (IsRawDataType(type))
                inst.InitTypeTemplate(template);
        }

        public override void OnGUI(string title)
        {
            if (inst == null)
                initInst();

            inst.OnGUI(this.title);
        }

        public override void ToDataSection(DataSection.DataSection root)
        {
            var section = root.createSection(title);
            if (inst != null)
                inst.ToDataSection(section);
        }

        public override void FromDataSection(DataSection.DataSection root)
        {
            if (inst == null)
                initInst();

            var section = root;
            inst.FromDataSection(section);
        }

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            var section = new JsonData();
            if (inst != null)
                inst.ToJsonData(ref section);
            root = section;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            if (inst == null)
                initInst();

            var section = root;
            inst.FromJsonData(section);
        }
    }
}
