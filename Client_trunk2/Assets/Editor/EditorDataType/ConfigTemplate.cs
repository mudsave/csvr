using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using DataSection;

namespace EditorDataType
{
	class ConfigTemplateLoader
	{
		private List<ConfigTemplate> _templates = new List<ConfigTemplate>();

		public List<ConfigTemplate> templates
		{
			get { return _templates; }
		}

		public int identities2index(string identName)
		{
			for (int i = 0; i < _templates.Count; ++i)
			{
				if (_templates[i].identitiesName == identName)
					return i;
			}
			return -1;
		}

		public void Init(string identKey, string assertPath)
		{
            //在添加数据和还原的时候会重复添加数据，所以初始化之前清空
            if (_templates.Count != 0)
            {
                _templates.Clear();
            }

			DataSection.XMLParser parser = new XMLParser();
			var root = parser.loadFile(assertPath);
			foreach (var section in root.values())
			{
				var t = new ConfigTemplate(identKey);
				t.Init(section);
				_templates.Add(t);
			}
		}

	}

	[System.Serializable]
	class ConfigTemplate
	{
		private List<DataType> _warppers = new List<DataType>();
		private string _identitiesKey = "";
		private string _identitiesName = "";
        private string _identitiesDescribe = "";

		public string identitiesName
		{
			get { return _identitiesName; }
		}

        public string identitiesDescribe
        {
            get { return _identitiesDescribe; }
        }

		public ConfigTemplate(string identitiesKey)
		{
			_identitiesKey = identitiesKey;
		}

		public virtual ConfigTemplate Clone()
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, this);
			stream.Position = 0;
			return formatter.Deserialize(stream) as ConfigTemplate;
		}

		public void Init(DataSection.DataSection root)
		{
			DataType t = null;
			foreach (var section in root.values())
			{
				if (section.name == _identitiesKey)
				{
					_identitiesName = section.asString;
					t = new DT_RawDataTypeWarpper<DT_String>();
					t.InitTypeTemplate(section);
					t.cantModify = true;
                    _identitiesDescribe = t.describe;
					t.ParseDefaultValue(section.asString);
				}
				else
				{
					t = new DT_FixedItemWarpper();
					t.InitTypeTemplate(section);
				}

				_warppers.Add(t);
			}
		}

		public void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			{
				foreach (var d in _warppers)
				{
					d.OnGUI("");
				}
			}
			EditorGUILayout.EndVertical() ;
		}

		public void ToDataSection(DataSection.DataSection root)
		{
			foreach (var v in _warppers)
				v.ToDataSection(root);
		}

		public void FromDataSection(DataSection.DataSection root)
		{
			foreach (var v in _warppers)
				v.FromDataSection(root);
		}

        public void ToJsonData(ref LitJson.JsonData root)
        {
            foreach (var v in _warppers)
                v.ToJsonData(ref root);
        }

        public void FromJsonData(LitJson.JsonData root)
        {
            foreach (var v in _warppers)
                v.FromJsonData(root);
        }
	}
}
