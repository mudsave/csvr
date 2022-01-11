using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

using Mono.Xml;

/* writed by penghuawei
	一个简单读取 xml 配置的工具。
	using DataSection
	XMLParser parser = new XMLParser();
	//XMLSection root = parser.loadXML( "<abc><a k=\"1\">100</a></abc>" );
	XMLSection root = parser.loadFile( "test" );
	rootSection["key"] = value
	rootSection.readString( "anykey" )

	SXML["key"].save( filename )	# save as ... 木有实现
	SXML.save() # save to src file

	注：
	1.由于XMLParser使用的是Mono.Xml来解释xml字符串，因此需要Mono.Xml库支持
	2.由于XMLParser使用的是Resources.Load()方法加载资源，因此配置需要放到Resources文件夹下，且读取时不能输入文件扩展名，例如：要读取"test.xml"，则需要parser.loadFile( "test" )
	
	
*/
namespace DataSection
{
    [System.Serializable]
	public class XMLSection : DataSection
	{
		private string filename_ = "";

		public XMLSection() : base( "", "", null ) {}
		public XMLSection( string name ) : base( name, "", null ) {}
		public XMLSection( string name, string value ) : base( name, value, null ) {}
		public XMLSection( string name, string value, XMLSection parent) : base( name, value, parent ) {}

		public string filename
		{
			get {
				return filename_;
			}
			set {
				filename_ = value;
			}
		}

		protected override DataSection makeSection()
		{
			return new XMLSection();
		}

		/// <summary>
		/// 格式化当前及子section为字符串流
		/// </summary>
		public override string ToString( string prefix = "" )
		{
			var attrList = new List<string>();
			foreach (var e in this.attrs)
			{
				attrList.Add(string.Format("{0}='{1}'", e.Key, e.Value));
			}
			var attrDatas = string.Join(" ", attrList.ToArray());
			
			var valData = "";
			var heads = "";
			var strB = "";
			var strE = "";
				
			if (attrDatas.Length > 0)
			{
				heads = string.Format("<{0} {1}>", this.name, attrDatas);
			}
			else
			{
				heads = string.Format("<{0}>", this.name);
			}

			if (this.value.Length > 0)  // 当前section有值
			{
				if (this.childCount == 0)  // 有子section
				{
					var spv = this.value.Split( '\n');
					if (spv.Length == 1)  // 仅有一行数据
					{
						strB = prefix + heads;
						strE = string.Format("</{0}>\n", this.name );
						valData = this.value;
					}
					else  // 有多行数据
					{
						strB = prefix + heads + "\n";
						strE = string.Format("{0}</{1}>\n", prefix, this.name );
						var valList = new List<string>();
						foreach(var e in spv)
						{
							valList.Add(string.Format("\t{0}{1}\n", prefix, e));
						}
						valData = string.Join("", valList.ToArray());
					}
				}
				else  // 没有子section
				{
					strB = prefix + heads + "\n";
					strE = string.Format("{0}</{1}>\n", prefix, this.name);
					var valList = new List<string>();
					foreach(var e in this.value.Split('\n'))
					{
						valList.Add(string.Format("\t{0}{1}\n", prefix, e));
					}
					valData = string.Join("", valList.ToArray());
				}
			}
			else  // 当前section没有值
			{
				if (this.childCount == 0)  // 有子section
				{
					strB = prefix + heads;
					strE = string.Format("</{0}>\n", this.name);
				}
				else  // 没有子section
				{
					strB = prefix + heads + "\n";
					strE = string.Format("{0}</{1}>\n", prefix, this.name);
				}
			}

			var childDatas = new List<string>();
			childDatas.Add(strB);
			childDatas.Add(valData);
			foreach( var e in this.values() )
			{
				childDatas.Add( e.ToString("\t" + prefix) );
			}
			childDatas.Add(strE);
			var vd = string.Join("", childDatas.ToArray());
			return vd;
		}
	}


	public class XMLParser : SmallXmlParser, SmallXmlParser.IContentHandler
	{
		
		private XMLSection root;
		
		public XMLParser () : base ()
		{
			stack = new Stack ();
		}
		
		public XMLSection loadFile( string file )
		{
			return loadXML( Resources.Load( file ).ToString() );
		}

		public XMLSection loadXML (string xml)
		{
			root = null;
			Parse (new StringReader (xml), this);
			return root;
		}
		
		// IContentHandler
		
		private XMLSection current;
		private Stack stack;
		
		public void OnStartParsing (SmallXmlParser parser) {}
		
		public void OnProcessingInstruction (string name, string text) {}
		
		public void OnIgnorableWhitespace (string s) {}
		
		public void OnStartElement (string name, SmallXmlParser.IAttrList attrs)
		{
			if (root == null)
			{
				root = new XMLSection (name);
				current = root;
			}
			else
			{
				XMLSection parent = (XMLSection) stack.Peek ();
				current = (XMLSection)parent.createSection(name);
			}
			stack.Push (current);
			// attributes
			int n = attrs.Length;
			for (int i=0; i < n; i++)
				current.attrs[attrs.GetName(i)] = attrs.GetValue(i);
		}
		
		public void OnEndElement (string name)
		{
			current = (XMLSection) stack.Pop ();
		}
		
		public void OnChars (string ch)
		{
			current.value = ch;
		}
		
		public void OnEndParsing (SmallXmlParser parser) {}


        public static void Save(string saveTo, XMLSection section)
		{
		}


        public static void Save(string saveTo, string[] section)
        {
            File.WriteAllLines(Application.dataPath + "/Resources/" + saveTo + ".xml", section);
        }

        public static void SaveTo(string saveTo, string[] section)
        {
            File.WriteAllLines(saveTo + ".xml", section);
        }

        public static string addString(DataSection section)
        {
            string str = section.ToString();
            return str;
        }
	}

}
