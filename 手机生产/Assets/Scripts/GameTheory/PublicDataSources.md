# 公开可复现实验数据建议

## 推荐数据口径（论文友好）
- World Bank（公开API，无需登录）
	- 中国GDP增速：`NY.GDP.MKTP.KD.ZG`
	- 中国通胀率：`FP.CPI.TOTL.ZG`
	- 中国贷款利率：`FR.INR.LEND`
- FRED（公开CSV）
	- 综合工业价格指数：`PPIACO`
	- 油价（交付运输成本代理）：`DCOILWTICO`
	- 房价指数（土地竞标成本代理）：`CSUSHPINSA`
	- 零售需求指数（订单需求代理）：`RSAFS`
	- 半导体PPI（芯片代理）：`PCU33441334413`

## 公开URL示例
- `https://api.worldbank.org/v2/country/CHN/indicator/NY.GDP.MKTP.KD.ZG?format=json&per_page=80`
- `https://api.worldbank.org/v2/country/CHN/indicator/FP.CPI.TOTL.ZG?format=json&per_page=80`
- `https://api.worldbank.org/v2/country/CHN/indicator/FR.INR.LEND?format=json&per_page=80`
- `https://fred.stlouisfed.org/graph/fredgraph.csv?id=PPIACO`
- `https://fred.stlouisfed.org/graph/fredgraph.csv?id=DCOILWTICO`
- `https://fred.stlouisfed.org/graph/fredgraph.csv?id=CSUSHPINSA`
- `https://fred.stlouisfed.org/graph/fredgraph.csv?id=RSAFS`
- `https://fred.stlouisfed.org/graph/fredgraph.csv?id=PCU33441334413`

## 数据处理原则
1. 统一时间粒度为月度（YYYY-MM）。
2. 所有价格统一到同一币种与税口径。
3. 对缺失值做线性插值，并在论文中标注处理方法。
4. 保留原始来源链接与抓取日期，保证可复核。
5. 若单个指标临时不可用，采用公开替代指标并在论文中注明代理关系。

## 映射到系统字段
- 经济预测：
	- `gdpGrowth` <- World Bank GDP增速
	- `inflation` <- World Bank CPI通胀
- 订单竞标：
	- `demandIndex` <- FRED零售需求指数
- 贷款：
	- `lendingRate` <- World Bank贷款利率
- 芯片采购：
	- `chip` <- FRED半导体PPI
- 屏幕/内存/摄像头/电池/外壳采购：
	- `display/memory/camera/battery/housing` <- 以FRED综合工业价格为基准生成部件代理价格（可替换为行业公开细分指数）
- 土地竞标：
	- `landIndex` <- FRED房价指数
- 组装生产：
	- 由 `chip/display/memory/camera/battery/housing` 多部件最小值约束
- 交付：
	- `deliveryIndex` <- FRED油价（运输成本代理）
- 销售：
	- `phone` <- 由公开工业价格指数与芯片指数组合构成销售参考价

## 合规与工程落地
- 论文实验优先使用公开数据 + 处理脚本。
- 企业演示可选接入授权API，作为扩展模块，不替代可复现实验基线。
- 项目已提供 `PublicDataWebImporter`，可一键将上述公开URL数据拉取并生成训练CSV。
