using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GovernmentPolicy_SO", menuName = "Government/GovernmentPolicy_SO")]
public class GovernmentPolicy_SO : ScriptableObject
{
    public List<GovernmentPolicy> policies = new List<GovernmentPolicy>();
    public int currentPolicyIndex = 0;
    
    //获取当前政策
    public GovernmentPolicy GetCurrentPolicy()
    {
        if(policies.Count>currentPolicyIndex)
        {
            return policies[currentPolicyIndex];
        }
        return null;
    }

    //获取指定年份的政策
    public GovernmentPolicy GetPolicyForYear(int year)
    {
        return policies.Find(p => p.year == year);
    }

    // 添加政策
    public void AddPolicy(GovernmentPolicy policy)
    {
        policies.Add(policy);
    }

    // 更新政策
    public void UpdatePolicy(int year, GovernmentPolicy newPolicy)
    {
        int index = policies.FindIndex(p => p.year == year);
        if (index >= 0)
        {
            policies[index] = newPolicy;
        }
        else
        {
            policies.Add(newPolicy);
        }
    }
}
