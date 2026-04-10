using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class EconomicButlerAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Button analyzeButton;

    [Header("Section Texts (Optional)")]
    [SerializeField] private Text summaryText;
    [SerializeField] private Text riskText;
    [SerializeField] private Text actionText;

    [Header("Pagination UI")]
    [SerializeField] private Text pageTitleText;
    [SerializeField] private Text pageContentText;
    [SerializeField] private Text pageIndicatorText;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;

    [Header("Compatibility")]
    [SerializeField] private Text responseText;
    [SerializeField] private Text requestPreviewText;

    [Header("Typing Effect")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Prompt Settings")]
    [SerializeField] private string butlerSystemHint =
        "你是手机工厂经营模拟器中的经济管家。你需要基于给定经营数据，输出精简、可执行的经营建议，直接输出unity的text组件可以显示文本内容。";

    private bool isRequesting;
    private readonly List<PageData> pages = new List<PageData>();
    private int currentPage;
    private Coroutine typingCoroutine;

    private class PageData
    {
        public string Title;
        public string Content;
    }

    private void Awake()
    {
        if (analyzeButton != null)
        {
            analyzeButton.onClick.AddListener(OnAnalyzeButtonClicked);
        }

        if (prevPageButton != null)
        {
            prevPageButton.onClick.AddListener(ShowPrevPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(ShowNextPage);
        }
    }

    private void OnDestroy()
    {
        if (analyzeButton != null)
        {
            analyzeButton.onClick.RemoveListener(OnAnalyzeButtonClicked);
        }

        if (prevPageButton != null)
        {
            prevPageButton.onClick.RemoveListener(ShowPrevPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveListener(ShowNextPage);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // 按钮点击事件
    /// <summary>
    /// 分析按钮点击事件，构建经营数据并发起AI请求。
    /// </summary>
    private void OnAnalyzeButtonClicked()
    {
        if (isRequesting)
        {
            return;
        }

        if (dialogueManager == null)
        {
            ShowStatus("未配置 DialogueManager，无法调用大模型。");
            return;
        }
        // 构建经营数据和本地预警
        string summaryData = BusinessContextService.BuildCompactBusinessSummary();
        List<string> localAlerts = BusinessContextService.BuildLocalAlerts();
        string prompt = BuildButlerPrompt(summaryData, localAlerts);

        // 显示请求预览
        if (requestPreviewText != null)
        {
            requestPreviewText.text = "已发送给大模型的数据：\n" + summaryData + "\n\n本地预警：\n- " + string.Join("\n- ", localAlerts);
        }

        isRequesting = true;
        if (analyzeButton != null)
        {
            analyzeButton.interactable = false;
        }

        ShowStatus("Mari分析中，请稍候...");
        dialogueManager.SendDialogueRequest(prompt, OnButlerResponse);
    }

    /// <summary>
    /// AI回复回调，处理返回内容并更新UI。
    /// </summary>
    /// <param name="response">AI回复内容</param>
    /// <param name="success">请求是否成功</param>
    private void OnButlerResponse(string response, bool success)
    {
        isRequesting = false;
        if (analyzeButton != null)
        {
            analyzeButton.interactable = true;
        }

        if (!success || string.IsNullOrEmpty(response))
        {
            ShowStatus("Mari暂时未返回有效建议，请稍后重试。");
            return;
        }

        BindResponseToUi(response);
    }

    /// <summary>
    /// 解析AI回复内容并绑定到UI各分区。
    /// </summary>
    /// <param name="rawResponse">AI原始回复</param>
    private void BindResponseToUi(string rawResponse)
    {
        // 按标签分割AI回复
        string summary = ExtractSection(rawResponse, "[SUMMARY]", new[] { "[RISKS]", "[ACTIONS]", "[STRATEGIES]" });
        string risks = ExtractSection(rawResponse, "[RISKS]", new[] { "[ACTIONS]", "[STRATEGIES]" });
        string actions = ExtractSection(rawResponse, "[ACTIONS]", new[] { "[STRATEGIES]" });
        string strategies = ExtractSection(rawResponse, "[STRATEGIES]", Array.Empty<string>());

        summary = BeautifyModelText(summary);
        risks = BeautifyModelText(risks);
        actions = BeautifyModelText(actions);
        strategies = BeautifyModelText(strategies);

        // 若无结构化标签，则整体作为summary
        if (string.IsNullOrEmpty(summary) && string.IsNullOrEmpty(risks) && string.IsNullOrEmpty(actions))
        {
            summary = BeautifyModelText(rawResponse);
        }

        if (summaryText != null) summaryText.text = summary;
        if (riskText != null) riskText.text = risks;
        if (actionText != null) actionText.text = actions;

        BuildPages(summary, risks, actions, strategies, rawResponse);
        RenderCurrentPage();
    }

    /// <summary>
    /// 构建分页数据，按内容分区。
    /// </summary>
    /// <param name="summary">经营现状</param>
    /// <param name="risks">风险</param>
    /// <param name="actions">行动建议</param>
    /// <param name="strategies">策略组合</param>
    /// <param name="raw">原始回复</param>
    private void BuildPages(string summary, string risks, string actions, string strategies, string raw)
    {
        pages.Clear();

        TryAddPage("经营现状", summary);
        TryAddPage("关键风险", risks);
        TryAddPage("行动建议", actions);
        TryAddPage("策略组合", strategies);

        if (pages.Count == 0)
        {
            TryAddPage("经济管家回复", raw);
        }

        currentPage = 0;
    }

    /// <summary>
    /// 添加分页，内容为空不添加
    /// </summary>
    /// <param name="title">分页标题</param>
    /// <param name="content">分页内容</param>
    private void TryAddPage(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        pages.Add(new PageData
        {
            Title = title,
            Content = content.Trim()
        });
    }

    private void ShowPrevPage()
    {
        if (pages.Count == 0)
        {
            return;
        }

        currentPage = Mathf.Max(0, currentPage - 1);
        RenderCurrentPage();
    }

    private void ShowNextPage()
    {
        if (pages.Count == 0)
        {
            return;
        }

        currentPage = Mathf.Min(pages.Count - 1, currentPage + 1);
        RenderCurrentPage();
    }

    /// <summary>
    /// 渲染当前页内容到UI。
    /// </summary>
    private void RenderCurrentPage()
    {
        if (pages.Count == 0)
        {
            ShowStatus("暂无可展示内容。");
            return;
        }

        PageData page = pages[currentPage];

        if (pageTitleText != null)
        {
            pageTitleText.text = page.Title;
        }

        Text contentText = pageContentText != null ? pageContentText : responseText;
        // 翻页时直接显示全部内容，不用逐渐显示
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (contentText != null)
        {
            contentText.text = page.Content;
        }

        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = (currentPage + 1) + "/" + pages.Count;
        }

        if (prevPageButton != null)
        {
            prevPageButton.interactable = currentPage > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPage < pages.Count - 1;
        }
    }

    /// <summary>
    /// 显示状态文本
    /// </summary>
    /// <param name="text">要显示的文本</param>
    private void ShowStatus(string text)
    {
        Text contentText = pageContentText != null ? pageContentText : responseText;
        if (contentText == null)
        {
            return;
        }
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        contentText.text = text;
    }

    /// <summary>
    /// 显示文本
    /// </summary>
    /// <param name="text">要显示的文本</param>
    private void SetContentText(string text)
    {
        Text contentText = pageContentText != null ? pageContentText : responseText;
        if (contentText == null)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = StartCoroutine(TypewriterEffect(contentText, text));
    }

    /// <summary>
    /// 逐字显示文本内容
    /// </summary>
    /// <param name="targetText">目标UI文本</param>
    /// <param name="text">要显示的完整文本</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator TypewriterEffect(Text targetText, string text)
    {
        targetText.text = string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        string currentText = string.Empty;
        for (int i = 0; i < text.Length; i++)
        {
            currentText += text[i];
            targetText.text = currentText;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    /// <summary>
    /// 构建经济专家AI提示词，包含经营数据和本地预警。
    /// </summary>
    /// <param name="compactData">经营数据摘要</param>
    /// <param name="localAlerts">本地预警列表</param>
    /// <returns>完整AI提示词</returns>
    private string BuildButlerPrompt(string compactData, List<string> localAlerts)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(butlerSystemHint);
        sb.AppendLine("请严格按以下标签格式输出，不要遗漏标签：");
        sb.AppendLine("[SUMMARY]");
        sb.AppendLine("3-5句经营现状总结");
        sb.AppendLine("[RISKS]");
        sb.AppendLine("最多3条关键风险");
        sb.AppendLine("[ACTIONS]");
        sb.AppendLine("按优先级给3条下一步行动建议，每条包含理由");
        sb.AppendLine("[STRATEGIES]");
        sb.AppendLine("给出保守策略和进取策略");
        sb.AppendLine("请使用简体中文文本。不要输出Markdown标记语言。\n");

        if (localAlerts != null && localAlerts.Count > 0)
        {
            sb.AppendLine("本地规则预警（请结合判断）：");
            for (int i = 0; i < localAlerts.Count; i++)
            {
                sb.AppendLine("- " + localAlerts[i]);
            }
            sb.AppendLine();
        }

        sb.AppendLine("数据如下：");
        sb.Append(compactData);
        return sb.ToString();
    }

    /// <summary>
    /// 提取AI回复中的指定标签内容
    /// </summary>
    /// <param name="text">AI回复原文</param>
    /// <param name="startTag">起始标签</param>
    /// <param name="endTags">结束标签数组</param>
    /// <returns>标签内的内容</returns>
    private string ExtractSection(string text, string startTag, string[] endTags)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(startTag))
        {
            return string.Empty;
        }

        int startIndex = text.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
        {
            return string.Empty;
        }

        startIndex += startTag.Length;
        int endIndex = text.Length;

        if (endTags != null)
        {
            for (int i = 0; i < endTags.Length; i++)
            {
                if (string.IsNullOrEmpty(endTags[i]))
                {
                    continue;
                }

                int candidate = text.IndexOf(endTags[i], startIndex, StringComparison.OrdinalIgnoreCase);
                if (candidate >= 0)
                {
                    endIndex = Mathf.Min(endIndex, candidate);
                }
            }
        }

        if (endIndex <= startIndex)
        {
            return string.Empty;
        }

        return text.Substring(startIndex, endIndex - startIndex).Trim();
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

        text = text.Replace("**", string.Empty)
                   .Replace("__", string.Empty)
                   .Replace("`", string.Empty);

        text = Regex.Replace(text, @"^\s{0,3}#{1,6}\s*", string.Empty, RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*>\s?", string.Empty, RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*[-*]\s+", "• ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*(\d+)\.\s+", "$1) ", RegexOptions.Multiline);
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
}

public static class BusinessContextService
{
    public static string BuildCompactBusinessSummary()
    {
        GameManager gameMgr = GameManager.Instance;
        if (gameMgr == null)
        {
            return "GameManager 不存在，无法读取经营数据。";
        }

        Inventory bag = GetCurrentInventory(gameMgr.Inventory);
        formance yearlyFormance = GetPerformanceForYear(gameMgr.formance, gameMgr.currentYear);
        Delivery yearlyDelivery = GetDeliveryForYear(gameMgr.delivery, gameMgr.currentYear);
        subsidy yearlySubsidy = GetSubsidyForYear(gameMgr.subsidy, gameMgr.currentYear);
        GovernmentPolicy yearlyPolicy = GetPolicyForYear(gameMgr.governmentPolicy, gameMgr.currentYear);

        int chipCount = SumPartCount(gameMgr.ChipData != null ? gameMgr.ChipData.chipList : null, c => c.number);
        int displayCount = SumPartCount(gameMgr.Displayscreen != null ? gameMgr.Displayscreen.disList : null, d => d.number);
        int memoryCount = SumPartCount(gameMgr.Memory != null ? gameMgr.Memory.MeList : null, m => m.number);
        int cameraCount = SumPartCount(gameMgr.Camera1 != null ? gameMgr.Camera1.caList : null, c => c.number);
        int batteryCount = SumPartCount(gameMgr.Battery != null ? gameMgr.Battery.baList : null, b => b.number);
        int housingCount = SumPartCount(gameMgr.Housing != null ? gameMgr.Housing.hoList : null, h => h.number);

        int phoneStock = 0;
        if (bag != null && bag.phones != null)
        {
            for (int i = 0; i < bag.phones.Count; i++)
            {
                phoneStock += bag.phones[i] != null ? bag.phones[i].number : 0;
            }
        }

        float estimatedPartStockValue = CalculateEstimatedPartStockValue(gameMgr, bag);

        int activeOrders = gameMgr.orderData != null && gameMgr.orderData.activeOrders != null ? gameMgr.orderData.activeOrders.Count : 0;
        int wonOrders = gameMgr.orderData != null && gameMgr.orderData.wonOrders != null ? gameMgr.orderData.wonOrders.Count : 0;
        int completedOrders = gameMgr.orderData != null && gameMgr.orderData.completedOrders != null ? gameMgr.orderData.completedOrders.Count : 0;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 工厂经营简报（精简） ===");
        sb.AppendLine("年份: " + gameMgr.currentYear);

        sb.AppendLine("-- 部件库存总量 --");
        sb.AppendLine("芯片: " + chipCount);
        sb.AppendLine("显示屏: " + displayCount);
        sb.AppendLine("内存: " + memoryCount);
        sb.AppendLine("摄像头: " + cameraCount);
        sb.AppendLine("电池: " + batteryCount);
        sb.AppendLine("外壳: " + housingCount);
        sb.AppendLine("手机库存: " + phoneStock);
        sb.AppendLine("部件库存估值(按现有单价): " + estimatedPartStockValue.ToString("F2"));

        sb.AppendLine("-- 订单与交付 --");
        sb.AppendLine("活跃订单数: " + activeOrders);
        sb.AppendLine("中标订单数: " + wonOrders);
        sb.AppendLine("完成订单数: " + completedOrders);
        if (yearlyDelivery != null)
        {
            sb.AppendLine("当年交付收入: " + yearlyDelivery.TotalRevenue.ToString("F2"));
            sb.AppendLine("当年违约金: " + yearlyDelivery.liquidatedDamages.ToString("F2"));
        }

        sb.AppendLine("-- 资金与财务 --");
        sb.AppendLine("现金余额: " + (bag != null ? bag.money.ToString("F2") : "0.00"));
        sb.AppendLine("累计贷款总额: " + (bag != null ? bag.TotalLoanedAmount.ToString("F2") : "0.00"));
        sb.AppendLine("累计支出: " + (bag != null ? bag.expense.ToString("F2") : "0.00"));
        sb.AppendLine("原料成本: " + (bag != null ? bag.rawMaterial.ToString("F2") : "0.00"));
        sb.AppendLine("人工成本: " + (bag != null ? bag.manual.ToString("F2") : "0.00"));

        if (yearlyFormance != null)
        {
            sb.AppendLine("销售收入(不含税): " + yearlyFormance.salesRevenue.ToString("F2"));
            sb.AppendLine("毛利: " + yearlyFormance.grossProfit.ToString("F2"));
            sb.AppendLine("营业利润: " + yearlyFormance.operatingProfit.ToString("F2"));
            sb.AppendLine("净利润: " + yearlyFormance.netProfit.ToString("F2"));
            sb.AppendLine("所得税: " + yearlyFormance.incomeTax.ToString("F2"));
        }

        if (yearlySubsidy != null)
        {
            sb.AppendLine("当年补贴资金: " + yearlySubsidy.subsidyFunds.ToString("F2"));
        }

        if (yearlyPolicy != null)
        {
            sb.AppendLine("-- 政策环境(当年) --");
            sb.AppendLine("最低利率(%): " + yearlyPolicy.PraInterestRate.ToString("F2"));
            sb.AppendLine("增值税率(%): " + yearlyPolicy.vatRate.ToString("F2"));
            sb.AppendLine("所得税率(%): " + yearlyPolicy.incomeTaxRate.ToString("F2"));
            sb.AppendLine("补贴比率(%): " + yearlyPolicy.SubsidyRate.ToString("F2"));
            sb.AppendLine("可贷资金: " + yearlyPolicy.LoanableFunds.ToString("F2"));
            sb.AppendLine("订单数量上限: " + yearlyPolicy.BidNumber);
        }

        return sb.ToString();
    }

    /// <summary>
    /// 构建本地预警列表，基于当前经营数据进行规则判断
    /// </summary>
    public static List<string> BuildLocalAlerts()
    {
        List<string> alerts = new List<string>();
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            alerts.Add("无法读取经营状态（GameManager 为空）");
            return alerts;
        }

        Inventory bag = GetCurrentInventory(gm.Inventory);
        if (bag == null)
        {
            alerts.Add("库存与财务主体数据为空");
            return alerts;
        }

        if (bag.money < 0)
        {
            alerts.Add("现金为负，存在流动性风险。");
        }
        else if (bag.money < 2000f)
        {
            alerts.Add("现金偏低，采购与竞标需谨慎控制节奏。");
        }

        if (bag.TotalLoanedAmount > 0f && bag.money > 0f && bag.TotalLoanedAmount > bag.money * 2f)
        {
            alerts.Add("贷款规模相对现金偏高，需关注利息压力和偿债安排。");
        }

        formance yearlyFormance = GetPerformanceForYear(gm.formance, gm.currentYear);
        if (yearlyFormance != null)
        {
            if (yearlyFormance.netProfit < 0f)
            {
                alerts.Add("当年净利润为负，建议优先修复毛利和费用结构。");
            }

            if (yearlyFormance.operatingProfit < 0f)
            {
                alerts.Add("营业利润为负，当前经营模型存在效率问题。");
            }
        }

        int activeOrders = gm.orderData != null && gm.orderData.activeOrders != null ? gm.orderData.activeOrders.Count : 0;
        int phoneStock = 0;
        if (bag.phones != null)
        {
            for (int i = 0; i < bag.phones.Count; i++)
            {
                phoneStock += bag.phones[i] != null ? bag.phones[i].number : 0;
            }
        }

        if (activeOrders > 0 && phoneStock <= 0)
        {
            alerts.Add("存在活跃订单但成品库存为0，交付压力较高。");
        }

        int[] partCounts = new int[]
        {
            SumPartCount(gm.ChipData != null ? gm.ChipData.chipList : null, c => c.number),
            SumPartCount(gm.Displayscreen != null ? gm.Displayscreen.disList : null, d => d.number),
            SumPartCount(gm.Memory != null ? gm.Memory.MeList : null, m => m.number),
            SumPartCount(gm.Camera1 != null ? gm.Camera1.caList : null, c => c.number),
            SumPartCount(gm.Battery != null ? gm.Battery.baList : null, b => b.number),
            SumPartCount(gm.Housing != null ? gm.Housing.hoList : null, h => h.number)
        };

        int maxPart = 0;
        int minPart = int.MaxValue;
        for (int i = 0; i < partCounts.Length; i++)
        {
            if (partCounts[i] > maxPart)
            {
                maxPart = partCounts[i];
            }

            if (partCounts[i] < minPart)
            {
                minPart = partCounts[i];
            }
        }

        if (maxPart > 0 && minPart < maxPart * 0.3f)
        {
            alerts.Add("部件库存结构不均衡，可能卡住组装产能。");
        }

        if (alerts.Count == 0)
        {
            alerts.Add("未发现明显硬性风险，建议围绕利润率和订单质量做优化。");
        }

        return alerts;
    }

    /// <summary>
    /// 构建当前活跃模块提示，基于UI显示状态判断当前玩家
    /// </summary>
    public static string BuildActiveModuleHint()
    {
        UIManager ui = UIManager.Instance;
        if (ui == null)
        {
            return "模块未知";
        }

        if (ui.BidSimulation != null && ui.BidSimulation.activeInHierarchy) return "订单竞标";
        if (ui.BidResult != null && ui.BidResult.activeInHierarchy) return "竞标结果";
        if (ui.BidStatus != null && ui.BidStatus.activeInHierarchy) return "竞标状态";

        if (ui.LandBidPanel != null && ui.LandBidPanel.activeInHierarchy) return "土地竞标";
        if (ui.resultPanel != null && ui.resultPanel.activeInHierarchy) return "贷款结果";
        if (ui.LoanBidPanel != null && ui.LoanBidPanel.activeInHierarchy) return "贷款竞标";

        if (ui.ProductContent != null && ui.ProductContent.activeInHierarchy) return "组装生产";
        if (ui.PhoneInformation != null && ui.PhoneInformation.activeInHierarchy) return "手机信息";

        return "综合经营";
    }

    /// <summary>
    /// 获取当前库存数据，优先使用Inventory_SO中的Bag数据
    /// </summary>
    /// <param name="inventorySo"></param>
    /// <returns></returns>
    private static Inventory GetCurrentInventory(Inventory_SO inventorySo)
    {
        if (inventorySo == null || inventorySo.Bag == null || inventorySo.Bag.Count == 0)
        {
            return null;
        }

        return inventorySo.Bag[0];
    }

    private static formance GetPerformanceForYear(Formance_SO formanceSo, int year)
    {
        if (formanceSo == null)
        {
            return null;
        }

        formance byYear = formanceSo.GetformanceForYear(year);
        return byYear ?? formanceSo.GetCurrentFormance();
    }

    private static Delivery GetDeliveryForYear(Delivery_SO deliverySo, int year)
    {
        if (deliverySo == null)
        {
            return null;
        }

        Delivery byYear = deliverySo.GetDeliveryForYear(year);
        return byYear ?? deliverySo.GetCurrentDelivery();
    }

    private static subsidy GetSubsidyForYear(subsidy_SO subsidySo, int year)
    {
        if (subsidySo == null)
        {
            return null;
        }

        subsidy byYear = subsidySo.GetSubsidyForYear(year);
        return byYear ?? subsidySo.GetCurrentSubsidy();
    }

    private static GovernmentPolicy GetPolicyForYear(GovernmentPolicy_SO policySo, int year)
    {
        if (policySo == null)
        {
            return null;
        }

        GovernmentPolicy byYear = policySo.GetPolicyForYear(year);
        return byYear ?? policySo.GetCurrentPolicy();
    }

    private static int SumPartCount<T>(List<T> list, Func<T, int> selector)
    {
        if (list == null || selector == null)
        {
            return 0;
        }

        int sum = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
            {
                sum += selector(list[i]);
            }
        }

        return sum;
    }

    /// <summary>
    /// 计算当前部件库存的估值，基于现有单价进行加权求和
    /// </summary>
    /// <param name="gm"></param>
    /// <param name="bag"></param>
    /// <returns></returns>
    private static float CalculateEstimatedPartStockValue(GameManager gm, Inventory bag)
    {
        float total = 0f;

        if (bag != null)
        {
            total += SumValue(bag.Chip, item => item != null ? item.number : 0, item => item != null ? item.price_usd : 0f);
            total += SumValue(bag.Displayscreen, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
            total += SumValue(bag.Memory, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
            total += SumValue(bag.Camera, item => item != null ? item.number : 0, item => item != null ? item.unit_price : 0f);
            total += SumValue(bag.Battery, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
            total += SumValue(bag.Housing, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
        }
        else
        {
            total += SumValue(gm.ChipData != null ? gm.ChipData.chipList : null, item => item != null ? item.number : 0, item => item != null ? item.price_usd : 0f);
            total += SumValue(gm.Displayscreen != null ? gm.Displayscreen.disList : null, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
            total += SumValue(gm.Memory != null ? gm.Memory.MeList : null, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
            total += SumValue(gm.Camera1 != null ? gm.Camera1.caList : null, item => item != null ? item.number : 0, item => item != null ? item.unit_price : 0f);
            total += SumValue(gm.Battery != null ? gm.Battery.baList : null, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
            total += SumValue(gm.Housing != null ? gm.Housing.hoList : null, item => item != null ? item.number : 0, item => item != null ? item.price : 0f);
        }

        return total;
    }

    private static float SumValue<T>(List<T> list, Func<T, int> countSelector, Func<T, float> priceSelector)
    {
        if (list == null || countSelector == null || priceSelector == null)
        {
            return 0f;
        }

        float total = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            T item = list[i];
            if (item == null)
            {
                continue;
            }

            total += countSelector(item) * priceSelector(item);
        }

        return total;
    }
}
