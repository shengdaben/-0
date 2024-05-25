using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    //������Ϸ�ϰ���ײ�ش̣�������Ϸ����
    //game manager reference
    private GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        //�ҵ���Ϸ������
        manager = GameManager.FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision other)
    {
        //��������������ϰ���������Ϸ
        if (other.gameObject.transform.root.CompareTag("Player"))
        {
            manager.GameOver();
        }
    }
}
