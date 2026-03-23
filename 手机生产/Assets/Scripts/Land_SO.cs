using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Land_SO", menuName = "Land/Land_SO")]
public class Land_SO : ScriptableObject
{
    public List<Land> land = new List<Land>();
    public int currentLandIndex = 0;

    //获取当前政策
    public Land GetCurrentLand()
    {
        if (land.Count > currentLandIndex)
        {
            return land[currentLandIndex];
        }
        return null;
    }

    //获取指定年份的贷款
    public Land GetLandForYear(int year)
    {
        return land.Find(p => p.year == year);
    }

    // 添加贷款
    public void AddLoan(Land lands)
    {
        land.Add(lands);
    }

    // 更新贷款
    public void UpdateLand(int year, Land newLand)
    {
        int index = land.FindIndex(p => p.year == year);
        if (index >= 0)
        {
            land[index] = newLand;
        }
        else
        {
            land.Add(newLand);
        }
    }
}
