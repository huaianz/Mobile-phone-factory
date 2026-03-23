using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;
using static Unity.Burst.Intrinsics.X86.Sse4_2;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class UIManager : Singleton<UIManager>
{
    [Header("芯片界面的点击")]
    public GameObject chipcontent;
    public GameObject historychipcontent;
    public GameObject chipbid;
    public Button Chipinformation;
    public Button ChipHistory;
    public Button ChipBid;

    public GameObject chip;
    public GameObject historychip;


    [Header("显示屏界面的点击")]
    public GameObject Displayscreen;
    public GameObject historydiscontent;
    public GameObject Displaybid;
    public Button Displayinformation;
    public Button DisHistory;
    public Button DisBid;

    public GameObject display;
    public GameObject historydis;

    [Header("内存界面的点击")]
    public GameObject Memory;
    public GameObject historymecontent;
    public GameObject Mebid;
    public Button Memoryinformation;
    public Button MeHistory;
    public Button MeBid;

    public GameObject memory;
    public GameObject historyme;

    [Header("摄像头界面的点击")]
    public GameObject Camera;
    public GameObject historycacontent;
    public GameObject Cabid;
    public Button Camerainformation;
    public Button CaHistory;
    public Button CaBid;

    public GameObject camera1;
    public GameObject historyca;

    [Header("电池界面的点击")]
    public GameObject Battery;
    public GameObject historybacontent;
    public GameObject Babid;
    public Button Batteryinformation;
    public Button BaHistory;
    public Button BaBid;

    public GameObject battery;
    public GameObject historyba;

    [Header("材料界面的点击")]
    public GameObject Housing;
    public GameObject historyhocontent;
    public GameObject Hobid;
    public Button Housinginformation;
    public Button HoHistory;
    public Button HoBid;

    public GameObject housing;
    public GameObject historyho;

    [Header("订单竞标界面的点击")]
    public GameObject BidStatus;
    public GameObject BidResult;
    public GameObject BidSimulation;

    public Button Status;
    public Button Result;
    public Button Simulation;

    public Button StartBid;
    public Button SubmitBid;

    public GameObject BidContent;

    [Header("金钱UI")]
    public Text Money;

    [Header("手机产业政府政策")]
    public GameObject policyPanel;
    public Text policyYearDisplay;
    //外围经济数据
    public Text LoanableFunds;

    public Text maxPhonePrice;
    public Text minPhonePrice;

    public Text maxChipPrice;

    public Text maxDisPrice;

    public Text maxMePrice;

    public Text maxCaPrice;

    public Text maxBaPrice;

    public Text maxHoPrice;

    //政府政策
    public Text maxLandSupply;
    public InputField PraLandSupply;
    public Text minInterestRate;
    public InputField PraInterestRate;
    public InputField Moneyupply;
    public InputField vatRate;
    public InputField incomeTaxRate;
    public Text FiscalStimulusRevenue;
    public Text Subsidy;
    public Text FiscalStimulusFunds;
    public InputField SubsidyRate;
    public InputField BidNumber;

    //按钮
    public Button submitPolicyButton;
    public Button prevYearButton;
    public Button nextYearButton;
    public Button startMeasuresButton;

    [Header("贷款")]
    public InputField loanAmountInput;
    public Text loanRateText;
    public Text actualReceivedText;
    public Text remainingLoanableText;
    //是否参与
    public Button Participation;
    public Button NoParticipation;

    //竞标结果
    public Text loanyear;
    //贷款界面切换点击
    public GameObject ParticpatePanel;
    public GameObject resultPanel;
    public GameObject LoanBidPanel;
    public Button LoanParticpate;
    public Button LoanResult;
    public Button LoanBid;
    //贷款竞标
    public InputField loanAmount;
    public Button applyLoanButton;
    public Button LoanStartButton;
    public Button LoanEndButton;
    public Button LoanDelayButton;

    //还缺个年份
    public Button LoannextYear;
    public Button LoanpreYear;
    public Text LoanYear;
    //贷款后要换算玩家的总资产


    [Header("土地")]
    public Button LandParticipation;
    public Button LandNoParticipation;
    public Button LandStartButton;
    public Button LandEndButton;
    public Button LandDelayButton;
    public Button LandSubmit;
    public InputField Landmu;
    //租金价格上限
    public InputField LandRent;
    public Button LandRentButton;

    public Text TotalSupply;
    public Text ActualAcquisition;
    public Text RemainingLand;

    //土地界面切换点击
    public GameObject LandParticpatePanel;
    public GameObject LandResultPanel;
    public GameObject LandBidPanel;
    public Button LandParticpate;
    public Button LandResult;
    public Button LandBid;

    //还缺个年份
    public Button LandnextYear;
    public Button LandpreYear;
    public Text LandYear;
    //结算的时候要把缴付的钱算上，这个放在竞拍结束的时候，等到时候和政府一起写

    [Header("组装生产")]
    public Text ProductYear;
    public Button ProductnextYear;
    public Button ProductpreYear;
    public Button StartProduct;
    public Button EndProduct;

    public InputField MbPhoneID;
    public InputField MbQuantity;

    public Button ProductLine;
    public Button ProductInformation;
    public GameObject ProductContent;
    public GameObject PhoneInformation;

    [Header("订单交付")]
    public Button Delivery;

    private void Start()
    {
        //刚开始要禁止掉的按钮
        applyLoanButton.interactable = false;
        LandSubmit.interactable = false;
        LandStartButton.interactable = false;//为了让玩家先输入土地租金再进行竞拍
        LandRentButton.interactable = false;//先判断是否参与再输入土地租金，防止报错

        //切换按钮的事件注册
        Chipinformation.onClick.AddListener(() => CloseAndOpen(chipcontent, historychipcontent,chipbid));
        ChipHistory.onClick.AddListener(()=>CloseAndOpen(historychipcontent, chipcontent,chipbid));
        ChipBid.onClick.AddListener(() => CloseAndOpen(chipbid, chipcontent,historychipcontent));

        Displayinformation.onClick.AddListener(() => CloseAndOpen(Displayscreen, historydiscontent,Displaybid));
        DisHistory.onClick.AddListener(() => CloseAndOpen(historydiscontent, Displayscreen,Displaybid));
        DisBid.onClick.AddListener(() => CloseAndOpen(Displaybid, Displayscreen, historydiscontent));

        Memoryinformation.onClick.AddListener(() => CloseAndOpen(Memory, historymecontent,Mebid));
        MeHistory.onClick.AddListener(() => CloseAndOpen(historymecontent, Memory,Mebid));
        MeBid.onClick.AddListener(() => CloseAndOpen(Mebid,Memory, historymecontent));

        Camerainformation.onClick.AddListener(() => CloseAndOpen(Camera, historycacontent,Cabid));
        CaHistory.onClick.AddListener(() => CloseAndOpen(historycacontent, Camera,Cabid));
        CaBid.onClick.AddListener(() => CloseAndOpen(Cabid,Camera, historycacontent));

        Batteryinformation.onClick.AddListener(() => CloseAndOpen(Battery, historybacontent,Babid));
        BaHistory.onClick.AddListener(() => CloseAndOpen(historybacontent, Battery,Babid));
        BaBid.onClick.AddListener(() => CloseAndOpen(Babid,Battery, historybacontent));

        Housinginformation.onClick.AddListener(() => CloseAndOpen(Housing, historyhocontent,Hobid));
        HoHistory.onClick.AddListener(() => CloseAndOpen(historyhocontent, Housing,Hobid));
        HoBid.onClick.AddListener(() => CloseAndOpen(Hobid,Housing, historyhocontent));

        Status.onClick.AddListener(() => CloseAndOpen(BidStatus,BidResult,BidSimulation));
        Result.onClick.AddListener(() => CloseAndOpen(BidResult,BidStatus,BidSimulation));
        Simulation.onClick.AddListener(() => CloseAndOpen(BidSimulation,BidResult,BidStatus));

        StartBid.onClick.AddListener(() => Unique());

        LoanParticpate.onClick.AddListener(() =>CloseAndOpen(ParticpatePanel, resultPanel, LoanBidPanel));
        LoanResult.onClick.AddListener(() => CloseAndOpen(resultPanel, ParticpatePanel, LoanBidPanel));
        LoanBid.onClick.AddListener(() => CloseAndOpen(LoanBidPanel, ParticpatePanel, resultPanel));

        LandParticpate.onClick.AddListener(() => CloseAndOpen(LandParticpatePanel,LandResultPanel,LandBidPanel));
        LandResult.onClick.AddListener(()=>CloseAndOpen(LandResultPanel,LandParticpatePanel,LandBidPanel));
        LandBid.onClick.AddListener(()=>CloseAndOpen(LandBidPanel,LandParticpatePanel,LandResultPanel));

        ProductLine.onClick.AddListener(() => CloseAndOpen(ProductContent, PhoneInformation));
        ProductInformation.onClick.AddListener(() => CloseAndOpen(PhoneInformation, ProductContent));

        //政府按钮
        if (submitPolicyButton != null)
            submitPolicyButton.onClick.AddListener(() => GameManager.Instance.SubmitPolicy());
        if (prevYearButton != null)
            prevYearButton.onClick.AddListener(() => GameManager.Instance.PrevYear());
        if (nextYearButton != null)
            nextYearButton.onClick.AddListener(() => GameManager.Instance.NextYear());
        if (startMeasuresButton != null)
            startMeasuresButton.onClick.AddListener(() => GameManager.Instance.StartMeasures());

        //贷款按钮
        if (applyLoanButton != null)
            applyLoanButton.onClick.AddListener(OnApplyLoanClick);
        if (Participation != null)
            Participation.onClick.AddListener(Participate);
        if(NoParticipation != null)
            NoParticipation.onClick.AddListener(NoParticipate);
        if (LoanStartButton != null)
            LoanStartButton.onClick.AddListener(StartLoan);
        if (LoanEndButton != null)
            LoanEndButton.onClick.AddListener(EndLoan);
        if (LoanDelayButton != null)
            LoanDelayButton.onClick.AddListener(DelayLoan);
        //上一年，下一年
        if (LoannextYear != null)
            LoannextYear.onClick.AddListener(() => GameManager.Instance.LoanNextYear());
        if (LoanpreYear!=null)
            LoanpreYear.onClick.AddListener(()=>GameManager.Instance.LoanPreYear());
        //土地按钮
        if (LandParticipation != null)
            LandParticipation.onClick.AddListener(LandParticipate);
        if (LandNoParticipation != null)
            LandNoParticipation.onClick.AddListener(LandNoParticipate);
        if (LandRentButton != null)
            LandRentButton.onClick.AddListener(LandRentSubmsit);
        if (LandSubmit != null)
            LandSubmit.onClick.AddListener(() => GameManager.Instance.LandBiddingSubmsit());
        if (LandStartButton != null)
            LandStartButton.onClick.AddListener(StartLand);
        if (LandEndButton != null)
            LandEndButton.onClick.AddListener(EndLand);
        if (LandDelayButton != null)
            LandDelayButton.onClick.AddListener(DelayLand);
        //上一年，下一年
        if(LandnextYear != null)
            LandnextYear.onClick.AddListener(()=>GameManager.Instance.LandNextYear());
        if(LandpreYear != null)
            LandpreYear.onClick.AddListener(()=>GameManager.Instance.LandPrevYear());

        //组装生产
        if(ProductpreYear != null)
            ProductpreYear.onClick.AddListener(()=>GameManager.Instance.ProductPreYear());
        if(ProductnextYear != null)
            ProductnextYear.onClick.AddListener(()=>GameManager.Instance.ProductNextYear());
        if (StartProduct != null)
            StartProduct.onClick.AddListener(() => GameManager.Instance.Model());
        if(EndProduct!= null) EndProduct.onClick.AddListener(() => GameManager.Instance.EndProduct());

        //订单交付
        if(Delivery!=null)
            Delivery.onClick.AddListener(()=>GameManager.Instance.DeliveryDisplay());
    }

    private void OnEnable()
    {
        EventHandler.UpdateMoneyUI += UpdateMoney;
        EventHandler.UpdateChipUI += UpdateChipText;
        EventHandler.UpdateDisUI += UpdateDisText;
        EventHandler.UpdateCaUI += UpdateCaText;
        EventHandler.UpdateBaUI += UpdateBaText;
        EventHandler.UpdateHoUI += UpdateHoText;
        EventHandler.UpdateMeUI += UpdateMeText;

        EventHandler.UpdateHistoryUI += Updatechiphistory;
        EventHandler.UpdateHistoryUI += Updatedishistory;
        EventHandler.UpdateHistoryUI += Updatemehistory;
        EventHandler.UpdateHistoryUI += Updatecahistory;
        EventHandler.UpdateHistoryUI += Updatebahistory;
        EventHandler.UpdateHistoryUI += Updatehohistory;

        EventHandler.UpdateOrderUI += UpdateOrdersDays;
        //政策事件
        EventHandler.UpdatePolicyUI += UpdatePolicyUI;
        //贷款竞标结果
        EventHandler.UpdateLoanUI += LoanAmount;
        //土地竞标更新
        EventHandler.UpdateLandUI += UpdateLand;

    }
    private void OnDisable()
    {
        EventHandler.UpdateMoneyUI -= UpdateMoney;
        EventHandler.UpdateChipUI -= UpdateChipText;
        EventHandler.UpdateDisUI -= UpdateDisText;
        EventHandler.UpdateCaUI -= UpdateCaText;
        EventHandler.UpdateBaUI -= UpdateBaText;
        EventHandler.UpdateHoUI -= UpdateHoText;
        EventHandler.UpdateMeUI -= UpdateMeText;

        EventHandler.UpdateHistoryUI -= Updatechiphistory;
        EventHandler.UpdateHistoryUI -= Updatedishistory;
        EventHandler.UpdateHistoryUI -= Updatemehistory;
        EventHandler.UpdateHistoryUI -= Updatecahistory;
        EventHandler.UpdateHistoryUI -= Updatebahistory;
        EventHandler.UpdateHistoryUI -= Updatehohistory;

        EventHandler.UpdateOrderUI -= UpdateOrdersDays;
        //政策事件
        EventHandler.UpdatePolicyUI -= UpdatePolicyUI;
        //贷款竞标结果
        EventHandler.UpdateLoanUI-=LoanAmount;
        //土地竞标更新
        EventHandler.UpdateLandUI -= UpdateLand;
    }

    private void UpdateMoney(List<Inventory> bag)
    {
        Money.text = bag[0].money.ToString();
    }


    public void UpdateChipText(List<Chip>ch)
    {
        int counter = 1;
        for (int i = 0; i < ch.Count; i++)
        {
            Text[] texts = chip.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "chip_model":
                        text.text = ch[i].chip_model;
                        break;

                    case "manufacturer":
                        text.text = ch[i].manufacturer;
                        break;

                    case "category":
                        text.text = ch[i].category;
                        break;

                    case "release_date":
                        text.text = ch[i].release_date;
                        break;

                    case "process_tech":
                        text.text = ch[i].process_tech;
                        break;

                    case "transistor_count":
                        text.text = ch[i].transistor_count;
                        break;

                    case "price_usd":
                        text.text = ch[i].price_usd.ToString();
                        break;

                    case "adopted_models":
                        text.text = ch[i].adopted_models;
                        break;
                    case "ID":
                        text.text = ch[i].ID.ToString();
                        break;
                    case "number":
                        text.text = ch[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    public void UpdateDisText(List<Displayscreen>dis)
    {
        int counter = 1;
        for (int i = 0; i < dis.Count; i++)
        {
            Text[] texts = display.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "display_model":
                        text.text = dis[i].display_model;
                        break;

                    case "manufacturer":
                        text.text = dis[i].manufacturer;
                        break;

                    case "resolution":
                        text.text = dis[i].resolution;
                        break;

                    case "refresh":
                        text.text = dis[i].refresh;
                        break;

                    case "price":
                        text.text = dis[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = dis[i].adopted_models;
                        break;
                    case "ID":
                        text.text = dis[i].ID.ToString();
                        break;
                    case "number":
                        text.text = dis[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    public void UpdateMeText(List<Memory>me)
    {
        int counter = 1;
        for (int i = 0; i < me.Count; i++)
        {
            Text[] texts = memory.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "memory_model":
                        text.text = me[i].memory_model;
                        break;

                    case "manufacturer":
                        text.text = me[i].manufacturer;
                        break;

                    case "type":
                        text.text = me[i].type;
                        break;

                    case "capacity":
                        text.text = me[i].capacity;
                        break;

                    case "price":
                        text.text = me[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = me[i].adopted_models;
                        break;
                    case "ID":
                        text.text = me[i].ID.ToString();
                        break;
                    case "number":
                        text.text = me[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    public void UpdateCaText(List<Camera1>ca)
    {
        int counter = 1;
        for (int i = 0; i < ca.Count; i++)
        {
            Text[] texts = camera1.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "camera_model":
                        text.text = ca[i].camera_model;
                        break;

                    case "manufacturer":
                        text.text = ca[i].manufacturer;
                        break;

                    case "type":
                        text.text = ca[i].type;
                        break;

                    case "main_specs":
                        text.text = ca[i].main_specs;
                        break;

                    case "unit_prie":
                        text.text = ca[i].unit_price.ToString();
                        break;

                    case "adopted_models":
                        text.text = ca[i].adopted_models;
                        break;
                    case "ID":
                        text.text = ca[i].ID.ToString();
                        break;
                    case "number":
                        text.text = ca[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    public void UpdateBaText(List<Battery>ba)
    {
        int counter = 1;
        for (int i = 0; i < ba.Count; i++)
        {
            Text[] texts = battery.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "battery_model":
                        text.text = ba[i].battery_model;
                        break;

                    case "manufacturer":
                        text.text = ba[i].manufacturer;
                        break;

                    case "capacity":
                        text.text = ba[i].capacity;
                        break;

                    case "price":
                        text.text = ba[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = ba[i].adopted_models;
                        break;
                    case "ID":
                        text.text = ba[i].ID.ToString();
                        break;
                    case "number":
                        text.text = ba[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    public void UpdateHoText(List<Housing>ho)
    {
        int counter = 1;
        for (int i = 0; i < ho.Count; i++)
        {
            Text[] texts = housing.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "part_model":
                        text.text = ho[i].part_model;
                        break;

                    case "manufacturer":
                        text.text = ho[i].manufacturer;
                        break;

                    case "material_type":
                        text.text = ho[i].material_type;
                        break;

                    case "price":
                        text.text = ho[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = ho[i].adopted_models;
                        break;
                    case "ID":
                        text.text = ho[i].ID.ToString();
                        break;
                    case "number":
                        text.text = ho[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //更新交易历史
    public void Updatechiphistory(List<Inventory> bag)
    {
        int counter = 1;
        for (int i = 1; i < bag[0].Chip.Count; i++)
        {
            Text[] texts = historychip.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1"://买入公司
                        text.text = "A";
                        break;

                    case "number":
                        text.text = bag[0].Chip[i].number.ToString();
                        break;

                    case "2":
                        text.text = "全额支付";
                        break;

                    case "3"://赊账比率
                        text.text = "无";
                        break;

                    case "4":
                        text.text = "第一年";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    public void Updatedishistory(List<Inventory> bag)
    {
        int counter = 1;
        for (int i = 1; i < bag[0].Displayscreen.Count; i++)
        {
            Text[] texts = historydis.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = "A";
                        break;

                    case "number":
                        text.text = bag[0].Displayscreen[i].number.ToString();
                        break;

                    case "2":
                        text.text = "全额支付";
                        break;

                    case "3":
                        text.text = "无";
                        break;

                    case "4":
                        text.text = "第一年";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    public void Updatemehistory(List<Inventory> bag)
    {
        int counter = 1;
        for (int i = 1; i < bag[0].Memory.Count; i++)
        {
            Text[] texts = historyme.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = "A";
                        break;
                    case "number":
                        text.text = bag[0].Memory[i].number.ToString();
                        break;

                    case "2":
                        text.text = "全额支付";
                        break;

                    case "3":
                        text.text = "无";
                        break;

                    case "4":
                        text.text = "第一年";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    public void Updatecahistory(List<Inventory> bag)
    {
        int counter = 1;
        for (int i = 1; i < bag[0].Camera.Count; i++)
        {
            Text[] texts = historyca.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = "A";
                        break;
                    case "number":
                        text.text = bag[0].Camera[i].number.ToString();
                        break;

                    case "2":
                        text.text = "全额支付";
                        break;

                    case "3":
                        text.text = "无";
                        break;

                    case "4":
                        text.text = "第一年";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    public void Updatebahistory(List<Inventory> bag)
    {
        int counter = 1;
        for (int i = 1; i < bag[0].Battery.Count; i++)
        {
            Text[] texts = historyba.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = "A";
                        break;
                    case "number":
                        text.text = bag[0].Battery[i].number.ToString();
                        break;

                    case "2":
                        text.text = "全额支付";
                        break;

                    case "3":
                        text.text = "无";
                        break;

                    case "4":
                        text.text = "第一年";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    public void Updatehohistory(List<Inventory> bag)
    {
        int counter = 1;
        for (int i = 1; i < bag[0].Housing.Count; i++)
        {
            Text[] texts = historyho.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = "A";
                        break;
                    case "number":
                        text.text = bag[0].Housing[i].number.ToString();
                        //Debug.Log(text.text);
                        break;

                    case "2":
                        text.text = "全额支付";
                        break;

                    case "3":
                        text.text = "无";
                        break;

                    case "4":
                        text.text = "第一年";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //更新竞标天数
    public void UpdateOrdersDays(List<Order> orders)
    {
        int counter = 1;
        for (int i = 0; i < orders.Count; i++)
        {
            Text[] texts = BidContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "daysRemaining":
                        text.text = orders[i].daysRemaining.ToString();
                        break;
                    case "isActive":
                        if (orders[i].daysRemaining <= 0)
                            text.text = "否";
                        break;
                    case "isWon":
                        if (orders[i].currentBidPrice > 0)
                            text.text = "是";
                        break;
                            default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    public void CloseAndOpen(GameObject Open,GameObject Close)
    {
        Open.SetActive(true);
        Close.SetActive(false);
    }
    public void CloseAndOpen(GameObject a,GameObject b,GameObject c)
    {
        a.SetActive(true);
        b.SetActive(false);
        c.SetActive(false);
    }
    public void ClosePanel(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    //竞标开始后防止重复点击
    public void Unique()
    {
        StartBid.interactable = false;
        GameManager.Instance.BidTimer();
    }

    //结束竞标
    public void EndBid()
    {
        SubmitBid.interactable = false;
    }

    //竞标开始按钮恢复点击
    public void RestoreClick()
    {
        StartBid.interactable= true;
        SubmitBid.interactable = true;
    }

    //外围经济更新
    public void PerEconomy()
    {
        float MaxPhonePrice = 0;
        float MinPhonePrice = 0;

        float MaxChipPrice = 0;

        float MaxDisPrice = 0;

        float MaxMePrice = 0;

        float MaxCaPrice = 0;

        float MaxBaPrice = 0;

        float MaxHoPrice = 0;
        GameManager.Instance.PerEconomy(ref MaxPhonePrice,ref MinPhonePrice,
            ref MaxChipPrice,
            ref MaxDisPrice,
            ref MaxMePrice,
            ref MaxCaPrice,
            ref MaxBaPrice,
            ref MaxHoPrice);
        maxPhonePrice.text = MaxPhonePrice.ToString();
        minPhonePrice.text = MinPhonePrice.ToString();
        maxChipPrice.text = MaxChipPrice.ToString();
        maxDisPrice.text = MaxDisPrice.ToString();;
        maxMePrice.text = MaxMePrice.ToString();
        maxCaPrice.text = MaxCaPrice.ToString();
        maxBaPrice.text = MaxBaPrice.ToString();
        maxHoPrice.text = MaxHoPrice.ToString();
    }

    //政府措施输入部分
    public void GovernmentMeasures(float praLandSupply, float praInterestRate,
        float moneyupply, float vatRate1, float incomeTaxRate1, float subsidyRate, int bidNumber)
    {
        praLandSupply = float.Parse(PraLandSupply.text);
        praInterestRate = float.Parse(PraInterestRate.text);
        moneyupply = float.Parse(Moneyupply.text);
        vatRate1 = float.Parse(vatRate.text);
        incomeTaxRate1 = float.Parse(incomeTaxRate.text);
        subsidyRate = float.Parse(SubsidyRate.text);
        bidNumber = int.Parse(BidNumber.text);

    }

    //----------政府政策UI更新---------
    public void UpdatePolicyUI(GovernmentPolicy policy)
    {
        if (policy == null) return;
        //年份显示
        if (policyYearDisplay != null) policyYearDisplay.text = policy.year.ToString();


        // 只读文本
        if (maxLandSupply != null) maxLandSupply.text = policy.maxLandSupply.ToString("F2");
        if (minInterestRate != null) minInterestRate.text = policy.minInterestRate.ToString("F2");
        if (FiscalStimulusRevenue != null) FiscalStimulusRevenue.text = policy.FiscalStimulusRevenue.ToString("F2");
        if (Subsidy != null) Subsidy.text = policy.Subsidy.ToString("F2");
        if (FiscalStimulusFunds != null) FiscalStimulusFunds.text = policy.FiscalStimulusFunds.ToString("F2");
        if (maxPhonePrice != null) maxPhonePrice.text = policy.maxPhonePrice.ToString("F2");
        if (minPhonePrice != null) minPhonePrice.text = policy.minPhonePrice.ToString("F2");
        if (maxChipPrice != null) maxChipPrice.text = policy.maxChipPrice.ToString("F2");
        if (maxDisPrice != null) maxDisPrice.text = policy.maxDisPrice.ToString("F2");
        if (maxMePrice != null) maxMePrice.text = policy.maxMePrice.ToString("F2");
        if (maxCaPrice != null) maxCaPrice.text = policy.maxCaPrice.ToString("F2");
        if (maxBaPrice != null) maxBaPrice.text = policy.maxBaPrice.ToString("F2");
        if (maxHoPrice != null) maxHoPrice.text = policy.maxHoPrice.ToString("F2");
        if (LoanableFunds != null) LoanableFunds.text = policy.LoanableFunds.ToString("F2");
        //PerEconomy();

        // 可编辑输入框
        if (PraLandSupply != null) PraLandSupply.text = policy.PraLandSupply.ToString();
        if (PraInterestRate != null) PraInterestRate.text = policy.PraInterestRate.ToString();
        if (Moneyupply != null) Moneyupply.text = policy.Moneyupply.ToString();
        if (vatRate != null) vatRate.text = policy.vatRate.ToString();
        if (incomeTaxRate != null) incomeTaxRate.text = policy.incomeTaxRate.ToString();
        if (SubsidyRate != null) SubsidyRate.text = policy.SubsidyRate.ToString();
        if (BidNumber != null) BidNumber.text = policy.BidNumber.ToString();
        
        //更新贷款利率
        loanRateText.text = policy.PraInterestRate.ToString();
        
    }

    // 措施开始/结束的UI控制
    public void OnMeasuresStarted()
    {
        if (submitPolicyButton != null)
            submitPolicyButton.interactable = false;
        startMeasuresButton.interactable=false;
    }

    public void OnMeasuresEnded()
    {
        if (submitPolicyButton != null)
            submitPolicyButton.interactable = true;
    }

    //----------贷款UI-------------
    private void OnApplyLoanClick()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Loan();
    }

    //参与
    public void Participate()
    {
        GameManager.Instance.ParticipateLoans(true);
        Participation.interactable=false;
        NoParticipation.interactable=false;
        GameManager.Instance.InitialResultsLoans();

    }

    //不参与
    public void NoParticipate()
    {
        GameManager.Instance.ParticipateLoans(false);
        Participation.interactable=false;
        NoParticipation.interactable=false;
    }

    //初始化贷款竞标界面的显示
    public void LoanBidpanelDisplay(GovernmentPolicy policy)
    {
        loanRateText.text = policy.PraInterestRate.ToString();
        actualReceivedText.text = "0";
        remainingLoanableText.text = policy.LoanableFunds.ToString();
    }

    //提交后的UI更新贷款竞标
    public void LoanAmount(Loan loan, GovernmentPolicy policy)
    {
        //贷款竞标界面的
        loanRateText.text = policy.PraInterestRate.ToString();
        actualReceivedText.text = loan.actualAmount.ToString();
        remainingLoanableText.text = loan.availableCreditLine.ToString();

        //贷款竞标结果界面的
        GameManager.Instance.UpdateLoanResult();
        //把年份加上
        LoanYear.text=loan.Loanyear.ToString();
    }

    //开始贷款竞拍
    public void StartLoan()
    {
        applyLoanButton.interactable = true;
        LoanStartButton.interactable = false;
        //计时器暂时不写

    }

    //结束贷款竞拍
    public void EndLoan()
    {
        LoanEndButton.interactable = false;
        applyLoanButton.interactable = false;
        GameManager.Instance.LoanEnd();
    }
    //延迟贷款竞拍
    public void DelayLoan()
    {
        LoanEndButton.interactable = true;
        applyLoanButton.interactable = true;
    }

    //

    //----------土地UI更新-------------
    //参与
    public void LandParticipate()
    {
        GameManager.Instance.LandInspection(true);
        LandParticipation.interactable = false;
        LandNoParticipation.interactable = false;
        //土地资金提交按钮可使用
        LandRentButton.interactable = true;
        GameManager.Instance.InitialResultsLands();
    }
    //不参与
    public void LandNoParticipate()
    {
        GameManager.Instance.LandInspection(false);
        Participation.interactable = false;
        NoParticipation.interactable = false;
        LandRentButton.interactable = true;
    }
    public void LandBidpanelDisplay(GovernmentPolicy policy)
    {
        TotalSupply.text=policy.PraLandSupply.ToString();
        ActualAcquisition.text="0";
        RemainingLand.text=policy.PraLandSupply.ToString();
    }
    //土地租金提交
    public void LandRentSubmsit()
    {
        //在提交了土地租金以后才能开始竞拍
        LandStartButton.interactable = true;
        GameManager.Instance.LandRentSubmsit();
        //禁止掉按钮
        LandRentButton.interactable = false;
    }
    //提交后的UI更新土地竞标
    public void UpdateLand(Land land,GovernmentPolicy policy)
    {
        TotalSupply.text=land.TotalSupply.ToString();
        ActualAcquisition.text= land.ActualAcquisition.ToString();
        RemainingLand.text=land.RemainingLand.ToString();
        GameManager.Instance.UpdateLandResult();
        //把年份更新也加上
        LandYear.text=land.year.ToString();
    }
    //开始土地竞拍
    public void StartLand()
    {
        LandSubmit.interactable=true;
        LandStartButton.interactable=false;
        //计时器先不写
    }
    //结束土地竞拍
    public void EndLand()
    {
        LandSubmit.interactable = false;
        LandEndButton.interactable=false;
        //上传数据
        GameManager.Instance.EndLand();
    }
    //延迟土地竞拍
    public void DelayLand()
    {
        LandEndButton.interactable = true;
        LandSubmit.interactable = true;
    }

    //土地查看界面的UI更新(土地查看主要是玩家的)+可能需要加一个租赁时长,直接在结束竞拍的时候把土地查看放上去，直接生成
    public void LandInspection()
    {

    }


}
