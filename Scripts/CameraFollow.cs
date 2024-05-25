using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    //设置捕获目标
    public Transform camTarget;

    //相机要比目标高一些
    public float height = 5f;

    //设置相机的高度和旋转平滑度
    public float heightDamping = 1f;
    //相机旋转平滑度
    public float rotationDamping = 1f;

    //设置相机偏移目标的距离
    public float distance;

    public float startDelay;

    float originalRotationDamping;
    bool canSwitch;
    // Start is called before the first frame update

    private void Start()
    {
        originalRotationDamping = rotationDamping;
        StartCoroutine(SwitchAngle());
    }
    private void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetAxis("Horizontal") != 0) && rotationDamping == 0.1f && canSwitch)
        {
            rotationDamping = originalRotationDamping;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (camTarget == null)
        {
            return;
        }

        //取一些值，分为目前的值和将要达到的值
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;
        //将来的值
        float wantedRotationAngle = camTarget.eulerAngles.y;
        float wantedHeight = camTarget.position.y + height;

        //目前的值，将来的和现在的做差值
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        //第一步把摄像机位置移动到被观察者位置
        transform.position = camTarget.position;
        //因为重叠所以要设置偏移一点
        transform.position -= currentRotation * Vector3.forward * distance;

        //重置相机的位置
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        //摄像机观察的物体
        transform.LookAt(camTarget);

       
    }

    IEnumerator SwitchAngle()
    {
        yield return new WaitForSeconds(startDelay);

        canSwitch = true;
    }
}
