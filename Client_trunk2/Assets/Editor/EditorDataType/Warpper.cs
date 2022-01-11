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
	public class DT_FixedItemWarpper : DataType
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

			if (!root.has_key(title))
				return;

			var section = root[title];
			inst.FromDataSection(section);
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            var section = new JsonData();
            if (inst != null)
                inst.ToJsonData(ref section);
            root[title] = section;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            if (inst == null)
                initInst();

            var section = root[title];
            inst.FromJsonData(section);
        }
	}

	[System.Serializable]
	public class DT_UnionItemWarpper : DT_FixedItemWarpper
	{
		public string value;
        public string key;

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);
            key = section.readString("key");
            value = section.readString("value");
        }

		public override void OnGUI(string title)
		{
			if (inst == null)
				initInst();

            if (value != "" && value != null)
            {
                inst.OnGUI(key + ":" + value);
            }
            else
            {
                inst.OnGUI(key);
            }
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			if (inst != null)
				inst.ToDataSection(root);
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			if (inst == null)
				initInst();

			inst.FromDataSection(root);
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            if (inst != null)
                inst.ToJsonData(ref root);
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            if (inst == null)
                initInst();

            inst.FromJsonData(root);
        }
	}



	/// <summary>
	/// 用于处理唯一标识的类型（该类型比较特殊）
	/// </summary>
	[System.Serializable]
	public class DT_RawDataTypeWarpper<T> : DataType
		where T : DataType, new()
	{
		private string _title;
		private DataType _inst;

		public DataType realInst
		{
			get { return _inst; }
		}

		public override bool cantModify
		{
			get { return base.cantModify; }
			set { 
				base.cantModify = value;
				if (_inst != null)
					_inst.cantModify = value;
			}
		}

		public override void InitTypeTemplate(DataSection.DataSection section)
		{
			base.InitTypeTemplate(section);

			_title = section.name;
			_inst = new T();
			_inst.describe = describe;
			_inst.ParseDefaultValue(section.asString);
		}

		public override void OnGUI(string title)
		{
			_inst.OnGUI(_title);
		}

		public override void ToDataSection(DataSection.DataSection root)
		{
			var section = root.createSection(_title);
			if (_inst != null)
				_inst.ToDataSection(section);
		}

		public override void FromDataSection(DataSection.DataSection root)
		{
			if (_inst == null)
				_inst = new T();

			if (!root.has_key(_title))
				return;

			var section = root[_title];
			_inst.FromDataSection(section);
		}

        public override void ToJsonData(ref LitJson.JsonData root)
        {
            var section = new JsonData();
            if (_inst != null)
                _inst.ToJsonData(ref section);
            root[_title] = section;
        }

        public override void FromJsonData(LitJson.JsonData root)
        {
            if (_inst == null)
                _inst = new T();

            var section = root[_title];
            _inst.FromJsonData(section);
        }
	}
}
