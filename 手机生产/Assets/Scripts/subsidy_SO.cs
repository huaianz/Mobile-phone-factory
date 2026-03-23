using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "subsidy_SO", menuName = "subsidy/subsidy_SO")]
public class subsidy_SO : ScriptableObject
{
    public List<subsidy> subsidy = new List<subsidy>();
    public int currentSubsidyIndex = 0;

    //获取当前补贴
    public subsidy GetCurrentSubsidy()
    {
        if (subsidy.Count > currentSubsidyIndex)
        {
            return subsidy[currentSubsidyIndex];
        }
        return null;
    }

    //获取指定年份的补贴
    public subsidy GetSubsidyForYear(int year)
    {
        return subsidy.Find(p => p.year == year);
    }

    // 添加
    public void AddSubsidy(subsidy subsidys)
    {
        subsidy.Add(subsidys);
    }

    // 更新
    public void UpdateSubsidy(int year, subsidy newsubsidy)
    {
        int index = subsidy.FindIndex(p => p.year == year);
        if (index >= 0)
        {
            subsidy[index] = newsubsidy;
        }
        else
        {
            subsidy.Add(newsubsidy);
        }
    }
}