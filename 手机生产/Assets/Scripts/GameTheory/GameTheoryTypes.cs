using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarketPricePoint
{
    public string date;
    public float gdpGrowth;
    public float inflation;
    public float lendingRate;
    public float demandIndex;
    public float landIndex;
    public float deliveryIndex;

    public float chip;
    public float display;
    public float memory;
    public float camera;
    public float battery;
    public float housing;
    public float phone;
}

[Serializable]
public class EpisodeMetrics
{
    public int episode;
    public float radicalProfit;
    public float conservativeProfit;
    public float radicalDeliveryRate;
    public float conservativeDeliveryRate;
    public float radicalAvgLoan;
    public float conservativeAvgLoan;
    public float radicalLandCost;
    public float conservativeLandCost;
}

[Serializable]
public class AgentDecision
{
    public float bidPremiumRatio;
    public float productionRatio;
    public float inventorySafetyFactor;
}

[Serializable]
public class AgentPolicyState
{
    public string agentName;
    public bool aggressive;
    public float bidPremiumRatio;
    public float productionRatio;
    public float inventorySafetyFactor;
    public float learningRate;

    public AgentPolicyState(string name, bool isAggressive)
    {
        agentName = name;
        aggressive = isAggressive;

        if (aggressive)
        {
            bidPremiumRatio = 0.18f;
            productionRatio = 0.85f;
            inventorySafetyFactor = 0.6f;
            learningRate = 0.08f;
        }
        else
        {
            bidPremiumRatio = 0.05f;
            productionRatio = 0.65f;
            inventorySafetyFactor = 1.8f;
            learningRate = 0.06f;
        }
    }

    public AgentDecision Decide(float demandForecast, float availableCash)
    {
        AgentDecision d = new AgentDecision();
        d.bidPremiumRatio = Mathf.Clamp(bidPremiumRatio, 0f, 0.5f);
        d.productionRatio = Mathf.Clamp(productionRatio, 0.2f, 1.0f);
        d.inventorySafetyFactor = Mathf.Clamp(inventorySafetyFactor, 0.3f, 2.5f);

        if (availableCash < 20000f)
        {
            d.bidPremiumRatio *= 0.6f;
            d.productionRatio *= 0.8f;
        }

        if (demandForecast > 0f)
        {
            float signal = Mathf.Clamp01(demandForecast / 1000f);
            d.productionRatio = Mathf.Clamp01(d.productionRatio + 0.1f * signal);
        }

        return d;
    }

    public void LearnFromReward(float reward)
    {
        float direction = Mathf.Clamp(reward / 100000f, -1f, 1f);

        if (aggressive)
        {
            bidPremiumRatio = Mathf.Clamp(bidPremiumRatio + learningRate * 0.03f * direction, 0f, 0.5f);
            productionRatio = Mathf.Clamp(productionRatio + learningRate * 0.04f * direction, 0.2f, 1f);
            inventorySafetyFactor = Mathf.Clamp(inventorySafetyFactor - learningRate * 0.02f * direction, 0.3f, 2.5f);
        }
        else
        {
            bidPremiumRatio = Mathf.Clamp(bidPremiumRatio + learningRate * 0.015f * direction, 0f, 0.5f);
            productionRatio = Mathf.Clamp(productionRatio + learningRate * 0.02f * direction, 0.2f, 1f);
            inventorySafetyFactor = Mathf.Clamp(inventorySafetyFactor + learningRate * 0.03f * (-direction), 0.3f, 2.5f);
        }
    }
}

public static class PublicMarketCsvLoader
{
    public static List<MarketPricePoint> Parse(TextAsset csvAsset)
    {
        List<MarketPricePoint> result = new List<MarketPricePoint>();
        if (csvAsset == null || string.IsNullOrWhiteSpace(csvAsset.text))
        {
            return result;
        }

        string[] lines = csvAsset.text.Replace("\r", string.Empty).Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            string[] parts = line.Split(',');
            if (parts.Length >= 14)
            {
                MarketPricePoint point = new MarketPricePoint();
                point.date = parts[0];
                point.gdpGrowth = ParseFloat(parts[1]);
                point.inflation = ParseFloat(parts[2]);
                point.lendingRate = ParseFloat(parts[3]);
                point.demandIndex = ParseFloat(parts[4]);
                point.landIndex = ParseFloat(parts[5]);
                point.deliveryIndex = ParseFloat(parts[6]);
                point.chip = ParseFloat(parts[7]);
                point.display = ParseFloat(parts[8]);
                point.memory = ParseFloat(parts[9]);
                point.camera = ParseFloat(parts[10]);
                point.battery = ParseFloat(parts[11]);
                point.housing = ParseFloat(parts[12]);
                point.phone = ParseFloat(parts[13]);
                result.Add(point);
                continue;
            }

            if (parts.Length < 8)
            {
                continue;
            }

            MarketPricePoint p = new MarketPricePoint();
            p.date = parts[0];
            p.gdpGrowth = 5f;
            p.inflation = 2f;
            p.lendingRate = 4f;
            p.demandIndex = 100f;
            p.landIndex = 100f;
            p.deliveryIndex = 100f;
            p.chip = ParseFloat(parts[1]);
            p.display = ParseFloat(parts[2]);
            p.memory = ParseFloat(parts[3]);
            p.camera = ParseFloat(parts[4]);
            p.battery = ParseFloat(parts[5]);
            p.housing = ParseFloat(parts[6]);
            p.phone = ParseFloat(parts[7]);
            result.Add(p);
        }

        return result;
    }

    private static float ParseFloat(string s)
    {
        float v;
        if (float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
        {
            return v;
        }
        if (float.TryParse(s, out v))
        {
            return v;
        }
        return 0f;
    }
}

public static class PriceSimulationEngine
{
    public static float NextGbmPrice(float currentPrice, float mu, float sigma, float dt, System.Random rng)
    {
        if (currentPrice <= 0f)
        {
            currentPrice = 1f;
        }

        double u1 = Math.Max(double.Epsilon, rng.NextDouble());
        double u2 = Math.Max(double.Epsilon, rng.NextDouble());
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

        double drift = (mu - 0.5 * sigma * sigma) * dt;
        double diffusion = sigma * Math.Sqrt(dt) * z;
        double next = currentPrice * Math.Exp(drift + diffusion);
        return Mathf.Max(0.01f, (float)next);
    }
}
