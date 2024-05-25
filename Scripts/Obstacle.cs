using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    //设置游戏障碍碰撞地刺，调动游戏结束
    //game manager reference
    private GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        //找到游戏管理器
        manager = GameManager.FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision other)
    {
        //如果玩家碰到这个障碍，结束游戏
        if (other.gameObject.transform.root.CompareTag("Player"))
        {
            manager.GameOver();
        }
    }
}
