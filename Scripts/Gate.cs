using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    //�ӷ�ԭ��
    private GameManager manager;
    //�ڼ�����пɼ�(�ο�����Ƶ)
    public AudioSource scoreAudio;

    private bool addedScore;
    // Start is called before the first frame update
    void Start()
    {
        //�ҵ���Ϸ������
        manager = GameObject.FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        //�������Ƿ񿪳���������ţ����ǻ�û������κλ���
        if (!other.gameObject.transform.root.CompareTag("Player") || addedScore)
        {
            return;
        }
        //��������1������һЩ��Ƶ
        addedScore = true;
        manager.UpdateScore(1);
        scoreAudio.Play();

    }
         



}
