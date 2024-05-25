using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    //加分原理
    private GameManager manager;
    //在检查器中可见(参考门音频)
    public AudioSource scoreAudio;

    private bool addedScore;
    // Start is called before the first frame update
    void Start()
    {
        //找到游戏管理器
        manager = GameObject.FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        //检查玩家是否开车穿过这个门，我们还没有添加任何积分
        if (!other.gameObject.transform.root.CompareTag("Player") || addedScore)
        {
            return;
        }
        //将分数加1并播放一些音频
        addedScore = true;
        manager.UpdateScore(1);
        scoreAudio.Play();

    }
         



}
