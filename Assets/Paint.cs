//#define quadBrush
//#define circleBrush
#define jitteredBrush

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paint : MonoBehaviour {
    public Material swapMat;
    public RenderTexture render;
    public GameObject brushSplat;

    private Mesh mesh;
    private Vector3 brushPos;
    private Vector3 brushPosPre;
    private Vector3 velocity;

    private int verticles_per_bristle = 6;
    private int BristleCount = 50;
    private float bristleLength = 1;
    #region 方形刷子
    private const int BristleRows = 10;
    private int bristlesCols;
    #endregion
    #region 圆形刷子
    private const int BristleRadiateNums =20;
    private int circleNums;
    #endregion
    #region 向日葵刷子
    Vector2[] randoms;
    Vector3[] Bristle_verticles;
    Vector3[] Bristle_verticlesPre;
    //private int circleNums;
    #endregion
    private float BristlePadding;
    private float BristleWidth;
    private const float BristleWidthInit = 0.05f;
    private const float BristlePaddingInit = 0.02f;
    
    private float brushSize = 1;

    private const float brushSize_min = 0.1f;
    private const float brushSize_max = 10;
    private const float BristlePadding_min = 0.001f;
    private const float BristlePadding_Max = 0.5f;
    private const float jitter = 5;

    private const float PI= 3.14159265f;
    const float PHI = 1.618033988749895f;
    private Vector2 splatMeshSize;

    RenderTexture tempRender = null;
    bool isPushedMouse = false;
    // Use this for initialization
    void Start () {
        splatMeshSize = new Vector2(19.2f,10.8f);
        //初始化刷子形状
        SetBrushShape();
        //初始化刷子尺寸
        SetBrushScale();
        //初始化刷毛宽度
        SetBristleWidth();
        //初始化刷子顶点
        Bristle_verticles = new Vector3[BristleCount * verticles_per_bristle];
        Bristle_verticlesPre = new Vector3[BristleCount * verticles_per_bristle];
        //初始化刷子墨迹网格
        mesh =brushSplat.GetComponent<MeshFilter>().mesh;
        Init_Bristles_verticles();

    }

	// Update is called once per frame
	void Update () {
        if (!isPushedMouse)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                brushSize += Input.GetAxis("Mouse ScrollWheel");
                Clamp(brushSize,brushSize_min,brushSize_max);
                SetBrushScale();
                //SetBristleWidth();
                brushPos = ScreenPos2Mesh(Input.mousePosition, 0.01f, splatMeshSize);
                //DrawSplatMesh(mesh, brushPos);
            }
            else
            {
                //mesh.Clear();
            }
        }
        if(brushPosPre==null)
        {
            brushPosPre = ScreenPos2Mesh(Input.mousePosition, 0.01f, splatMeshSize);
        }
        brushPos = ScreenPos2Mesh(Input.mousePosition, 0.01f, splatMeshSize);
        velocity = brushPos - brushPosPre;
        Update_Bristles_verticles();
        DrawSplatMesh(mesh, brushPos);

        brushPosPre = brushPos;
        
    }
    public void Fun_OnMouseDown()
    {
        brushPos = ScreenPos2Mesh(Input.mousePosition,0.01f, splatMeshSize);
        DrawSplatMesh(mesh,brushPos);
        StartCoroutine(SwapSplat());
        isPushedMouse = true;
    }
    public void Fun_OnMouseDrag()
    {
        brushPos = ScreenPos2Mesh(Input.mousePosition, 0.01f, splatMeshSize);
        DrawSplatMesh(mesh, brushPos);
        StartCoroutine(SwapSplat());
    }
    public void Fun_OnMouseUp()
    {
        mesh.Clear();
        isPushedMouse = false;
        //起笔后重置刷子形状，得到每次落笔是形状与上次落笔不同
        SetBrushShape();
    }
    public void Fun_Clear()
    {
        swapMat.SetTexture("_MainTex", null);
    }

    #region 方法
    /// <summary>
    /// 屏幕坐标转换为网格坐标
    /// </summary>
    /// <param name="MousePos">屏幕坐标</param>
    /// <param name="scaleSize">网格尺寸与屏幕尺寸缩放比例</param>
    /// <param name="meshSize">网格尺寸</param>
    /// <returns></returns>
    Vector3 ScreenPos2Mesh(Vector3 MousePos,float scaleSize,Vector3 meshSize)
    {
        Vector3 meshPos=MousePos*scaleSize-meshSize/2;
        return meshPos;
    }
    /// <summary>
    /// 根据鼠标位置绘制刷子在mesh上的墨迹
    /// </summary>
    /// <param name="mesh">要绘制的mesh</param>
    /// <param name="brushPos">刷子位置</param>
    void DrawSplatMesh(Mesh mesh,Vector3 brushPos)
    {
#if quadBrush
        bristlesCols = BristleCount / BristleRows;
        Vector2 brushArea = new Vector2((bristlesCols-1)* BristlePadding, (BristleRows - 1) * BristlePadding);
        for (int i = 0; i < BristleRows; i++)
        {
            for (int j = 0; j < bristlesCols; j++)
            {
                Bristle_verticles[i * bristlesCols + j] =brushPos+new Vector3(j * BristlePadding, i * BristlePadding, 0) - new Vector3(brushArea.x / 2, brushArea.y / 2, 0);
            }
        }
#elif circleBrush
        circleNums = BristleCount / BristleRadiateNums;
        float angleDelt = (float)360 / (BristleRadiateNums);
        for (int i =0; i < circleNums; i++)
        {
            for (float angle = 0, j=0; Mathf.Floor(angle) < 360;)
            {
                float r = BristlePadding * (i+1);
                float x =r* Mathf.Cos(angle / 180 * PI);
                float y = r * Mathf.Sin(angle / 180 * PI);
                Bristle_verticles[i * BristleRadiateNums + (int)j]=brushPos + new Vector3(x, y, 0);
                Debug.Log(i * BristleRadiateNums + (int)j);
                angle += angleDelt;
                j++;
            }
        }
#elif jitteredBrush
        //jittered sunflower distribution
        
      
#endif
        Vector3[] Mesh_verticles = new Vector3[BristleCount*verticles_per_bristle*4];
        Vector2[] uvs = new Vector2[BristleCount * verticles_per_bristle * 4];
        for (int i = 0; i < BristleCount * verticles_per_bristle; i++)
        {
            Mesh_verticles[i * 4] =Bristle_verticles[i]+new Vector3(-BristleWidth / 2,-BristleWidth / 2,0);
            Mesh_verticles[i * 4+1] = Bristle_verticles[i] + new Vector3(-BristleWidth / 2, BristleWidth / 2, 0);
            Mesh_verticles[i * 4+2] = Bristle_verticles[i] + new Vector3(BristleWidth / 2, BristleWidth / 2, 0);
            Mesh_verticles[i * 4+3] = Bristle_verticles[i] + new Vector3(BristleWidth / 2, -BristleWidth / 2, 0);
            uvs[i * 4] = new Vector2(0,0);
            uvs[i * 4+1] = new Vector2(0, 1);
            uvs[i * 4+2] = new Vector2(1,1);
            uvs[i * 4+3] = new Vector2(1, 0);
        }
        int[] indexes = new int[BristleCount * verticles_per_bristle * 6];
        for (int i = 0; i < BristleCount * verticles_per_bristle; i++)
        {
            indexes[i * 6] = 4*i+0;
            indexes[i * 6+1] = 4 * i+ 1;
            indexes[i * 6+2] = 4 * i + 3;
            indexes[i * 6+3] = 4 * i + 3;
            indexes[i * 6+4] = 4 * i + 1;
            indexes[i * 6+5] = 4 * i + 2;
        }
        mesh.Clear();
        mesh.vertices =Mesh_verticles;
        mesh.triangles = indexes;
        mesh.uv = uvs;
        //mesh.UploadMeshData();
    }
    /// <summary>
    /// 开启携程在帧末尾置换笔刷墨迹到画板上
    /// </summary>
    /// <returns>帧末尾</returns>
    IEnumerator SwapSplat()
    {
        yield return new WaitForEndOfFrame();
        Swap();
    }
    /// <summary>
    /// 将刷子的墨迹置换到置换层上
    /// </summary>
    void Swap()
    {
        if (tempRender != null)
        {
            Destroy(tempRender);
        }
        tempRender = new RenderTexture(render.width, render.height, 0);
        tempRender.format = RenderTextureFormat.ARGBFloat;
        tempRender.wrapMode = TextureWrapMode.Repeat;
        tempRender.filterMode = FilterMode.Bilinear;
        Graphics.Blit(render, tempRender,swapMat);
        swapMat.SetTexture("_MainTex", tempRender);
        //Graphics.Blit(tempRender,tempRender,swapMat);
        //RenderTexture.active = tempRender;
        //Destroy(tempRender);
    }
    /// <summary>
    /// 设置刷子尺寸
    /// </summary>
    void SetBrushScale()
    {
        BristlePadding = BristlePaddingInit * brushSize;
        //Clamp(BristlePadding,BristlePadding_min,BristlePadding_Max);
    }
    /// <summary>
    /// 设置刷毛宽度
    /// </summary>
    void SetBristleWidth()
    {
        BristleWidth = BristleWidthInit * brushSize;
        //Clamp(BristlePadding, BristlePadding_min, BristlePadding_Max);
    }
    /// <summary>
    /// 对数字限定取值
    /// </summary>
    /// <param name="x">取值数字</param>
    /// <param name="min">限定上线</param>
    /// <param name="max">限定下线</param>
    /// <returns></returns>
    float Clamp(float x,float min,float max)
    {
        float value;
        value = x > min ? x : min;
        value = value < max ? value : max;
        return value;
    }
    /// <summary>
    /// 设置刷子形状
    /// </summary>
    void SetBrushShape()
    {
        randoms = new Vector2[BristleCount ];
        for (int i = 0; i < BristleCount ; i++)
        {
            randoms[i].x = Random.Range(0.0f, 1.0f);
            randoms[i].y = Random.Range(0.0f, 1.0f);
        }
    }
    void Init_Bristles_verticles()
    {
        for (int i = 0; i < BristleCount; i++)
        {
            for (int j = 0; j < verticles_per_bristle; j++)
            {
                float theta = (float)((i + (randoms[i].x - 0.5f) * jitter) * 2.0 * PI / (PHI * PHI));
                float r = Mathf.Sqrt(i + (randoms[i].y - 0.5f) * jitter) / Mathf.Sqrt(BristleCount);

                float spacing = (float)(bristleLength / (verticles_per_bristle - 1.0));
                Vector3 brushSpaceBristlePosition = new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), -j * spacing);

                Vector3 bristlePosition = brushPos + brushSpaceBristlePosition * brushSize;
                Bristle_verticles[i * verticles_per_bristle + j] = bristlePosition;
            }
        }
    }
    void Update_Bristles_verticles()
    {
        for (int i = 0; i < BristleCount; i++)
        {
            for (int j = 0; j < verticles_per_bristle; j++)
            {
                Bristle_verticles[i * verticles_per_bristle + j] += velocity * (bristleLength + Bristle_verticles[i * verticles_per_bristle + j].z);
            }
        }
    }
    #endregion
}
