using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Car : MonoBehaviour
{
    //定义一个轮子,用来获取轮子网格
    public Transform[]wheelMeshs;
    //用来获取轮子的碰撞器
    public WheelCollider[] wheelColliders;
    //旋转速度
    public int rotationSpeed;
    //旋转角度
    public int rotationAngle;
    //轮子旋转速度
    public int wheelRotationSpeed;
    //定义
    private int targetRotation;


    //定义打滑划痕延迟
    public float skidMarkDelay;
    //定义打滑对象
    public GameObject skidMark;
    //定义划痕尺寸
    public float skidMarkSize;
    //定义划痕所在的点
    public Transform[] skidMarkPivots;

    //定义一个向下偏移 
    public float gressOffset;
    //定义绿草特效
    public Transform[] grassEffects;

    //定义地图
    private WorldGenerator generator;

    //定义一个刚体；给小车增加增加力
    public Rigidbody rB;

    //定义车身后面的一个点
    public Transform back;

    //定义施加的力
    public float constantBackForce;
    //定义一个布尔值，判断什么时候有痕迹
    private bool skidMarkRoutine;

    private float lastRotation;
    //和上一针对比，最小的旋转角
    public float minRotationDifference;


    //以下是小车损坏动画
    public GameObject ragdoll;


   
    
    // Start is called before the first frame update
    void Start()
    {
        generator = GameObject.FindObjectOfType<WorldGenerator>();
        StartCoroutine(SkidMark());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 0; i <wheelMeshs.Length; i++)
        {
            Quaternion qua;
            Vector3 pos;
            //获取轮子碰撞器的位置跟角度
            wheelColliders[i].GetWorldPose(out pos, out qua);
            //设置轮子位置
           wheelMeshs[i].position = pos;

           wheelMeshs[i].Rotate(Vector3.right * Time.deltaTime * wheelRotationSpeed);
        }
        //设置输入方式
        if (Input.GetMouseButton(0)|| Input.GetAxis("Horizontal")!=0)
        {
            UpdateTargetRotation();
        }
        else if(targetRotation !=0)
        {
            targetRotation = 0;
        }
        Vector3 rotation = new Vector3(transform.localEulerAngles.x, targetRotation, transform.localEulerAngles.z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), rotationSpeed * Time.deltaTime);

    }

    //设置车的角度
    void UpdateTargetRotation()
    {
        if (Input.GetAxis("Horizontal") == 0)
        {
            if (Input.mousePosition.x > Screen.width * 0.5)
            {
                //右转
                targetRotation = rotationAngle;
            }
            else
            {
                //左转
                targetRotation = -rotationAngle;
            }
        }
        else
        {
            targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
        }
    }



    
    
   //因为while（true），用方法会导致死循环，所以采用协成，将特效显示出来 
    IEnumerator SkidMark()
    {
        //循环持续
        while (true)
        {
            //显示skidmarks如果我们现在需要skidmark
            if (skidMarkRoutine)
            {
                for (int i = 0; i < skidMarkPivots.Length; i++)
                {
                    ////对于两个后轮，实例化一个刹车标记，并将其父化到环境中，使其真实地移动
                    GameObject newSkidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
                    newSkidMark.transform.parent = generator.GetWorldPoece();
                    newSkidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
                }
            }
            //等待单个滑痕之间的延迟
            yield return new WaitForSeconds(skidMarkDelay);
            
        }
    }


    //物理的东西一般写在fixedUpdate里，更新物理轮胎痕迹，实现在地面的痕迹
    //这个方法是为了解决小车不滑动生成特效，和在地图上显示特效的方法
    void UpdateEffects()
    {
        //轮子在地面上，需要加上轮胎的动力,轮胎在地上，就不加力了
        bool addForce = true;

       
        //需要两个轮子
        for (int i = 0; i < 2; i++)
        {
            //获取轮子的网格，偏移两个位置，取到成对的东西
            Transform wheelMesh = wheelMeshs[i];
            //写物理的检测，向下看小车的后轮在不在地面上
            if (Physics.Raycast(wheelMesh.position, Vector3.down, gressOffset * 1.5f))
            {
                //如果粒子没有显示，让他显示出来
                if (!grassEffects[i].gameObject.activeSelf)
                {
                    grassEffects[i].gameObject.SetActive(true);
                }
                //更新草的效果高度和滑痕高度，以匹配这个车轮
                float effectHeight = wheelMesh.position.y - gressOffset;
                Vector3 targetPosition = new Vector3(grassEffects[i].position.x, effectHeight, wheelMesh.position.z);
                grassEffects[i].position = targetPosition;
                skidMarkPivots[i].position = targetPosition;

                //此时轮胎在地上，不需要加力
                addForce = false;
            }
            else if (grassEffects[i].gameObject.activeSelf)
            {
                grassEffects[i].gameObject.SetActive(false);
            }

        }
        //判断是不是在旋转 绝对值,如果他比最小的旋转值都大，他一定是旋转的
        bool rotated = Mathf.Abs(lastRotation - transform.localEulerAngles.y) > minRotationDifference;
        //在汽车后部加力以保持稳定
        if (addForce)
        {
            rB.AddForceAtPosition(back.position, Vector3.down * constantBackForce);
            //不要显示滑痕
            skidMarkRoutine = false;
        }
        else
        {
            if (targetRotation != 0)
            {
                //如果汽车已经旋转，显示刹车痕迹
                if (rotated && !skidMarkRoutine)
                {
                    skidMarkRoutine = true;
                }
                else if (!rotated && skidMarkRoutine)
                {
                    skidMarkRoutine = false;
                }

            }
            else
            {
                //直走不需要
                skidMarkRoutine = false;
            }
        }
        //最后一针的旋转角度
        lastRotation = transform.localEulerAngles.y;
    }

    void FixedUpdate()
    {
        UpdateEffects();
    }

    //小车催坏
    public void FallApart()
    {
        Instantiate(ragdoll, transform.position, transform.rotation);
        gameObject.SetActive(false);
    }
}
