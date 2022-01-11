using UnityEngine;

/// <summary>
/// 技能手势路径
/// </summary>
public class SkillGesturePath : MonoBehaviour
{
    //图片偏移量
    private const float OFFSET_DETAL = 0.08F;

    [SerializeField]
    private Transform[] nodes;  //路径点

    [SerializeField]
    private string gesture;     //手势名称

    private LineRenderer pathRenderer;
    private Material pathMaterial;
    private Vector2 textureOffset;
    private int vertexCount = 0;

    public Transform[] Path
    {
        get { return nodes; }
    }

    public string Gesture
    {
        get { return gesture; }
    }

    void Start()
    {
        pathRenderer = GetComponent<LineRenderer>();
        pathMaterial = pathRenderer.material;
        textureOffset = Vector2.zero;
    }

    /// <summary>
    /// 画下一个路径点
    /// </summary>
    /// <param name="position"></param>
    public void DrawNextNode(Vector3 position)
    {
        pathRenderer.SetVertexCount(vertexCount + 1);
        pathRenderer.SetPosition(vertexCount, position);
        vertexCount++;
    }

    /// <summary>
    /// UV偏移效果
    /// </summary>
    public void TextureOffset()
    {
        textureOffset.Set(textureOffset.x - OFFSET_DETAL * Time.deltaTime, textureOffset.y);
        pathMaterial.SetTextureOffset("_MainTex", textureOffset);
    }

    /// <summary>
    /// 清除路径
    /// </summary>
    public void Clear()
    {
        textureOffset = Vector2.zero;
        vertexCount = 0;
        pathRenderer.SetVertexCount(vertexCount);
    }

    #region Draw Gizimos

    void OnDrawGizmos()
    {
        iTween.DrawPathGizmos(nodes);
    }

    #endregion Draw Gizimos
}