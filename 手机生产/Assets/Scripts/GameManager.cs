using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : Singleton<GameManager>
{
    [Header("手机数据")]
    public Mbphone_SO Mbphone;
    //芯片
    public ChipDatalist_SO ChipData;
    public GameObject chip;
    public GameObject chipContent;
    //历史交易
    public GameObject chiphistory;
    public GameObject historychip;
    //显示屏
    public Displayscreen_SO Displayscreen;
    public GameObject Dis;
    public GameObject disContent;

    public GameObject historydis;
    //内存
    public Memory_SO Memory;
    public GameObject Me;
    public GameObject MeContent;

    public GameObject historyme;
    //摄像头
    public Camera_SO Camera1;
    public GameObject Ca;
    public GameObject CaContent;

    public GameObject historyca;
    //电池
    public Battery_SO Battery;
    public GameObject Ba;
    public GameObject BaContent;

    public GameObject historyba;
    //外壳
    public Housing_SO Housing;
    public GameObject Ho;
    public GameObject HoContent;

    public GameObject historyho;

    [Header("数据的加载路径")]
    private string resourcesPath;
    [Header("资产")]
    public Inventory_SO Inventory;

    [Header("输入数据（暂时不知道放哪）")]
    public InputField chipid;
    public InputField chipcount;

    public InputField disid;
    public InputField discount;

    public InputField meid;
    public InputField mecount;

    public InputField caid;
    public InputField cacount;

    public InputField baid;
    public InputField bacount;

    public InputField hoid;
    public InputField hocount;

    public InputField BidID;
    public InputField Quotation;

    //交易界面
    public Text chipID;
    public Text chipCount;
    public Text chipname;

    public Text disID;
    public Text disCount;
    public Text disname;

    public Text meID;
    public Text meCount;
    public Text mename;

    public Text caID;
    public Text caCount;
    public Text caname;

    public Text baID;
    public Text baCount;
    public Text baname;

    public Text hoID;
    public Text hoCount;
    public Text honame;

    [Header("订单系统")]
    public OrderList_SO orderData;
    public OrderCreate orderGenerator;

    public GameObject Bid;
    public GameObject BidContent;

    public float bidRoundTime = 10f; // 每轮竞拍时间（秒）
    private Coroutine bidTimerCoroutine; // 用于存储竞拍协程的引用


    [Header("手机产业政策")]
    public GovernmentPolicy_SO governmentPolicy;
    public GovernmentPolicy currentPolicy;
    public Text policyYearText;//用来在UI上显示当前年份
    public int currentYear = 1;
    public bool isPolicyActive = false;

    [Header("贷款系统")]
    public Loan_SO loan;
    public Loan currentLoan;
    public GameObject ParticpateContent;
    //贷款和土地参与状态共用一个
    public GameObject CommercialLoanStatus;

    public GameObject resultContent;
    public GameObject CommercialLoanResults;
    public bool IsLoanYear;
    public int currentLoanYear = 1;

    [Header("土地系统")]
    public Land_SO land;
    public Land currentLand;
    public GameObject LandPartcpateContent;

    public GameObject LandResultContent;
    public GameObject LandResutlts;
    public bool IsLandYear;
    public int currentLandYear = 1;

    //土地查看
    public GameObject LandInspectionContent;
    public GameObject LandView;

    [Header("组装生产")]
    public Product_SO product;
    public Product currentProduct;
    public GameObject ProductContent;
    public GameObject Productprefab;

    public bool IsProductYear;
    public int currentProductYear;

    //手机生产信息表
    public GameObject MbPhonePrefab;
    public GameObject MbPhoneInformationContent;

    [Header("订单交付")]
    public Delivery_SO delivery;
    public Delivery currentDelivery;
    public GameObject deliveryContent;
    public GameObject deliveryprefabs;
    //关于年份好几个没搞

    [Header("补贴")]
    public subsidy_SO subsidy;
    public GameObject subsidyContent;
    public GameObject subsidyPrefabs;
    public subsidy currentSubsidy;

    [Header("绩效")]
    public Formance_SO formance;
    public formance currentformance;
    public GameObject performanceContent;
    public GameObject performancePrefab;

    public GameObject AssetsContent;
    //public GameObject AssetsPrefab;

    public GameObject costAccountingContent;
    //public GameObject costAccountingPrefab;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            StartCoroutine(LoadAllData());
        }
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);

    }

    protected override void Awake()
    {
        base.Awake();               // 执行基类的单例初始化
        resourcesPath = Application.dataPath + "/Resources/";
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            PackageLocalData.Instance.SaveMb(Mbphone);
        if (Input.GetKeyDown(KeyCode.A))
        {
            Mbphone = PackageLocalData.Instance.LoadMb(resourcesPath + "phone.json");
        }
    }

    private IEnumerator LoadAllData()
    {
        yield return new WaitForSeconds(0.5f);
        ChipData = PackageLocalData.Instance.LoadChip(resourcesPath + "chip.json");
        Displayscreen = PackageLocalData.Instance.LoadDisplay(resourcesPath + "dis.json");
        Memory = PackageLocalData.Instance.LoadMe(resourcesPath + "memory.json");
        Camera1 = PackageLocalData.Instance.LoadCa(resourcesPath + "camera.json");
        Battery = PackageLocalData.Instance.LoadBa(resourcesPath + "battery.json");
        Housing = PackageLocalData.Instance.LoadHo(resourcesPath + "housing.json");
        Mbphone = PackageLocalData.Instance.LoadMb(resourcesPath + "phone.json");
        GetChipText();
        GetDisText();
        GetMeText();
        GetCaText();
        GetBaText();
        GetHoText();

        ShowChipHistory();
        ShowDisHistory();
        ShowMeHistory();
        ShowCaHistory();
        ShowBaHistory();
        ShowHoHistory();

        //刚开始让花销为0
        Inventoryexpense();
        //销售的原料费为0
        rawMaterial();
        //人工费为0
        manual();
        //初始化订单系统
        InitializeOrderSystem();

        //初始化当前政策
        LoadPolicyForYear(currentYear);

        //显示手机基础信息
        ShowMbphoneInformation();
        //将手机基础信息存入背包
        InventoryPhones();

    }

    //-------------解析辅助方法-----------------
    private float ParseFloat(InputField input, float defaultValue)
    {
        if (input != null && float.TryParse(input.text, out float result))
            return result;
        return defaultValue;

    }

    private int ParseInt(InputField input, int defaultValue)
    {
        if (input != null && int.TryParse(input.text, out int result))
            return result;
        return defaultValue;
    }

    public void Inventoryexpense()
    {
        for(int i = 0; i < Inventory.Bag.Count; i++)
        {
            Inventory.Bag[i].expense = 0;
        }
    }

    public void rawMaterial()
    {
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            Inventory.Bag[i].rawMaterial = 0;
        }
    }

    public void manual()
    {
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            Inventory.Bag[i].manual = 0;
        }
    }

    //------------政府政策------------
    //提交按钮的方法
    public void SubmitPolicy()
    {
        if (isPolicyActive)
        {
            return;
        }

        //要判断一下GovernmentPolicy_SO里是否有位置存入
        if (governmentPolicy.policies == null)
            governmentPolicy.policies = new List<GovernmentPolicy>();

        if (governmentPolicy.policies.Count == 0)
        {
            GovernmentPolicy defaultPolicy = new GovernmentPolicy();
            governmentPolicy.policies.Add(defaultPolicy);
        }

        currentPolicy = new GovernmentPolicy { year = currentYear };
        // 基础政策
        currentPolicy.maxLandSupply = 1100;//UnityEngine.Random.Range(1100, 1300);
        currentPolicy.minInterestRate = 6;//UnityEngine.Random.Range(6, 8);


        // 从UI读取输入
        if (ParseFloat(UIManager.Instance.PraLandSupply, currentPolicy.PraLandSupply) > currentPolicy.maxLandSupply)
        {
            Debug.Log("输入的土地实际供给量太大了");
            EventHandler.CallUpdatePolicyUI(currentPolicy);//刷新UI
            return;
        }
        if (ParseFloat(UIManager.Instance.PraInterestRate, currentPolicy.PraInterestRate) < currentPolicy.minInterestRate)
        {
            Debug.Log("输入的年度最低利率太大了");
            EventHandler.CallUpdatePolicyUI(currentPolicy);//刷新UI
            return;
        }
        currentPolicy.PraLandSupply = ParseFloat(UIManager.Instance.PraLandSupply, currentPolicy.PraLandSupply);
        currentPolicy.PraInterestRate = ParseFloat(UIManager.Instance.PraInterestRate, currentPolicy.PraInterestRate);
        currentPolicy.Moneyupply = ParseFloat(UIManager.Instance.Moneyupply, currentPolicy.Moneyupply);
        Debug.Log(currentPolicy.Moneyupply + "新增货币");
        currentPolicy.vatRate = ParseFloat(UIManager.Instance.vatRate, currentPolicy.vatRate);
        currentPolicy.incomeTaxRate = ParseFloat(UIManager.Instance.incomeTaxRate, currentPolicy.incomeTaxRate);
        currentPolicy.SubsidyRate = ParseFloat(UIManager.Instance.SubsidyRate, currentPolicy.SubsidyRate);
        currentPolicy.BidNumber = ParseInt(UIManager.Instance.BidNumber, currentPolicy.BidNumber);
        //外围经济数据
        float maxPhonePrice = 0;
        float minPhonePrice = 0;
        float maxChipPrice = 0;
        float maxDisPrice = 0;
        float maxMePrice = 0;
        float maxCaPrice = 0;
        float maxBaPrice = 0;
        float maxHoPrice = 0;
        PerEconomy(ref maxPhonePrice, ref minPhonePrice, ref maxChipPrice,
        ref maxDisPrice, ref maxMePrice, ref maxCaPrice, ref maxBaPrice, ref maxHoPrice);
        currentPolicy.maxBaPrice = maxBaPrice;
        currentPolicy.minPhonePrice = minPhonePrice;
        currentPolicy.maxChipPrice = maxChipPrice;
        currentPolicy.maxDisPrice = maxDisPrice;
        currentPolicy.maxMePrice = maxMePrice;
        currentPolicy.maxCaPrice = maxCaPrice;
        currentPolicy.maxBaPrice = maxBaPrice;
        currentPolicy.maxHoPrice = maxHoPrice;

        //新增货币量影响可贷款资金
        if (governmentPolicy.policies[currentYear - 1] != null)
            currentPolicy.LoanableFunds = governmentPolicy.policies[currentYear - 1].LoanableFunds + currentPolicy.Moneyupply;
        else
        {
            currentPolicy.LoanableFunds = currentPolicy.Moneyupply;
        }
        //上届政府可用于财政刺激的收入（万元）=增值税收入（销售额*增值税）+所得税收入（利润*所得税）+土地收入+上届政府结余财政刺激资金（万元）
        //上届政府发放补贴=补贴比率*销售额（支出）
        //上届结余=上届财政收入−上届财政支出
        //补贴比对于玩家出售    厂商实际收入=售价×(1+补贴率)，所有在支付时，玩家不仅有利润，还包括补贴

        //保存到SO
        governmentPolicy.UpdatePolicy(currentYear, currentPolicy);

        //ApplyCurrentPolicy();//应用政府效果（订单数），放到开始政府政策按钮上
        EventHandler.CallUpdatePolicyUI(currentPolicy);//刷新UI
        Debug.Log($"政策已提交，年份是{currentYear}");
    }

    public void PrevYear()
    {
        if (isPolicyActive)
        {
            Debug.LogWarning("政府措施已开始，无法切换年份");
            return;
        }
        currentYear = Mathf.Max(1, currentYear - 1);
        LoadPolicyForYear(currentYear);
    }

    public void NextYear()
    {
        if (isPolicyActive)
        {
            Debug.LogWarning("政府措施已开始，无法切换年份");
            return;
        }
        currentYear++;
        LoadPolicyForYear(currentYear);
    }

    private void LoadPolicyForYear(int year)
    {
        currentPolicy = governmentPolicy.GetPolicyForYear(year);
        if (currentPolicy == null)
        {
            currentPolicy = CreateDefaultPhoneIndustryPolicy();
        }
        EventHandler.CallUpdatePolicyUI(currentPolicy);
    }

    //开始政府措施
    public void StartMeasures()
    {
        isPolicyActive = true;
        UIManager.Instance.OnMeasuresStarted();
        //执行政府措施时，需要达到的一些效果
        ApplyCurrentPolicy();
        ProductionInitialization();
        //补贴初始信息
        SubsidyInitialization();
        //初始化损益表
        performanceInitialization();
    }

    public void EndMeasures()
    {
        isPolicyActive = false;
        UIManager.Instance.OnMeasuresEnded();
    }

    //在UI上显示芯片信息
    public void GetChipText()
    {
        int counter = 1;
        for (int i = 0; i < ChipData.chipList.Count; i++)
        {
            GameObject A = Instantiate(chip, chipContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "chip_model":
                        text.text = ChipData.chipList[i].chip_model;
                        break;

                    case "manufacturer":
                        text.text = ChipData.chipList[i].manufacturer;
                        break;

                    case "category":
                        text.text = ChipData.chipList[i].category;
                        break;

                    case "release_date":
                        text.text = ChipData.chipList[i].release_date;
                        break;

                    case "process_tech":
                        text.text = ChipData.chipList[i].process_tech;
                        break;

                    case "transistor_count":
                        text.text = ChipData.chipList[i].transistor_count;
                        break;

                    case "price_usd":
                        text.text = ChipData.chipList[i].price_usd.ToString();
                        break;

                    case "adopted_models":
                        text.text = ChipData.chipList[i].adopted_models;
                        break;
                    case "ID":
                        text.text = ChipData.chipList[i].ID.ToString();
                        break;
                    case "number":
                        text.text = ChipData.chipList[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }

    }
    //在UI上显示显示屏信息
    public void GetDisText()
    {
        int counter = 1;
        for (int i = 0; i < Displayscreen.disList.Count; i++)
        {
            GameObject A = Instantiate(Dis, disContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "display_model":
                        text.text = Displayscreen.disList[i].display_model;
                        break;

                    case "manufacturer":
                        text.text = Displayscreen.disList[i].manufacturer;
                        break;

                    case "resolution":
                        text.text = Displayscreen.disList[i].resolution;
                        break;

                    case "refresh":
                        text.text = Displayscreen.disList[i].refresh;
                        break;

                    case "price":
                        text.text = Displayscreen.disList[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = Displayscreen.disList[i].adopted_models;
                        break;
                    case "ID":
                        text.text = Displayscreen.disList[i].ID.ToString();
                        break;
                    case "number":
                        text.text = Displayscreen.disList[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //在UI上显示内存信息
    public void GetMeText()
    {
        int counter = 1;
        for (int i = 0; i < Memory.MeList.Count; i++)
        {
            GameObject A = Instantiate(Me, MeContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "memory_model":
                        text.text = Memory.MeList[i].memory_model;
                        break;

                    case "manufacturer":
                        text.text = Memory.MeList[i].manufacturer;
                        break;

                    case "type":
                        text.text = Memory.MeList[i].type;
                        break;

                    case "capacity":
                        text.text = Memory.MeList[i].capacity;
                        break;

                    case "price":
                        text.text = Memory.MeList[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = Memory.MeList[i].adopted_models;
                        break;
                    case "ID":
                        text.text = Memory.MeList[i].ID.ToString();
                        break;
                    case "number":
                        text.text = Memory.MeList[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //在UI上显示摄像头信息
    public void GetCaText()
    {
        int counter = 1;
        for (int i = 0; i < Camera1.caList.Count; i++)
        {
            GameObject A = Instantiate(Ca, CaContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "camera_model":
                        text.text = Camera1.caList[i].camera_model;
                        break;

                    case "manufacturer":
                        text.text = Camera1.caList[i].manufacturer;
                        break;

                    case "type":
                        text.text = Camera1.caList[i].type;
                        break;

                    case "main_specs":
                        text.text = Camera1.caList[i].main_specs;
                        break;

                    case "unit_prie":
                        text.text = Camera1.caList[i].unit_price.ToString();
                        break;

                    case "adopted_models":
                        text.text = Camera1.caList[i].adopted_models;
                        break;
                    case "ID":
                        text.text = Camera1.caList[i].ID.ToString();
                        break;
                    case "number":
                        text.text = Camera1.caList[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //在UI上显示电池信息
    public void GetBaText()
    {
        int counter = 1;
        for (int i = 0; i < Battery.baList.Count; i++)
        {
            GameObject A = Instantiate(Ba, BaContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "battery_model":
                        text.text = Battery.baList[i].battery_model;
                        break;

                    case "manufacturer":
                        text.text = Battery.baList[i].manufacturer;
                        break;

                    case "capacity":
                        text.text = Battery.baList[i].capacity;
                        break;

                    case "price":
                        text.text = Battery.baList[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = Battery.baList[i].adopted_models;
                        break;
                    case "ID":
                        text.text = Battery.baList[i].ID.ToString();
                        break;
                    case "number":
                        text.text = Battery.baList[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //在UI上显示材料信息
    public void GetHoText()
    {
        int counter = 1;
        for (int i = 0; i < Housing.hoList.Count; i++)
        {
            GameObject A = Instantiate(Ho, HoContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "part_model":
                        text.text = Housing.hoList[i].part_model;
                        break;

                    case "manufacturer":
                        text.text = Housing.hoList[i].manufacturer;
                        break;

                    case "material_type":
                        text.text = Housing.hoList[i].material_type;
                        break;

                    case "price":
                        text.text = Housing.hoList[i].price.ToString();
                        break;

                    case "adopted_models":
                        text.text = Housing.hoList[i].adopted_models;
                        break;
                    case "ID":
                        text.text = Housing.hoList[i].ID.ToString();
                        break;
                    case "number":
                        text.text = Housing.hoList[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //在UI上显示成交历史
    public void ShowChipHistory()
    {
        int counter = 1;
        for (int i = 0; i < ChipData.chipList.Count; i++)
        {
            GameObject A = Instantiate(chiphistory, historychip.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Chip_model":
                        text.text = ChipData.chipList[i].chip_model;
                        break;
                    case "manufacturer":
                        text.text = ChipData.chipList[i].manufacturer;
                        break;

                    case "1":
                        text.text = "A";
                        break;

                    case "price_usd":
                        text.text = ChipData.chipList[i].price_usd.ToString();
                        break;

                    case "number":
                        text.text = "0";
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

    public void ShowDisHistory()
    {
        int counter = 1;
        for (int i = 0; i < Displayscreen.disList.Count; i++)
        {
            GameObject A = Instantiate(chiphistory, historydis.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Chip_model":
                        text.text = Displayscreen.disList[i].display_model;
                        break;
                    case "manufacturer":
                        text.text = Displayscreen.disList[i].manufacturer;
                        break;

                    case "1":
                        text.text = "A";
                        break;

                    case "price_usd":
                        text.text = Displayscreen.disList[i].price.ToString();
                        break;

                    case "number":
                        text.text = "0";
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

    public void ShowMeHistory()
    {
        int counter = 1;
        for (int i = 0; i < Memory.MeList.Count; i++)
        {
            GameObject A = Instantiate(chiphistory, historyme.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Chip_model":
                        text.text = Memory.MeList[i].memory_model;
                        break;
                    case "manufacturer":
                        text.text = Memory.MeList[i].manufacturer;
                        break;

                    case "1":
                        text.text = "A";
                        break;

                    case "price_usd":
                        text.text = Memory.MeList[i].price.ToString();
                        break;

                    case "number":
                        text.text = "0";
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

    public void ShowCaHistory()
    {
        int counter = 1;
        for (int i = 0; i < Camera1.caList.Count; i++)
        {
            GameObject A = Instantiate(chiphistory, historyca.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Chip_model":
                        text.text = Camera1.caList[i].camera_model;
                        break;
                    case "manufacturer":
                        text.text = Camera1.caList[i].manufacturer;
                        break;

                    case "1":
                        text.text = "A";
                        break;

                    case "price_usd":
                        text.text = Camera1.caList[i].unit_price.ToString();
                        break;

                    case "number":
                        text.text = "0";
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

    public void ShowBaHistory()
    {
        int counter = 1;
        for (int i = 0; i < Battery.baList.Count; i++)
        {
            GameObject A = Instantiate(chiphistory, historyba.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Chip_model":
                        text.text = Battery.baList[i].battery_model;
                        break;
                    case "manufacturer":
                        text.text = Battery.baList[i].manufacturer;
                        break;

                    case "1":
                        text.text = "A";
                        break;

                    case "price_usd":
                        text.text = Battery.baList[i].price.ToString();
                        break;

                    case "number":
                        text.text = "0";
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

    public void ShowHoHistory()
    {
        int counter = 1;
        for (int i = 0; i < Housing.hoList.Count; i++)
        {
            GameObject A = Instantiate(chiphistory, historyho.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Chip_model":
                        text.text = Housing.hoList[i].part_model;
                        break;
                    case "manufacturer":
                        text.text = Housing.hoList[i].manufacturer;
                        break;

                    case "1":
                        text.text = "A";
                        break;

                    case "price_usd":
                        text.text = Housing.hoList[i].price.ToString();
                        break;

                    case "number":
                        text.text = "0";
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

    //在UI上显示订单信息
    public void ShowBid()
    {
        int counter = 1;
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            GameObject A = Instantiate(Bid, BidContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "orderID":
                        text.text = orderData.activeOrders[i].orderID.ToString();
                        break;
                    case "customerName":
                        text.text = orderData.activeOrders[i].customerName;
                        break;

                    case "phoneModelID":
                        text.text = orderData.activeOrders[i].phoneModelID.ToString();
                        break;

                    case "quantity":
                        text.text = orderData.activeOrders[i].quantity.ToString();
                        break;

                    case " maxPrice":
                        text.text = orderData.activeOrders[i].maxPrice.ToString();
                        break;

                    case "currentBidPrice":
                        text.text = orderData.activeOrders[i].currentBidPrice.ToString();
                        break;

                    case "daysRemaining":
                        text.text = orderData.activeOrders[i].daysRemaining.ToString();
                        break;

                    case "isActive":
                        if (orderData.activeOrders[i].isActive)
                            text.text = "是";
                        else
                            text.text = "否";
                        break;
                    case "isWon":
                        if (orderData.activeOrders[i].isWon)
                            text.text = "是";
                        else
                            text.text = "否";
                        break;
                    case "deliveryDeadline":
                        text.text = orderData.activeOrders[i].deliveryDeadline.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
    }
    //重新竞拍后显示信息
    public void AgainShowBid()
    {
        int counter = 1;
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            Text[] texts = BidContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "orderID":
                        text.text = orderData.activeOrders[i].orderID.ToString();
                        break;
                    case "customerName":
                        text.text = orderData.activeOrders[i].customerName;
                        break;

                    case "phoneModelID":
                        text.text = orderData.activeOrders[i].phoneModelID.ToString();
                        break;

                    case "quantity":
                        text.text = orderData.activeOrders[i].quantity.ToString();
                        break;

                    case " maxPrice":
                        text.text = orderData.activeOrders[i].maxPrice.ToString();
                        break;

                    case "currentBidPrice":
                        text.text = orderData.activeOrders[i].currentBidPrice.ToString();
                        break;

                    case "daysRemaining":
                        text.text = orderData.activeOrders[i].daysRemaining.ToString();
                        break;

                    case "isActive":
                        if (orderData.activeOrders[i].isActive)
                            text.text = "是";
                        else
                            text.text = "否";
                        break;
                    case "isWon":
                        if (orderData.activeOrders[i].isWon)
                            text.text = "是";
                        else
                            text.text = "否";
                        break;
                    case "deliveryDeadline":
                        text.text = orderData.activeOrders[i].deliveryDeadline.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //关于交易的操作
    public void BuyChip()
    {
        int number1 = 0;
        if (string.IsNullOrEmpty(chipid.text) || string.IsNullOrEmpty(chipcount.text))
        {
            Debug.Log("id或count不能为空");
            return;
        }
        if (!int.TryParse(chipid.text, out int idValue) || !int.TryParse(chipcount.text, out int countValue))
        {
            Debug.Log("id和count必须是整数");
            return;
        }
        foreach (var item in ChipData.chipList)
        {
            if (idValue == item.ID)
            {
                if (item.number - countValue < 0)
                {
                    Debug.Log("数量不够了,还剩" + item.number.ToString());
                    return;
                }
                //计算花销
                Inventory.Bag[0].expense += countValue * item.price_usd;
                //原料费
                Inventory.Bag[0].rawMaterial+= countValue * item.price_usd;
                number1 = item.number - countValue;
            }
        }
        ChipData.chipList[idValue - 1].number = number1;
        Inventory.Bag[0].Chip[idValue].number += countValue;
        Inventory.Bag[0].Chip[idValue].ID = idValue;
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);
        EventHandler.CallUpdateChipUI(ChipData.chipList);
        EventHandler.CallUpdateHistoryUI(Inventory.Bag);

        //交易显示
        chipID.text = chipid.text;
        chipCount.text = chipcount.text;
        chipname.text = ChipData.chipList[int.Parse(chipID.text) - 1].chip_model;
        return;
    }

    public void BuyDisplay()
    {
        int number1 = 0;
        if (string.IsNullOrEmpty(disid.text) || string.IsNullOrEmpty(discount.text))
        {
            Debug.Log("id或count不能为空");
            return;
        }
        if (!int.TryParse(disid.text, out int idValue) || !int.TryParse(discount.text, out int countValue))
        {
            Debug.Log("id和count必须是整数");
            return;
        }
        foreach (var item in Displayscreen.disList)
        {
            if (idValue == item.ID)
            {
                if (item.number - countValue < 0)
                {
                    Debug.Log("数量不够了,还剩" + item.number.ToString());
                    return;
                }
                Inventory.Bag[0].expense+= countValue * item.price;
                //原料费
                Inventory.Bag[0].rawMaterial += countValue * item.price;

                number1 = item.number - countValue;
            }
        }
        Displayscreen.disList[idValue - 1].number = number1;
        //Inventory.Bag[0].Displayscreen += countValue;
        Inventory.Bag[0].Displayscreen[idValue].number += countValue;
        Inventory.Bag[0].Displayscreen[idValue].ID = idValue;
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);
        EventHandler.CallUpdateDisUI(Displayscreen.disList);
        EventHandler.CallUpdateHistoryUI(Inventory.Bag);

        //交易显示
        disID.text = disid.text;
        disCount.text = discount.text;
        disname.text = Displayscreen.disList[int.Parse(disID.text) - 1].display_model;
        return;
    }

    public void BuyMemory()
    {
        int number1 = 0;
        if (string.IsNullOrEmpty(meid.text) || string.IsNullOrEmpty(mecount.text))
        {
            Debug.Log("id或count不能为空");
            return;
        }
        if (!int.TryParse(meid.text, out int idValue) || !int.TryParse(mecount.text, out int countValue))
        {
            Debug.Log("id和count必须是整数");
            return;
        }
        foreach (var item in Memory.MeList)
        {
            if (idValue == item.ID)
            {
                if (item.number - countValue < 0)
                {
                    Debug.Log("数量不够了,还剩" + item.number.ToString());
                    return;
                }
                Inventory.Bag[0].expense += countValue * item.price;
                //原料费
                Inventory.Bag[0].rawMaterial += countValue * item.price;
                number1 = item.number - countValue;
            }
        }
        Memory.MeList[idValue - 1].number = number1;
        Inventory.Bag[0].Memory[idValue].number += countValue;
        Inventory.Bag[0].Memory[idValue].ID = idValue;
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);
        EventHandler.CallUpdateMeUI(Memory.MeList);
        EventHandler.CallUpdateHistoryUI(Inventory.Bag);

        //交易显示
        meID.text = meid.text;
        meCount.text = mecount.text;
        mename.text = Memory.MeList[int.Parse(meID.text) - 1].memory_model;
        return;
    }

    public void BuyCamera()
    {
        int number1 = 0;
        if (string.IsNullOrEmpty(caid.text) || string.IsNullOrEmpty(cacount.text))
        {
            Debug.Log("id或count不能为空");
            return;
        }
        if (!int.TryParse(caid.text, out int idValue) || !int.TryParse(cacount.text, out int countValue))
        {
            Debug.Log("id和count必须是整数");
            return;
        }
        foreach (var item in Camera1.caList)
        {
            if (idValue == item.ID)
            {
                if (item.number - countValue < 0)
                {
                    Debug.Log("数量不够了,还剩" + item.number.ToString());
                    return;
                }
                Inventory.Bag[0].expense += countValue * item.unit_price;
                //原料费
                Inventory.Bag[0].rawMaterial += countValue * item.unit_price;
                number1 = item.number - countValue;
            }
        }
        Camera1.caList[idValue - 1].number = number1;
        Inventory.Bag[0].Camera[idValue].number += countValue;
        Inventory.Bag[0].Camera[idValue].ID = idValue;
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);
        EventHandler.CallUpdatecaUI(Camera1.caList);
        EventHandler.CallUpdateHistoryUI(Inventory.Bag);

        //交易显示
        caID.text = caid.text;
        caCount.text = cacount.text;
        caname.text = Camera1.caList[int.Parse(caID.text) - 1].camera_model;
        return;
    }
    public void BuyBattery()
    {
        int number1 = 0;
        if (string.IsNullOrEmpty(baid.text) || string.IsNullOrEmpty(bacount.text))
        {
            Debug.Log("id或count不能为空");
            return;
        }
        if (!int.TryParse(baid.text, out int idValue) || !int.TryParse(bacount.text, out int countValue))
        {
            Debug.Log("id和count必须是整数");
            return;
        }
        foreach (var item in Battery.baList)
        {
            if (idValue == item.ID)
            {
                if (item.number - countValue < 0)
                {
                    Debug.Log("数量不够了,还剩" + item.number.ToString());
                    return;
                }
                Inventory.Bag[0].expense += countValue * item.price;
                //原料费
                Inventory.Bag[0].rawMaterial+=countValue * item.price;
                number1 = item.number - countValue;
            }
        }
        Battery.baList[idValue - 1].number = number1;
        Inventory.Bag[0].Battery[idValue].number += countValue;
        Inventory.Bag[0].Battery[idValue].ID = idValue;
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);
        EventHandler.CallUpdateBaUI(Battery.baList);
        EventHandler.CallUpdateHistoryUI(Inventory.Bag);

        //交易显示
        baID.text = baid.text;
        baCount.text = bacount.text;
        baname.text = Battery.baList[int.Parse(baID.text) - 1].battery_model;
        return;
    }

    public void BuyHousing()
    {
        int number1 = 0;
        if (string.IsNullOrEmpty(hoid.text) || string.IsNullOrEmpty(hocount.text))
        {
            Debug.Log("id或count不能为空");
            return;
        }
        if (!int.TryParse(hoid.text, out int idValue) || !int.TryParse(hocount.text, out int countValue))
        {
            Debug.Log("id和count必须是整数");
            return;
        }
        foreach (var item in Housing.hoList)
        {
            if (idValue == item.ID)
            {
                if (item.number - countValue < 0)
                {
                    Debug.Log("数量不够了,还剩" + item.number.ToString());
                    return;
                }
                Inventory.Bag[0].expense += countValue * item.price;
                //原料费
                Inventory.Bag[0].rawMaterial += countValue * item.price;
                number1 = item.number - countValue;
            }
        }
        Housing.hoList[idValue - 1].number = number1;
        Inventory.Bag[0].Housing[idValue].number += countValue;
        Inventory.Bag[0].Housing[idValue].ID = idValue;
        EventHandler.CallUpdateMoneyUI(Inventory.Bag);
        EventHandler.CallUpdateHoUI(Housing.hoList);
        EventHandler.CallUpdateHistoryUI(Inventory.Bag);

        //交易显示
        hoID.text = hoid.text;
        hoCount.text = hocount.text;
        honame.text = Housing.hoList[int.Parse(hoID.text) - 1].part_model;
        return;
    }

    //计算组装手机数量(要优先考虑订单）
    //public void Assemble()
    //{
    //    for (int i = 0; i < 10; i++)
    //    {

    //        if (Inventory.Bag[0].Chip[Mbphone.mbList[i].Chip].number != 0
    //            && Inventory.Bag[0].Displayscreen[Mbphone.mbList[i].Displayscreen].number != 0
    //            && Inventory.Bag[0].Memory[Mbphone.mbList[i].Memory].number != 0
    //            && (Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[0]].number != 0|| Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number != 0|| 
    //            Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number != 0)
    //            && Inventory.Bag[0].Battery[Mbphone.mbList[i].Battery].number != 0
    //            && Inventory.Bag[0].Housing[Mbphone.mbList[i].Housing].number != 0)
    //        {
    //            int t = Math.Min(
    //                Math.Min
    //                (Math.Min(Inventory.Bag[0].Chip[Mbphone.mbList[i].Chip].number, Inventory.Bag[0].Displayscreen[Mbphone.mbList[i].Displayscreen].number),
    //                Math.Min(Inventory.Bag[0].Memory[Mbphone.mbList[i].Memory].number, Inventory.Bag[0].Battery[Mbphone.mbList[i].Battery].number)),
    //                Math.Min(Inventory.Bag[0].Housing[Mbphone.mbList[i].Housing].number
    //                ,(Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[0]].number+ Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number)));
    //            //Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[2]].number
    //            Inventory.Bag[0].phones[i+1].ID = i+1;
    //            Inventory.Bag[0].phones[i].number += t;

    //            //组装后减去数量
    //            Inventory.Bag[0].Chip[Mbphone.mbList[i].Chip].number -=t;
    //            Inventory.Bag[0].Displayscreen[Mbphone.mbList[i].Displayscreen].number -= t;
    //            Inventory.Bag[0].Memory[Mbphone.mbList[i].Memory].number -= t;
    //            Inventory.Bag[0].Battery[Mbphone.mbList[i].Battery].number -= t;
    //            Inventory.Bag[0].Housing[Mbphone.mbList[i].Housing].number -= t;
    //            //关于摄像头的
    //            if(t- Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[0]].number >= 0)
    //            {
    //                Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[0]].number=0;
    //                t -= Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[0]].number;
    //                if(t- Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number >= 0)
    //                {
    //                    Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number = 0;
    //                    t -= Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number;
    //                    //if(t - Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[2]].number >= 0)
    //                    //{
    //                    //    Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[2]].number = 0;
    //                    //    t -= Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[2]].number;
    //                    //}
    //                    //else
    //                    //{
    //                    //    Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[2]].number -= t;
    //                    //}
    //                }
    //                else
    //                {
    //                    Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[1]].number -= t;
    //                }
    //            }
    //            else
    //            {
    //                Inventory.Bag[0].Camera[Mbphone.mbList[i].Camera1[0]].number -= t;
    //            }
    //            Debug.Log(t);
    //        }
    //    }
    //}
    public void Assemble()
    {
        for (int i = 0; i < Mbphone.mbList.Count; i++)
        {
            var phoneModel = Mbphone.mbList[i];

            // 检查基本组件是否足够
            if (Inventory.Bag[0].Chip[phoneModel.Chip].number <= 0 ||
                Inventory.Bag[0].Displayscreen[phoneModel.Displayscreen].number <= 0 ||
                Inventory.Bag[0].Memory[phoneModel.Memory].number <= 0 ||
                Inventory.Bag[0].Battery[phoneModel.Battery].number <= 0 ||
                Inventory.Bag[0].Housing[phoneModel.Housing].number <= 0)
            {
                continue; // 缺少基本组件，跳过
            }

            // 计算所有可用的摄像头总数（所有备选型号之和）
            int totalAvailableCameras = 0;
            List<int> availableCameraIds = new List<int>();
            List<int> availableCameraCounts = new List<int>();

            foreach (var cameraId in phoneModel.Camera1)
            {
                if (cameraId < 0 || cameraId >= Inventory.Bag[0].Camera.Count)
                {
                    continue; // 跳过无效的摄像头ID
                }

                int cameraCount = Inventory.Bag[0].Camera[cameraId].number;
                if (cameraCount > 0)
                {
                    totalAvailableCameras += cameraCount;
                    availableCameraIds.Add(cameraId);
                    availableCameraCounts.Add(cameraCount);
                }
            }

            if (totalAvailableCameras <= 0) continue; // 没有可用的摄像头

            // 计算可组装的手机数量
            int[] availableCounts = new int[]
            {
            Inventory.Bag[0].Chip[phoneModel.Chip].number,
            Inventory.Bag[0].Displayscreen[phoneModel.Displayscreen].number,
            Inventory.Bag[0].Memory[phoneModel.Memory].number,
            Inventory.Bag[0].Battery[phoneModel.Battery].number,
            Inventory.Bag[0].Housing[phoneModel.Housing].number,
            totalAvailableCameras  // 所有备选摄像头的总数量
            };

            // 找出最小可组装数量
            int t = availableCounts.Min();

            if (t <= 0) continue;

            // 更新手机库存
            if (i < Inventory.Bag[0].phones.Count)
            {
                Inventory.Bag[0].phones[i].ID = i;
                Inventory.Bag[0].phones[i].number += t;
            }
            else
            {
                Inventory.Bag[0].phones.Add(new phone { ID = i, number = t });
            }

            // 减去基本组件数量
            Inventory.Bag[0].Chip[phoneModel.Chip].number -= t;
            Inventory.Bag[0].Displayscreen[phoneModel.Displayscreen].number -= t;
            Inventory.Bag[0].Memory[phoneModel.Memory].number -= t;
            Inventory.Bag[0].Battery[phoneModel.Battery].number -= t;
            Inventory.Bag[0].Housing[phoneModel.Housing].number -= t;

            // 减去摄像头数量（智能分配，混合使用不同型号）
            int remainingCamerasToRemove = t;
            StringBuilder cameraUsageLog = new StringBuilder($"手机型号 {phoneModel.phone_model} 摄像头使用情况: ");

            // 按顺序从可用摄像头列表中扣除
            for (int j = 0; j < availableCameraIds.Count; j++)
            {
                if (remainingCamerasToRemove <= 0) break;

                int cameraId = availableCameraIds[j];
                int availableCount = Inventory.Bag[0].Camera[cameraId].number;
                int camerasToRemove = Math.Min(availableCount, remainingCamerasToRemove);

                Inventory.Bag[0].Camera[cameraId].number -= camerasToRemove;
                remainingCamerasToRemove -= camerasToRemove;

                cameraUsageLog.Append($"ID{cameraId}×{camerasToRemove}");
                if (j < availableCameraIds.Count - 1 && remainingCamerasToRemove > 0)
                {
                    cameraUsageLog.Append(" + ");
                }
            }

            Debug.Log($"成功组装 {t} 台手机型号 {phoneModel.phone_model}");
            Debug.Log(cameraUsageLog.ToString());
        }
    }


    /// <summary>
    /// 关于订单系统
    /// </summary>
    private void InitializeOrderSystem()
    {
        orderGenerator.InitializeReferences(
            Mbphone,
            ChipData,
            Displayscreen,
            Memory,
            Camera1,
            Battery,
            Housing
            );
        if (orderData.activeOrders.Count == 0)
        {
            GenerateInitialOrders();
        }
    }

    //生成初始订单
    private void GenerateInitialOrders()
    {
        List<Order> newOrders = orderGenerator.GenerateNewOrders(5);

        orderData.activeOrders.AddRange(newOrders);
        ShowBid();
        Debug.Log($"生成了{newOrders.Count}个初始订单");
    }

    //生成新订单
    public void UpdateOrders()
    {
        for (int i = orderData.activeOrders.Count - 1; i >= 0; i--)
        {
            Order order = orderData.activeOrders[i];
            if (order.daysRemaining <= 0)
            {
                //结束竞标
                ResolveOrder(order);
            }
        }
    }

    //竞标结束，决定中标者
    private void ResolveOrder(Order order)
    {
        if (order.currentBidPrice > 0)
        {
            order.isActive = false;
            order.isWon = true;

            orderData.activeOrders.Remove(order);
            orderData.wonOrders.Add(order);

            Debug.Log($"订单 {order.orderID} 竞标结束，玩家中标！");


        }
        else
        {
            orderData.activeOrders.Remove(order);
            Debug.Log($"订单 {order.orderID} 竞标结束，无人中标");
        }
    }

    //把中标订单放到玩家背包里
    public void BidPutinbag()
    {
        foreach (Order order in orderData.wonOrders)
        {
            Inventory.Bag[0].Orders.Add(order);
        }
    }
    //修正订单输入
    public void RevisionBid()
    {
        if (BidID != null)
        {
            BidID.text = Regex.Replace(BidID.text, @"[^\d]", "");
        }

        if (Quotation != null)
        {
            string cleaned = Regex.Replace(Quotation.text, @"[^\d\.]", "");

            string[] parts = cleaned.Split('.');
            if (parts.Length > 1)
            {
                cleaned = parts[0] + "." + string.Join("", parts, 1, parts.Length - 1);
            }
            Quotation.text = cleaned;
        }
    }
    //更新订单当前价格信息
    public void UpdateOrdersprice()
    {
        int counter = 1;
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            if (orderData.activeOrders[i].orderID == int.Parse(BidID.text))
            {
                Text[] texts = BidContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
                foreach (Text text in texts)
                {
                    switch (text.name)
                    {
                        case "currentBidPrice":
                            if (float.Parse(text.text) < float.Parse(Quotation.text))
                                text.text = Quotation.text;
                            break;
                        case "bidCompany":
                            text.text = "hhh";
                            break;
                        default:
                            break;
                    }
                    orderData.activeOrders[i].currentBidPrice = float.Parse(Quotation.text);
                }
                texts = Array.Empty<Text>();
            }
            counter++;
        }
    }

    //关于点击开始竞拍后计时器计时
    //关于这个地方的想法就是给每个人60秒时间，60秒内有一次抬价机会，如果没有执行，默认放弃，60秒后提交按钮暂时锁住，由另一方抬价，彼此交替，直到时间结束
    public void BidTimer()
    {
        //StartCoroutine(BidTimerCoroutine());
        // 停止之前的协程
        if (bidTimerCoroutine != null)
        {
            StopCoroutine(bidTimerCoroutine);
            bidTimerCoroutine = null;
        }
        // 重置计时器状态
        CountdownTimer.Instance.ResetTimerToZero();

        bidTimerCoroutine = StartCoroutine(BidTimerCoroutine());
    }

    public void SubtractDays()
    {
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            if (orderData.activeOrders[i].daysRemaining > 0)
            {
                orderData.activeOrders[i].daysRemaining--;
            }
        }

    }

    public IEnumerator BidTimerCoroutine()
    {
        int MaxdaysRemaining = 0;
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            if (orderData.activeOrders[i].daysRemaining > MaxdaysRemaining)
            {
                MaxdaysRemaining = orderData.activeOrders[i].daysRemaining;
            }
        }

        CountdownTimer.Instance.countdownTime = bidRoundTime;
        while (MaxdaysRemaining > 0)
        {
            yield return CountdownTimer.Instance.StartCountdownAndWait();
            MaxdaysRemaining--;
            // 更新UI
            EventHandler.CallUpdateOrderUI(orderData.activeOrders);
            UpdateOrders();
            if (MaxdaysRemaining > 0)
            {
                Debug.Log("准备开始下一轮竞拍...");
                yield return new WaitForSeconds(1f); // 等待1秒后继续
            }
        }
        UIManager.Instance.EndBid();
        //存入玩家背包
        BidPutinbag();

    }


    //直接结束竞拍
    public void EndBid()
    {
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            orderData.activeOrders[i].daysRemaining = 0;
        }
        // 更新UI
        EventHandler.CallUpdateOrderUI(orderData.activeOrders);
        UpdateOrders();
        UIManager.Instance.EndBid();

        //存入到当前生产里
        ProductNumber();
        //更新生产
        UpdateProduct();
        //存入玩家背包
        BidPutinbag();
        //初始化订单交付
        DeliveryInitialization();
    }

    //延迟结束竞拍
    public void DelayBid()
    {
        for (int i = 0; i < orderData.activeOrders.Count; i++)
        {
            orderData.activeOrders[i].daysRemaining += 10;
        }
        // 更新UI
        EventHandler.CallUpdateOrderUI(orderData.activeOrders);
    }

    //重新竞拍
    public void AgainBid()
    {

        //计时器停止，按钮又可以重新按动，外加玩家的订单清空
        // 停止所有正在运行的竞拍协程
        if (bidTimerCoroutine != null)
        {
            StopCoroutine(bidTimerCoroutine);
            bidTimerCoroutine = null;
        }

        // 停止倒计时器
        CountdownTimer.Instance.StopCountdown();
        CountdownTimer.Instance.ResetTimerToZero();

        orderData.activeOrders.Clear();
        List<Order> newOrders = orderGenerator.GenerateNewOrders(currentPolicy.BidNumber);
        orderData.activeOrders.Clear();
        orderData.activeOrders.AddRange(newOrders);
        AgainShowBid();

        UIManager.Instance.RestoreClick();

    }

    /// <summary>
    /// 贷款
    /// </summary>

    //提交贷款金额后的影响
    public void Loan()
    {
        //先把金额变成实际金额
        currentLoan.loanID = 1;
        currentLoan.companyType = "手机";
        currentLoan.interestRate = currentPolicy.PraInterestRate;
        if (ParseFloat(UIManager.Instance.loanAmount, currentLoan.amount) > currentLoan.availableCreditLine)
        {
            Debug.Log("想要竞拍的金额大于剩余可贷金额");
            return;
        }
        currentLoan.amount = ParseFloat(UIManager.Instance.loanAmount, currentLoan.amount);
        //实际贷款金额要根据人数来定，比如一个人最多贷款多少，这里为一个人，就简单的等于贷款金额
        currentLoan.actualAmount = currentLoan.amount;
        //关于剩余可贷金额在结算时，要存入
        currentLoan.availableCreditLine -= currentLoan.actualAmount;
        currentPolicy.LoanableFunds = currentLoan.availableCreditLine;
        //支付利息为每年利息
        currentLoan.payInterest = currentLoan.actualAmount * currentLoan.interestRate;


        //将第一年利息算进去
        Inventory.Bag[0].expense += currentLoan.payInterest;

        //贷款期限暂定为三年
        currentLoan.year = 3;
        //当前信息年份
        currentLoan.Loanyear = currentPolicy.year;
        //提交时间暂时不记
        EventHandler.CallUpdateLoanUI(currentLoan, currentPolicy);
    }

    //是否参与贷款按钮（参与以后要在界面上显示贷款参与信息）目前就一个人，所以简单写
    public void ParticipateLoans(bool Isparticipate)
    {
        Inventory.Bag[0].Loans[0].loanID = 1;
        currentLoan.availableCreditLine = currentPolicy.LoanableFunds;
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(CommercialLoanStatus, ParticpateContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company name":
                        text.text = "AAA手机厂";
                        break;
                    case "Company_model":
                        text.text = "手机";
                        break;

                    case "Participation Status":
                        if (Isparticipate)
                            text.text = "参与";
                        else
                        {
                            text.text = "不参与";
                        }
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
    }

    //参与贷款后的竞标结果初始化
    public void InitialResultsLoans()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(CommercialLoanResults, resultContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company name":
                        text.text = "AAA手机厂";
                        break;
                    case "companyType":
                        text.text = "手机";
                        break;
                    case "interestRate":
                        text.text = currentPolicy.PraInterestRate.ToString();
                        break;
                    case "amount":
                        text.text = "0";
                        break;
                    case "actualAmount":
                        text.text = "0";
                        break;
                    case "availableCreditLine":
                        text.text = currentPolicy.LoanableFunds.ToString();
                        break;
                    case "payInterest":
                        text.text = "0";
                        break;
                    case "year":
                        text.text = currentPolicy.year.ToString();
                        break;
                    case "SubmissionTime":
                        text.text = "00:00";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
        UIManager.Instance.LoanBidpanelDisplay(currentPolicy);
    }

    public void UpdateLoanResult()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            Text[] texts = resultContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company name":
                        text.text = "AAA手机厂";
                        break;
                    case "companyType":
                        text.text = currentLoan.companyType;
                        break;
                    case "interestRate":
                        text.text = currentLoan.interestRate.ToString();
                        break;
                    case "amount":
                        text.text = currentLoan.amount.ToString();
                        break;
                    case "actualAmount":
                        text.text = currentLoan.actualAmount.ToString();
                        break;
                    case "availableCreditLine":
                        text.text = currentLoan.availableCreditLine.ToString();
                        break;
                    case "payInterest":
                        text.text = currentLoan.payInterest.ToString();
                        break;
                    case "year":
                        text.text = currentLoan.year.ToString();
                        break;
                    case "SubmissionTime":
                        text.text = "00:00";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //结束贷款竞标
    public void LoanEnd()
    {
        Loan newloan = new Loan();
        Inventory.Bag[0].Loans.Add(newloan);
        //存入个人
        foreach (var Loan in Inventory.Bag[0].Loans)
        {
            if (Loan.loanID == 0)
            {
                Loan.loanID = 1;
                Loan.companyType = currentLoan.companyType;
                Loan.interestRate = currentLoan.interestRate;
                Loan.amount = currentLoan.amount;
                Loan.actualAmount = currentLoan.actualAmount;
                Loan.availableCreditLine = currentLoan.availableCreditLine;
                Loan.payInterest = currentLoan.payInterest;
                Loan.year = currentLoan.year;
                Loan.SubmissionTime = currentLoan.SubmissionTime;
                break;
            }
        }
        //根据年份存入贷款
        loan.UpdateLoan(currentYear, currentLoan);
        EventHandler.CallUpdateLoanUI(currentLoan, currentPolicy);
        Debug.Log("已经存入贷款，存入年份为" + currentYear);
    }

    //根据年份找到相关的贷款信息
    private void LoadLoanForYear(int year)
    {
        //首先看有没有这一年的信息，没有则返回
        if (land.GetLandForYear(year) == null)
        {
            IsLoanYear = false;
            return;
        }
        currentLoan = loan.GetLoanForYear(year);
        EventHandler.CallUpdateLoanUI(currentLoan, currentPolicy);
    }

    public void LoanPreYear()
    {
        LoadLoanForYear(Mathf.Max(1, currentLoanYear - 1));
        if (!IsLoanYear)
        {
            Debug.Log("没有当前年份的贷款信息");
            return;
        }
        currentLoanYear = Mathf.Max(1, currentLoanYear - 1);
    }

    public void LoanNextYear()
    {
        LoadLoanForYear(currentLoanYear++);
        if (!IsLoanYear)
        {
            Debug.Log("没有当前年份的贷款信息");
            return;
        }
        currentLoanYear++;
    }
    //剩余贷款用来查看玩家的贷款情况，先把土地竞标搞完再写剩余贷款


    /// <summary>
    /// 政府政策
    /// </summary>
    /// <returns></returns>


    // 初始化政策
    public GovernmentPolicy CreateDefaultPhoneIndustryPolicy()
    {
        float maxPhonePrice = 0;
        float minPhonePrice = 0;
        float maxChipPrice = 0;
        float maxDisPrice = 0;
        float maxMePrice = 0;
        float maxCaPrice = 0;
        float maxBaPrice = 0;
        float maxHoPrice = 0;
        PerEconomy(ref maxPhonePrice, ref minPhonePrice, ref maxChipPrice,
   ref maxDisPrice, ref maxMePrice, ref maxCaPrice, ref maxBaPrice, ref maxHoPrice);


        GovernmentPolicy policy = new GovernmentPolicy
        {
            year = currentYear,

            // 基础政策
            maxLandSupply = 1100,         // 最大土地供应量
            PraLandSupply = 0,
            minInterestRate = 6,          // 最低利率
            PraInterestRate = 6,
            Moneyupply = 0,
            vatRate = 17,                 // 增值税率
            incomeTaxRate = 25,            // 所得税率

            FiscalStimulusRevenue = 0,
            Subsidy = 0,
            FiscalStimulusFunds = 0,

            SubsidyRate = 0,
            BidNumber = 0,
            // 手机价格调控
            maxPhonePrice = maxPhonePrice,         // 手机最高售价（万元）
            minPhonePrice = minPhonePrice,        // 手机最低售价（万元）

            // 手机零部件进口价格
            maxChipPrice = maxChipPrice,

            maxDisPrice = maxDisPrice,

            maxMePrice = maxMePrice,

            maxCaPrice = maxCaPrice,

            maxBaPrice = maxBaPrice,

            maxHoPrice = maxHoPrice,

            LoanableFunds = 0,
        };

        return policy;
    }

    //确保当前年份政策存在
    public void ApplyPhoneIndustryPolicy()
    {
        if (governmentPolicy == null)
        {
            return;
        }
        LoadPolicyForYear(currentYear);
    }

    private void ApplyCurrentPolicy()
    {
        //这里实现政策对游戏影响
        //1.订单数量的影响
        OrderMeasures();
        //2.更新贷款信息

        ApplyMaterialPriceControls();
        ApplyPhoneIndustryPolicy();
        ApplyFinancialControls();
    }

    //政府措施中的订单数量
    private void OrderMeasures()
    {
        foreach (Transform child in BidContent.transform)
        {
            if (!child.CompareTag("BidName"))
            {
                Destroy(child.gameObject);
            }
        }//先删除初始的订单GameObject
        List<Order> newOrders = orderGenerator.GenerateNewOrders(currentPolicy.BidNumber);
        orderData.activeOrders.Clear();
        orderData.activeOrders.AddRange(newOrders);
        ShowBid();//重新生成
        Debug.Log($"生成了{newOrders.Count}个政府措施订单");
    }
    //关于修改原材料价格（暂时不考虑修改）
    private void ApplyMaterialPriceControls()
    {

    }

    //手机价格控制
    private void ApplyPhonePriceControls()
    {
        if (orderGenerator != null)
        {
            // 可以调整订单生成时的价格计算
        }

    }

    //应用财务控制
    private void ApplyFinancialControls()
    {
        //最低利率，增值税率，所得税率
    }



    // 计算税费
    public float CalculateTax(float profit)
    {
        if (currentPolicy == null) return profit * 0.25f; // 默认25%

        float tax = profit * (currentPolicy.incomeTaxRate / 100);

        return tax;
    }

    //外围经济数据,需要实时更新，根据玩家的订单来计算最大价格
    public void PerEconomy(ref float maxPhonePrice, ref float minPhonePrice, ref float maxChipPrice,
    ref float maxDisPrice, ref float maxMePrice, ref float maxCaPrice, ref float maxBaPrice, ref float maxHoPrice)
    {
        minPhonePrice = float.MaxValue;
        foreach (var order in orderData.activeOrders)
        {
            maxPhonePrice = Math.Max(order.maxPrice, maxPhonePrice);
            minPhonePrice = Math.Min(order.maxPrice, minPhonePrice);
            Debug.Log(minPhonePrice);
            Debug.Log(maxPhonePrice);
        }
        foreach (var chip in ChipData.chipList)
        {
            maxChipPrice = Math.Max(chip.price_usd, maxChipPrice);
            Debug.Log(maxChipPrice);
        }
        foreach (var dis in Displayscreen.disList)
        {
            maxDisPrice = Math.Max(dis.price, maxDisPrice);
            Debug.Log(maxDisPrice);
        }
        foreach (var me in Memory.MeList)
        {
            maxMePrice = Math.Max(me.price, maxMePrice);
        }
        foreach (var ca in Camera1.caList)
        {
            maxCaPrice = Math.Max(ca.unit_price, maxCaPrice);
        }
        foreach (var ba in Battery.baList)
        {
            maxBaPrice = Math.Max(ba.price, maxBaPrice);
        }
        foreach (var ho in Housing.hoList)
        {
            maxHoPrice = Math.Max(ho.price, maxHoPrice);
        }
        //预计可贷款资金（到时候写贷款的时候在写）

    }

    //政府措施
    public void GovernmentMeasures(float praLandSupply, float praInterestRate,
        float moneyupply, float vatRate1, float incomeTaxRate1, float subsidyRate, int bidNumber)
    {
        UIManager.Instance.GovernmentMeasures(praLandSupply, praInterestRate,
        moneyupply, vatRate1, incomeTaxRate1, subsidyRate, bidNumber);

    }

    /// <summary>
    /// 土地
    /// </summary>
    //是否参与土地竞拍

    public void LandInspection(bool Isparticipate)
    {
        Inventory.Bag[0].Lands[0].landID = 1;
        currentLand.TotalSupply = currentPolicy.PraLandSupply;
        currentLand.RemainingLand = currentLand.TotalSupply;
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(CommercialLoanStatus, LandPartcpateContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company name":
                        text.text = "AAA手机厂";
                        break;
                    case "Company_model":
                        text.text = "手机";
                        break;

                    case "Participation Status":
                        if (Isparticipate)
                            text.text = "参与";
                        else
                        {
                            text.text = "不参与";
                        }
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
    }
    //参与土地后的竞标结果初始化
    public void InitialResultsLands()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(LandResutlts, LandResultContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company Name":
                        text.text = "AAA手机厂";
                        break;
                    case "TotalSupply":
                        text.text = currentLand.TotalSupply.ToString();
                        break;
                    case "RentBidding":
                        text.text = "0";
                        break;
                    case "ApplyforLand":
                        text.text = "0";
                        break;
                    case "ActualAcquisition":
                        text.text = "0";
                        break;
                    case " RemainingLand":
                        text.text = currentLand.TotalSupply.ToString();
                        break;
                    case "IsAccept":
                        text.text = "否";
                        break;
                    case "year":
                        text.text = currentPolicy.year.ToString();
                        break;
                    case "Time":
                        text.text = "00:00";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
        //初始化竞标界面显示
        UIManager.Instance.LandBidpanelDisplay(currentPolicy);
    }
    //土地租金提交后
    public void LandRentSubmsit()
    {
        currentLand.RentBidding = ParseFloat(UIManager.Instance.LandRent, currentLand.RentBidding);

        //土地租金也算进花销
        Inventory.Bag[0].expense += currentLand.RentBidding;

        //更新UI
        EventHandler.CallUpdateLandUI(currentLand, currentPolicy);
    }
    public void UpdateLandResult()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            Text[] texts = LandResultContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company Name":
                        text.text = "AAA手机厂";
                        break;
                    case "TotalSupply":
                        text.text = currentLand.TotalSupply.ToString();
                        break;
                    case "RentBidding":
                        text.text = currentLand.RentBidding.ToString();
                        break;
                    case "ApplyforLand":
                        text.text = currentLand.ApplyforLand.ToString();
                        break;
                    case "ActualAcquisition":
                        text.text = currentLand.ActualAcquisition.ToString();
                        break;
                    case " RemainingLand":
                        text.text = currentLand.RemainingLand.ToString();
                        break;
                    case "IsAccept":
                        if (currentLand.IsAccept)
                            text.text = "是";
                        else
                            text.text = "否";
                        break;
                    case "year":
                        text.text = currentPolicy.year.ToString();
                        break;
                    case "Time":
                        text.text = "00:00";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //开始竞拍
    public void StartLandBidding()
    {
        //先把剩余土地算进去
        currentLand.RemainingLand = currentPolicy.PraLandSupply;
    }
    //土地竞标提交
    public void LandBiddingSubmsit()
    {
        currentLand.landID = 1;
        currentLand.CompanyName = "AAA手机厂";
        currentLand.TotalSupply = currentPolicy.PraLandSupply;
        //currentLand.RentBidding
        if (ParseFloat(UIManager.Instance.Landmu, currentLand.ApplyforLand) > currentLand.RemainingLand)
        {
            Debug.Log("申请的土地太大了。超过了目前剩余的土地");
            return;
        }
        currentLand.ApplyforLand = ParseFloat(UIManager.Instance.Landmu, currentLand.ApplyforLand);
        currentLand.ActualAcquisition = currentLand.ApplyforLand;
        currentLand.RemainingLand -= currentLand.ActualAcquisition;
        currentLand.IsAccept = true;
        currentLand.Time = "00:00";
        currentLand.year = currentYear;
        EventHandler.CallUpdateLandUI(currentLand, currentPolicy);
    }
    //结束土地竞标
    public void EndLand()
    {
        Land newland = new Land();
        Inventory.Bag[0].Lands.Add(newland);
        //存入个人
        foreach (var Land in Inventory.Bag[0].Lands)
        {
            if (Land.landID == 0)
            {
                Land.landID = currentLoan.loanID;
                Land.CompanyName = currentLand.CompanyName;
                Land.TotalSupply = currentLand.TotalSupply;
                Land.RentBidding = currentLand.RentBidding;
                Land.ApplyforLand = currentLand.ApplyforLand;
                Land.ActualAcquisition = currentLand.ActualAcquisition;
                Land.RemainingLand = currentLand.RemainingLand;
                Land.IsAccept = currentLand.IsAccept;
                Land.Time = currentLand.Time;
                Land.year = currentLand.year;
                break;
            }
        }
        //根据年份存入贷款
        land.UpdateLand(currentYear, currentLand);
        EventHandler.CallUpdateLandUI(currentLand, currentPolicy);
        Debug.Log("已经存入贷款，存入年份为" + currentYear);
        //生成新的土地查看
        LandInspection();
    }
    //根据年份找到相关的土地信息
    private void LoadLandForYear(int year)
    {
        //首先看有没有这一年的信息，没有则返回
        if (land.GetLandForYear(year) == null)
        {
            IsLandYear = false;
            return;
        }
        currentLand = land.GetLandForYear(year);
        EventHandler.CallUpdateLandUI(currentLand, currentPolicy);
    }
    public void LandPrevYear()
    {
        LoadLandForYear(Mathf.Max(1, currentLandYear - 1));
        if (!IsLandYear)
        {
            Debug.Log("没有当前年份的土地信息");
            return;
        }
        currentLandYear = Mathf.Max(1, currentLandYear - 1);
    }

    public void LandNextYear()
    {
        LoadPolicyForYear(currentLandYear++);
        if (!IsLandYear)
        {
            Debug.Log("没有当前年份的土地信息");
            return;
        }
        currentLandYear++;
    }

    //土地查看的生成
    public void LandInspection()
    {
        int counter = 1;
        for (int i = 0; i < land.land.Count; i++)
        {
            GameObject A = Instantiate(LandView, LandInspectionContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company Name":
                        text.text = "AAA手机厂";
                        break;
                    case "Company Type":
                        text.text = "手机";
                        break;
                    case "Lease Year":
                        text.text = land.land[i].year.ToString();
                        break;
                    case "Year of termination of lease":
                        text.text = "2";//暂时定为一年，之后再进行修改
                        break;
                    case "Ispay":
                        text.text = "是";//之后修改，看玩家的总资产是否可以缴付土地租赁
                        break;
                    case "acreage":
                        text.text = land.land[i].ActualAcquisition.ToString();
                        break;
                    case "land status":
                        if (currentYear > land.land[i].year + 1)
                            text.text = "未使用";
                        else
                            text.text = "使用中";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //---------------组装生产-------------
    //生产线要根据土地亩数和工人数量来决定生产速度，同时加班费可以提高工人组装的速度
    //暂时设定土地每一亩可以可以容纳10位工人，每月工资为3500元，每人每年可以组装50个，每提高400工资可以可以提高0.1的速度
    //这一部分直接采用自动生产，根据计算扣除相应的费用
    //先写一个生产的初始化信息,在政府措施开始后初始化
    //第二次更新放在玩家竞拍完订单后

    private Dictionary<int, int> productionDemands = new Dictionary<int, int>();

    public void ProductionInitialization()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(Productprefab, ProductContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company Name":
                        text.text = "AAA手机厂";
                        break;
                    case "CompanyType":
                        text.text = "手机";
                        break;
                    case "ProductionLineName":
                        text.text = "001";
                        break;
                    case "YearOfCreation":
                        text.text = "0";
                        break;
                    case "YearOfSale":
                        text.text = "0";
                        break;
                    case "RefurbishedStatus":
                        text.text = "未翻新改造";
                        break;
                    case "TotalNumber":
                        text.text = "0";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
    }

    public void UpdateProduct()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            Text[] texts = ProductContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "Company Name":
                        text.text = currentProduct.CompanyName;
                        break;
                    case "CompanyType":
                        text.text = currentProduct.CompanyType;
                        break;
                    case "ProductionLineName":
                        text.text = currentProduct.ProductionLineName;
                        break;
                    case "YearOfCreation":
                        text.text = currentProduct.YearOfCreation.ToString();
                        break;
                    case "YearOfSale":
                        text.text = currentProduct.YearOfSale.ToString();
                        break;
                    case "RefurbishedStatus":
                        text.text = currentProduct.RefurbishedStatus;
                        break;
                    case "TotalNumber":
                        text.text = currentProduct.TotalNumber.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
        UIManager.Instance.ProductYear.text = currentProductYear.ToString();
    }

    //计算生产总数量
    public void ProductNumber()
    {
        productionDemands.Clear();
        foreach (Order order in orderData.wonOrders)
        {
            int modelID = order.phoneModelID;
            if (productionDemands.ContainsKey(modelID))
                productionDemands[modelID] += order.quantity;
            else
                productionDemands[modelID] = order.quantity;
        }

        int total = 0;
        foreach (var kvp in productionDemands) total += kvp.Value;
        currentProduct.TotalNumber = total;

        //这里暂时先填好生产线基础信息
        currentProduct.productID = 1;
        currentProduct.CompanyName = "AAA手机厂";
        currentProduct.CompanyType = "手机";
        currentProduct.ProductionLineName = "001";
        currentProduct.YearOfCreation = currentYear;
        currentProduct.YearOfSale = currentYear;
        currentProduct.RefurbishedStatus = "未翻新改造";
        currentProduct.year = currentYear;
        UpdateProduct();
    }

    //结束生产，存入
    public void EndProduct()
    {
        Product p = new Product();
        product.product.Add(p);
        //存入个人
        foreach (var pt in product.product)
        {
            if (pt.productID == 0)
            {
                pt.productID = currentProduct.productID;
                pt.CompanyName = currentProduct.CompanyName;
                pt.CompanyType = currentProduct.CompanyType;
                pt.ProductionLineName = currentProduct.ProductionLineName;
                pt.YearOfCreation = currentProduct.YearOfCreation;
                pt.YearOfSale = currentProduct.YearOfSale;
                pt.TotalNumber = currentProduct.TotalNumber;
                pt.RefurbishedStatus = currentProduct.RefurbishedStatus;
                pt.year = currentProduct.year;
                break;
            }
        }
        //根据年份存入
        product.UpdateProduct(currentYear, currentProduct);
        UpdateProduct();
        Debug.Log("已经存入生产线信息，存入年份为" + currentYear);
    }


    /// <summary>
    /// 手动生产指定型号的手机
    /// </summary>
    /// <param name="modelID">手机型号ID</param>
    /// <param name="quantity">要生产的数量</param>
    /// <returns>是否成功（资金、工人、组件等条件满足）</returns>
    /// 关于工人未重复使用的问题先放一边

    public bool StartProduct(int modelID, int quantity)
    {
        if (quantity <= 0)
        {
            Debug.Log("生产数量必须大于0");
            return false;
        }

        // 1. 检查手机型号是否存在
        var phoneModel = Mbphone.mbList.Find(m => m.ID == modelID);
        if (phoneModel == null)
        {
            Debug.LogError($"手机型号 ID {modelID} 不存在");
            return false;
        }

        // 2. 计算土地总亩数（向下取整），确定最大可雇佣工人数
        float totalLand = 0;
        foreach (var land in Inventory.Bag[0].Lands)
        {
            // 过滤条件：land.landID = 1 表示有效土地（根据您的数据定义）
            if (land.landID == 1)
                totalLand += land.ActualAcquisition;
        }
        int landMu = Mathf.FloorToInt(totalLand);
        int maxWorkers = landMu * 10; // 每亩最多容纳10名工人

        if (maxWorkers <= 0)
        {
            Debug.LogWarning("没有足够的土地，无法雇佣工人进行生产");
            return false;
        }

        // 3. 计算正常速度所需最少工人数（每人每年50台）
        int minWorkersNormal = Mathf.CeilToInt(quantity / 50f);

        int workersToUse;
        int k = 0; // 加班等级
        float totalCost;

        if (minWorkersNormal <= maxWorkers)
        {
            // 正常生产，只需雇佣所需工人
            workersToUse = minWorkersNormal;
            totalCost = workersToUse * 42000; // 年工资（42000 = 3500*12）
        }
        else
        {
            // 需要加班，使用全部工人，计算所需加班等级 k
            workersToUse = maxWorkers;
            float requiredK = (quantity / (workersToUse * 50f) - 1f) / 0.1f;
            k = Mathf.CeilToInt(requiredK);
            if (k < 0) k = 0;

            const int maxK = 10; // 最大加班等级（速度翻倍）
            if (k > maxK)
            {
                Debug.LogWarning($"目标产量 {quantity} 即使极限加班（{maxK} 级）也无法达到，请增加土地或减少目标。");
                return false;
            }

            // 计算实际可生产数量（工人与加班限制）
            int actualByLabor = Mathf.FloorToInt(workersToUse * 50 * (1 + 0.1f * k));
            if (actualByLabor < quantity)
            {
                Debug.LogWarning($"因工人和加班限制，实际最多可生产 {actualByLabor} 台，将按此数量生产。");
                quantity = actualByLabor; // 调整目标为劳动力允许的最大值
            }

            totalCost = workersToUse * 42000 + workersToUse * k * 4800; // 4800 = 400*12
        }

        // 4. 检查资金是否足够
        if (Inventory.Bag[0].money < totalCost)
        {
            Debug.LogWarning($"资金不足，需要 {totalCost} 元，当前 {Inventory.Bag[0].money} 元");
            return false;
        }

        // 5. 实际组装（受组件限制）
        int assembled = AssembleModel(modelID, quantity);
        if (assembled == 0)
        {
            Debug.Log("组件不足，无法组装任何手机");
            return false;
        }

        // 6. 扣除工资，先算成花销
        //Inventory.Bag[0].money -= totalCost;
        Inventory.Bag[0].expense += totalCost;
        //人工费
        Inventory.Bag[0].manual += totalCost;

        //把生产的手机存入背包
        Inventory.Bag[0].phones[modelID - 1].ID = modelID;
        Inventory.Bag[0].phones[modelID - 1].Name = Mbphone.mbList[modelID - 1].phone_model;
        Inventory.Bag[0].phones[modelID - 1].number = quantity;

        EventHandler.CallUpdateMoneyUI(Inventory.Bag);

        //更新数据表
        UpdateMbphoneInformation();
        Debug.Log($"生产完成：型号 {modelID}，目标 {quantity}，实际组装 {assembled} 台，使用工人 {workersToUse}，加班等级 {k}，总成本 {totalCost} 元");
        return true;
    }

    /// <summary>
    /// UI 调用入口：从输入框读取型号ID和数量，启动生产
    /// </summary>
    public void Model()
    {
        int modelID = ParseInt(UIManager.Instance.MbPhoneID, 0);
        int quantity = ParseInt(UIManager.Instance.MbQuantity, 0);

        if (modelID <= 0 || quantity <= 0)
        {
            Debug.Log("请输入有效的手机型号ID和数量（正整数）");
            return;
        }

        StartProduct(modelID, quantity);
    }

    private int AssembleModel(int modelID, int quantity)
    {
        if (quantity <= 0) return 0;

        //先检查手机型号
        var phoneModel = Mbphone.mbList.Find(m => m.ID == modelID);
        if (phoneModel == null)
        {
            Debug.LogError($"手机型号 ID {modelID} 不存在");
            return 0;
        }

        // 检查基本组件库存（取最小值）
        int chipStock = Inventory.Bag[0].Chip[phoneModel.Chip].number;
        int displayStock = Inventory.Bag[0].Displayscreen[phoneModel.Displayscreen].number;
        int memoryStock = Inventory.Bag[0].Memory[phoneModel.Memory].number;
        int batteryStock = Inventory.Bag[0].Battery[phoneModel.Battery].number;
        int housingStock = Inventory.Bag[0].Housing[phoneModel.Housing].number;

        // 摄像头总库存（所有备选之和）
        int totalCameraStock = 0;
        foreach (int camId in phoneModel.Camera1)
        {
            if (camId >= 0 && camId < Inventory.Bag[0].Camera.Count)
                totalCameraStock += Inventory.Bag[0].Camera[camId].number;
        }

        int maxByComponents = Mathf.Min(
            chipStock, displayStock, memoryStock, batteryStock, housingStock, totalCameraStock
        );

        int actualQuantity = Mathf.Min(quantity, maxByComponents);
        if (actualQuantity <= 0)
        {
            Debug.Log("组件不足，无法组装");
            return 0;
        }

        // 扣除基本组件
        Inventory.Bag[0].Chip[phoneModel.Chip].number -= actualQuantity;
        Inventory.Bag[0].Displayscreen[phoneModel.Displayscreen].number -= actualQuantity;
        Inventory.Bag[0].Memory[phoneModel.Memory].number -= actualQuantity;
        Inventory.Bag[0].Battery[phoneModel.Battery].number -= actualQuantity;
        Inventory.Bag[0].Housing[phoneModel.Housing].number -= actualQuantity;

        // 扣除摄像头（按顺序消耗）
        int remaining = actualQuantity;
        foreach (int camId in phoneModel.Camera1)
        {
            if (remaining <= 0) break;
            int available = Inventory.Bag[0].Camera[camId].number;
            int take = Mathf.Min(available, remaining);
            Inventory.Bag[0].Camera[camId].number -= take;
            remaining -= take;
        }

        // 增加手机库存
        var phoneEntry = Inventory.Bag[0].phones.Find(p => p.ID == modelID);
        if (phoneEntry != null)
            phoneEntry.number += actualQuantity;
        else
            Inventory.Bag[0].phones.Add(new phone { ID = modelID, number = actualQuantity });

        Debug.Log($"成功组装 {actualQuantity} 台 {phoneModel.phone_model}");
        return actualQuantity;
    }

    //目前能做的就是先做一个手机生产表，把所有手机的生产信息写出来，至少要包括手机的ID和名称，然后根据相应的ID进行生产
    //设置一个ID和数量输入，根据ID和数量来看是否可以生产

    //先写一个简单的手机信息表,一方面用来了解手机ID，另一方面查看生产数量
    public void ShowMbphoneInformation()
    {
        int counter = 1;
        for (int i = 0; i < Mbphone.mbList.Count; i++)
        {
            GameObject A = Instantiate(MbPhonePrefab, MbPhoneInformationContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "MbPhoneID":
                        text.text = Mbphone.mbList[i].ID.ToString();
                        break;
                    case "MbPhoneName":
                        text.text = Mbphone.mbList[i].phone_model;
                        break;
                    case "PhoneNumber":
                        text.text = "0";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
    }
    public void InventoryPhones()
    {
        List<phone> p = new List<phone>();
        //让每个玩家都提前有适量手机格子
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            for (int j = 0; j < Mbphone.mbList.Count; j++)
            {
                Inventory.Bag[i].phones.Add(new phone());
            }
        }

        //同时存入基本信息
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            for (int j = 0; j < Mbphone.mbList.Count; j++)
            {
                Inventory.Bag[i].phones[j].ID = Mbphone.mbList[j].ID;
                Inventory.Bag[i].phones[j].Name = Mbphone.mbList[j].phone_model;
            }
        }

    }


    //生产的时候更新
    public void UpdateMbphoneInformation()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag[0].phones.Count; i++)
        {
            Text[] texts = MbPhoneInformationContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "MbPhoneID":
                        text.text = Inventory.Bag[0].phones[i].ID.ToString();
                        break;
                    case "MbPhoneName":
                        text.text = Inventory.Bag[0].phones[i].Name;
                        break;
                    case "PhoneNumber":
                        text.text = Inventory.Bag[0].phones[i].number.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //根据年份找到相关的生产组装信息
    private void LoadProductForYear(int year)
    {
        //首先看有没有这一年的信息，没有则返回
        if (product.GetProductForYear(year) == null)
        {
            IsProductYear = false;
            return;
        }
        currentProduct = product.GetProductForYear(year);
        //这里的更新直接调用上面的方法就行
        UpdateProduct();
    }
    public void ProductPreYear()
    {
        LoadProductForYear(Mathf.Max(1, currentProductYear - 1));
        if (!IsProductYear)
        {
            Debug.Log("没有当前年份的生产组装信息");
            return;
        }
        currentProductYear = Mathf.Max(1, currentProductYear - 1);
    }

    public void ProductNextYear()
    {
        LoadProductForYear(currentProductYear++);
        if (!IsProductYear)
        {
            Debug.Log("没有当前年份的生产组装信息");
            return;
        }
        currentProductYear++;
    }


    //-----------订单交付------------
    //订单信息初始化,在竞标完的时候初始化
    public void DeliveryInitialization()
    {
        int counter = 1;
        for (int i = 0; i < orderData.wonOrders.Count; i++)
        {
            GameObject A = Instantiate(deliveryprefabs, deliveryContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "CompanyName":
                        text.text = "AAA手机厂";
                        break;
                    case "deliveryID":
                        text.text = orderData.wonOrders[i].orderID.ToString();
                        break;
                    case "MbphoneName":
                        text.text = Mbphone.mbList[orderData.wonOrders[i].phoneModelID - 1].phone_model;
                        break;
                    case "inventory":
                        text.text = Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number.ToString();
                        break;
                    case "quotation":
                        //暂时用最大价格作为报价，考虑一下竞拍的时候的目前报价
                        text.text = orderData.wonOrders[i].maxPrice.ToString();
                        break;
                    case "orderQuantity":
                        text.text = orderData.wonOrders[i].quantity.ToString();
                        break;
                    case "SubmittedCount":
                        text.text = "0";
                        break;
                    case "UnpaidQuantity":
                        text.text = orderData.wonOrders[i].quantity.ToString();
                        break;
                    case "TotalRevenue":
                        text.text = "0";
                        break;
                    case "liquidatedDamages":
                        text.text = "0";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //订单交付后的显示
    public void DeliveryDisplay()
    {
        int counter = 1;
        for (int i = 0; i < orderData.wonOrders.Count; i++)
        {
            Text[] texts = deliveryContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "CompanyName":
                        text.text = "AAA手机厂";
                        break;
                    case "deliveryID":
                        text.text = orderData.wonOrders[i].orderID.ToString();
                        break;
                    case "MbphoneName":
                        text.text = Mbphone.mbList[orderData.wonOrders[i].phoneModelID - 1].phone_model;
                        break;
                    case "inventory":
                        text.text = Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number.ToString();
                        break;
                    case "quotation":
                        //暂时用最大价格作为报价，考虑一下竞拍的时候的目前报价
                        text.text = orderData.wonOrders[i].maxPrice.ToString();
                        break;
                    case "orderQuantity":
                        text.text = orderData.wonOrders[i].quantity.ToString();
                        break;
                    case "SubmittedCount":
                        if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
                        {
                            text.text = orderData.wonOrders[i].quantity.ToString();
                        }
                        else
                        {
                            text.text = Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number.ToString();
                        }
                        break;
                    case "UnpaidQuantity":
                        if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
                        {
                            text.text = "0";
                        }
                        else
                        {
                            text.text = (orderData.wonOrders[i].quantity - Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number).ToString();
                        }
                        break;
                    case "TotalRevenue":
                        if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
                        {
                            text.text = (orderData.wonOrders[i].quantity * orderData.wonOrders[i].maxPrice).ToString();
                        }
                        else
                        {
                            text.text = (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number * orderData.wonOrders[i].maxPrice).ToString();
                        }
                        break;
                    case "liquidatedDamages":
                        //未交货罚款 = 未交数量 × 订单竞标报价 × 20 %
                        if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
                        {
                            text.text = "0";
                        }
                        else
                        {
                            text.text = ((orderData.wonOrders[i].quantity - Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number) * orderData.wonOrders[i].maxPrice * 0.2f).ToString();
                        }
                        break;
                    default:
                        break;
                }
            }
            //扣除相应库存
            if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
            {
                Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number -= orderData.wonOrders[i].quantity;
            }
            else
            {
                Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number = 0;
            }
            texts = Array.Empty<Text>();
        }

        //同时存入数据
        DeliveryInformation();
        //交付按钮只能点击一次，防止重复扣除
        UIManager.Instance.Delivery.interactable = false;
    }

    public void DeliveryInformation()
    {
        float Income = 0;
        for (int i = 0; i < orderData.wonOrders.Count; i++)
        {
            currentDelivery.CompanyName = "AAA手机厂";
            currentDelivery.deliveryID = orderData.wonOrders[i].orderID;
            currentDelivery.MbphoneName = Mbphone.mbList[orderData.wonOrders[i].phoneModelID - 1].phone_model;
            currentDelivery.inventory = Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number;
            currentDelivery.quotation = orderData.wonOrders[i].maxPrice;
            currentDelivery.orderQuantity = orderData.wonOrders[i].quantity;
            if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
            {
                currentDelivery.SubmittedCount = orderData.wonOrders[i].quantity;
            }
            else
            {
                currentDelivery.SubmittedCount = Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number;
            }
            if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
            {
                currentDelivery.UnpaidQuantity = 0;
            }
            else
            {
                currentDelivery.UnpaidQuantity = orderData.wonOrders[i].quantity - Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number;
            }
            if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
            {
                currentDelivery.TotalRevenue = orderData.wonOrders[i].quantity * orderData.wonOrders[i].maxPrice;
            }
            else
            {
                currentDelivery.TotalRevenue = Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number * orderData.wonOrders[i].maxPrice;
            }
            if (Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number >= orderData.wonOrders[i].quantity)
            {
                currentDelivery.liquidatedDamages = 0;
            }
            else
            {
                currentDelivery.liquidatedDamages = (orderData.wonOrders[i].quantity - Inventory.Bag[0].phones[orderData.wonOrders[i].phoneModelID - 1].number) * orderData.wonOrders[i].maxPrice * 0.2f;
            }
            currentDelivery.year = currentYear;

            //计算出销售收入
            Income += (currentDelivery.TotalRevenue - currentDelivery.liquidatedDamages);
            currentSubsidy.income = Income;
            Debug.Log(currentSubsidy.income);
            delivery.UpdateDelivery(currentYear, currentDelivery);
            SubsidyDisplay();
        }
    }


    //-----------政府补贴------------
    //初始化（放在政府措施下来以后）
    public void SubsidyInitialization()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(subsidyPrefabs, subsidyContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "CompanyName":
                        text.text = "AAA手机厂";
                        break;
                    case "SubsidyRate":
                        text.text = currentPolicy.SubsidyRate.ToString();
                        break;
                    case "income":
                        text.text = "0";
                        break;
                    case "subsidyFunds":
                        text.text = "0";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }
    //交付完订单后的显示
    public void SubsidyDisplay()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            Text[] texts = subsidyContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "CompanyName":
                        text.text = "AAA手机厂";
                        break;
                    case "SubsidyRate":
                        text.text = currentPolicy.SubsidyRate.ToString();
                        break;
                    case "income":
                        text.text = currentSubsidy.income.ToString();//生产销售收入（这里可能需要把所有订单获取的钱加一起计算）
                        break;
                    case "subsidyFunds":
                        text.text = (currentSubsidy.income * currentPolicy.SubsidyRate).ToString();//政府补贴
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

            //存入
            currentSubsidy.CompanyName = "AAA手机厂";
            currentSubsidy.SubsidyRate = currentPolicy.SubsidyRate;
            currentSubsidy.subsidyFunds = currentSubsidy.income * currentPolicy.SubsidyRate;
            currentSubsidy.year = currentYear;
            subsidy.UpdateSubsidy(currentYear, currentSubsidy);

            //更新最后的损益表
            Updateperformance();
        }


    }


    //3.钱要进行加减
    //4.补贴的显示预制体要调一下大小，同时它未显示钱
    //5.年份
    //报空的问题

    //---------绩效-----------
    //1.损益表
    //先初始化
    public void performanceInitialization()
    {
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            GameObject A = Instantiate(performancePrefab, performanceContent.transform);
            A.name = counter.ToString();
            counter++;
            Text[] texts = A.GetComponentsInChildren<Text>();

            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = "AAA手机厂";
                        break;
                    case "2":
                        text.text = "0";
                        break;
                    case "3":
                        text.text = currentPolicy.vatRate.ToString();
                        break;
                    case "4":
                        text.text = "0";
                        break;
                    case "5":
                        text.text = "0";//销售收入
                        break;
                    case "6":
                        text.text = "0";
                        break;
                    case "7":
                        text.text = "0";
                        break;
                    case "8":
                        text.text = "0";//毛利
                        break;
                    case "9":
                        text.text = "0";
                        break;
                    case "10":
                        text.text = "0";
                        break;
                    case "11":
                        text.text = "0";
                        break;
                    case "12":
                        text.text = "0";
                        break;
                    case "13":
                        text.text = "0";//营业利润
                        break;
                    case "14":
                        text.text = "0";
                        break;
                    case "15":
                        text.text = "0";
                        break;
                    case "16":
                        text.text = "0";//利润总额
                        break;
                    case "17":
                        text.text = currentPolicy.incomeTaxRate.ToString();
                        break;
                    case "18":
                        text.text = "0";
                        break;
                    case "19":
                        text.text = "0";//净利润
                        break;
                    case "20":
                        text.text = "0";
                        break;
                    case "21":
                        text.text = "0";
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();
        }
    }

    //在交付后更新损益表
    public void Updateperformance()
    {
        //先保存，再更新
        int counter = 1;
        for (int i = 0; i < Inventory.Bag.Count; i++)
        {
            currentformance.companyName="AAA手机厂";          // 公司名字
            currentformance.revenueWithTax= delivery.delivery[i].TotalRevenue * (1 + currentPolicy.vatRate);        // 含税收入 暂时用销售收入代替
            currentformance.vatRate=currentPolicy.vatRate;               // 增值税率（%）
            currentformance.vatPayable = currentformance.outputTax- currentformance.inputTax;// 减：增值税
            currentformance.salesRevenue = delivery.delivery[i].TotalRevenue * (1 + currentPolicy.vatRate);          // 一、销售收入（不含税）
            currentformance.costOfGoodsSold=Inventory.Bag[i].rawMaterial+Inventory.Bag[i].manual;       // 减：已销售产品的原料与人工成本(不含税)
            currentformance.landRent=land.land[i].RentBidding;              // 土地和厂房租赁费
            currentformance.grossProfit= subsidy.subsidy[i].income - Inventory.Bag[i].expense;           // 二、毛利
            currentformance.advertisingExpense=50000;    // 广告费暂时算5万
            currentformance.renovationCost=0;        // 翻新改造投入
            currentformance.managementExpense=20000;     // 管理费用
            currentformance.interestExpense= loan.loan[i].payInterest;  // 利息
            currentformance.operatingProfit= subsidy.subsidy[i].income - Inventory.Bag[i].expense - currentformance.renovationCost- currentformance.advertisingExpense- currentformance.managementExpense- currentformance.interestExpense;       // 三、营业利润
            currentformance.currentReward= subsidy.subsidy[i].subsidyFunds;//奖励，目前只有补贴
            currentformance.penalty= delivery.delivery[i].liquidatedDamages;               // 减：违规操作罚金
            currentformance.totalProfit = currentformance.operatingProfit+ currentformance.currentReward - currentformance.penalty;           // 四、利润总额
            currentformance.incomeTaxRate=currentPolicy.incomeTaxRate;         // 所得税率（%）
            currentformance.incomeTax= currentformance.totalProfit* currentformance.incomeTaxRate;             // 减：所得税
            currentformance.netProfit= currentformance.totalProfit - currentformance.incomeTax;             // 五、净利润
            currentformance.inputTax= Inventory.Bag[i].rawMaterial;              // 进项税  暂时用原料费用代替
            currentformance.outputTax = currentformance.salesRevenue;             // 销项税  暂时用销售费用代替

            currentformance.year = currentYear;

            formance.UpdateFormance(currentYear, currentformance);
            Text[] texts = performanceContent.transform.Find(counter.ToString()).GetComponentsInChildren<Text>();
            counter++;
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "1":
                        text.text = currentformance.companyName;
                        break;
                    case "2":
                        text.text = currentformance.revenueWithTax.ToString();
                        break;
                    case "3":
                        text.text = currentformance.vatRate.ToString();
                        break;
                    case "4":
                        text.text = currentformance.vatPayable.ToString();
                        break;
                    case "5":
                        text.text = currentformance.salesRevenue.ToString();    
                        break;
                    case "6":
                        text.text = currentformance.costOfGoodsSold.ToString();
                        break;
                    case "7":
                        text.text = currentformance.landRent.ToString();
                        break;
                    case "8":
                        text.text = currentformance.grossProfit.ToString();
                        break;
                    case "9":
                        text.text = currentformance.advertisingExpense.ToString();
                        break;
                    case "10":
                        text.text = currentformance.renovationCost.ToString();
                        break;
                    case "11":
                        text.text = currentformance.managementExpense.ToString();
                        break;
                    case "12":
                        text.text = currentformance.interestExpense.ToString();
                        break;
                    case "13":
                        text.text = currentformance.operatingProfit.ToString();
                        break;
                    case "14":
                        text.text = currentformance.currentReward.ToString();
                        break;
                    case "15":
                        text.text = currentformance.penalty.ToString();
                        break;
                    case "16":
                        text.text = currentformance.totalProfit.ToString();
                        break;
                    case "17":
                        text.text = currentformance.incomeTaxRate.ToString();
                        break;
                    case "18":
                        text.text = currentformance.incomeTax.ToString();
                        break;
                    case "19":
                        text.text = currentformance.netProfit.ToString();
                        break;
                    case "20":
                        text.text = currentformance.inputTax.ToString();
                        break;
                    case "21":
                        text.text = currentformance.outputTax.ToString();
                        break;
                    default:
                        break;
                }
            }
            texts = Array.Empty<Text>();

        }
    }
}