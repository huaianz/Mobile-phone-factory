using System.Collections;
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

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果的字符显示速度


    void Start()
    {
        characterName = dialogueManager.npcCharacter.name;//角色姓名赋值
        //输入框提交后执行的回调函数
        inputField.onSubmit.AddListener((text) =>
        {
            dialogueManager.SendDialogueRequest(text, HandleAIResponse);//发送对话请求到DeepSeek
            SetResponseText(characterName + ":（正在思考...）");
        });
    }
    /// <summary>
    /// 处理AI的响应
    /// </summary>
    /// <param name="response">AI的回复内容</param>
    /// <param name="success">请求是否成功</param>
    private void HandleAIResponse(string response, bool success)
    {
        StartCoroutine(TypewriterEffect(success ? characterName +":" + response : characterName +":（通讯中断）"));//启动打字机效果协程
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
}