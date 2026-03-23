using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Formance_SO", menuName = "Formance/Formance_SO")]
public class Formance_SO : ScriptableObject
{
    public List<formance> formances = new List<formance>();
    public int currentFormanceIndex = 0;

    //获取当前政策
    public formance GetCurrentFormance()
    {
        if (formances.Count > currentFormanceIndex)
        {
            return formances[currentFormanceIndex];
        }
        return null;
    }

    //获取指定年份的政策
    public formance GetformanceForYear(int year)
    {
        return formances.Find(p => p.year == year);
    }

    // 添加政策
    public void AddFormance(formance formance)
    {
        formances.Add(formance);
    }

    // 更新政策
    public void UpdateFormance(int year, formance newformance)
    {
        int index = formances.FindIndex(p => p.year == year);
        if (index >= 0)
        {
            formances[index] = newformance;
        }
        else
        {
            formances.Add(newformance);
        }
    }
}

