using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loan_SO", menuName = "Loan/Loan_SO")]
public class Loan_SO : ScriptableObject
{
    public List<Loan> loan = new List<Loan>();
    public int currentLoanIndex = 0;

    //获取当前政策
    public Loan GetCurrentLoan()
    {
        if (loan.Count > currentLoanIndex)
        {
            return loan[currentLoanIndex];
        }
        return null;
    }

    //获取指定年份的贷款
    public Loan GetLoanForYear(int year)
    {
        return loan.Find(p => p.year == year);
    }

    // 添加贷款
    public void AddLoan(Loan loans)
    {
        loan.Add(loans);
    }

    // 更新贷款
    public void UpdateLoan(int year, Loan newLoan)
    {
        int index = loan.FindIndex(p => p.Loanyear == year);
        if (index >= 0)
        {
            loan[index] = newLoan;
        }
        else
        {
            loan.Add(newLoan);
        }
    }
}