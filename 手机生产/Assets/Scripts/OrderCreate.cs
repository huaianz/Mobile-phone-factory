using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderCreate : MonoBehaviour
{
    [Header("数据引用")]
    public Mbphone_SO mbphoneData;
    public ChipDatalist_SO chipData;
    public Displayscreen_SO displayData;
    public Memory_SO memoryData;
    public Camera_SO cameraData;
    public Battery_SO batteryData;
    public Housing_SO housingData;

    [Header("订单生成设置")]
    public int minOrderQuantity = 100;     // 最小订单数量
    public int maxOrderQuantity = 1000;    // 最大订单数量
    public int ordersToGenerate = 9;//待生成订单
    public float minProfitMargin = 0.1f;   // 最小利润率
    public float maxProfitMargin = 0.5f;   // 最大利润率
    public int bidDays = 7;//竞标天数


    // 客户名称列表
    private string[] customerNames = new string[]
    {
        "中国移动", "中国联通", "中国电信",
        "京东商城", "天猫商城", "苏宁易购",
        "小米官方", "华为商城", "苹果官网",
        "迪信通", "国美电器", "海外客户A",
        "海外客户B", "企业客户C", "政府采购"
    };

    // 初始化数据引用
    public void InitializeReferences(
        Mbphone_SO mbphone,
        ChipDatalist_SO chip,
        Displayscreen_SO display,
        Memory_SO memory,
        Camera_SO camera,
        Battery_SO battery,
        Housing_SO housing)
    {
        mbphoneData = mbphone;
        chipData = chip;
        displayData = display;
        memoryData = memory;
        cameraData = camera;
        batteryData = battery;
        housingData = housing;
    }
    //生成新订单列表
    public List<Order> GenerateNewOrders(int count)
    {
        List<Order> newOrder=new List<Order>();

        for(int i=0;i<count;i++)
        {
            Order order = GenerateSingleOrder();
            newOrder.Add(order);
        }
        return newOrder;
    }

    //生成单个订单
    private Order GenerateSingleOrder()
    {
        int phoneIndex=Random.Range(0,mbphoneData.mbList.Count);
        Mobilephone phoneModel = mbphoneData.mbList[phoneIndex];

        //生产成本
        float productionCost = CalculateProductionCost(phoneModel);
        //生成订单数量
        int quantity =Random.Range(minOrderQuantity,maxOrderQuantity+1);

        //计算最高价格
        float profitMargin = UnityEngine.Random.Range(minProfitMargin, maxProfitMargin);
        float maxPrice = productionCost * (1 + profitMargin);

        //当前出价
        float currentBidPrice = 0f;

        //生成订单
        Order newOrder = new Order
        {
            orderID = GenerateOrderID(),
            customerName = GetCustomerName(),
            phoneModelID = phoneModel.ID,
            quantity = quantity,
            maxPrice = maxPrice,
            currentBidPrice = currentBidPrice,
            daysRemaining = bidDays,
            isActive = true,
            isWon = false,
            deliveryDeadline = Random.Range(30f, 91f)
        };


        return newOrder;
    }

    //计算生产成本
    private float CalculateProductionCost(Mobilephone phoneModel)
    {
        float totalCost = 0f;
        totalCost += chipData.chipList[phoneModel.Chip - 1].price_usd;
        totalCost += displayData.disList[phoneModel.Displayscreen - 1].price;
        totalCost += memoryData.MeList[phoneModel.Memory - 1].price;
        totalCost += batteryData.baList[phoneModel.Battery - 1].price;
        totalCost += housingData.hoList[phoneModel.Housing - 1].price;

        //摄像头取最贵的
        float CameraCost = float.MinValue;
        foreach (int cameraId in phoneModel.Camera1)
        {
            float cameraPrice = cameraData.caList[cameraId - 1].unit_price;
            if (cameraPrice > CameraCost)
                CameraCost = cameraPrice;
        }
        totalCost += CameraCost;
        totalCost *= 1.1f;//除了组装外别的费用，暂时先变成1.1倍
        return totalCost;
    }

    //生成订单ID
    private int GenerateOrderID()
    {
        return Random.Range(10000, 99999);
    }

    //随机客户名称
    private string GetCustomerName()
    {
        return customerNames[Random.Range(0, customerNames.Length)];
    }
}
