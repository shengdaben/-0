using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WorldGenerator : MonoBehaviour
{
    //设置一个材质
    public Material meshMaterial;
    //公布一个尺寸
    public Vector2 dimensions;
    //设置一个系数
    public float scale;
    //设置一个柏林噪声
    public float perLinScale;
    //设置一个排列位移
    public float offset;
    //设置一个波高
    public float waveHeight;

    //定义一个地形数组
    GameObject[] pieces = new GameObject[2];
    //全局速度
    public float globalSpeed;

    //以下是消除地形的空白
    private Vector3[] beginPoints;
    //随机地形
    public float randomness;
    //确保连接2个圆柱体用的，看起来衔接的平滑，值越小就越不精细
    public int startTranstionLength;

    public BasicMoveMent lampMovement;


    // 障碍生成概率，值越大意味着障碍越少，值越小意味着障碍越多。
    public int startObstacleChance;
    //生成障碍的门
    public GameObject gate;
    //障碍数组，你也可以自己制作新的障碍，加到数组里随机
    public GameObject[] obstacles;
    //设置门的生成
    public int gateChance;
    //定义一个父节点
    private GameObject currentCyclinder;

    public int obstacleChanceAcceleration;

    public int showItemDistance;
    public float shadowHeight;


    // Start is called before the first frame update
    void Start()
    {
        beginPoints = new Vector3[(int)dimensions.x + 1];

        for (int i = 0; i < 2; i++)
        {
            GenerateWolrdPiece(i);
        }
    }
    // Update is called once per frame
   
    //把生成的地形放入数组，并摆正位置
    void GenerateWolrdPiece(int i)
    {
        //生成圆柱体，保存数组
        pieces[i] = GreateCyLinder();
        //根据他的索引，摆正他的位置
        pieces[i].transform.Translate(Vector3.forward * ( dimensions.y *scale* Mathf.PI) * i);

        //在写一个函数，标记尾部位置，将来移动
        UpdateSinglePiece(pieces[i]); 
    }
    //设置尾部位置
    void UpdateSinglePiece(GameObject piece)
    {
        //通过调用脚本控制移动
        BasicMoveMent movement = piece.AddComponent<BasicMoveMent>();
        movement.moveSpeed = -globalSpeed;
       
        //设置灯(定向灯)的旋转速度
        if (lampMovement !=null)
        {
            movement.rotateSpeed = lampMovement.rotateSpeed;
        }

        //创建结束点并且设置他的位置
        GameObject endPoint = new GameObject();
        endPoint.transform.position = piece.transform.position + Vector3.forward * (dimensions.y * Mathf.PI * scale);
        //创建父节点
        endPoint.transform.parent = piece.transform;
        endPoint.name = "End Point";

        //每一次加偏移，确保柏林噪声不一样
        offset += randomness;

        if (startObstacleChance > 5)
        {
            startObstacleChance -= obstacleChanceAcceleration;
        }
    }
    IEnumerator UpdateWorldPieces()
    {
        //这时候第一个已经过去了，删除掉
        Destroy(pieces[0]);
        //把当前的往前串一位
        pieces[0] = pieces[1];
        //第二个块再生成新的
        pieces[1] = GreateCyLinder();

        //重新设置位置
        pieces[1].transform.position = pieces[0].transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
        pieces[1].transform.rotation = pieces[0].transform.rotation;

        UpdateSinglePiece(pieces[1]);

        yield return 0;
    }

    
    private void LateUpdate()
    {
        if (pieces[1] && pieces[1].transform.position.z <= -15)
        {
            //携程
            StartCoroutine(UpdateWorldPieces());
        }
        UpdateAllItems();
    }

    void UpdateAllItems()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        for (int i = 0; i < items.Length; i++)
        {
            foreach (MeshRenderer renderer in items[i].GetComponentsInChildren<MeshRenderer>())
            {
                bool show = items[i].transform.position.z < showItemDistance;

                if (show)
                {
                    renderer.shadowCastingMode = (items[i].transform.position.y < shadowHeight) ? ShadowCastingMode.On : ShadowCastingMode.Off;
                }

                renderer.enabled = show;
            }
        }
    }

    public GameObject GreateCyLinder()
    {
        //mesh通过网格绘制的，meshFilter持有mesh的引用，MeshRenderer
        //创建gameobject并且命名
        GameObject newCyLinder = new GameObject();
        //定义对象的名字 
        newCyLinder.name = "World piece";

        currentCyclinder = newCyLinder;

        //添加meshFitter和meshRenderer
        MeshFilter meshFilter = newCyLinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newCyLinder.AddComponent<MeshRenderer>();

        //设置材质，材质包含纹理，shader
        meshRenderer.material = meshMaterial;
        //设置网格形状
        meshFilter.mesh = Genrate();

        //创建网格以后，添加碰撞，适配新的Mesh
        newCyLinder.AddComponent<MeshCollider>();

        return newCyLinder;
    }
    Mesh Genrate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "MESH";

        //需要UV，三角形，顶点
        Vector3[] vertices = null;
        Vector2[] uvs = null; 
        int[] triangles = null;

        //创建形状
        GreateShape(ref vertices, ref uvs, ref triangles);

        //再去赋值
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        //让网格变得凹凸有质感*
        mesh.RecalculateNormals();

        return mesh;
    }

    void GreateShape(ref Vector3[] vertices,ref Vector2[] uvs,ref int[] triangles)
    {
        //向z轴延伸，x是横截面
        int xCount = (int)dimensions.x;
        int zCount = (int)dimensions.y;

        //初始化顶点和uvs数组，通过定义尺寸
        vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        uvs = new Vector2[(xCount + 1) * (zCount + 1)];

        int index = 0;
        //半径计算
        float radius = xCount * scale * 0.5f;

        //通过一个顶点和双循环，设置顶点和uvs
        for (int x = 0; x <=xCount; x++)
        {
            for (int z = 0;z <=zCount;z++)
            {
                //首先获取圆柱体的角度，根据x的位置
                float angle = x * Mathf.PI * 2f / xCount;

                //通过角度计算了顶点的值
                vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);

                //接下来可以计算出uvs的值
                uvs[index] = new Vector2(x * scale, z * scale);

                //现在我们可以用之前的柏林噪声了
                float pX = (vertices[index].x * perLinScale) + offset;
                float pZ = (vertices[index].z * perLinScale) + offset;

                //需要一个中心点和当前的顶点做减法然后归一化，再去计算柏林噪声
                Vector3 center = new Vector3(0, 0, vertices[index].z);
                vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;

                //解决地图链接问题
                //如果是这样，我们必须将柏林噪声值与起始点结合起来
                //我们需要增加来自柏林噪声的顶点的百分比
                //从起始点开始减小百分比
                //这样，它将从最后一个世界片段过渡到新的柏林噪声值

                if (z < startTranstionLength && beginPoints[0] != Vector3.zero)
                {
                    //当我们进一步进入圆柱体时，顶点的柏林噪声百分比会增加
                    float perLinPercentage = z * (1f / startTranstionLength);
                    //不要使用z起始点，因为它没有正确的位置，我们只关心x和y轴上的噪声
                    Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);
                    //结合起始点(前一个世界块的最后一个顶点)和原始顶点，平滑过渡到新的世界块
                    vertices[index] = (perLinPercentage * vertices[index]) + ((1f - perLinPercentage) * beginPoint);
                }
                else if (z ==zCount)
                {
                    //如果这些是最后的顶点，更新起始点，使下一个部分也能顺利过渡
                    beginPoints[x] = vertices[index];
                }


                //设置生成障碍物 ,使用网格顶点在随机位置生成物品
                if (Random.Range(0,startObstacleChance) == 0 &&!(gate == null && obstacles.Length ==0))
                {
                    GreateItem(vertices[index],x); 
                }
                index++;

            }
        }

        //初始化三角形数组，x乘以z这样一个总数，1个矩形2个三角形，一个三角形3个顶点，那么一个正方形就是6个顶点
        triangles = new int[xCount * zCount * 6];

        //创建一个数组，存6个三角形顶点，方便使用
        int[] boxBase = new int[6];
        int current = 0;

        for (int x = 0; x < xCount; x++)
        {
            //每次重新赋值，根据x的变化
            boxBase = new int[]
            {
                x*(zCount+1),
                x*(zCount+1)+1,
                (x+1)*(zCount+1),
                x*(zCount+1)+1,
                (x+1)*(zCount+1)+1,
                (x+1)*(zCount+1)
            };
            for (int z = 0; z < zCount; z++)
            {
                //增长一下这个索引，方便计算下一个正方形
                for (int i = 0; i < 6; i++)
                {
                    boxBase[i] = boxBase[i]+1;
                }

                //把这6个顶点填充到三角形里去
                for (int j = 0; j < 6; j++)
                {
                    triangles[current + j] = boxBase[j] -1;
                }
                current += 6;

            }
        }
    }
    //生成地刺
    void GreateItem(Vector3 vert,int x)
    {
        ////获取圆柱体的中心，但使用来自顶点的z值
        Vector3 zCenter = new Vector3(0, 0, vert.z);

        if (zCenter -vert == Vector3.zero || x ==(int)dimensions.x/4||x==(int)dimensions.x/4*3 )
        {
            return;
        }
        //检查中心和顶点之间的夹角是否正确
        //if (zCenter - vert == Vector3.zero || x == (int)dimensions.x / 4 || x == (int)dimensions.x / 4 * 3)
        //    return;
        //创建一个小概率成为门(gateChance)和大概率成为障碍的新道具
        GameObject newItem = Instantiate(UnityEngine.Random.Range(0, gateChance) == 0 ? gate:obstacles[(UnityEngine.Random.Range(0, obstacles.Length))]);
        //向中心位置旋转项目
        newItem.transform.rotation = Quaternion.LookRotation(zCenter - vert, Vector3.up);
        //将项目放置在垂直位置
        newItem.transform.position = vert;
        //将新项作为当前柱面的父项，这样它就会移动和旋转
        newItem.transform.SetParent(currentCyclinder.transform, false);

    }
    //获取地图
    public Transform GetWorldPoece()
    {
        return pieces[0].transform;
    }

   
}
