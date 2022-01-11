using UnityEngine;
using System.Collections.Generic;

/* writed by penghuawei
 * DataSection基类，声明了一些基础的接口
 */

namespace DataSection {

    [System.Serializable]
	public class SectionBase {
		private string name_ = "";	// section key
		private string value_ = "";	// section value

		public SectionBase( string name, string value )
		{
			name_ = name;
			value_ = value.Trim();
		}

		public string name
		{
			get {
				return name_;
			}
			protected set {
				name_ = value;
			}
		}

		public string value
		{
			get {
				return value_;
			}
			set {
				value_ = value.Trim();
			}
		}

		public string asString
		{
			get {
				return value_;
			}
			set {
				value_ = value;
			}
		}

		public bool asBool
		{
			get {
				return value_ == "True" || value_ == "true";
			}
			set {
				if (value)
					value_ = "true";
				else
					value_ = "false";
			}
		}
		
		public int asInt
		{
			get {
				return int.Parse(value_);
			}
			set {
				value_ = value.ToString();
			}
		}
		
		public float asFloat
		{
			get {
				return float.Parse(value_);
			}
			set {
				value_ = value.ToString();
			}
		}
		
		public double asDouble
		{
			get {
				return double.Parse(value_);
			}
			set {
				value_ = value.ToString();
			}
		}
		
		public Vector2 asVector2
		{
			get {
				Vector2 vec = new Vector2(0.0f, 0.0f);
				string[] vs = value_.Split( ' ' );
				if (vs.Length >= 2)
				{
					vec.Set( float.Parse(vs[0]), float.Parse(vs[1]) );
				}
				return vec;
			}
			set {
				value_ = string.Format( "%f %f", value.x, value.y );
			}
		}
		
		public Vector3 asVector3
		{
			get {
				Vector3 vec = new Vector3(0.0f, 0.0f, 0.0f);
				string[] vs = value_.Split( ' ' );
				if (vs.Length >= 3)
				{
					vec.Set( float.Parse(vs[0]), float.Parse(vs[1]), float.Parse(vs[2]) );
				}
				return vec;
			}
			set {
				value_ = string.Format( "%f %f %f", value.x, value.y, value.z );
			}
		}
		
		public Vector4 asVector4
		{
			get {
				Vector4 vec = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				string[] vs = value_.Split( ' ' );
				if (vs.Length >= 4)
				{
					vec.Set( float.Parse(vs[0]), float.Parse(vs[1]), float.Parse(vs[2]), float.Parse(vs[3]) );
				}
				return vec;
			}
			set {
				value_ = string.Format( "%f %f %f %f", value.x, value.y, value.z, value.w );
			}
		}
		
		public int[] asIntArray
		{
			get {
				string[] vs = value_.Split( ' ' );
				int[] array = new int[vs.Length];
				int index = 0;
				foreach (string v in vs)
				{
					array[index] = int.Parse(v);
					index++;
				}
				return array;
			}
			set {
				string[] s = new string[value.Length];
				int index = 0;
				foreach (int v in value)
				{
					s[index] = v.ToString();
					index++;
				}
				value_ = string.Join (" ", s);
			}
		}

		public float[] asFloatArray
		{
			get {
				string[] vs = value_.Split( ' ' );
				float[] array = new float[vs.Length];
				int index = 0;
				foreach (string v in vs)
				{
					array[index] = float.Parse(v);
					index++;
				}
				return array;
			}
			set {
				string[] s = new string[value.Length];
				int index = 0;
				foreach (float v in value)
				{
					s[index] = v.ToString();
					index++;
				}
				value_ = string.Join (" ", s);
			}
		}

	}


    [System.Serializable]
	public class DataSection : SectionBase
	{
		private DataSection parent_ = null;
		private List<DataSection> childNodes_ = new List<DataSection>();
		private Dictionary<string, string> attrs_ = new Dictionary<string, string>();

		public DataSection(string name, string value, DataSection parent)
			: base(name, value)
		{
			parent_ = parent;
		}

		public Dictionary<string, string> attrs
		{
			get
			{
				return attrs_;
			}
		}

		/// <summary>
		/// 格式化当前及子section为字符串流
		/// </summary>
		public virtual string ToString(string prefix = "")
		{
			return "";
		}

