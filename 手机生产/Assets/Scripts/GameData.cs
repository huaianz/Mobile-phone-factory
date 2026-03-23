using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

[System.Serializable]
public class research
{
    public string Project;
    public string Performance;
    public float funds;
    public int Time;
    public float Suggestion;
}

[System.Serializable]
public class Event
{
    public string Title;
    public string Description;
    public string Effect;
}


[Serializable]
public class Chip
{
    public int ID;
    public string chip_model;
    public string manufacturer;
    public string category;
    public string release_date;
    public string process_tech;
    public string transistor_count;
    public float price_usd;
    public string adopted_models;
    public int number;
}

[Serializable]
public class Displayscreen
{
    public int ID;
    public string display_model;
    public string manufacturer;
    public string resolution;
    public string refresh;
    public float price;
    public string adopted_models;
    public int number;
}

[Serializable]
public class Memory
{
    public int ID;
    public string memory_model;
    public string manufacturer;
    public string type;
    public string capacity;
    public float price;
    public string adopted_models;
    public int number;
}

[Serializable]
public class Camera1
{
    public int ID;
    public string camera_model;
    public string manufacturer;
    public string type;
    public string main_specs;
    public float unit_price;
    public string adopted_models;
    public int number;
}

[Serializable]
public class Battery
{
    public int ID;
    public string battery_model;
    public string manufacturer;
    public string capacity;
    public float price;
    public string adopted_models;
    public int number;
}

[Serializable]
public class Housing
{
    public int ID;
    public string part_model;
    public string manufacturer;
    public string material_type;
    public float price;
    public string adopted_models;
    public int number;
}

[Serializable]
public class Mobilephone
{
    public int ID;
    public string phone_model;
    public int Chip;
    public int Displayscreen;
    public int Memory;
    public List<int> Camera1;
    public int Battery;
    public int Housing;
}

[Serializable]
public class phone
{
    public int ID;
    public string Name;
    public int number;
}

[System.Serializable]
public class Inventory
{
    public int ID;
    public float money;
    public List<phone> phones;
    public List<Chip> Chip;
    public List<Displayscreen> Displayscreen;
    public List<Memory> Memory;
    public List<Camera1> Camera;
    public List<Battery> Battery;
    public List<Housing> Housing;
    public List<Order> Orders;

    //关于贷款部分
    public List<Loan> Loans;
    public float TotalLoanedAmount;

    //关于土地部分
    public List<Land> Lands;

    //专门用一个值来计算扣除费用
    public float expense;
    //销售的原料费用
    public float rawMaterial;
    //人工费
    public float manual;

}

[System.Serializable]
public class Order
{
    public int orderID;
    public string customerName;
    public int phoneModelID;
    public int quantity;
    public float maxPrice;
    public float currentBidPrice;
    public  int daysRemaining;
    public bool isActive;
    public bool isWon;
    public float deliveryDeadline;
}

//政府政策
[System.Serializable]
public class GovernmentPolicy
{
    public int year;

    // 手机产业政策
    public float maxLandSupply;           // 最大可供土地量（亩）
    public float PraLandSupply;           //实际土地
    public float minInterestRate;         // 最低利率（%）
    public float PraInterestRate;         // 实际最低利率（%）
    public float Moneyupply;              //货币供给
    public float vatRate;                 // 增值税率（%）
    public float incomeTaxRate;           // 所得税率（%）
    public float FiscalStimulusRevenue;   // 上届政府可用于财政刺激的收入
    public float Subsidy;                 // 上届政府发放补贴
    public float FiscalStimulusFunds;     //上届政府结余财政刺激资金（万元）
    public float SubsidyRate;             //政府按厂商实际销售收入补贴比率（%）
    public int BidNumber;                 //订单数量

    // 手机价格调控
    public float maxPhonePrice;           // 手机最高售价（万元）
    public float minPhonePrice;           // 手机最低售价（万元）

    // 原材料进口调控（改为与手机生产相关的）
    public float maxChipPrice;      // 芯片最高报价（万元/万个）

    public float maxDisPrice;    // 屏幕最高报价（万元/万个）

    public float maxMePrice;    

    public float maxCaPrice;     

    public float maxBaPrice;   

    public float maxHoPrice;   

    public float LoanableFunds; //预计可贷资金(原本资金+新增货币供给)
}

//贷款
[System.Serializable]
public class Loan
{
    public int loanID;
    public string companyType;
    public float interestRate;//年利率
    public float amount;
    public float actualAmount;
    public float availableCreditLine;
    public float payInterest;
    public int year;//这里的年份指的是贷款年限
    public int Loanyear;//这个用来指定目前看到的贷款信息年份
    public string SubmissionTime;
}

//土地
[System.Serializable]
public class Land
{
    public int landID;
    public string CompanyName;
    public float TotalSupply;
    public float RentBidding;
    public float ApplyforLand;
    public float ActualAcquisition;
    public float RemainingLand;
    public bool IsAccept;
    public string Time;
    public int year;

}

//组装生产
[System.Serializable]
public class Product
{
    public int productID;
    public string CompanyName;
    public string CompanyType;
    public string ProductionLineName;
    public int YearOfCreation;
    public int YearOfSale;
    public string RefurbishedStatus;
    public int TotalNumber;
    public int year;
}

//订单交付
[System.Serializable]
public class Delivery
{
    public int deliveryID;
    public string CompanyName;
    public int inventory;
    public string MbphoneName;
    public float quotation;
    public int orderQuantity;
    public int SubmittedCount;
    public int UnpaidQuantity;
    public float TotalRevenue;
    public float liquidatedDamages;
    public int year;
}

//补贴
[System.Serializable]
public class subsidy
{
    public int subsidyID;
    public string CompanyName;
    public float SubsidyRate;
    public float income;
    public float subsidyFunds;
    public int year;
}

//损益表
[System.Serializable]
public class formance
{
    public string companyName;          // 公司名字

    // ==================== 增值税相关 ====================
    public float revenueWithTax;        // 含税收入
    public float vatRate;               // 增值税率（%）
    public float vatPayable;            // 减：增值税

    // ==================== 收入与成本 ====================
    public float salesRevenue;          // 一、销售收入（不含税）
    public float costOfGoodsSold;       // 减：已销售产品的原料与人工成本(不含税)
    public float landRent;              // 土地和厂房租赁费

    // ==================== 利润中间值 ====================
    public float grossProfit;           // 二、毛利

    // ==================== 费用 ====================
    public float advertisingExpense;    // 减：广告费
    public float renovationCost;        // 翻新改造投入
    public float managementExpense;     // 管理费用
    public float interestExpense;       // 利息

    // ==================== 营业利润及调整 ====================
    public float operatingProfit;       // 三、营业利润
    public float currentReward;         // 加：本期奖励
    public float penalty;               // 减：违规操作罚金

    // ==================== 利润与所得税 ====================
    public float totalProfit;           // 四、利润总额
    public float incomeTaxRate;         // 所得税率（%）
    public float incomeTax;             // 减：所得税
    public float netProfit;             // 五、净利润
    public float inputTax;              // 进项税
    public float outputTax;             // 销项税

    public int year;
}
