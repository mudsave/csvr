using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using DataSection;

namespace EditorDataType
{
	[System.Serializable]
	public class DataType
	{
		private static Dictionary<string, System.Type> _rawDataType = new Dictionary<string, System.Type>() {
				{ "BOOL",         typeof(DT_Bool) },
				{ "INT32",        typeof(DT_Int32) },
				{ "FLOAT",        typeof(DT_Float) },
				{ "STRING",       typeof(DT_String) },
				{ "VECTOR2",      typeof(DT_Vector2) },
				{ "VECTOR3",      typeof(DT_Vector3) },
				{ "VECTOR4",      typeof(DT_Vector4) },
				{ "ARRAY",        typeof(DT_Array) },
				{ "FIXED_DICT",   typeof(DT_FixedDict) },
				{ "UNION",        typeof(DT_NonStandUnion) },  // 非标准union，仅能支持FIXTED_DICT、ENUME、ARRAY
				{ "STAND_UNION",  typeof(DT_Union) },  // 标准union，支持任意类型（与非标准union的导出格式不一样）
				{ "ENUME",        typeof(DT_Enume) },
				{ "NUMINPUT",     typeof(DT_MultipleSelect) },
                { "FIXED_ARRAY",  typeof(DT_FixedArray) },
			};

		public static Dictionary<string, DataType> _dataTypes = new Dictionary<string, DataType>();

		public bool IsRawDataType(string type)
		{
			return _rawDataType.ContainsKey(type);
		}

		public static DataType MakeDataType(string name)
		{
			DataType inst = null;
			if (_rawDataType.ContainsKey(name))
			{
				inst = System.Activator.CreateInstance(_rawDataType[name]) as DataType;
			}
			else if (_dataTypes.ContainsKey(name))
			{
				inst = _dataTypes[name].Clone();
			}
			return inst;
		}

		/// <summary>
		/// 初始化数据类型
		/// </summary>
		/// <param name="root"></param>
		public static void InitDataType(DataSection.DataSection root)
		{
			_dataTypes.Clear();

			for (int i = 0; i < root.childCount; ++i)
			{
				var section = root.child(i);
				var key = section.name;
				var value = section.asString;
				var t = MakeDataType(value);
				if (t == null)
					throw new System.NullReferenceException(string.Format("Invalid data type '{0} : {1}'.", key, value));

				t.aliasName = key;
				_dataTypes[key] = t;
				t.InitTypeTemplate(section);
			}
		}



		private DataSection.DataSection template_;
		private string aliasName_;         // 类型别名
		private string describe_;          // 说明
		private bool cantModify_ = false;  // 不允许修改

		public DataType() {}

		public string aliasName
		{
			get { return aliasName_; }
			set { aliasName_ = value; }
		}

		public string describe
		{
			get { return describe_; }
			set { describe_ = value; }
		}

		public DataSection.DataSection template
		{
			get { return template_; }
		}

		public virtual bool cantModify
		{
			get { return cantModify_; }
			set { cantModify_ = value; }
		}

		public virtual DataType Clone()
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, this);
			stream.Position = 0;
			return formatter.Deserialize(stream) as DataType;
		}

		public virtual void ParseDefaultValue(string val)
		{
		}

		public virtual void InitTypeTemplate(DataSection.DataSection section)
		{
			// 复制并保存一份配置，不直接引用的原因是为了避免clone()效率过慢
			template_ = new DataSection.DataSection(section.name, "", null);
			template_.copy(section);

			describe_ = section.readString("describe");
			cantModify_ = section.readBool("cantModify");
		}

		public virtual void OnGUI(string title)
		{
		
		}

		/// <summary>
		/// 把数据写入到DataSection中
		/// </summary>
		/// <param name="root"></param>
		public virtual void ToDataSection(DataSection.DataSection root)
		{
		
		}

		/// <summary>
		/// 使用DataSection的数据来初始化数据
		/// </summary>
		/// <param name="root"></param>
		public virtual void FromDataSection(DataSection.DataSection root)
		{
		
		}

        /// <summary>
        /// 把数据写入到JsonData中
        /// </summary>
        /// <param name="root"></param>
        public virtual void ToJsonData(ref LitJson.JsonData root)
        {

        }

        /// <summary>
        /// 使用JsonData的数据来初始化数据
        /// </summary>
        /// <param name="root"></param>
        public virtual void FromJsonData(LitJson.JsonData root)
        {

        }
	}

}
