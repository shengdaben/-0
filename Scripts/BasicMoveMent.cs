using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMoveMent : MonoBehaviour
{
    //设置跟随汽车移动和旋转
    //检查器中可见的公共变量
    public float moveSpeed = 30f;//设置地图移动
    public float rotateSpeed = 30f;
    public bool lamp;

    //在检查器中不可见
    private WorldGenerator generator;
    private Car car;
    private Transform carTransform;
    // Start is called before the first frame update
    void Start()
    {
        //找到汽车和世界生成器
        car = GameObject.FindObjectOfType<Car>();
        generator = GameObject.FindObjectOfType<WorldGenerator>();

        if (car != null)
        {
            carTransform = car.gameObject.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //控制地形向前移动移动 向车移动
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
       
        //如果有一辆车，当玩家的车旋转时也旋转
        if (car != null)
            CheckRotation();
        
    }
    void CheckRotation()
    {
        //定向光在世界物体以外的另一个轴上旋转
        Vector3 direction = (lamp) ? Vector3.right : Vector3.forward;
        //获取汽车旋转
        float carRotation = carTransform.localEulerAngles.y;
        //获得左旋转(eulerAngles总是返回正旋转)
        if (carRotation > car.rotationAngle *2f)
        {
            carRotation = (360 - carRotation) * -1f;
        }
        //根据方向值、速度值、小车旋转和世界尺寸旋转该对象
        transform.Rotate(direction * -rotateSpeed * (carRotation / car.rotationAngle) * (36f / generator.dimensions.x) * Time.deltaTime);
    }
}
