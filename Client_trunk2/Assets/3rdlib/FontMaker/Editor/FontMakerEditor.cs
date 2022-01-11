using UnityEngine;
using System.Collections;
using UnityEditor;

public class FontMakerEditor : MonoBehaviour
{

    /// <summary>
    /// 配置自定义字体
    /// </summary>
    [MenuItem("Tools/选中(*.fnt)文件->导出自定义字体")]
    static void ExportCustomFont()
    {
        TextAsset fntfile = Selection.activeObject as TextAsset;
        string path = AssetDatabase.GetAssetPath(fntfile).Replace(".fnt", "");

        Material mat = AssetDatabase.LoadAssetAtPath(path + ".mat", typeof(Material)) as Material;
        if (mat == null)
        {
            mat = new Material(Shader.Find("GUI/Text Shader"));
            AssetDatabase.CreateAsset(mat, path + ".mat");
        }
        AssetDatabase.Refresh();

        Texture tex = AssetDatabase.LoadAssetAtPath(path + "_0.tga", typeof(Texture)) as Texture;
        mat.mainTexture = tex;

        Font font = new Font();
        font.material = mat;

        font.characterInfo = null;

        BMFont mbFont = new BMFont();
        BMFontReader.Load(mbFont, fntfile.name, fntfile.bytes);  // 借用NGUI封装的读取类
        CharacterInfo[] characterInfo = new CharacterInfo[mbFont.glyphs.Count];
        for (int i = 0; i < mbFont.glyphs.Count; i++)
        {
            BMGlyph bmInfo = mbFont.glyphs[i];
            CharacterInfo info = new CharacterInfo();
            info.index = bmInfo.index;
            info.uv.x = (float)bmInfo.x / (float)mbFont.texWidth;
            info.uv.y = 1 - (float)bmInfo.y / (float)mbFont.texHeight;
            info.uv.width = (float)bmInfo.width / (float)mbFont.texWidth;
            info.uv.height = -1f * (float)bmInfo.height / (float)mbFont.texHeight;
            info.vert.x = (float)bmInfo.offsetX;
            //info.vert.y = (float)bmInfo.offsetY;
            info.vert.y = 0f;//自定义字库UV从下往上，所以这里不需要偏移，填0即可。
            info.vert.width = (float)bmInfo.width;
            info.vert.height = (float)bmInfo.height;
            info.advance = bmInfo.advance;
            characterInfo[i] = info;
        }
        font.characterInfo = characterInfo;

        AssetDatabase.CreateAsset(font, path + ".fontsettings");
        AssetDatabase.Refresh();
    }
}
