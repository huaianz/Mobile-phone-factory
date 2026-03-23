using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : Singleton<CountdownTimer>
{
    [Header("倒计时设置")]
    public float countdownTime = 60f;
    public Text countdownText;
    private bool isCounting = false;
    private Coroutine timerCoroutine;

    // 添加回调函数支持
    private System.Action onCountdownComplete;

    /// <summary>
    /// 开始倒计时（原有方法，用于按钮点击）
    /// </summary>
    public void StartCountdown()
    {
        // 如果已经在倒计时，先停止
        if (isCounting && timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        // 标记为开始倒计时
        isCounting = true;

        // 启动倒计时协程
        timerCoroutine = StartCoroutine(CountdownRoutine());
    }

    /// <summary>
    /// 开始倒计时并等待完成（新增方法，用于协程等待）
    /// </summary>
    /// <returns>协程，可以在外部用yield return等待</returns>
    public IEnumerator StartCountdownAndWait()
    {
        // 如果已经在倒计时，先停止
        if (isCounting && timerCoroutine != null)  // 添加null检查
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null; 
        }

        // 标记为开始倒计时
        isCounting = true;

        // 启动倒计时并等待完成
        timerCoroutine = StartCoroutine(CountdownRoutine());  // 记录协程引用
        yield return timerCoroutine; 
    }

    /// <summary>
    /// 倒计时协程
    /// </summary>
    IEnumerator CountdownRoutine()
    {
        // 重置当前时间
        float currentTime = countdownTime;

        // 倒计时循环：从countdownTime倒数到0
        while (currentTime > 0)
        {
            // 更新UI显示
            UpdateTimeDisplay(currentTime);

            // 等待一帧
            yield return null;

            // 减去一帧的时间
            currentTime -= Time.deltaTime;
        }

        // 显示00:00
        UpdateTimeDisplay(0);

        // 等待1秒（保持显示00:00一段时间）
        yield return new WaitForSeconds(1f);

        // 标记为结束
        isCounting = false;
        timerCoroutine = null;  // 协程结束，清空引用

        // 通知GameManager天数减1
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubtractDays();
        }
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    void UpdateTimeDisplay(float time)
    {
        // 确保countdownText不为空
        if (countdownText == null) return;

        // 计算分钟和秒
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // 格式化为"分:秒"显示
        countdownText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// 检查是否正在倒计时
    /// </summary>
    public bool IsCounting()
    {
        return isCounting;
    }

    /// <summary>
    /// 停止倒计时
    /// </summary>
    public void StopCountdown()
    {
        if (isCounting && timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            isCounting = false;
            timerCoroutine = null;  // 清空引用
        }
    }

    /// <summary>
    /// 重置计时器时间
    /// </summary>
    public void ResetTimerToZero()
    {
        // 如果正在倒计时，停止协程
        if (isCounting && timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            isCounting = false;
            timerCoroutine = null;
        }
        else
        {
            isCounting = false;
        }

        // 强制显示00:00
        UpdateTimeDisplay(0);
    }
}