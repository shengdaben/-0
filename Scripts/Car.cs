using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Car : MonoBehaviour
{
    //����һ������,������ȡ��������
    public Transform[]wheelMeshs;
    //������ȡ���ӵ���ײ��
    public WheelCollider[] wheelColliders;
    //��ת�ٶ�
    public int rotationSpeed;
    //��ת�Ƕ�
    public int rotationAngle;
    //������ת�ٶ�
    public int wheelRotationSpeed;
    //����
    private int targetRotation;


    //����򻬻����ӳ�
    public float skidMarkDelay;
    //����򻬶���
    public GameObject skidMark;
    //���廮�۳ߴ�
    public float skidMarkSize;
    //���廮�����ڵĵ�
    public Transform[] skidMarkPivots;

    //����һ������ƫ�� 
    public float gressOffset;
    //�����̲���Ч
    public Transform[] grassEffects;

    //�����ͼ
    private WorldGenerator generator;

    //����һ�����壻��С������������
    public Rigidbody rB;

    //���峵������һ����
    public Transform back;

    //����ʩ�ӵ���
    public float constantBackForce;
    //����һ������ֵ���ж�ʲôʱ���кۼ�
    private bool skidMarkRoutine;

    private float lastRotation;
    //����һ��Աȣ���С����ת��
    public float minRotationDifference;


    //������С���𻵶���
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
            //��ȡ������ײ����λ�ø��Ƕ�
            wheelColliders[i].GetWorldPose(out pos, out qua);
            //��������λ��
           wheelMeshs[i].position = pos;

           wheelMeshs[i].Rotate(Vector3.right * Time.deltaTime * wheelRotationSpeed);
        }
        //�������뷽ʽ
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

    //���ó��ĽǶ�
    void UpdateTargetRotation()
    {
        if (Input.GetAxis("Horizontal") == 0)
        {
            if (Input.mousePosition.x > Screen.width * 0.5)
            {
                //��ת
                targetRotation = rotationAngle;
            }
            else
            {
                //��ת
                targetRotation = -rotationAngle;
            }
        }
        else
        {
            targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
        }
    }



    
    
   //��Ϊwhile��true�����÷����ᵼ����ѭ�������Բ���Э�ɣ�����Ч��ʾ���� 
    IEnumerator SkidMark()
    {
        //ѭ������
        while (true)
        {
            //��ʾskidmarks�������������Ҫskidmark
            if (skidMarkRoutine)
            {
                for (int i = 0; i < skidMarkPivots.Length; i++)
                {
                    ////�����������֣�ʵ����һ��ɲ����ǣ������丸���������У�ʹ����ʵ���ƶ�
                    GameObject newSkidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
                    newSkidMark.transform.parent = generator.GetWorldPoece();
                    newSkidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
                }
            }
            //�ȴ���������֮����ӳ�
            yield return new WaitForSeconds(skidMarkDelay);
            
        }
    }


    //����Ķ���һ��д��fixedUpdate�����������̥�ۼ���ʵ���ڵ���ĺۼ�
    //���������Ϊ�˽��С��������������Ч�����ڵ�ͼ����ʾ��Ч�ķ���
    void UpdateEffects()
    {
        //�����ڵ����ϣ���Ҫ������̥�Ķ���,��̥�ڵ��ϣ��Ͳ�������
        bool addForce = true;

       
        //��Ҫ��������
        for (int i = 0; i < 2; i++)
        {
            //��ȡ���ӵ�����ƫ������λ�ã�ȡ���ɶԵĶ���
            Transform wheelMesh = wheelMeshs[i];
            //д����ļ�⣬���¿�С���ĺ����ڲ��ڵ�����
            if (Physics.Raycast(wheelMesh.position, Vector3.down, gressOffset * 1.5f))
            {
                //�������û����ʾ��������ʾ����
                if (!grassEffects[i].gameObject.activeSelf)
                {
                    grassEffects[i].gameObject.SetActive(true);
                }
                //���²ݵ�Ч���߶Ⱥͻ��۸߶ȣ���ƥ���������
                float effectHeight = wheelMesh.position.y - gressOffset;
                Vector3 targetPosition = new Vector3(grassEffects[i].position.x, effectHeight, wheelMesh.position.z);
                grassEffects[i].position = targetPosition;
                skidMarkPivots[i].position = targetPosition;

                //��ʱ��̥�ڵ��ϣ�����Ҫ����
                addForce = false;
            }
            else if (grassEffects[i].gameObject.activeSelf)
            {
                grassEffects[i].gameObject.SetActive(false);
            }

        }
        //�ж��ǲ�������ת ����ֵ,���������С����תֵ������һ������ת��
        bool rotated = Mathf.Abs(lastRotation - transform.localEulerAngles.y) > minRotationDifference;
        //�������󲿼����Ա����ȶ�
        if (addForce)
        {
            rB.AddForceAtPosition(back.position, Vector3.down * constantBackForce);
            //��Ҫ��ʾ����
            skidMarkRoutine = false;
        }
        else
        {
            if (targetRotation != 0)
            {
                //��������Ѿ���ת����ʾɲ���ۼ�
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
                //ֱ�߲���Ҫ
                skidMarkRoutine = false;
            }
        }
        //���һ�����ת�Ƕ�
        lastRotation = transform.localEulerAngles.y;
    }

    void FixedUpdate()
    {
        UpdateEffects();
    }

    //С���߻�
    public void FallApart()
    {
        Instantiate(ragdoll, transform.position, transform.rotation);
        gameObject.SetActive(false);
    }
}
