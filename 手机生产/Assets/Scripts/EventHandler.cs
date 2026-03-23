using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    //更新钱币
    public static event Action<List<Inventory>> UpdateMoneyUI;
    public static void CallUpdateMoneyUI(List<Inventory> bag)
    {
        UpdateMoneyUI?.Invoke(bag);
    }

    //更新信息界面数据
    public static event Action<List<Chip>> UpdateChipUI;
    public static void CallUpdateChipUI(List<Chip> chip)
    {
        UpdateChipUI?.Invoke(chip);
    }

    public static event Action<List<Displayscreen>> UpdateDisUI;
    public static void CallUpdateDisUI(List<Displayscreen> dis)
    {
        UpdateDisUI?.Invoke(dis);
    }

    public static event Action<List<Memory>> UpdateMeUI;
    public static void CallUpdateMeUI(List<Memory> me)
    {
        UpdateMeUI?.Invoke(me);
    }

    public static event Action<List<Camera1>> UpdateCaUI;
    public static void CallUpdatecaUI(List<Camera1> ca)
    {
        UpdateCaUI?.Invoke(ca);
    }

    public static event Action<List<Battery>> UpdateBaUI;
    public static void CallUpdateBaUI(List<Battery> ba)
    {
        UpdateBaUI?.Invoke(ba);
    }

    public static event Action<List<Housing>> UpdateHoUI;
    public static void CallUpdateHoUI(List<Housing> ho)
    {
        UpdateHoUI?.Invoke(ho);
    }

    //更新交易历史
    public static event Action<List<Inventory>> UpdateHistoryUI;
    public static void CallUpdateHistoryUI(List<Inventory> bag)
    {
        UpdateHistoryUI?.Invoke(bag);
    }

    //更新订单信息
    public static event Action<List<Order>> UpdateOrderUI;
    public static void CallUpdateOrderUI(List<Order> orders)
    {
        UpdateOrderUI?.Invoke(orders);
    }

    //更新政府政策
    public static event Action<GovernmentPolicy> UpdatePolicyUI;
    public static void CallUpdatePolicyUI(GovernmentPolicy policy)
    {
        UpdatePolicyUI?.Invoke(policy);
    }

    //更新贷款竞标结果
    public static event Action<Loan,GovernmentPolicy> UpdateLoanUI;
    public static void CallUpdateLoanUI(Loan loan,GovernmentPolicy policy)
    {
        UpdateLoanUI?.Invoke(loan,policy);
    }

    //更新土地竞拍结果
    public static event Action<Land,GovernmentPolicy> UpdateLandUI;
    public static void CallUpdateLandUI(Land land,GovernmentPolicy policy)
    {
        UpdateLandUI?.Invoke(land,policy);
    }

}
