using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 数据可序列化
/// </summary>
[System.Serializable]
public class SerializedChipData
{
    public List<Chip> ChipList;

    public SerializedChipData()
    {
        ChipList = new List<Chip>();
    }

    public SerializedChipData(ChipDatalist_SO chipSO)
    {
        ChipList = new List<Chip>(chipSO.chipList);
    }
}

[System.Serializable]
public class DisplayscreenData
{
    public List<Displayscreen> disList;

    public DisplayscreenData()
    {
        disList = new List<Displayscreen>();
    }

    public DisplayscreenData(Displayscreen_SO disSO)
    {
        disList = new List<Displayscreen>(disSO.disList);
    }
}

[System.Serializable]
public class MemoryData
{
    public List<Memory> meList;

    public MemoryData()
    {
        meList = new List<Memory>();
    }

    public MemoryData(Memory_SO meSO)
    {
        meList = new List<Memory>(meSO.MeList);
    }
}

[System.Serializable]
public class CameraData
{
    public List<Camera1> caList;

    public CameraData()
    {
        caList = new List<Camera1>();
    }

    public CameraData(Camera_SO caSO)
    {
        caList = new List<Camera1>(caSO.caList);
    }
}

public class BatteryData
{
    public List<Battery> baList;

    public BatteryData()
    {
        baList = new List<Battery>();
    }

    public BatteryData(Battery_SO baSO)
    {
        baList = new List<Battery>(baSO.baList);
    }
}

public class HousingData
{
    public List<Housing> hoList;

    public HousingData()
    {
        hoList = new List<Housing>();
    }

    public HousingData(Housing_SO hoSO)
    {
        hoList = new List<Housing>(hoSO.hoList);
    }
}

public class PhoneData
{
    public List<Mobilephone> mbList;

    public PhoneData()
    {
        mbList = new List<Mobilephone>();
    }

    public PhoneData(Mbphone_SO mbSO)
    {
        mbList = new List<Mobilephone>(mbSO.mbList);
    }
}

/// <summary>
/// 单例
/// </summary>
public class PackageLocalData
{
    //单例
    private static PackageLocalData _instance;

