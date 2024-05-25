using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPrelin : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public bool usePerLin;
    private float a=0.06f; //平滑度
    // Start is called before the first frame update
    void Start()
    {
        //获取这个Line
        lineRenderer = GetComponent<LineRenderer>();

        Vector3[] posArray = new Vector3[100];
        float ranx = Random.Range(1, 1000);
        float rany = Random.Range(1,1000);
        //给v3赋值
        for (int i = 0; i < posArray.Length; i++)
        {
            if (usePerLin)
            {
                posArray[i] = new Vector3(i * 0.1f, Mathf.PerlinNoise(i*a+ranx,i*a+rany), 0);
            }
            else
            {
                posArray[i] = new Vector3(i * 0.1f, Random.value, 0);
            }
             
        }
        //设置点
        lineRenderer.SetPositions(posArray);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
