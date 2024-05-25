using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //在检查器中可见
    public Text scoreLabel;
    public Text timeLabel;
    //在检查器中不可见
    private float time;
    private int score;
    private bool gameOver;

    public Animator scoreEffect;
    public Animator UIAnimator;
    public Animator gameOverAnimator;

    public Car car;

    //设置结束动画文字输入
    public Text gameoverScoreLabel;
    public Text gameoverBestLabel;

    //定义游戏结束声音
    public AudioSource gameOverAudio;

    // Start is called before the first frame update
    void Start()
    {
        //显示初始分数为0
        UpdateScore(0);
    }
    
    // Update is called once per frame
    void Update()
    {
        //显示当前时间
        UpdateTimer();

        //游戏结束后按回车键或鼠标左键重启游戏
        if (gameOver && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            UIAnimator.SetTrigger("Start");
            StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
        }
    }

    //设计游戏计时
    void UpdateTimer()
    {
        //增加时间
        time += Time.deltaTime;
        int timer = (int)time;
        //获取分钟和秒
        int seconds = timer % 60;
        int minutes = timer / 60;
        //将它们放入正确的0字符串中
        string secondRounded = ((seconds < 10) ? "0" : "") + seconds;
        string minuteRounded = ((minutes < 10) ? "0" : "") + minutes;
        //显示时间
        timeLabel.text = minuteRounded + ":" + secondRounded;
    }
    //游戏开始的计数加分
    public void UpdateScore(int points)
    {
        // 添加分数
        score += points;
        //更新分数文本
        scoreLabel.text = score.ToString();
        //显示蓝色小动画
        if (points != 0)
            scoreEffect.SetTrigger("Score");
    }
    //游戏结束
    public void GameOver()
    {
        //游戏不能多次结束，如果游戏已经结束，我们需要返回
        if (gameOver)
        {
            return;
        }

        SetScore();
        //显示游戏动画并播放音频
        gameOverAnimator.SetTrigger("game over");
        //结束声音，游戏结束
        gameOver = true;
        gameOverAudio.Play();
        
        //停止世界的移动或旋转
        foreach (BasicMoveMent basicMovement in GameObject.FindObjectsOfType<BasicMoveMent>())
        {
            basicMovement.moveSpeed = 0;
            basicMovement.rotateSpeed = 0;
        }
        //小车摧毁
        car.FallApart();
    }

    //设置游戏结束时分数显示，和游戏积分显示 
    public void SetScore()
    {
        //如果我们的分数高于之前的最高分，则更新highscore
        if (score > PlayerPrefs.GetInt("best"))
        {
            PlayerPrefs.SetInt("best", score);
        }
        //显示分数和最高分
        gameoverScoreLabel.text = "score ：" + score;
        gameoverBestLabel.text = "best :" + PlayerPrefs.GetInt("best");
    }
    //等待不到一秒，加载给定的场景
    IEnumerator LoadScene(string scene)
    {
        yield return new WaitForSeconds(0.6f);

        SceneManager.LoadScene(scene);
    }
}
