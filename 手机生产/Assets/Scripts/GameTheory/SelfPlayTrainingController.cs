using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SelfPlayTrainingController : MonoBehaviour
{
    [Header("数据输入")]
    [SerializeField] private TextAsset publicMarketCsv;

    [Header("训练参数")]
    [SerializeField] private int seed = 20260421;
    [SerializeField] private int episodes = 20;
    [SerializeField] private int roundsPerEpisode = 12;
    [SerializeField] private float demandForecast = 600f;
    [SerializeField] private float orderQuantityPerRound = 500f;

    [Header("GBM参数")]
    [SerializeField] private bool useGbmPerturbation = true;
    [SerializeField] private float gbmMu = 0.03f;
    [SerializeField] private float gbmSigma = 0.22f;
    [SerializeField] private float gbmDt = 1f / 12f;

    [Header("调用预算")]
    [SerializeField] private bool enableLlmReflection = false;
    [SerializeField] private int llmReflectionInterval = 5;
    [SerializeField] private ApiBudgetGuard apiBudget = new ApiBudgetGuard();
    [SerializeField] private DialogueManager dialogueManager;

    [Header("输出")]
    [SerializeField] private Text trainingLogText;
    [SerializeField] private bool exportPaperTable = true;

    private readonly StringBuilder trainingLog = new StringBuilder(4096);
    private List<MarketPricePoint> marketSeries = new List<MarketPricePoint>();
    private readonly List<EpisodeMetrics> reportRows = new List<EpisodeMetrics>();
    private System.Random rng;

    public void StartTraining()
    {
        StopAllCoroutines();
        StartCoroutine(RunTraining());
    }

    private IEnumerator RunTraining()
    {
        rng = new System.Random(seed);
        marketSeries = PublicMarketCsvLoader.Parse(publicMarketCsv);
        if (marketSeries.Count == 0)
        {
            AppendLog("训练终止：未加载到公开市场数据CSV。请先绑定 publicMarketCsv。");
            yield break;
        }

        apiBudget.BeginRun();
        reportRows.Clear();

        AgentPolicyState radical = new AgentPolicyState("Radical", true);
        AgentPolicyState conservative = new AgentPolicyState("Conservative", false);

        for (int ep = 1; ep <= episodes; ep++)
        {
            apiBudget.BeginEpisode();
            float radicalRewardAcc = 0f;
            float conservativeRewardAcc = 0f;
            float radicalDeliveredAcc = 0f;
            float conservativeDeliveredAcc = 0f;
            float radicalOrderAcc = 0f;
            float conservativeOrderAcc = 0f;
            float radicalLoanAcc = 0f;
            float conservativeLoanAcc = 0f;
            float radicalLandCostAcc = 0f;
            float conservativeLandCostAcc = 0f;

            float radicalCash = 500000f;
            float conservativeCash = 500000f;

            float radicalChip = 200f;
            float radicalDisplay = 200f;
            float radicalMemory = 200f;
            float radicalCamera = 200f;
            float radicalBattery = 200f;
            float radicalHousing = 200f;

            float conservativeChip = 200f;
            float conservativeDisplay = 200f;
            float conservativeMemory = 200f;
            float conservativeCamera = 200f;
            float conservativeBattery = 200f;
            float conservativeHousing = 200f;

            for (int round = 1; round <= roundsPerEpisode; round++)
            {
                MarketPricePoint basePoint = marketSeries[(ep * roundsPerEpisode + round - 1) % marketSeries.Count];
                float chipPrice = basePoint.chip;
                float displayPrice = basePoint.display;
                float memoryPrice = basePoint.memory;
                float cameraPrice = basePoint.camera;
                float batteryPrice = basePoint.battery;
                float housingPrice = basePoint.housing;
                float phonePrice = basePoint.phone;

                float macroDemand = demandForecast * Mathf.Clamp(1f + basePoint.gdpGrowth / 100f - basePoint.inflation / 250f, 0.6f, 1.6f);
                macroDemand *= Mathf.Clamp(basePoint.demandIndex / 100f, 0.7f, 1.5f);

                if (useGbmPerturbation)
                {
                    chipPrice = PriceSimulationEngine.NextGbmPrice(chipPrice, gbmMu, gbmSigma, gbmDt, rng);
                    displayPrice = PriceSimulationEngine.NextGbmPrice(displayPrice, gbmMu, gbmSigma, gbmDt, rng);
                    memoryPrice = PriceSimulationEngine.NextGbmPrice(memoryPrice, gbmMu, gbmSigma, gbmDt, rng);
                    cameraPrice = PriceSimulationEngine.NextGbmPrice(cameraPrice, gbmMu, gbmSigma, gbmDt, rng);
                    batteryPrice = PriceSimulationEngine.NextGbmPrice(batteryPrice, gbmMu, gbmSigma, gbmDt, rng);
                    housingPrice = PriceSimulationEngine.NextGbmPrice(housingPrice, gbmMu, gbmSigma, gbmDt, rng);
                    phonePrice = PriceSimulationEngine.NextGbmPrice(phonePrice, gbmMu * 0.6f, gbmSigma * 0.7f, gbmDt, rng);
                }

                AgentDecision rd = radical.Decide(macroDemand, radicalCash);
                AgentDecision cd = conservative.Decide(macroDemand, conservativeCash);

                // 1) 订单竞标：按溢价竞争订单配额
                float orderBidR = 1f + rd.bidPremiumRatio;
                float orderBidC = 1f + cd.bidPremiumRatio;
                float orderTotal = orderBidR + orderBidC;
                float orderR = orderQuantityPerRound * orderBidR / orderTotal;
                float orderC = orderQuantityPerRound - orderR;

                radicalOrderAcc += orderR;
                conservativeOrderAcc += orderC;

                // 2) 贷款：现金不足时按真实利率代理借款
                float loanRate = Mathf.Max(0.01f, basePoint.lendingRate / 100f);
                float radicalLoan = 0f;
                float conservativeLoan = 0f;
                if (radicalCash < 100000f)
                {
                    radicalLoan = 120000f;
                    radicalCash += radicalLoan;
                    radicalCash -= radicalLoan * loanRate / 12f;
                }
                if (conservativeCash < 100000f)
                {
                    conservativeLoan = 120000f;
                    conservativeCash += conservativeLoan;
                    conservativeCash -= conservativeLoan * loanRate / 12f;
                }
                radicalLoanAcc += radicalLoan;
                conservativeLoanAcc += conservativeLoan;

                // 3) 芯片+各部件采购竞价
                float supply = 1000f;
                float totalBidWeight = (1f + rd.bidPremiumRatio) + (1f + cd.bidPremiumRatio);
                float radicalAllocated = supply * ((1f + rd.bidPremiumRatio) / totalBidWeight);
                float conservativeAllocated = supply - radicalAllocated;

                radicalChip += radicalAllocated;
                radicalDisplay += radicalAllocated;
                radicalMemory += radicalAllocated;
                radicalCamera += radicalAllocated;
                radicalBattery += radicalAllocated;
                radicalHousing += radicalAllocated;

                conservativeChip += conservativeAllocated;
                conservativeDisplay += conservativeAllocated;
                conservativeMemory += conservativeAllocated;
                conservativeCamera += conservativeAllocated;
                conservativeBattery += conservativeAllocated;
                conservativeHousing += conservativeAllocated;

                // 4) 土地竞标：土地成本与地价指数绑定
                float radicalLandCost = basePoint.landIndex * (1f + rd.bidPremiumRatio) * 45f;
                float conservativeLandCost = basePoint.landIndex * (1f + cd.bidPremiumRatio) * 45f;
                radicalCash -= radicalLandCost;
                conservativeCash -= conservativeLandCost;
                radicalLandCostAcc += radicalLandCost;
                conservativeLandCostAcc += conservativeLandCost;

                // 5) 组装生产：受多部件最小值与土地产能限制
                float radicalBottleneck = Mathf.Min(radicalChip, Mathf.Min(radicalDisplay, Mathf.Min(radicalMemory, Mathf.Min(radicalCamera, Mathf.Min(radicalBattery, radicalHousing)))));
                float conservativeBottleneck = Mathf.Min(conservativeChip, Mathf.Min(conservativeDisplay, Mathf.Min(conservativeMemory, Mathf.Min(conservativeCamera, Mathf.Min(conservativeBattery, conservativeHousing)))));

                float radicalCapacityByLand = Mathf.Max(50f, 420f * rd.productionRatio);
                float conservativeCapacityByLand = Mathf.Max(50f, 420f * cd.productionRatio);

                float radicalProduction = Mathf.Min(radicalBottleneck, radicalCapacityByLand);
                float conservativeProduction = Mathf.Min(conservativeBottleneck, conservativeCapacityByLand);

                radicalChip -= radicalProduction;
                radicalDisplay -= radicalProduction;
                radicalMemory -= radicalProduction;
                radicalCamera -= radicalProduction;
                radicalBattery -= radicalProduction;
                radicalHousing -= radicalProduction;

                conservativeChip -= conservativeProduction;
                conservativeDisplay -= conservativeProduction;
                conservativeMemory -= conservativeProduction;
                conservativeCamera -= conservativeProduction;
                conservativeBattery -= conservativeProduction;
                conservativeHousing -= conservativeProduction;

                // 6) 交付：只能交付订单份额以内数量
                float radicalDelivered = Mathf.Min(radicalProduction, orderR);
                float conservativeDelivered = Mathf.Min(conservativeProduction, orderC);
                radicalDeliveredAcc += radicalDelivered;
                conservativeDeliveredAcc += conservativeDelivered;

                float radicalRevenue = radicalDelivered * phonePrice;
                float conservativeRevenue = conservativeDelivered * phonePrice;

                float radicalMaterialCost = radicalAllocated * (
                    chipPrice + displayPrice + memoryPrice + cameraPrice + batteryPrice + housingPrice) * (1f + rd.bidPremiumRatio) * 0.16f;
                float conservativeMaterialCost = conservativeAllocated * (
                    chipPrice + displayPrice + memoryPrice + cameraPrice + batteryPrice + housingPrice) * (1f + cd.bidPremiumRatio) * 0.16f;

                float deliveryCostR = radicalDelivered * basePoint.deliveryIndex * 0.08f;
                float deliveryCostC = conservativeDelivered * basePoint.deliveryIndex * 0.08f;

                float radicalHoldingCost = (radicalChip + radicalDisplay + radicalMemory + radicalCamera + radicalBattery + radicalHousing) * 0.25f * rd.inventorySafetyFactor;
                float conservativeHoldingCost = (conservativeChip + conservativeDisplay + conservativeMemory + conservativeCamera + conservativeBattery + conservativeHousing) * 0.25f * cd.inventorySafetyFactor;

                float radicalShortagePenalty = Mathf.Max(0f, orderR - radicalDelivered) * 12f;
                float conservativeShortagePenalty = Mathf.Max(0f, orderC - conservativeDelivered) * 12f;

                float radicalReward = radicalRevenue - radicalMaterialCost - radicalHoldingCost - radicalShortagePenalty - deliveryCostR;
                float conservativeReward = conservativeRevenue - conservativeMaterialCost - conservativeHoldingCost - conservativeShortagePenalty - deliveryCostC;

                radicalCash += radicalReward;
                conservativeCash += conservativeReward;

                radicalRewardAcc += radicalReward;
                conservativeRewardAcc += conservativeReward;

                radical.LearnFromReward(radicalReward);
                conservative.LearnFromReward(conservativeReward);

                if (enableLlmReflection && round % Mathf.Max(1, llmReflectionInterval) == 0)
                {
                    yield return TryLlmReflection(ep, round, radical, conservative, radicalReward, conservativeReward);
                }
            }

            AppendLog(string.Format(
                "[Episode {0}] Radical累计收益={1:F2}, Conservative累计收益={2:F2}, 调用计数={3}/{4}",
                ep,
                radicalRewardAcc,
                conservativeRewardAcc,
                apiBudget.callsThisRun,
                apiBudget.maxCallsPerTrainingRun));

            EpisodeMetrics row = new EpisodeMetrics();
            row.episode = ep;
            row.radicalProfit = radicalRewardAcc;
            row.conservativeProfit = conservativeRewardAcc;
            row.radicalDeliveryRate = radicalOrderAcc <= 0f ? 0f : radicalDeliveredAcc / radicalOrderAcc;
            row.conservativeDeliveryRate = conservativeOrderAcc <= 0f ? 0f : conservativeDeliveredAcc / conservativeOrderAcc;
            row.radicalAvgLoan = radicalLoanAcc / Mathf.Max(1, roundsPerEpisode);
            row.conservativeAvgLoan = conservativeLoanAcc / Mathf.Max(1, roundsPerEpisode);
            row.radicalLandCost = radicalLandCostAcc;
            row.conservativeLandCost = conservativeLandCostAcc;
            reportRows.Add(row);

            yield return null;
        }

        AppendLog("训练完成。你可以将最终策略参数写回AI对手配置，实现对抗式策略演化。\n" +
                  string.Format("最终参数 Radical(premium={0:F3}, prod={1:F3}, safety={2:F3}) | Conservative(premium={3:F3}, prod={4:F3}, safety={5:F3})",
                  radical.bidPremiumRatio,
                  radical.productionRatio,
                  radical.inventorySafetyFactor,
                  conservative.bidPremiumRatio,
                  conservative.productionRatio,
                  conservative.inventorySafetyFactor));

        if (exportPaperTable)
        {
            ExportPaperTables();
        }
    }

    private void ExportPaperTables()
    {
        string dir = Path.Combine(Application.dataPath, "Resources/Data/GameTheoryReports");
        
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string mdPath = Path.Combine(dir, "training_report_table.md");
        string csvPath = Path.Combine(dir, "training_report_table.csv");
        ExportAsMarkdown(reportRows, mdPath);
        ExportAsCsv(reportRows, csvPath);

        AppendLog("论文表格已导出: " + mdPath);
        AppendLog("论文CSV已导出: " + csvPath);
    }

    private static void ExportAsMarkdown(List<EpisodeMetrics> metrics, string filePath)
    {
        StringBuilder sb = new StringBuilder(2048);
        sb.AppendLine("# 博弈训练结果表");
        sb.AppendLine();
        sb.AppendLine("| Episode | Radical利润 | Conservative利润 | Radical交付率 | Conservative交付率 | Radical平均贷款 | Conservative平均贷款 | Radical土地成本 | Conservative土地成本 |");
        sb.AppendLine("|---:|---:|---:|---:|---:|---:|---:|---:|---:|");

        for (int i = 0; i < metrics.Count; i++)
        {
            EpisodeMetrics m = metrics[i];
            sb.AppendLine(string.Format(
                "| {0} | {1:F2} | {2:F2} | {3:P2} | {4:P2} | {5:F2} | {6:F2} | {7:F2} | {8:F2} |",
                m.episode,
                m.radicalProfit,
                m.conservativeProfit,
                m.radicalDeliveryRate,
                m.conservativeDeliveryRate,
                m.radicalAvgLoan,
                m.conservativeAvgLoan,
                m.radicalLandCost,
                m.conservativeLandCost));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static void ExportAsCsv(List<EpisodeMetrics> metrics, string filePath)
    {
        StringBuilder sb = new StringBuilder(2048);
        sb.AppendLine("Episode,RadicalProfit,ConservativeProfit,RadicalDeliveryRate,ConservativeDeliveryRate,RadicalAvgLoan,ConservativeAvgLoan,RadicalLandCost,ConservativeLandCost");

        for (int i = 0; i < metrics.Count; i++)
        {
            EpisodeMetrics m = metrics[i];
            sb.AppendLine(string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1:F4},{2:F4},{3:F6},{4:F6},{5:F4},{6:F4},{7:F4},{8:F4}",
                m.episode,
                m.radicalProfit,
                m.conservativeProfit,
                m.radicalDeliveryRate,
                m.conservativeDeliveryRate,
                m.radicalAvgLoan,
                m.conservativeAvgLoan,
                m.radicalLandCost,
                m.conservativeLandCost));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private IEnumerator TryLlmReflection(
        int episode,
        int round,
        AgentPolicyState radical,
        AgentPolicyState conservative,
        float radicalReward,
        float conservativeReward)
    {
        if (dialogueManager == null)
        {
            yield break;
        }

        if (!apiBudget.TryConsume("Reflection E" + episode + " R" + round))
        {
            yield break;
        }

        string prompt = string.Format(
            "你是博弈教练。请基于以下对抗回合结果给一句不超过30字的参数调整建议。" +
            "\nEpisode={0}, Round={1}" +
            "\nRadical: premium={2:F3}, prod={3:F3}, safety={4:F3}, reward={5:F2}" +
            "\nConservative: premium={6:F3}, prod={7:F3}, safety={8:F3}, reward={9:F2}",
            episode,
            round,
            radical.bidPremiumRatio,
            radical.productionRatio,
            radical.inventorySafetyFactor,
            radicalReward,
            conservative.bidPremiumRatio,
            conservative.productionRatio,
            conservative.inventorySafetyFactor,
            conservativeReward);

        bool completed = false;
        string response = null;
        bool success = false;

        dialogueManager.SendDialogueRequest(prompt, (text, ok) =>
        {
            completed = true;
            response = text;
            success = ok;
        });

        float timeout = 10f;
        float elapsed = 0f;
        while (!completed && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (success && !string.IsNullOrEmpty(response))
        {
            AppendLog("[LLM Reflection] " + response.Replace("\n", " "));
        }
        else
        {
            AppendLog("[LLM Reflection] 调用失败或超时");
        }
    }

    private void AppendLog(string line)
    {
        trainingLog.AppendLine(line);
        Debug.Log("[SelfPlayTraining] " + line);
        if (trainingLogText != null)
        {
            trainingLogText.text = trainingLog.ToString();
        }
    }
}
