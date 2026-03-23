using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "OrderList_SO", menuName = "Order/OrderList_SO")]
public class OrderList_SO : ScriptableObject
{
    //活跃订单，中标订单，完成订单
    public List<Order> activeOrders=new List<Order>();

    public List<Order> wonOrders=new List<Order>();

    public List<Order> completedOrders=new List<Order>();

    public void ClearAllOrders()
    {
        activeOrders.Clear();
        wonOrders.Clear();
        completedOrders.Clear();
    }
}