using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class PublicDataWebImporter : MonoBehaviour
{
    [Header("输出")]
    [SerializeField] private string outputFileName = "market_prices_real_web.csv";
    [SerializeField] private int fromYear = 2019;
    [SerializeField] private int toYear = 2024;

    [Header("World Bank(中国)")]
    [SerializeField] private string gdpGrowthUrl = "https://api.worldbank.org/v2/country/CHN/indicator/NY.GDP.MKTP.KD.ZG?format=json&per_page=80";
    [SerializeField] private string inflationUrl = "https://api.worldbank.org/v2/country/CHN/indicator/FP.CPI.TOTL.ZG?format=json&per_page=80";
    [SerializeField] private string lendingRateUrl = "https://api.worldbank.org/v2/country/CHN/indicator/FR.INR.LEND?format=json&per_page=80";

    [Header("FRED 指标代理")]
    [SerializeField] private string ppiAllCommoditiesUrl = "https://fred.stlouisfed.org/graph/fredgraph.csv?id=PPIACO";
    [SerializeField] private string oilUrl = "https://fred.stlouisfed.org/graph/fredgraph.csv?id=DCOILWTICO";
    [SerializeField] private string housePriceUrl = "https://fred.stlouisfed.org/graph/fredgraph.csv?id=CSUSHPINSA";
    [SerializeField] private string demandProxyUrl = "https://fred.stlouisfed.org/graph/fredgraph.csv?id=RSAFS";
    [SerializeField] private string chipProxyUrl = "https://fred.stlouisfed.org/graph/fredgraph.csv?id=PCU33441334413";

    private readonly Dictionary<int, float> gdpByYear = new Dictionary<int, float>();
    private readonly Dictionary<int, float> inflByYear = new Dictionary<int, float>();
    private readonly Dictionary<int, float> lendByYear = new Dictionary<int, float>();

    private readonly Dictionary<int, float> ppiByYear = new Dictionary<int, float>();
    private readonly Dictionary<int, float> oilByYear = new Dictionary<int, float>();
    private readonly Dictionary<int, float> landByYear = new Dictionary<int, float>();
    private readonly Dictionary<int, float> demandByYear = new Dictionary<int, float>();
    private readonly Dictionary<int, float> chipByYear = new Dictionary<int, float>();

    public void ImportFromWeb()
    {
        StartCoroutine(ImportCoroutine());
    }

    private IEnumerator ImportCoroutine()
    {
        yield return FetchWorldBankYearValue(gdpGrowthUrl, gdpByYear);
        yield return FetchWorldBankYearValue(inflationUrl, inflByYear);
        yield return FetchWorldBankYearValue(lendingRateUrl, lendByYear);

        yield return FetchFredCsvYearAverage(ppiAllCommoditiesUrl, ppiByYear);
        yield return FetchFredCsvYearAverage(oilUrl, oilByYear);
        yield return FetchFredCsvYearAverage(housePriceUrl, landByYear);
        yield return FetchFredCsvYearAverage(demandProxyUrl, demandByYear);
        yield return FetchFredCsvYearAverage(chipProxyUrl, chipByYear);

        string path = Path.Combine(Application.persistentDataPath, outputFileName);
        StringBuilder sb = new StringBuilder(2048);
        sb.AppendLine("date,gdpGrowth,inflation,lendingRate,demandIndex,landIndex,deliveryIndex,chip,display,memory,camera,battery,housing,phone");

        for (int y = fromYear; y <= toYear; y++)
        {
            float gdp = GetValueOrDefault(gdpByYear, y, 5f);
            float infl = GetValueOrDefault(inflByYear, y, 2f);
            float lend = GetValueOrDefault(lendByYear, y, 4.35f);
            float demand = Normalize(GetValueOrDefault(demandByYear, y, 100f), 100f);
            float land = Normalize(GetValueOrDefault(landByYear, y, 100f), 100f);
            float delivery = Normalize(GetValueOrDefault(oilByYear, y, 70f), 70f);

            float ppi = GetValueOrDefault(ppiByYear, y, 200f);
            float chip = GetValueOrDefault(chipByYear, y, ppi * 1.05f);
            float display = ppi * 0.13f;
            float memory = ppi * 0.10f;
            float camera = ppi * 0.11f;
            float battery = ppi * 0.08f;
            float housing = ppi * 0.07f;
            float phone = ppi * 0.65f + chip * 0.15f;

            string line = string.Format(CultureInfo.InvariantCulture,
                "{0}-01,{1:F4},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4},{12:F4},{13:F4}",
                y,
                gdp,
                infl,
                lend,
                demand,
                land,
                delivery,
                chip,
                display,
                memory,
                camera,
                battery,
                housing,
                phone);

            sb.AppendLine(line);
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log("[PublicDataWebImporter] 数据已写入: " + path);
    }

    private IEnumerator FetchWorldBankYearValue(string url, Dictionary<int, float> target)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[PublicDataWebImporter] WorldBank请求失败: " + url + " | " + req.error);
                yield break;
            }

            string text = req.downloadHandler.text;
            string[] lines = text.Replace("\r", string.Empty).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                int dateIdx = l.IndexOf("\"date\":\"");
                int valueIdx = l.IndexOf("\"value\":");
                if (dateIdx < 0 || valueIdx < 0)
                {
                    continue;
                }

                int yStart = dateIdx + 8;
                int yEnd = l.IndexOf('"', yStart);
                if (yEnd < 0)
                {
                    continue;
                }

                string ys = l.Substring(yStart, yEnd - yStart);
                int year;
                if (!int.TryParse(ys, out year))
                {
                    continue;
                }

                string valueRaw = l.Substring(valueIdx + 8).Trim();
                if (valueRaw.StartsWith("null"))
                {
                    continue;
                }

                int comma = valueRaw.IndexOf(',');
                if (comma > 0)
                {
                    valueRaw = valueRaw.Substring(0, comma);
                }

                float v;
                if (float.TryParse(valueRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                {
                    target[year] = v;
                }
            }
        }
    }

    private IEnumerator FetchFredCsvYearAverage(string url, Dictionary<int, float> target)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[PublicDataWebImporter] FRED请求失败: " + url + " | " + req.error);
                yield break;
            }

            string[] lines = req.downloadHandler.text.Replace("\r", string.Empty).Split('\n');
            Dictionary<int, float> sum = new Dictionary<int, float>();
            Dictionary<int, int> count = new Dictionary<int, int>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] p = line.Split(',');
                if (p.Length < 2 || p[1] == ".")
                {
                    continue;
                }

                int year;
                if (p[0].Length < 4 || !int.TryParse(p[0].Substring(0, 4), out year))
                {
                    continue;
                }

                float val;
                if (!float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                {
                    continue;
                }

                if (!sum.ContainsKey(year))
                {
                    sum[year] = 0f;
                    count[year] = 0;
                }
                sum[year] += val;
                count[year] += 1;
            }

            foreach (KeyValuePair<int, float> kv in sum)
            {
                int c = count[kv.Key];
                if (c > 0)
                {
                    target[kv.Key] = kv.Value / c;
                }
            }
        }
    }

    private static float GetValueOrDefault(Dictionary<int, float> map, int year, float fallback)
    {
        float v;
        if (map.TryGetValue(year, out v))
        {
            return v;
        }
        return fallback;
    }

    private static float Normalize(float v, float baseValue)
    {
        if (Mathf.Abs(baseValue) < 0.0001f)
        {
            return 100f;
        }
        return v / baseValue * 100f;
    }
}
