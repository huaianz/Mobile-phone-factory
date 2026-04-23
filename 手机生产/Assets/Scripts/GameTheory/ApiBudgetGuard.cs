using System;
using UnityEngine;

[Serializable]
public class ApiBudgetGuard
{
    [Header("API预算控制")]
    public int maxCallsPerTrainingRun = 20;
    public int maxCallsPerEpisode = 2;

    [NonSerialized] public int callsThisRun;
    [NonSerialized] public int callsThisEpisode;

    public void BeginRun()
    {
        callsThisRun = 0;
        callsThisEpisode = 0;
    }

    public void BeginEpisode()
    {
        callsThisEpisode = 0;
    }

    public bool TryConsume(string reason)
    {
        if (callsThisRun >= maxCallsPerTrainingRun)
        {
            Debug.Log("[ApiBudgetGuard] 跳过调用，达到训练总调用上限: " + reason);
            return false;
        }

        if (callsThisEpisode >= maxCallsPerEpisode)
        {
            Debug.Log("[ApiBudgetGuard] 跳过调用，达到单局调用上限: " + reason);
            return false;
        }

        callsThisRun++;
        callsThisEpisode++;
        return true;
    }
}