		private string[] stringLastSplit( string str )
		{
			int pos = str.LastIndexOf( "/" );
			if (pos == -1)
			{
				return new string[1]{ str };
			}
			else
			{
				return new string[2]{str.Substring(0, pos), str.Substring( pos + 1 )};
			}
		}

		protected virtual DataSection makeSection()
		{
			return new DataSection("", "", null);
		}

		private DataSection newSection_(string key)
		{
			DataSection section = makeSection();
			section.name = key;
			section.parent = (DataSection)this;
			childNodes_.Add( section );
			return section;
		}

		private int findChildIndex( string key )
		{
			return childNodes_.FindIndex((DataSection v) => v.name == key);
		}

		private int findChildIndex( int startIndex, string key )
		{
			return childNodes_.FindIndex(startIndex, (DataSection v) => v.name == key);
		}

		private DataSection getSection_(string path, bool createIfNotExisted)
		{
			string[] p = path.Split( '/' );
			DataSection section = (DataSection)this;
			foreach (string e in p)
			{
				int index = section.findChildIndex( e );
				if (index == -1)
				{
					if (createIfNotExisted)
						section = newSection_( e );
					else
						return null;
				}
				else
				{
					section = section.childNodes_[index];
				}
			}
			return section;
		}

		/* 取得路径指向的section的前一个section，并返最后一个key
		 * 例如：getPrevSection_( section, "a/b/c/d", true ) 则返回“d”，并且section指向"a/b/c"
		 */
		private string getPrevSection_(out DataSection section, string path, bool createIfNotExisted)
		{
			string[] splitP = stringLastSplit( path );
			string key;
			if (splitP.Length == 1)
			{
				section = (DataSection)this;
				key = splitP[0];
			}
			else
			{
				section = getSection_( splitP[0], createIfNotExisted );
				key = splitP[1];
			}
			return key;
		}

		private List<DataSection> getSections_(string path)
		{
			string[] splitPath = stringLastSplit( path );
			DataSection section = null;
			string p;

			if (splitPath.Length == 1)
			{
				section = (DataSection)this;
				p = splitPath[0];
			}
			else
			{
				p = splitPath[1];
				section = getSection_( splitPath[0], false );
			}

			int sectionPos = 0;
			List<DataSection> nodes = new List<DataSection>();
			while (true)
			{
				int index = section.findChildIndex( sectionPos, p );
				if (index == -1)
					return nodes;

				sectionPos = index + 1;
				nodes.Add( section.childNodes_[index] );
			}
		}

		/* 仅在当前section中移除指定名称的子section（仅删除一个），不做深度搜索 */
		private bool removeSection_( string key )
		{
			int index = findChildIndex( key );
			if (index != -1)
			{
				childNodes_.RemoveAt( index );
				return true;
			}
			return false;
		}

        private void removeSectionIndex(int index)
        {
            childNodes_.RemoveAt(index);
        }

		public DataSection parent
		{
			get {
				return parent_;
			}
			set {
				parent_ = value;
			}
		}

		public int childCount
		{
			get { return childNodes_.Count; }
		}

		public DataSection child(int index)
		{
			return childNodes_[index];
		}

		public string childName( int index )
		{
			return childNodes_[index].name;
		}

		public DataSection createSection(string path)
		{
			string[] splitP = path.Split ( '/' );
			DataSection section = (DataSection)this;

			if (splitP.Length == 1)
			{
				section = section.newSection_( splitP[0] );
			}
			else
			{
				foreach (string key in splitP)
				{
					section = section.newSection_( key );
				}
			}

			return section;
		}

		public bool deleteSection( string path )
		{
			string[] splitS = stringLastSplit( path );
			DataSection section = null;
			string key = "";
			if (splitS.Length == 1)
			{
				key = splitS[0];
				section = (DataSection)this;
			}
			else
			{
				key = splitS[1];
				section = getSection_( splitS[0], false );
			}

			if (section != null)
			{
				section.removeSection_( key );
				return true;
			}
			return false;
		}

        public void deleteSectionIndex(string path, int index)
        {
            string[] splitS = stringLastSplit(path);
            DataSection section = null;
            if (splitS.Length == 1)
            {
                section = (DataSection)this;
            }
            else
            {
                section = getSection_(splitS[0], false);
            }

            if (section != null)
            {
                section.removeSectionIndex(index);
            };
        }

