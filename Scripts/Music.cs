using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    //设置游戏的声音
    private static Music instance;
    // Start is called before the first frame update
    private void Awake()
    {
        //检查实例，如果已经存在，销毁该对象
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
        //确保这个对象不会被破坏(这样背景音乐就会继续播放)
        DontDestroyOnLoad(gameObject);
    }
}
