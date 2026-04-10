using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    //引用和配置
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;//对话管理器
    [SerializeField] private InputField inputField;//玩家问题输入框
    [SerializeField] private Text dialogueText;//角色回复的文本内容
    private string characterName;
    private UnityEngine.Events.UnityAction<string> submitHandler;
    private bool isRequesting;
    private string pendingQuestion;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果的字符显示速度
    [SerializeField] private bool enableEconomicAdvisorMode = true;
    [SerializeField] private bool includeBusinessStateForAllQuestions = true;
    [SerializeField] private int maxMemoryFacts = 6;

    private readonly Dictionary<string, string> memoryFacts = new Dictionary<string, string>();
    private readonly Queue<string> memoryFactOrder = new Queue<string>();

    /// <summary>
    /// 初始化NPC交互组件，绑定输入框提交事件。
    /// </summary>
    void Start()
    {
        characterName = dialogueManager != null ? dialogueManager.npcCharacter.name : "助手";//角色姓名赋值
        //输入框提交后执行的回调函数
        submitHandler = (text) =>
        {
            OnUserSubmit(text);
        };

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(submitHandler);
        }
    }

    /// <summary>
    /// 处理玩家输入的文本，构建上下文并发起AI请求。
    /// </summary>
    /// <param name="userText">玩家输入内容</param>
    private void OnUserSubmit(string userText)
    {
        if (string.IsNullOrWhiteSpace(userText) || isRequesting)
        {
            return;
        }

        if (dialogueManager == null)
        {
            SetResponseText(characterName + ":（未连接对话服务）");
            return;
        }

        pendingQuestion = userText.Trim();
        string outgoingPrompt = enableEconomicAdvisorMode
            ? BuildContextualPrompt(pendingQuestion)
            : pendingQuestion;

        isRequesting = true;
        SetResponseText(characterName + ":正在认真思考...");
        dialogueManager.SendDialogueRequest(outgoingPrompt, HandleAIResponse);//发送对话请求到DeepSeek
    }

    /// <summary>
    /// 处理AI的响应
    /// </summary>
    /// <param name="response">AI的回复内容</param>
    /// <param name="success">请求是否成功</param>
    private void HandleAIResponse(string response, bool success)
    {
        isRequesting = false;

        if (success && !string.IsNullOrEmpty(response) && enableEconomicAdvisorMode)
        {
            UpdateMemoryPoolFromUserQuestion(pendingQuestion);
        }

        string pretty = success ? BeautifyModelText(response) : "（通讯中断）";
        StartCoroutine(TypewriterEffect(success ? characterName +":" + pretty : characterName +":（通讯中断）"));//启动打字机效果协程
    }
    /// <summary>
    /// 打字机效果协程
    /// </summary>
    /// <param name="text">角色的回复内容</param>
    /// <returns></returns>
    private IEnumerator TypewriterEffect(string text)
    {
        string currentText = "";//当前显示的文本
        foreach (char c in text)//遍历每个字符
        {
            currentText += c;//添加字符到当前文本
            dialogueText.text = currentText;//更新显示文本
            yield return new WaitForSeconds(typingSpeed);//等待一定时间
        }
    }

    /// <summary>
    /// 直接设置对话文本内容。
    /// </summary>
    /// <param name="text">要显示的文本</param>
    private void SetResponseText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
        else
        {
            Debug.LogWarning("EconomicButlerAgent: responseText 未绑定。\n" + text);
        }
    }

    /// <summary>
    /// 将模型回复格式化为更适合Unity Text显示的样式。
    /// </summary>
    /// <param name="raw">模型原始文本</param>
    /// <returns>格式化后的文本</returns>
    private string BeautifyModelText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string text = raw.Replace("\r\n", "\n").Replace("\r", "\n");

        // 去掉常见Markdown符号，保留可读结构。
        text = text.Replace("**", string.Empty)
                   .Replace("__", string.Empty)
                   .Replace("`", string.Empty);

        text = Regex.Replace(text, @"^\s{0,3}#{1,6}\s*", string.Empty, RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*>\s?", string.Empty, RegexOptions.Multiline);
        // text = Regex.Replace(text, @"^\s*[-*]\s+", "•", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*[-*]\s+", string.Empty, RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*(\d+)\.\s+", "$1) ", RegexOptions.Multiline);

        // 去除“角色名:”前缀，避免与本地角色名前缀重复显示。
        text = Regex.Replace(text, @"^\s*(Mari|玛丽|伊落玛丽)\s*[：:]\s*", string.Empty, RegexOptions.IgnoreCase);

        string[] lines = text.Split('\n');
        StringBuilder sb = new StringBuilder(text.Length + 32);
        int blankCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0)
            {
                blankCount++;
                if (blankCount <= 1)
                {
                    sb.AppendLine();
                }
                continue;
            }

            blankCount = 0;
            sb.AppendLine(line);
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// 构建带有经营上下文和记忆信息的AI提示词。
    /// </summary>
    /// <param name="userQuestion">玩家当前问题</param>
    /// <returns>完整的AI提示词</returns>
    private string BuildContextualPrompt(string userQuestion)
    {
        bool isMemoryQuestion = IsMemoryIntentQuestion(userQuestion);
        StringBuilder sb = new StringBuilder();
        // sb.AppendLine("你是手机工厂经营模拟器的经济管家聊天助手。");
        // sb.AppendLine("请结合当前经营状态回答用户问题，给出明确、可执行建议。避免空泛回复。\n");
        sb.AppendLine("请结合用户问题和上述信息，给出针对性的分析和建议。如果用户问题与上述信息无关，则优先满足用户要求，避免无关回复。不需要输出md格式，直接给出文本回复。");

        if (includeBusinessStateForAllQuestions || !isMemoryQuestion)
        {
            sb.AppendLine("当前模块: " + BusinessContextService.BuildActiveModuleHint());

            List<string> alerts = BusinessContextService.BuildLocalAlerts();
            sb.AppendLine("本地规则预警:");
            for (int i = 0; i < alerts.Count; i++)
            {
                sb.AppendLine("- " + alerts[i]);
            }

            sb.AppendLine();
            sb.AppendLine(BusinessContextService.BuildCompactBusinessSummary());
            sb.AppendLine();
        }

        if (isMemoryQuestion)
        {
            AppendMemoryFactsToPrompt(sb);
            sb.AppendLine();
        }

        sb.AppendLine("当前问题:");
        sb.AppendLine(userQuestion);
        sb.AppendLine();

        if (isMemoryQuestion)
        {
            sb.AppendLine("如果记忆中有明确事实就直接回答；如果没有，就明确说目前没有记住该信息。");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 判断玩家问题是否为记忆相关意图。
    /// </summary>
    /// <param name="userQuestion">玩家输入内容</param>
    /// <returns>是记忆意图返回true，否则false</returns>
    private bool IsMemoryIntentQuestion(string userQuestion)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return false;
        }

        string q = userQuestion;
        return q.Contains("记住") ||
               q.Contains("你记得") ||
               q.Contains("还记得") ||
               q.Contains("进度");
    }

    /// <summary>
    /// 根据玩家输入更新记忆池
    /// </summary>
    /// <param name="userQuestion">玩家输入内容</param>
    private void UpdateMemoryPoolFromUserQuestion(string userQuestion)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return;
        }

        string nameFromPattern = ExtractPreferredName(userQuestion);
        if (!string.IsNullOrEmpty(nameFromPattern))
        {
            UpsertMemoryFact("preferred_name", nameFromPattern);
            return;
        }

        if (userQuestion.Contains("记住") && userQuestion.Contains("我"))
        {
            string compact = userQuestion.Trim();
            if (compact.Length > 60)
            {
                compact = compact.Substring(0, 60);
            }
            UpsertMemoryFact("last_user_identity_hint", compact);
        }
    }

    /// <summary>
    /// 从玩家输入中提取偏好称呼（如“我叫XX”）。
    /// </summary>
    /// <param name="userQuestion">玩家输入内容</param>
    /// <returns>提取到的名字，未提取到返回null</returns>
    private string ExtractPreferredName(string userQuestion)
    {
        Match m1 = Regex.Match(userQuestion, "我叫\\s*([^，。！？,.!?\\s]{1,12})");
        if (m1.Success)
        {
            return m1.Groups[1].Value;
        }

        Match m2 = Regex.Match(userQuestion, "叫我\\s*([^，。！？,.!?\\s]{1,12})");
        if (m2.Success)
        {
            return m2.Groups[1].Value;
        }

        return null;
    }

    /// <summary>
    /// 向记忆池插入或更新一条事实，超出容量自动淘汰最早项。
    /// </summary>
    /// <param name="key">事实键</param>
    /// <param name="value">事实值</param>
    private void UpsertMemoryFact(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!memoryFacts.ContainsKey(key))
        {
            memoryFactOrder.Enqueue(key);
        }

        memoryFacts[key] = value;

        while (memoryFactOrder.Count > Mathf.Max(1, maxMemoryFacts))
        {
            string oldestKey = memoryFactOrder.Dequeue();
            if (memoryFacts.ContainsKey(oldestKey))
            {
                memoryFacts.Remove(oldestKey);
            }
        }
    }

    /// <summary>
    /// 将记忆池内容追加到AI提示词中。
    /// </summary>
    /// <param name="sb">StringBuilder对象</param>
    private void AppendMemoryFactsToPrompt(StringBuilder sb)
    {
        if (memoryFacts.Count == 0)
        {
            sb.AppendLine("记忆池: 暂无记录");
            return;
        }

        sb.AppendLine("记忆池(仅关键信息):");
        foreach (KeyValuePair<string, string> pair in memoryFacts)
        {
            sb.AppendLine("- " + pair.Key + ": " + pair.Value);
        }
    }

    /// <summary>
    /// 组件销毁时解绑输入框事件
    /// </summary>
    private void OnDestroy()
    {
        if (inputField != null && submitHandler != null)
        {
            inputField.onSubmit.RemoveListener(submitHandler);
        }
    }
}