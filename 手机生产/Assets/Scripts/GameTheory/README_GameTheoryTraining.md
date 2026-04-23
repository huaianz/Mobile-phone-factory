# 博弈对抗最小实现说明（全流程覆盖 + API预算受控）

## 1. 目标
- 在 Unity 中实现双智能体（激进/稳健）对抗训练。
- 不超过 GitHub API 大模型调用限制。
- 使用公开、可复现的数据驱动价格序列。
- 覆盖业务全流程：经济预测、订单竞标、贷款、芯片采购、各部件采购、土地竞标、组装生产、交付。

## 2. 脚本组成与技术用途

### 2.1 `GameTheoryTypes.cs`
- `MarketPricePoint`：统一价格数据结构（芯片/屏幕/内存/摄像头/电池/外壳/手机）。
- `PublicMarketCsvLoader`：把公开 CSV 数据加载为可复现实验输入。
- `PriceSimulationEngine.NextGbmPrice(...)`：GBM 随机过程生成扰动价格，模拟真实市场波动。
- `AgentPolicyState`：智能体策略参数（溢价、排产、安全库存）+ 回报驱动更新（轻量学习）。

### 2.2 `ApiBudgetGuard.cs`
- 预算守卫：
  - `maxCallsPerTrainingRun`：整轮训练总调用上限。
  - `maxCallsPerEpisode`：单局调用上限。
- 每次尝试调用 LLM 前必须 `TryConsume(...)`，超限即跳过。

### 2.3 `SelfPlayTrainingController.cs`
- 自博弈训练控制器：
  - 每轮按完整流程执行：
    1) 经济预测：GDP/通胀/需求指数调整需求
    2) 订单竞标：按溢价竞争订单份额
    3) 贷款：现金不足时按真实利率代理借款
    4) 芯片与各部件采购：按竞价分配供给
    5) 土地竞标：地价指数驱动土地成本
    6) 组装生产：受多部件瓶颈与土地产能约束
    7) 交付：按订单配额交付并计算违约惩罚
  - 计算收益：销售收入 - 采购成本 - 持有成本 - 缺货罚金 - 交付成本。
  - 用每轮收益更新策略参数，实现“学习”。
- 可选 `enableLlmReflection`：仅在指定间隔回合调用一次 LLM 给短建议，并受预算守卫限制。

### 2.4 `PublicDataWebImporter.cs`
- 从公开网络接口导入真实数据并生成训练 CSV（运行时写到 `Application.persistentDataPath`）：
  - World Bank: GDP增速、通胀、贷款利率（中国）
  - FRED: 综合工业价格、油价、房价指数、零售需求指数、半导体PPI

### 2.5 `TrainingReportExporter.cs`
- 自动导出论文可用表格：
  - Markdown表：`training_report_table.md`
  - CSV表：`training_report_table.csv`

## 3. 公开可复现数据规范
- 示例文件：`Assets/Resources/PublicData/market_prices_sample.csv`
- 列头要求：
  `date,gdpGrowth,inflation,lendingRate,demandIndex,landIndex,deliveryIndex,chip,display,memory,camera,battery,housing,phone`
- 若使用在线导入器，可直接从公开接口拉取并自动生成上述列结构。

## 4. 为什么满足论文“博弈对抗+决策优化”
- 双主体竞争：激进 vs 稳健策略。
- 资源稀缺博弈：通过竞价权重进行冲突仲裁。
- 动态市场：公开历史 + GBM 扰动。
- 学习闭环：回合收益反馈到策略参数更新。
- 可解释性：每回合日志可追踪收益与参数变化。
- 流程完整性：覆盖从宏观预测到生产交付的企业决策闭环。

## 5. 最终参数如何使用（写回AI对手配置）
训练日志会输出参数，例如：
`Radical(premium=0.492, prod=1.000, safety=0.392)`
`Conservative(premium=0.147, prod=0.779, safety=1.607)`

- `premium`（竞价溢价系数）
  - 决定订单竞标与采购竞标激进程度。
  - 写回后可直接影响中标率和采购份额。
- `prod`（排产系数）
  - 决定可用产能利用比例。
  - 写回后影响产量与交付率。
- `safety`（库存安全系数）
  - 决定库存持有策略。
  - 值越低偏激进，越高偏稳健。

写回方式：
1. 在 `AgentPolicyState` 初始值中替换默认参数。
2. 或在启动训练/仿真前加载外部配置JSON后覆盖。

## 6. 两个 Agent 如何博弈
1. 同状态输入：两者读取同一轮市场状态与订单需求。
2. 独立决策：各自输出 `premium/prod/safety` 驱动的动作。
3. 共享环境结算：按竞价规则分配订单与资源，执行生产与交付。
4. 收益反馈：根据利润与惩罚更新各自参数。
5. 重复多轮：形成对抗式策略演化。

## 7. 如何运行
1. 在场景中创建空物体，挂载 `SelfPlayTrainingController`。
2. 将 `market_prices_sample.csv` 作为 `TextAsset` 绑定到 `publicMarketCsv`。
3. 可选：创建另一个空物体挂 `PublicDataWebImporter`，先点 `ImportFromWeb()` 生成真实CSV。
4. 如需 LLM 反思：绑定 `DialogueManager`，并开启 `enableLlmReflection`。
5. 设置预算：例如总上限 20、单局上限 2。
6. 调用 `StartTraining()` 开始训练（可在按钮事件中绑定）。
7. 训练结束后在 `Application.persistentDataPath/GameTheoryReports/` 查看论文表格。

## 8. API限额建议
- 默认先关闭 `enableLlmReflection`，纯本地训练零API消耗。
- 开启后建议：
  - `llmReflectionInterval >= 5`
  - `maxCallsPerTrainingRun <= 20`
  - `maxCallsPerEpisode <= 2`
- 同时降低 `DialogueManager.maxTokens`（例如 120-220）以控制 token 费用。
