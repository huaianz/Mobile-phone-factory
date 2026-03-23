using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Delivery_SO", menuName = "Delivery/Delivery_SO")]
public class Delivery_SO : ScriptableObject
{
    public List<Delivery> delivery = new List<Delivery>();
    public int currentDeliveryIndex = 0;

    //获取当前订单交付信息
    public Delivery GetCurrentDelivery()
    {
        if (delivery.Count > currentDeliveryIndex)
        {
            return delivery[currentDeliveryIndex];
        }
        return null;
    }

    //获取指定年份的交付
    public Delivery GetDeliveryForYear(int year)
    {
        return delivery.Find(p => p.year == year);
    }

    // 添加
    public void AddDelivery(Delivery deliverys)
    {
        delivery.Add(deliverys);
    }

    // 更新
    public void UpdateDelivery(int year, Delivery newDelivery)
    {
        int index = delivery.FindIndex(p => p.year == year);
        if (index >= 0)
        {
            delivery[index] = newDelivery;
        }
        else
        {
            delivery.Add(newDelivery);
        }
    }
}