    public static PackageLocalData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLocalData();
            }
            return _instance;
        }
    }


    //数据
    private ChipDatalist_SO _chipDataSO;
    private Displayscreen_SO _displayscreenSO;
    private Memory_SO _memorySO;
    private Camera_SO _cameraSO;
    private Battery_SO _batterySO;
    private Housing_SO _housingSO;
    private Mbphone_SO _mbPhoneSO;

    /// <summary>
    /// 关于芯片的数据加载和存储
    /// </summary>
    /// <param name="chipSO"></param>
    public void SaveChip(ChipDatalist_SO chipSO)
    {
        // 1. 检查输入是否为空
        if (chipSO == null)
        {
            Debug.LogError("SaveChip: 传入的chipSO为null");
            return;
        }

        // 2. 转换为可序列化的数据
        SerializedChipData data = new SerializedChipData(chipSO);
        string chipJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/chip/chip.json";
        File.WriteAllText(filePath, chipJson);

        Debug.Log($"保存成功: {filePath}, 保存了 {data.ChipList.Count} 个芯片");
    }

    public ChipDatalist_SO LoadChip(string filePath)
    {
        // 1. 确保私有变量不为空
        if (_chipDataSO == null)
        {
            _chipDataSO = ScriptableObject.CreateInstance<ChipDatalist_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _chipDataSO.chipList = new List<Chip>();
            return _chipDataSO;
        }

        // 3. 读取文件
        string chipJson = File.ReadAllText(filePath);

        // 4. 反序列化
        SerializedChipData serializedData = JsonUtility.FromJson<SerializedChipData>(chipJson);

        // 5. 检查反序列化结果
        if (serializedData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _chipDataSO.chipList = new List<Chip>();
            return _chipDataSO;
        }

        // 6. 设置到ScriptableObject
        if (serializedData.ChipList != null)
        {
            _chipDataSO.chipList = serializedData.ChipList;
            Debug.Log($"加载成功: {_chipDataSO.chipList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("ChipList为null");
            _chipDataSO.chipList = new List<Chip>();
        }

        return _chipDataSO;
    }
    /// <summary>
    /// 关于显示屏的数据存储
    /// </summary>
    /// <param name="chipSO"></param>
    public void SaveDisplay(Displayscreen_SO disSO)
    {
        // 1. 检查输入是否为空
        if (disSO == null)
        {
            Debug.LogError("SaveChip: 传入的disSO为null");
            return;
        }

        // 2. 转换为可序列化的数据
        DisplayscreenData data = new DisplayscreenData(disSO);
        string disJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/dis/dis.json";
        File.WriteAllText(filePath, disJson);

        Debug.Log($"保存成功: {filePath}, 保存了 {data.disList.Count} 个芯片");
    }

    public Displayscreen_SO LoadDisplay(string filePath)
    {
        // 1. 确保私有变量不为空
        if (_displayscreenSO == null)
        {
            _displayscreenSO = ScriptableObject.CreateInstance<Displayscreen_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _displayscreenSO.disList = new List<Displayscreen>();
            return _displayscreenSO;
        }

        // 3. 读取文件
        string disJson = File.ReadAllText(filePath);

        // 4. 反序列化
        DisplayscreenData displayData = JsonUtility.FromJson<DisplayscreenData>(disJson);
        //Debug.Log(displayData.disList[6]);

        // 5. 检查反序列化结果
        if (displayData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _displayscreenSO.disList = new List<Displayscreen>();
            return _displayscreenSO;
        }

        // 6. 设置到ScriptableObject
        if (displayData.disList != null)
        {
            _displayscreenSO.disList = displayData.disList;
            Debug.Log($"加载成功: {displayData.disList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("ChipList为null");
            _displayscreenSO.disList= new List<Displayscreen>();
        }

        return _displayscreenSO;
    }
    /// <summary>
    /// 关于内存的数据存储
    /// </summary>
    /// <param name="meSO"></param>
    public void SaveMe(Memory_SO meSO)
    {
        MemoryData data = new MemoryData(meSO);
        string meJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/memory/memory.json";
        File.WriteAllText(filePath, meJson);
    }
    public Memory_SO LoadMe(string  filePath)
    {
        // 1. 确保私有变量不为空
        if (_memorySO == null)
        {
            _memorySO = ScriptableObject.CreateInstance<Memory_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _memorySO.MeList = new List<Memory>();
            return _memorySO;
        }

        // 3. 读取文件
        string chipJson = File.ReadAllText(filePath);

        // 4. 反序列化
        MemoryData serializedData = JsonUtility.FromJson<MemoryData>(chipJson);

        // 5. 检查反序列化结果
        if (serializedData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _memorySO.MeList = new List<Memory>();
            return _memorySO;
        }

        // 6. 设置到ScriptableObject
        if (serializedData.meList != null)
        {
            _memorySO.MeList = serializedData.meList;
            Debug.Log($"加载成功: {_memorySO.MeList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("ChipList为null");
            _memorySO.MeList = new List<Memory>();
        }

        return _memorySO;
    }
    /// <summary>
    /// 关于摄像头的数据存储
    /// </summary>
    /// <param name="caSO"></param>
    public void SaveCa(Camera_SO caSO)
    {
        CameraData data = new CameraData(caSO);
        string meJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/camera/camera.json";
        File.WriteAllText(filePath, meJson);
    }
    public Camera_SO LoadCa(string filePath)
    {
        // 1. 确保私有变量不为空
        if (_cameraSO == null)
        {
            _cameraSO = ScriptableObject.CreateInstance<Camera_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _cameraSO.caList = new List<Camera1>();
            return _cameraSO;
        }

        // 3. 读取文件
        string disJson = File.ReadAllText(filePath);

        // 4. 反序列化
        CameraData cameraData = JsonUtility.FromJson<CameraData>(disJson);
        //Debug.Log(displayData.disList[6]);

        // 5. 检查反序列化结果
        if (cameraData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _cameraSO.caList = new List<Camera1>();
            return _cameraSO;
        }

        // 6. 设置到ScriptableObject
        if (cameraData.caList != null)
        {
            _cameraSO.caList = cameraData.caList;
            Debug.Log($"加载成功: {cameraData.caList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("camera为null");
            _cameraSO.caList = new List<Camera1>();
        }

        return _cameraSO;
    }
    /// <summary>
    /// 关于电池的数据存储
    /// </summary>
    /// <param name="baSO"></param>
    public void SaveBa(Battery_SO baSO)
    {
        BatteryData data = new BatteryData(baSO);
        string baJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/battery/battery.json";
        File.WriteAllText(filePath, baJson);
    }
    public Battery_SO LoadBa(string filePath)
    {
        // 1. 确保私有变量不为空
        if (_batterySO == null)
        {
            _batterySO = ScriptableObject.CreateInstance<Battery_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _batterySO.baList = new List<Battery>();
            return _batterySO;
        }

        // 3. 读取文件
        string baJson = File.ReadAllText(filePath);

        // 4. 反序列化
        BatteryData batteryData = JsonUtility.FromJson<BatteryData>(baJson);
        //Debug.Log(displayData.disList[6]);

        // 5. 检查反序列化结果
        if (batteryData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _batterySO.baList = new List<Battery>();
            return _batterySO;
        }

        // 6. 设置到ScriptableObject
        if (batteryData.baList != null)
        {
            _batterySO.baList = batteryData.baList;
            Debug.Log($"加载成功: {batteryData.baList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("camera为null");
            _batterySO.baList = new List<Battery>();
        }

        return _batterySO;
    }

    /// <summary>
    /// 关于外壳的数据存储
    /// </summary>
    /// <param name="baSO"></param>
    public void SaveHo(Housing_SO hoSO)
    {
        HousingData data = new HousingData(hoSO);
        string hoJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/housing/housing.json";
        File.WriteAllText(filePath, hoJson);
    }
    public Housing_SO LoadHo(string filePath)
    {
        // 1. 确保私有变量不为空
        if (_housingSO == null)
        {
            _housingSO = ScriptableObject.CreateInstance<Housing_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _housingSO.hoList = new List<Housing>();
            return _housingSO;
        }

        // 3. 读取文件
        string hoJson = File.ReadAllText(filePath);

        // 4. 反序列化
        HousingData housingData = JsonUtility.FromJson<HousingData>(hoJson);
        //Debug.Log(displayData.disList[6]);

        // 5. 检查反序列化结果
        if (housingData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _housingSO.hoList = new List<Housing>();
            return _housingSO;
        }

        // 6. 设置到ScriptableObject
        if (housingData.hoList != null)
        {
            _housingSO.hoList = housingData.hoList;
            Debug.Log($"加载成功: {housingData.hoList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("camera为null");
            _housingSO.hoList = new List<Housing>();
        }

        return _housingSO;
    }

    /// <summary>
    /// 关于手机的数据存储
    /// </summary>
    /// <param name="hoSO"></param>
    public void SaveMb(Mbphone_SO mbSO)
    {
        PhoneData data = new PhoneData(mbSO);
        string mbJson = JsonUtility.ToJson(data, true);

        // 3. 保存到文件
        string filePath = Application.dataPath + "/Resources/phone/phone.json";
        File.WriteAllText(filePath, mbJson);
    }
    public Mbphone_SO LoadMb(string filePath)
    {
        // 1. 确保私有变量不为空
        if (_mbPhoneSO == null)
        {
            _mbPhoneSO = ScriptableObject.CreateInstance<Mbphone_SO>();
        }

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            _mbPhoneSO.mbList = new List<Mobilephone>();
            return _mbPhoneSO;
        }

        // 3. 读取文件
        string mbJson = File.ReadAllText(filePath);

        // 4. 反序列化
        PhoneData phoneData = JsonUtility.FromJson<PhoneData>(mbJson);
        //Debug.Log(displayData.disList[6]);

        // 5. 检查反序列化结果
        if (phoneData == null)
        {
            Debug.LogError("JSON反序列化失败");
            _mbPhoneSO.mbList = new List<Mobilephone>();
            return _mbPhoneSO;
        }

        // 6. 设置到ScriptableObject
        if (phoneData.mbList != null)
        {
            _mbPhoneSO.mbList = phoneData.mbList;
            Debug.Log($"加载成功: {phoneData.mbList.Count} 个芯片");
        }
        else
        {
            Debug.LogWarning("camera为null");
            _mbPhoneSO.mbList = new List<Mobilephone>();
        }

        return _mbPhoneSO;
    }
}