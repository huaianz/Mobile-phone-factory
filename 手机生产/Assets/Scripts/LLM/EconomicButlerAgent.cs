using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class EconomicButlerAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Button analyzeButton;
    [SerializeField] private Text responseText;
    [SerializeField] private Text requestPreviewText;
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果的字符显示速度

    [Header("Prompt Settings")]
    [SerializeField] private string butlerSystemHint =
        "你是手机工厂经营模拟器中的经济管家。你需要基于给定经营数据，输出精简、可执行的经营建议，不需要输出md标记语言，直接输出文本内容。";

    private bool isRequesting;

    private void Awake()
    {
        if (analyzeButton != null)
        {
            analyzeButton.onClick.AddListener(OnAnalyzeButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (analyzeButton != null)
        {
            analyzeButton.onClick.RemoveListener(OnAnalyzeButtonClicked);
        }
    }

    private void OnAnalyzeButtonClicked()
    {
        if (isRequesting)
        {
            return;
        }

        if (dialogueManager == null)
        {
            HandleAIResponse("未配置 DialogueManager，无法调用大模型。", false);
            return;
        }

        string summaryData = BuildCompactBusinessSummary();
        string prompt = BuildButlerPrompt(summaryData);

        if (requestPreviewText != null)
        {
            requestPreviewText.text = "已发送给大模型的数据：\n" + summaryData;
        }

        isRequesting = true;
        if (analyzeButton != null)
        {
            analyzeButton.interactable = false;
        }

        HandleAIResponse("Mari分析中，请稍候...", true);
        dialogueManager.SendDialogueRequest(prompt, OnButlerResponse);
    }

    private void OnButlerResponse(string response, bool success)
    {
        isRequesting = false;
        if (analyzeButton != null)
        {
            analyzeButton.interactable = true;
        }

        if (!success || string.IsNullOrEmpty(response))
        {
            HandleAIResponse("Mari暂时未返回有效建议，请稍后重试。", false);
            return;
        }

        HandleAIResponse(response, true);
    }
        /// <summary>
    /// 处理AI的响应
    /// </summary>
    /// <param name="response">AI的回复内容</param>
    /// <param name="success">请求是否成功</param>
    private void HandleAIResponse(string response, bool success)
    {
        StartCoroutine(TypewriterEffect(success ? "Mari :" + response : "Mari:（通讯中断）"));//启动打字机效果协程
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
            responseText.text = currentText;//更新显示文本
            yield return new WaitForSeconds(typingSpeed);//等待一定时间
        }
    }

    private string BuildButlerPrompt(string compactData)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(butlerSystemHint);
        sb.AppendLine("请基于以下数据，输出：");
        sb.AppendLine("1) 经营现状总结（3-5句）");
        sb.AppendLine("2) 关键风险（最多3条）");
        sb.AppendLine("3) 下一步行动建议（按优先级给3条，每条写明理由）");
        sb.AppendLine("4) 如果适用，请给出一个保守策略和一个进取策略");
        sb.AppendLine("数据如下：");
        sb.Append(compactData);
        return sb.ToString();
    }

    private string BuildCompactBusinessSummary()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            return "GameManager 不存在，无法读取经营数据。";
        }

        Inventory bag = GetCurrentInventory(gm.Inventory);
        formance yearlyFormance = GetPerformanceForYear(gm.formance, gm.currentYear);
        Delivery yearlyDelivery = GetDeliveryForYear(gm.delivery, gm.currentYear);
        subsidy yearlySubsidy = GetSubsidyForYear(gm.subsidy, gm.currentYear);
        GovernmentPolicy yearlyPolicy = GetPolicyForYear(gm.governmentPolicy, gm.currentYear);

        int chipCount = SumPartCount(gm.ChipData != null ? gm.ChipData.chipList : null, c => c.number);
        int displayCount = SumPartCount(gm.Displayscreen != null ? gm.Displayscreen.disList : null, d => d.number);
        int memoryCount = SumPartCount(gm.Memory != null ? gm.Memory.MeList : null, m => m.number);
        int cameraCount = SumPartCount(gm.Camera1 != null ? gm.Camera1.caList : null, c => c.number);
        int batteryCount = SumPartCount(gm.Battery != null ? gm.Battery.baList : null, b => b.number);
        int housingCount = SumPartCount(gm.Housing != null ? gm.Housing.hoList : null, h => h.number);

        int phoneStock = 0;
        if (bag != null && bag.phones != null)
        {
            for (int i = 0; i < bag.phones.Count; i++)
            {
                phoneStock += bag.phones[i] != null ? bag.phones[i].number : 0;
            }
        }

        float estimatedPartStockValue = CalculateEstimatedPartStockValue(gm, bag);

        int activeOrders = gm.orderData != null && gm.orderData.activeOrders != null ? gm.orderData.activeOrders.Count : 0;
        int wonOrders = gm.orderData != null && gm.orderData.wonOrders != null ? gm.orderData.wonOrders.Count : 0;
        int completedOrders = gm.orderData != null && gm.orderData.completedOrders != null ? gm.orderData.completedOrders.Count : 0;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 工厂经营简报（精简） ===");
        sb.AppendLine("年份: " + gm.currentYear);

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

    private Inventory GetCurrentInventory(Inventory_SO inventorySo)
    {
        if (inventorySo == null || inventorySo.Bag == null || inventorySo.Bag.Count == 0)
        {
            return null;
        }

        return inventorySo.Bag[0];
    }

    private formance GetPerformanceForYear(Formance_SO formanceSo, int year)
    {
        if (formanceSo == null)
        {
            return null;
        }

        formance byYear = formanceSo.GetformanceForYear(year);
        return byYear ?? formanceSo.GetCurrentFormance();
    }

    private Delivery GetDeliveryForYear(Delivery_SO deliverySo, int year)
    {
        if (deliverySo == null)
        {
            return null;
        }

        Delivery byYear = deliverySo.GetDeliveryForYear(year);
        return byYear ?? deliverySo.GetCurrentDelivery();
    }

    private subsidy GetSubsidyForYear(subsidy_SO subsidySo, int year)
    {
        if (subsidySo == null)
        {
            return null;
        }

        subsidy byYear = subsidySo.GetSubsidyForYear(year);
        return byYear ?? subsidySo.GetCurrentSubsidy();
    }

    private GovernmentPolicy GetPolicyForYear(GovernmentPolicy_SO policySo, int year)
    {
        if (policySo == null)
        {
            return null;
        }

        GovernmentPolicy byYear = policySo.GetPolicyForYear(year);
        return byYear ?? policySo.GetCurrentPolicy();
    }

    private int SumPartCount<T>(List<T> list, System.Func<T, int> selector)
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

    private float CalculateEstimatedPartStockValue(GameManager gm, Inventory bag)
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

    private float SumValue<T>(List<T> list, System.Func<T, int> countSelector, System.Func<T, float> priceSelector)
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