		public void copy(DataSection source)
		{
			value = source.value;
			foreach (DataSection section in source.childNodes_)
			{
				DataSection newSection = createSection(section.name);
				newSection.copy ( section );
			}
		}

		public bool has_key( string key )
		{
			return childNodes_.FindIndex((DataSection v) => v.name == key) > -1;
		}

		public string[] keys()
		{
			string[] result = new string[childNodes_.Count];
			int i = 0;
			foreach (DataSection section in childNodes_)
			{
				result[i] = section.name;
				i++;
			}
			return result;
		}

		public DataSection[] values()
		{
			return childNodes_.ToArray();
		}

		public DataSection this[string path]
		{
			get {
				return getSection_( path, false );
			}
		}

		public bool readBool( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asBool;
			else
				return false;
		}

		public float readFloat( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asFloat;
			else
				return 0.0f;
		}
		
		public float[] readFloats( string path )
		{
			List<float> result = new List<float>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asFloat );
			}
			return result.ToArray();
		}
		
		public double readDouble( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asDouble;
			else
				return 0.0;
		}
		
		public double[] readDoubles( string path )
		{
			List<double> result = new List<double>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asDouble );
			}
			return result.ToArray();
		}
		
		public int readInt( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asInt;
			else
				return 0;
		}
		
		public int[] readInts( string path )
		{
			List<int> result = new List<int>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asInt );
			}
			return result.ToArray();
		}
		
		public string readString( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asString;
			else
				return "";
		}
		
		public string[] readStrings( string path )
		{
			List<string> result = new List<string>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asString );
			}
			return result.ToArray();
		}
		
		public Vector2 readVector2( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asVector2;
			else
				return new Vector2();
		}
		
		public Vector2[] readVector2s( string path )
		{
			List<Vector2> result = new List<Vector2>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asVector2 );
			}
			return result.ToArray();
		}
		
		public Vector3 readVector3( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asVector3;
			else
				return new Vector3();
		}
		
		public Vector3[] readVector3s( string path )
		{
			List<Vector3> result = new List<Vector3>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asVector3 );
			}
			return result.ToArray();
		}
		
		public Vector4 readVector4( string path )
		{
			DataSection section = getSection_(path, false);
			if (section != null)
				return section.asVector4;
			else
				return new Vector4();
		}
		
		public Vector4[] readVector4s( string path )
		{
			List<Vector4> result = new List<Vector4>();
			List<DataSection> sections = getSections_(path);
			foreach (DataSection section in sections)
			{
				result.Add( section.asVector4 );
			}
			return result.ToArray();
		}
		
		public int[] readIntArray( string path, char sep )
		{
			List<int> result = new List<int>();
			
			foreach (var s in readStringArray(path, sep))
			{
				if (s.Length == 0)
					continue;
				result.Add( int.Parse( s ) );
			}
			
			return result.ToArray();
		}

		public int[][] readIntArrays(string path, char sep, char sep2)
		{
			var result = new List<int[]>();

			foreach (var ss in readStringArrays(path, sep, sep2))
			{
				if (ss.Length == 0)
					continue;

				var subRes = new List<int>();
				foreach (var s in ss)
				{
					if (s.Length == 0)
						continue;

					subRes.Add( int.Parse( s ) );
				}
				result.Add(subRes.ToArray());
			}

			return result.ToArray();
		}

		public float[] readFloatArray(string path, char sep)
		{
			List<float> result = new List<float>();
			
			foreach (var s in readStringArray(path, sep))
			{
				if (s.Length == 0)
					continue;
				result.Add( float.Parse( s ) );
			}
			
			return result.ToArray();
		}

		public float[][] readFloatArrays(string path, char sep, char sep2)
		{
			var result = new List<float[]>();

			foreach (var ss in readStringArrays(path, sep, sep2))
			{
				if (ss.Length == 0)
					continue;

				var subRes = new List<float>();
				foreach (var s in ss)
				{
					if (s.Length == 0)
						continue;

					subRes.Add(float.Parse(s));
				}
				result.Add(subRes.ToArray());
			}

			return result.ToArray();
		}

		/// <summary>
		/// example: section.readStringArrays( "abc/def/g/h", ';' );
		/// </summary>
		/// <returns></returns>
		public string[] readStringArray(string path, char sep)
		{
			var section = getSection_(path, false);
			if (section != null)
			{
				string val = section.asString;
				return val.Split(sep);
			}

			return new string[0];
		}

		/// <summary>
		/// example: section.readStringArrays( "abc/def/g/h", ';', ',' );
		/// </summary>
		/// <returns></returns>
		public string[][] readStringArrays(string path, char sep, char sep2)
		{
			var section = getSection_(path, false);
			if (section == null)
			{
				return new string[0][];
			}

			var result = new List<string[]>();
			string val = section.asString;
			
			foreach (var ss in val.Split(sep))
			{
				if (string.IsNullOrEmpty( ss ))
					continue;

				result.Add( ss.Split(sep2) );
			}

			return result.ToArray();
	}

		public DataSection writeBool(string path, bool val)
		{
			DataSection section = getSection_(path, true);
			section.asBool = val;
			return section;
		}

		public DataSection writeFloat(string path, float val)
		{
			DataSection section = getSection_(path, true);
			section.asFloat = val;
			return section;
		}
		
		public void writeFloats( string path, float[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );

			foreach (float v in vals)
			{
				section.createSection( key ).asFloat = v;
			}
		}

		public DataSection writeDouble(string path, double val)
		{
			DataSection section = getSection_(path, true);
			section.asDouble = val;
			return section;
		}
		
		public void writeDoubles( string path, double[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );
			
			foreach (double v in vals)
			{
				section.createSection( key ).asDouble = v;
			}
		}

		public DataSection writeInt(string path, int val)
		{
			DataSection section = getSection_(path, true);
			section.asInt = val;
			return section;
		}
		
		public void writeInts( string path, int[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );
			
			foreach (int v in vals)
			{
				section.createSection( key ).asInt = v;
			}
		}

		public DataSection writeString(string path, string val)
		{
			DataSection section = getSection_(path, true);
			section.asString = val;
			return section;
		}

		public void writeStrings( string path, string[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );
			
			foreach (string v in vals)
			{
				section.createSection( key ).asString = v;
			}
		}

		public DataSection writeVector2(string path, Vector2 val)
		{
			DataSection section = getSection_(path, true);
			section.asVector2 = val;
			return section;
		}
		
		public void writeVector2s( string path, Vector2[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );
			
			foreach (Vector2 v in vals)
			{
				section.createSection( key ).asVector2 = v;
			}
		}

		public DataSection writeVector3(string path, Vector3 val)
		{
			DataSection section = getSection_(path, true);
			section.asVector3 = val;
			return section;
		}
		
		public void writeVector3s( string path, Vector3[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );
			
			foreach (Vector3 v in vals)
			{
				section.createSection( key ).asVector3 = v;
			}
		}

		public DataSection writeVector4(string path, Vector4 val)
		{
			DataSection section = getSection_(path, true);
			section.asVector4 = val;
			return section;
		}
		
		public void writeVector4S( string path, Vector4[] vals )
		{
			DataSection section = null;
			string key = getPrevSection_( out section, path, true );
			
			foreach (Vector4 v in vals)
			{
				section.createSection( key ).asVector4 = v;
			}
		}

		public DataSection writeIntArray(string path, string sep, params int[] values)
		{
			DataSection section = getSection_(path, true);
			string[] str = new string[values.Length];
			for( int i = 0; i < values.Length; ++i )
			{
				str[i] = values[i].ToString();
			}

			section.asString = string.Join( sep, str );
			return section;
		}

		public DataSection writeFloatArray(string path, string sep, params float[] values)
		{
			DataSection section = getSection_(path, true);
			string[] str = new string[values.Length];
			for( int i = 0; i < values.Length; ++i )
			{
				str[i] = values[i].ToString();
			}
			
			section.asString = string.Join( sep, str );
			return section;
		}

		public DataSection writeStringArray(string path, string sep, params string[] values)
		{
			DataSection section = getSection_(path, true);

			section.asString = string.Join(sep, values);
			return section;
		}
	}
}
